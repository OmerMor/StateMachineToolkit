using System;
using System.Threading;
using Sanford.Threading;

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// The ActiveStateMachine class uses the Active Object design pattern. 
	/// What this means is that an ActiveStateMachine object runs in its own thread. 
	/// Internally, ActiveStateMachines use <see cref="DelegateQueue"/> objects for handling 
	/// and dispatching events. 
	/// You derive your state machines from this class when you want them to be active objects.<para/>
	/// The ActiveStateMachine class implements the <see cref="IDisposable"/> interface. 
	/// Since it represents an  active object, it needs to be disposed of at some point to 
	/// shut its thread down. 
	/// The Dispose method was made virtual so that derived ActiveStateMachine classes can override it. 
	/// Typically, a derived ActiveStateMachine will override the Dispose method, and when it is called, 
	/// will send an event to itself using the <see cref="SendPriority"/> method telling it to dispose of itself. 
	/// In other words, disposing of an ActiveStateMachine is treated like an event. 
	/// How your state machine handles the disposing event depends on its current state. 
	/// However, at some point, your state machine will need to call the ActiveStateMachine's 
	/// <see cref="Dispose(bool)"/> base class method, passing it a true value. 
	/// This lets the base class dispose of its <see cref="DelegateQueue"/> object, thus shutting down the 
	/// thread in which it is running.
	/// </summary>
	/// <typeparam name="TState">The state enumeration type.</typeparam>
	/// <typeparam name="TEvent">The event enumeration type.</typeparam>
	public abstract class ActiveStateMachine<TState, TEvent> : StateMachine<TState, TEvent>, IDisposable
		where TState : struct, IComparable, IFormattable /*, IConvertible*/
		where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ActiveStateMachine{TState, TEvent}"/> class.
		/// </summary>
		protected ActiveStateMachine()
		{
			m_context = SynchronizationContext.Current;
			m_queue.PostCompleted +=
				delegate(object sender, PostCompletedEventArgs args)
					{
						if (args.Error != null)
						{
							OnExceptionThrown(
								new TransitionErrorEventArgs<TState, TEvent>(
									m_currentEventContext, args.Error));
						}
					};
			m_queue.InvokeCompleted +=
				delegate(object sender, InvokeCompletedEventArgs args)
					{
						if (args.Error != null)
						{
							OnExceptionThrown(
								new TransitionErrorEventArgs<TState, TEvent>(
									m_currentEventContext, args.Error));
						}
					};
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="ActiveStateMachine{TState, TEvent}"/> is reclaimed by garbage collection.
		/// </summary>
		~ActiveStateMachine()
		{
			Dispose(false);
		}

		#region Fields

		private readonly SynchronizationContext m_context;
		// Used for queuing events.
		private readonly DelegateQueue m_queue = new DelegateQueue();

		private volatile bool m_disposed;
		private bool m_syncContext;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the state machine type: active or passive.
		/// </summary>
		/// <value></value>
		public override StateMachineType StateMachineType
		{
			get { return StateMachineType.Active; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is disposed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
		/// </value>
		protected bool IsDisposed
		{
			get { return m_disposed; }
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			#region Guard

			if (IsDisposed)
			{
				return;
			}

			#endregion

			Dispose(true);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; 
		/// <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing) return;
			m_disposed = true;
			m_queue.Dispose();

			GC.SuppressFinalize(this);

		}

		/// <summary>
		/// Initializes the StateMachine's initial state.
		/// </summary>
		/// <param name="initialState">The state that will initially receive events from the StateMachine.</param>
		protected override void Initialize(State initialState)
		{
			Exception initException = null;
			m_queue.Send(delegate
			           	{
			           		try
			           		{
			           			InitializeStateMachine(initialState);
			           		}
			           		catch (Exception ex)
			           		{
			           			initException = ex;
			           		}
			           	}, null);

			if (initException != null)
			{
				throw new InvalidOperationException("State machine failed to initialize.", initException);
			}
		}

		/// <summary>
		/// Asserts that the state machine was initialized and not disposed.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the state machine was not initialized.</exception>
		/// <exception cref="ObjectDisposedException">Thrown when the state machine was already disposed.</exception>"
		protected override void AssertMachineIsValid()
		{
			base.AssertMachineIsValid();
			if (IsDisposed)
			{
				throw new ObjectDisposedException(
					string.Format("ActiveStateMachine<{0},{1}>", typeof (TState).Name, typeof (TEvent).Name),
					"State machine was already disposed.");
			}
		}

		/// <summary>
		/// Sends an event to the StateMachine.
		/// </summary>
		/// <param name="eventID">
		/// The event ID.
		/// </param>
		/// <param name="args">
		/// The data accompanying the event.
		/// </param>
		public override void Send(TEvent eventID, object[] args)
		{
			AssertMachineIsValid();
			m_queue.Post(delegate { Dispatch(eventID, args); }, null);
		}

		/// <summary>
		/// Sends an event to the StateMachine, and blocks until it processing ends.
		/// </summary>
		/// <param name="eventID">
		/// The event ID.
		/// </param>
		/// <param name="args">
		/// The data accompanying the event.
		/// </param>
		public void SendSynchronously(TEvent eventID, params object[] args)
		{
			AssertMachineIsValid();
			m_queue.Send(delegate
			           	{
			           		m_syncContext = true;
			           		try
			           		{
			           			Dispatch(eventID, args);
			           		}
			           		finally
			           		{
			           			m_syncContext = false;
			           		}
			           	}, null);
		}

		/// <summary>
		/// Sends an event to the state machine, that might trigger a transition.
		/// This event will have precedence over other pending events that were sent using
		/// the <see cref="Send"/> method.
		/// </summary>
		/// <param name="eventID">The event.</param>
		/// <param name="args">Optional event arguments.</param>
		protected override void SendPriority(TEvent eventID, object[] args)
		{
			AssertMachineIsValid();
			m_queue.PostPriority(delegate { Dispatch(eventID, args); }, null);
		}

		/// <summary>
		/// Template method for handling dispatch exceptions.
		/// </summary>
		/// <param name="ex">The exception.</param>
		protected override void HandleDispatchException(Exception ex)
		{
			OnExceptionThrown(new TransitionErrorEventArgs<TState, TEvent>(m_currentEventContext, ex));
		}

		/// <summary>
		/// Raises the <see cref="StateMachine{TState,TEvent}.BeginDispatch"/> event.
		/// </summary>
		/// <param name="eventContext">The event context.</param>
		protected override void OnBeginDispatch(EventContext eventContext)
		{
			if (m_context == null || m_syncContext)
			{
				base.OnBeginDispatch(eventContext);
			}
			else
			{
				// overcome Compiler Warning (level 1) CS1911 
				Func<EventContext> baseMethod = base.OnBeginDispatch;

				// we call the synchronous Send method, so that user code could
				// perform just before dispatch begins, and not concurrently.
				m_context.Send(delegate { baseMethod(eventContext); }, null);
			}
		}

		/// <summary>
		/// Raises the <see cref="StateMachine{TState,TEvent}.BeginTransition"/> event.
		/// </summary>
		/// <param name="eventContext">The event context.</param>
		protected override void OnBeginTransition(EventContext eventContext)
		{
			if (m_context == null || m_syncContext)
			{
				base.OnBeginTransition(eventContext);
			}
			else
			{
				// overcome Compiler Warning (level 1) CS1911 
				Func<EventContext> baseMethod = base.OnBeginTransition;

				// we call the synchronous Send method, so that user code could
				// perform just before dispatch begins, and not concurrently.
				m_context.Send(delegate { baseMethod(eventContext); }, null);
			}
		}

		/// <summary>
		/// Raises the <see cref="StateMachine{TState,TEvent}.TransitionDeclined"/> event.
		/// </summary>
		/// <param name="eventContext">The event context.</param>
		protected override void OnTransitionDeclined(EventContext eventContext)
		{
			if (m_context != null)
			{
				// overcome Compiler Warning (level 1) CS1911 
				Func<EventContext> baseMethod = base.OnTransitionDeclined;
				m_context.Post(delegate { baseMethod(eventContext); }, null);
			}
			else
			{
				base.OnTransitionDeclined(eventContext);
			}
		}

		/// <summary>
		/// Raises the <see cref="StateMachine{TState,TEvent}.TransitionCompleted"/> event.
		/// </summary>
		/// <param name="args">The <see cref="TransitionCompletedEventArgs{TState,TEvent}"/> instance
		/// containing the event data.</param>
		protected override void OnTransitionCompleted(TransitionCompletedEventArgs<TState, TEvent> args)
		{
			if (m_context != null)
			{
				// overcome Compiler Warning (level 1) CS1911 
				Func<TransitionCompletedEventArgs<TState, TEvent>> baseMethod = 
					base.OnTransitionCompleted;
				m_context.Post(delegate { baseMethod(args); }, null);
			}
			else
			{
				base.OnTransitionCompleted(args);
			}
		}

		/// <summary>
		/// Raises the <see cref="StateMachine{TState,TEvent}.ExceptionThrown"/> event.
		/// </summary>
		/// <param name="args">The <see cref="TransitionErrorEventArgs{TState,TEvent}"/> instance containing the event data.</param>
		protected override void OnExceptionThrown(TransitionErrorEventArgs<TState, TEvent> args)
		{
			if (m_context != null)
			{
				// overcome Compiler Warning (level 1) CS1911 
				Func<TransitionErrorEventArgs<TState, TEvent>> baseMethod =
					base.OnExceptionThrown;
				m_context.Post(delegate { baseMethod(args); }, null);
			}
			else
			{
				base.OnExceptionThrown(args);
			}
		}

		/// <summary>
		/// Waits for pending events.
		/// </summary>
		public void WaitForPendingEvents()
		{
			SendOrPostCallback nop = delegate {  };
			m_queue.Send(nop, null);
		}

		#endregion

		private delegate void Func<T>(T arg);
	}
}