using System;
using Sanford.Collections;

namespace Sanford.StateMachineToolkit
{
	public class PassiveStateMachine<TState, TEvent> : StateMachine<TState, TEvent>
		where TState : struct, IComparable, IFormattable/*, IConvertible*/
		where TEvent : struct, IComparable, IFormattable/*, IConvertible*/
	{
        private readonly Deque eventDeque = new Deque();

        public void Execute()
        {
            StateMachineEvent e;

            while(eventDeque.Count > 0)
            {
                e = (StateMachineEvent)eventDeque.PopFront();

                Dispatch(e.EventID, e.GetArgs());
            }
        }

        public override void Send(TEvent eventID, object[] args)
        {
            #region Require

            if(!IsInitialized)
            {
                throw new InvalidOperationException("State machine has not been initialized.");
            }

            #endregion

            eventDeque.PushBack(new StateMachineEvent(eventID, args));
        }

		protected override void SendPriority(TEvent eventID, object[] args)
        {
            #region Require

            if(!IsInitialized)
            {
                throw new InvalidOperationException("State machine has not been initialized.");
            }

            #endregion
            
            eventDeque.PushFront(new StateMachineEvent(eventID, args));
        }

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

        	OnTransitionCompleted(e);
        }

        public override StateMachineType StateMachineType
        {
            get
            {
                return StateMachineType.Passive;
            }
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
                get
                {
                    return eventID;
                }
            }
        }
    }
}
