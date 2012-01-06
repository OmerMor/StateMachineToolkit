// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable VirtualMemberNeverOverriden.Global

using System;
using System.Collections.Generic;
using Sanford.Collections.Generic;

namespace Sanford.StateMachineToolkit
{
    /// <summary>
    /// Unlike the <see cref="ActiveStateMachine{TState,TEven,TArgst}"/> class, 
    /// the PassiveStateMachine class does not run in its own thread. Sometimes using an active 
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
    /// <typeparam name="TArgs">The event arguments type.</typeparam>
    public abstract class PassiveStateMachine<TState, TEvent, TArgs> 
        : StateMachine<TState, TEvent, TArgs>, IPassiveStateMachine<TState, TEvent, TArgs> 
        //where TArgs : EventArgs 
        //where TState : struct, IComparable, IFormattable /*, IConvertible*/
        //where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PassiveStateMachine{TState, TEvent,TArgs}"/> class.
        /// </summary>
        /// <param name="comparer"> </param>
        /// <param name="stateStorage">The state storage.</param>
        protected PassiveStateMachine(IEqualityComparer<TEvent> comparer = null, IStateStorage<TState> stateStorage = null)
            : base(comparer, stateStorage)
        {
        }

        private readonly Deque<EventContext> m_eventDeque = new Deque<EventContext>();

        /// <summary>
        /// Executes pending events.
        /// </summary>
        public void Execute()
        {
            while (m_eventDeque.Count > 0)
            {
                var eventContext = m_eventDeque.PopFront();

                Dispatch(eventContext.CurrentEvent, eventContext.Args);
            }
        }

        /// <summary>
        /// Sends an event to the state machine, that might trigger a transition.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="args">Optional event arguments.</param>
        public override void Send(TEvent eventId, TArgs args = default (TArgs))
        {
            AssertMachineIsValid();
            m_eventDeque.PushBack(new EventContext(default(TState), eventId, args));
        }

        /// <summary>
        /// Template method for handling dispatch exceptions.
        /// </summary>
        /// <param name="ex">The exception.</param>
        protected override void HandleDispatchException(Exception ex)
        {
            OnExceptionThrown(new TransitionErrorEventArgs<TState, TEvent, TArgs>(CurrentEventContext, ex));
            //throw new InvalidOperationException("Exception was thrown during dispatch.", ex);
        }

        /// <summary>
        /// Initializes the StateMachine's initial state.
        /// </summary>
        /// <param name="initialState">The state that will initially receive events from the StateMachine.</param>
        protected override void Initialize(TState initialState)
        {
            InitializeStateMachine(initialState);
        }

        /// <summary>
        /// Sends an event to the state machine, that might trigger a transition.
        /// This event will have precedence over other pending events that were sent using
        /// the <see cref="Send"/> method.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="args">Optional event arguments.</param>
        protected override void SendPriority(TEvent eventId, TArgs args)
        {
            AssertMachineIsValid();
            m_eventDeque.PushFront(new EventContext(default(TState), eventId, args));
        }
    }
}

// ReSharper enable MemberCanBePrivate.Global
// ReSharper enable VirtualMemberNeverOverriden.Global