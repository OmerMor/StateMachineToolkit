using System;
using Sanford.Collections;

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// Unlike the <see cref="ActiveStateMachine{TState,TEvent}"/> class, 
	/// the PassiveStateMachine class does not run in its  own thread. Sometimes using an active 
	/// object is overkill. In those cases, it is  appropriate to derive your state machine from 
	/// the PassiveStateMachine class.<para/>
	/// Because the PassiveStateMachine is, well, passive, it has to be prodded to 
	/// fire its transitions. You do this by calling its <see cref="Execute"/> method. After sending a 
	/// PassiveStateMachine derived class one or more events, you then call <see cref="Execute"/>. 
	/// The state machine responds by dequeueing all of the events in its event queue, 
	/// dispatching them one right after the other. 
	/// </summary>
	/// <typeparam name="TState">The state enumeration type.</typeparam>
	/// <typeparam name="TEvent">The event enumeration type.</typeparam>
	public class PassiveStateMachine<TState, TEvent> : StateMachine<TState, TEvent>
		where TState : struct, IComparable, IFormattable /*, IConvertible*/
		where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
	{
		private readonly Deque eventDeque = new Deque();

		protected override void Initialize(State<TState, TEvent> initialState)
		{
			InitializeStateMachine(initialState);
		}

		public void Execute()
		{
			StateMachineEvent e;

			while (eventDeque.Count > 0)
			{
				e = (StateMachineEvent) eventDeque.PopFront();

				Dispatch(e.EventID, e.GetArgs());
			}
		}

		public override void Send(TEvent eventID, object[] args)
		{
			#region Require

			if (!IsInitialized)
			{
				throw new InvalidOperationException("State machine has not been initialized.");
			}

			#endregion

			eventDeque.PushBack(new StateMachineEvent(eventID, args));
		}

		protected override void SendPriority(TEvent eventID, object[] args)
		{
			#region Require

			if (!IsInitialized)
			{
				throw new InvalidOperationException("State machine has not been initialized.");
			}

			#endregion

			eventDeque.PushFront(new StateMachineEvent(eventID, args));
		}

		protected override void Dispatch(TEvent eventID, object[] args)
		{
			// Reset action result.
			ActionResult = null;
			currentEventContext = new EventContext<TState, TEvent>(CurrentStateID, eventID, args);
			try
			{
				OnBeginDispatch(currentEventContext);

				// Dispatch event to the current state.
				TransitionResult<TState, TEvent> result = currentState.Dispatch(eventID, args);

				// report errors
				if (result.Error != null)
					OnExceptionThrown(
						new TransitionErrorEventArgs<TState, TEvent>(
							currentEventContext, result.Error));

				// If a transition was fired as a result of this event.
				if (!result.HasFired)
				{
					OnTransitionDeclined(currentEventContext);
					return;
				}

				currentState = result.NewState;

				TransitionCompletedEventArgs<TState, TEvent> e =
					new TransitionCompletedEventArgs<TState, TEvent>(
						currentState.ID, currentEventContext, ActionResult, result.Error);

				OnTransitionCompleted(e);
			}
			catch (Exception ex)
			{
				OnExceptionThrown(new TransitionErrorEventArgs<TState, TEvent>(currentEventContext, ex));
				throw new InvalidOperationException("Exception was thrown during dispatch.", ex);
			}
			finally
			{
				currentEventContext = null;
			}
		}

		public override StateMachineType StateMachineType
		{
			get { return StateMachineType.Passive; }
		}

		private class StateMachineEvent
		{
			private readonly TEvent eventID;

			private readonly object[] args;

			public StateMachineEvent(TEvent eventID, object[] args)
			{
				this.eventID = eventID;
				this.args = args;
			}

			public object[] GetArgs()
			{
				return args;
			}

			public TEvent EventID
			{
				get { return eventID; }
			}
		}
	}
}