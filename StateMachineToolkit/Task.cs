#region License

/* Copyright (c) 2006 Leslie Sanford
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 */

#endregion

#region Contact

/*
 * Leslie Sanford
 * Email: jabberdabber@hotmail.com
 */

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Sanford.Collections;

namespace Sanford.StateMachineToolkit
{
	internal sealed class Task : IDisposable, IComparable
	{
		// The number of times left to invoke the delegate associated with this Task.

		// The PriorityQueue belonging to the DelegateScheduler this Task 
		// belongs to. The Task will enqueue itself back into the PriorityQueue
		// after each timeout until the count reaches zero. If the count is
		// infinite, the Task will enqueue itself an unlimited number of times.
		private readonly object lockObject = new object();
		private readonly PriorityQueue queue;

		// The Task pool belonging to the DelegateScheduler this Task belongs
		// to. Once the Task has run out of timeouts, it will push itself into
		// the pool. It can then be reused by the DelegateScheduler.
		private readonly Stack taskPool;
		private readonly Thread thread;
		private volatile bool disposed;
		private object[] m_args;
		private int m_count;

		// Indicates whether the Task has been disposed.
		private Delegate m_method;
		private int m_millisecondsTimeout;
		private ISynchronizeInvoke m_synchronizingObject;
		private DateTime nextTimeout;

		// For locking.

		public Task(
			int count,
			int millisecondsTimeout,
			ISynchronizeInvoke synchronizingObject,
			Delegate method,
			object[] args,
			PriorityQueue queue,
			Stack taskPool)
		{
			#region Require

			if (queue == null)
			{
				throw new ArgumentNullException("queue");
			}

			if (taskPool == null)
			{
				throw new ArgumentNullException("taskPool");
			}

			#endregion

			this.queue = queue;
			this.taskPool = taskPool;

			Recycle(count, millisecondsTimeout, synchronizingObject, method, args);

			thread = new Thread(Run);

			lock (SyncRoot)
			{
				thread.Start();

				Monitor.Wait(SyncRoot);
			}
		}

		public object SyncRoot
		{
			get { return lockObject; }
		}

		public DateTime NextTimeout
		{
			get { return nextTimeout; }
		}

		public int MillisecondsTimeout
		{
			get { return m_millisecondsTimeout; }
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			Task t = obj as Task;

			if (t == null)
			{
				throw new ArgumentException("obj is not the same type as this instance.");
			}

			return -nextTimeout.CompareTo(t.nextTimeout);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			disposed = true;

			lock (SyncRoot)
			{
				Monitor.Pulse(SyncRoot);
			}
		}

		#endregion

		public void Recycle(
			int count,
			int millisecondsTimeout,
			ISynchronizeInvoke synchronizingObject,
			Delegate method,
			object[] args)
		{
			#region Require

			if (disposed)
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

			#endregion

			lock (SyncRoot)
			{
				m_count = count;
				m_millisecondsTimeout = millisecondsTimeout;
				m_synchronizingObject = synchronizingObject;
				m_method = method;
				m_args = args;

				nextTimeout = DateTime.Now.AddMilliseconds(millisecondsTimeout);

				queue.Enqueue(this);
			}
		}

		private void Run()
		{
			lock (SyncRoot)
			{
				// Let the constructor know that the thread is running.
				Monitor.Pulse(SyncRoot);

				// While the Task has not been disposed.
				while (!disposed)
				{
					// Wait for the next timeout.
					Monitor.Wait(SyncRoot);

					// If the Task has not been disposed.
					if (disposed) continue;

					// If the count is zero, the Task should have already 
					// taken itself out of the queue.
					Debug.Assert(m_count != 0);

					// If there is a synchronizing object.
					if (m_synchronizingObject != null)
					{
						// Use it for invoking the delegate.
						m_synchronizingObject.BeginInvoke(m_method, m_args);
					}
						// Else there is no synchronizing object.
					else
					{
						// Invoke the delegate directly.
						m_method.DynamicInvoke(m_args);
					}

					// If the count is less than zero (infinite).
					if (m_count < 0)
					{
						// Calculate the next timeout.
						nextTimeout = nextTimeout.AddMilliseconds(m_millisecondsTimeout);

						// Put Task back into queue to be run again.
						queue.Enqueue(this);
					}
						// Else the count is finite.
					else
					{
						m_count--;

						// If there are still timeouts for this Task.
						if (m_count > 0)
						{
							// Calculate the next timeout.
							nextTimeout = nextTimeout.AddMilliseconds(m_millisecondsTimeout);

							// Put Task back into queue to be run again.
							queue.Enqueue(this);
						}
							// Else there are no more timeouts for this Task.
						else
						{
							// Put Task into pool to be reused again.
							taskPool.Push(this);
						}
					}
				}
			}
		}
	}
}