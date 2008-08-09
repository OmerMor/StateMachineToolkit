using System;
using System.Collections.Generic;
using System.Threading;
using Sanford.Threading;

namespace Sanford.StateMachineToolkit
{
	public abstract class ActiveStateMachine<TState, TEvent> : StateMachine<TState, TEvent>, IDisposable
		where TState : struct, IComparable, IFormattable/*, IConvertible*/
		where TEvent : struct, IComparable, IFormattable/*, IConvertible*/
	{
		// Used for queuing events.
		private readonly SynchronizationContext context;
		private readonly DelegateQueue queue = new DelegateQueue();

		private Queue<DeferedEvent> deferedEvents = new Queue<DeferedEvent>();

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
/*
			if (eventID < 0 || eventID >= currentState.Transitions.Count)
			{
				throw new ArgumentOutOfRangeException("eventID", eventID,
				                                      "Event ID out of range.");
			}
*/

			#endregion

			queue.Post(delegate { Dispatch(eventID, args); }, null);
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
			
/*
			if (eventID < 0 || eventID >= currentState.Transitions.Count)
			{
				throw new ArgumentOutOfRangeException("eventID", eventID,
				                                      "Event ID out of range.");
			}
*/

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

		#region Nested type: DeferedEvent

		private class DeferedEvent
		{
			private readonly object[] args;
			private readonly TEvent eventID;

			public DeferedEvent(TEvent eventID, object[] args)
			{
				this.eventID = eventID;
				this.args = args;
			}

			public TEvent EventID
			{
				get { return eventID; }
			}

			public object[] GetArgs()
			{
				return args;
			}
		}

		#endregion
	}
}