using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using Sanford.Collections;
using Timer=System.Timers.Timer;

namespace Sanford.Threading
{
	public class DelegateScheduler : IComponent
	{
		private readonly SynchronizationContext context;
		private const int DefaultPollingInterval = 10;
		private bool disposed;
		public const int Infinity = -1;
		private readonly PriorityQueue queue;
		private bool running;
		private ISite site;
		private readonly Stack taskPool;
		private readonly Timer timer;

		public event EventHandler Disposed;

		public event EventHandler<InvokeCompletedEventArgs> InvokeCompleted;

		public DelegateScheduler()
		{
			queue = PriorityQueue.Synchronized(new PriorityQueue());
			taskPool = Stack.Synchronized(new Stack());
			timer = new Timer(DefaultPollingInterval);
			running = false;
			disposed = false;
			site = null;
			timer.Elapsed += HandleElapsed;
			context = SynchronizationContext.Current ?? new SynchronizationContext();
		}

		public DelegateScheduler(IContainer container)
		{
			queue = PriorityQueue.Synchronized(new PriorityQueue());
			taskPool = Stack.Synchronized(new Stack());
			timer = new Timer(10.0);
			running = false;
			disposed = false;
			site = null;
			container.Add(this);
			timer.Elapsed += HandleElapsed;
			context = SynchronizationContext.Current ?? new SynchronizationContext();
		}

		public void Add(int count, int millisecondsTimeout, Delegate method, params object[] args)
		{
			if (disposed)
			{
				throw new ObjectDisposedException("DelegateScheduler");
			}
			lock (taskPool.SyncRoot)
			{
				if (taskPool.Count > 0)
				{
					Task task = (Task) taskPool.Pop();
					task.Recycle(count, millisecondsTimeout, method, args);
				}
				else
				{
					new Task(count, millisecondsTimeout, method, args, this);
				}
			}
		}

		public void Clear()
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			lock (taskPool.SyncRoot)
			{
				while (taskPool.Count > 0)
				{
					((Task) taskPool.Pop()).Dispose();
				}
			}
			lock (queue.SyncRoot)
			{
				foreach (Task task in queue)
				{
					task.Dispose();
				}
				queue.Clear();
			}
			Debug.Assert(queue.Count == 0);
			Debug.Assert(taskPool.Count == 0);
		}

		public void Dispose()
		{
			if (!disposed)
			{
				Dispose(true);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing) return;
			Stop();
			timer.Dispose();
			Clear();
			disposed = true;
			OnDisposed(EventArgs.Empty);
			GC.SuppressFinalize(this);
		}

		~DelegateScheduler()
		{
			Dispose(false);
		}

		private void HandleElapsed(object sender, ElapsedEventArgs e)
		{
			if (queue.Count <= 0) return;
			Task task = (Task) queue.Peek();
			while ((queue.Count > 0) && (task.NextTimeout <= e.SignalTime))
			{
				queue.Dequeue();
				lock (task.SyncRoot)
				{
					Monitor.Pulse(task.SyncRoot);
				}
				if (queue.Count > 0)
				{
					task = (Task) queue.Peek();
				}
			}
		}

		protected virtual void OnDisposed(EventArgs e)
		{
			if (Disposed != null)
			{
				Disposed(this, e);
			}
		}

		protected virtual void OnInvokeCompleted(InvokeCompletedEventArgs e)
		{
			EventHandler<InvokeCompletedEventArgs> handler = InvokeCompleted;
			if (handler == null) return;
			SendOrPostCallback d = delegate { handler(this, e); };
			context.Post(d, null);
		}

		public void Start()
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (IsRunning) return;
			running = true;
			timer.Start();
		}

		public void Stop()
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			timer.Stop();
			running = false;
		}

		public bool IsRunning
		{
			get { return running; }
		}

		public double PollingInterval
		{
			get
			{
				if (disposed)
				{
					throw new ObjectDisposedException("PriorityQueue");
				}
				return timer.Interval;
			}
			set
			{
				if (disposed)
				{
					throw new ObjectDisposedException("PriorityQueue");
				}
				timer.Interval = value;
			}
		}

		public ISite Site
		{
			get { return site; }
			set { site = value; }
		}

		private class Task : IDisposable, IComparable
		{
			private object[] m_args;
			private int m_count;
			private volatile bool m_disposed;
			private readonly object m_lockObject = new object();
			private Delegate m_method;
			private int m_millisecondsTimeout;
			private DateTime m_nextTimeout;
			private readonly DelegateScheduler m_owner;
			private readonly Thread m_thread;

			public Task(int count, int millisecondsTimeout, Delegate method, object[] args, DelegateScheduler owner)
			{
				m_owner = owner;
				Recycle(count, millisecondsTimeout, method, args);
				m_thread = new Thread(Run);
				lock (SyncRoot)
				{
					m_thread.Start();
					Monitor.Wait(SyncRoot);
				}
			}

			public int CompareTo(object obj)
			{
				Task task = obj as Task;
				if (task == null)
				{
					throw new ArgumentException("obj is not the same type as this instance.");
				}
				return -m_nextTimeout.CompareTo(task.m_nextTimeout);
			}

			public void Dispose()
			{
				m_disposed = true;
				lock (SyncRoot)
				{
					Monitor.Pulse(SyncRoot);
				}
			}

			public void Recycle(int count, int millisecondsTimeout, Delegate method, object[] args)
			{
				if (m_disposed)
				{
					throw new ObjectDisposedException("Task");
				}
				if (count == 0)
				{
					throw new ArgumentException("Task count cannot be zero.", "count");
				}
				if (millisecondsTimeout < 0)
				{
					throw new ArgumentOutOfRangeException("millisecondsTimeout");
				}
				if (method == null)
				{
					throw new ArgumentNullException("method");
				}
				if (args == null)
				{
					throw new ArgumentNullException("args");
				}
				lock (SyncRoot)
				{
					m_count = count;
					m_millisecondsTimeout = millisecondsTimeout;
					m_method = method;
					m_args = args;
					m_nextTimeout = DateTime.Now.AddMilliseconds(millisecondsTimeout);
					m_owner.queue.Enqueue(this);
				}
			}

			private void Run()
			{
				lock (SyncRoot)
				{
					Monitor.Pulse(SyncRoot);
					while (true)
					{
						Monitor.Wait(SyncRoot);
						if (m_disposed)
						{
							return;
						}
						Debug.Assert(m_count != 0);
						InvokeCompletedEventArgs args;
						try
						{
							object result = m_method.DynamicInvoke(m_args);
							args = new InvokeCompletedEventArgs(m_method, m_args, result, null);
							m_owner.OnInvokeCompleted(args);
						}
						catch (Exception exception)
						{
							args = new InvokeCompletedEventArgs(m_method, m_args, null, exception);
							m_owner.OnInvokeCompleted(args);
						}
						if (m_count < 0)
						{
							m_nextTimeout = m_nextTimeout.AddMilliseconds(m_millisecondsTimeout);
							m_owner.queue.Enqueue(this);
						}
						else
						{
							m_count--;
							if (m_count > 0)
							{
								m_nextTimeout = m_nextTimeout.AddMilliseconds(m_millisecondsTimeout);
								m_owner.queue.Enqueue(this);
							}
							else
							{
								m_owner.taskPool.Push(this);
							}
						}
					}
				}
			}

			public DateTime NextTimeout
			{
				get { return m_nextTimeout; }
			}

			public object SyncRoot
			{
				get { return m_lockObject; }
			}
		}
	}
}