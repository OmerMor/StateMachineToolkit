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
		// Used for queuing events.
		private readonly SynchronizationContext context;
		private readonly DelegateQueue queue = new DelegateQueue();

		private bool disposed;

		protected ActiveStateMachine()
		{
			context = SynchronizationContext.Current;
		}

		public override StateMachineType StateMachineType
		{
			get { return StateMachineType.Active; }
		}

		protected bool IsDisposed
		{
			get { return disposed; }
		}

		#region IDisposable Members

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

		~ActiveStateMachine()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing) return;
			queue.Dispose();

			GC.SuppressFinalize(this);

			disposed = true;
		}

		protected override void Initialize(State<TState, TEvent> initialState)
		{
			queue.Send(delegate { InitializeStateMachine(initialState); }, null);
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
			#region Require

			if (!IsInitialized)
			{
				throw new InvalidOperationException();
			}
			if (IsDisposed)
			{
				throw new ObjectDisposedException("ActiveStateMachine");
			}

			#endregion

			queue.Post(delegate { Dispatch(eventID, args); }, null);
		}

		public void SendSynchronously(TEvent eventID, params object[] args)
		{
			#region Require

			if (!IsInitialized)
			{
				throw new InvalidOperationException();
			}
			if (IsDisposed)
			{
				throw new ObjectDisposedException("ActiveStateMachine");
			}

			#endregion

			queue.Send(delegate { Dispatch(eventID, args); }, null);
		}

		protected override void SendPriority(TEvent eventID, object[] args)
		{
			#region Require

			if (!IsInitialized)
			{
				throw new InvalidOperationException();
			}
			if (IsDisposed)
			{
				throw new ObjectDisposedException("ActiveStateMachine");
			}

			#endregion

			queue.PostPriority(delegate { Dispatch(eventID, args); }, null);
		}

		/// <summary>
		/// Dispatches events to the current state.
		/// </summary>
		/// <param name="eventID">
		/// The event ID.
		/// </param>
		/// <param name="args">
		/// The data accompanying the event.
		/// </param>
		private void Dispatch(TEvent eventID, object[] args)
		{
			// Reset action result.
			ActionResult = null;

			// Dispatch event to the current state.
			TransitionResult<TState, TEvent> result = currentState.Dispatch(eventID, args);

			// If a transition was fired as a result of this event.
			if (!result.HasFired) return;
			currentState = result.NewState;

			TransitionCompletedEventArgs<TState, TEvent> e =
				new TransitionCompletedEventArgs<TState, TEvent>(currentState.ID, eventID, ActionResult, result.Error);

			if (context != null)
			{
				context.Post(delegate { OnTransitionCompleted(e); }, null);
			}
			else
			{
				OnTransitionCompleted(e);
			}
		}
	}
}