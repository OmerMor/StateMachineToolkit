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

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable VirtualMemberNeverOverriden.Global

namespace Sanford.StateMachineToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Represents the base class for all state machines. You do not derive your state machine classes from this 
    /// class but rather from one of its derived classes, either the <see cref="ActiveStateMachine{TState,TEvent,TArgs}"/> 
    /// class or the <see cref="PassiveStateMachine{TState,TEvent,TArgs}"/> class.
    /// </summary>
    /// <typeparam name="TState">The state enumeration type.</typeparam>
    /// <typeparam name="TEvent">The event enumeration type.</typeparam>
    /// <typeparam name="TArgs">The event arguments type.</typeparam>
    public abstract partial class StateMachine<TState, TEvent, TArgs> 
        : IStateMachine<TState, TEvent, TArgs> 
        //where TArgs : EventArgs 
        //where TState : struct, IComparable, IFormattable /*, IConvertible*/
        //where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
    {
        #region StateMachine Members

        #region .ctor

        public StateMachine(IStateStorage<TState> stateStorage)
        {
            m_stateStorage = stateStorage;
        }

        public StateMachine() : this(new InternalStateStorage<TState>())
        {
        }

        #endregion

        #region Fields

        /// <summary>
        /// The return value of the last action.
        /// </summary>
        [ThreadStatic]
        private static StateMachine<TState, TEvent, TArgs> s_currentStateMachine;

        /// <summary>
        /// The states of the state machine.
        /// </summary>
        private readonly StateMap m_states = new StateMap();

        /// <summary>
        /// The results of the action performed during the last transition.
        /// </summary>
        private object m_actionResult;

        /// <summary>
        /// The current event context.
        /// </summary>
        private EventContext m_currentEventContext;

/*
        /// <summary>
        /// The current state.
        /// </summary>
        private TState m_currentStateId;
*/

        private readonly IStateStorage<TState> m_stateStorage;

        /// <summary>
        /// Indicates whether the state machine has been initialized.
        /// </summary>
        private bool m_initialized;

        #endregion

        #region Events

        /// <summary>
        /// Occurs before a dispatch starts.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> BeginDispatch;

        /// <summary>
        /// Occurs before a transition starts.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> BeginTransition;

        /// <summary>
        /// Occurs when an exception is thrown.
        /// </summary>
        public virtual event EventHandler<TransitionErrorEventArgs<TState, TEvent, TArgs>> ExceptionThrown;

        /// <summary>
        /// Occurs after a transition is completed.
        /// </summary>
        public event EventHandler<TransitionCompletedEventArgs<TState, TEvent, TArgs>> TransitionCompleted;

        /// <summary>
        /// Occurs when a transition is declined.
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> TransitionDeclined;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="IStateEventHandlers"/> for the specified state.
        /// </summary>
        /// <value>The state.</value>
        public IStateEventHandlers this[TState value]
        {
            get
            {
                return states[value];
            }
        }

        /// <summary>
        /// Gets the ID of the current state.
        /// </summary>
        public virtual TState CurrentStateID
        {
            get
            {
                AssertMachineIsValid();
                return StateStorage.Value;
            }
            protected set { StateStorage.Value = value; }
        }

        /// <summary>
        /// Gets or sets the results of the action performed during the last transition.
        /// </summary>
        /// <remarks>
        /// This property should only be set during the execution of an action method.
        /// </remarks>
        protected object ActionResult
        {
            get { return m_actionResult; }
            set { m_actionResult = value; }
        }

        /// <summary>
        /// Gets or sets the current event context.
        /// </summary>
        protected EventContext CurrentEventContext
        {
            get { return m_currentEventContext; }
            set { m_currentEventContext = value; }
        }

        /// <summary>
        /// Gets or sets the current state.
        /// </summary>
        private State CurrentState
        {
            get { return states[CurrentStateID]; }
        }

        /// <summary>
        /// Gets a value indicating whether this state machine is initialized.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this state machine is initialized; otherwise, <c>false</c>.
        /// </value>
        protected bool IsInitialized
        {
            get { return m_initialized; }
        }

        protected IStateStorage<TState> StateStorage
        {
            get { return m_stateStorage; }
        }

        /// <summary>
        /// Gets the states of the state machine.
        /// </summary>
        private StateMap states
        {
            get { return m_states; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new transition to the state machine.  The source and target states will be 
        /// implicitly added to the state machine if necesseray. 
        /// </summary>
        /// <param name="source">The source state.</param>
        /// <param name="eventId">The event that will trigger the transition.</param>
        /// <param name="target">The target state.</param>
        /// <param name="actions">Optional actions that will be performed during the transition.</param>
        public void AddTransition(TState source, TEvent eventId, TState target, 
            params EventHandler<TransitionEventArgs<TState,TEvent,TArgs>>[] actions)
        {
            states[source].Transitions.Add(eventId, states[target], actions);
        }

        /// <summary>
        /// Adds a new transition to the state machine.  The source and target states will be 
        /// implicitly added to the state machine if necessary. 
        /// </summary>
        /// <param name="source">The source state.</param>
        /// <param name="eventId">The event that will trigger the transition.</param>
        /// <param name="guard">A transition guard.</param>
        /// <param name="target">The target state.</param>
        /// <param name="actions">Optional actions that will be performed during the transition.</param>
        public void AddTransition(TState source, TEvent eventId, 
            GuardHandler<TState, TEvent, TArgs> guard, TState target, 
            params EventHandler<TransitionEventArgs<TState, TEvent, TArgs>>[] actions)
        {
            states[source].Transitions.Add(eventId, guard, states[target], actions);
        }

        /// <summary>
        /// Sends an event to the state machine, that might trigger a transition.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="args">Optional event arguments.</param>
        public abstract void Send(TEvent eventId, TArgs args);

        public void Send(TEvent eventId)
        {
            Send(eventId, default(TArgs));
        }

        /// <summary>
        /// Setups substates for hierarchical state machine.
        /// </summary>
        /// <param name="superState">The super state.</param>
        /// <param name="historyType">The history type.</param>
        /// <param name="initialSubstate">The initial substate.</param>
        /// <param name="additionalSubstates">Additional substates.</param>
        public void SetupSubstates(TState superState, HistoryType historyType, TState initialSubstate, params TState[] additionalSubstates)
        {
            State superstate = states[superState];
            State initial = states[initialSubstate];

            superstate.Substates.Clear();
            superstate.Substates.Add(initial);
            foreach (TState substate in additionalSubstates)
            {
                superstate.Substates.Add(states[substate]);
            }

            superstate.HistoryType = historyType;
            superstate.InitialState = initial;
        }

        /// <summary>
        /// Asserts that the state machine was initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the state machine was not initialized.</exception>
        protected virtual void AssertMachineIsValid()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("State machine was not initialized yet.");
            }
        }

        /// <summary>
        /// Dispatches events to the current state.
        /// </summary>
        /// <param name="eventId">
        /// The event ID.
        /// </param>
        /// <param name="args">
        /// The data accompanying the event.
        /// </param>
        protected virtual void Dispatch(TEvent eventId, TArgs args)
        {
            // Reset action result.
            ActionResult = null;
            State currentState = CurrentState;
            EventContext eventContext = new EventContext(currentState.ID, eventId, args);
            CurrentEventContext = eventContext;
            s_currentStateMachine = this;
            try
            {
                OnBeginDispatch(eventContext);

                // Dispatch event to the current state.
                TransitionResult result = currentState.Dispatch(eventContext);

/*
                // report errors
                if (result.Error != null)
                    OnExceptionThrown(
                        new TransitionErrorEventArgs(
                            m_currentEventContext, result.Error));
*/

                // If a transition was fired as a result of this event.
                if (!result.HasFired)
                {
                    OnTransitionDeclined(eventContext);
                    return;
                }

                CurrentStateID = result.NewState;

                TransitionCompletedEventArgs<TState, TEvent, TArgs> eventArgs =
                    new TransitionCompletedEventArgs<TState, TEvent, TArgs>(
                        result.NewState, eventContext, ActionResult, result.Error);

                OnTransitionCompleted(eventArgs);
            }
            catch (Exception ex)
            {
                HandleDispatchException(ex);
            }
            finally
            {
                CurrentEventContext = null;
                s_currentStateMachine = null;
            }
        }

        /// <summary>
        /// Template method for handling dispatch exceptions.
        /// </summary>
        /// <param name="ex">The exception.</param>
        protected abstract void HandleDispatchException(Exception ex);

/*
        /// <summary>
        /// Initializes the StateMachine's initial state.
        /// </summary>
        /// <param name="initialState">
        /// The state that will initially receive events from the StateMachine.
        /// </param>
        protected abstract void Initialize(State initialState);

*/
        /// <summary>
        /// Initializes the StateMachine's initial state.
        /// </summary>
        /// <param name="initialStateId">
        /// The state that will initially receive events from the StateMachine.
        /// </param>
        protected abstract void Initialize(TState initialStateId);

        /// <summary>
        /// Initializes the StateMachine's initial state.
        /// </summary>
        protected void Initialize()
        {
            Initialize(StateStorage.Value);
        }

        /// <summary>
        /// Initializes the StateMachine's initial state.
        /// </summary>
        /// <param name="initialStateId">
        /// The state that will initially receive events from the StateMachine.
        /// </param>
        protected void InitializeStateMachine(TState initialStateId)
        {
            State initialState = states[initialStateId];
            if (initialState == null)
            {
                throw new ArgumentException("Machine was not setup with given initial state.", "initialStateId");
            }

            if (m_initialized)
            {
                throw new InvalidOperationException("The state machine has already been initialized.");
            }

            m_initialized = true;
            s_currentStateMachine = this;
            State superstate = initialState;
            Stack<State> superstateStack = new Stack<State>();

            // If the initial state is a substate, travel up the state 
            // hierarchy in order to descend from the top state to the initial
            // state.
            while (superstate != null)
            {
                superstateStack.Push(superstate);
                superstate = superstate.Superstate;
            }

            // While there are superstates to traverse.
            while (superstateStack.Count > 0)
            {
                superstate = superstateStack.Pop();
                superstate.Entry(null);
            }

            CurrentStateID = initialState.EnterByHistory(null).ID;
            s_currentStateMachine = this;

        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent}.BeginDispatch"/> event.
        /// </summary>
        /// <param name="eventContext">The event context.</param>
        protected virtual void OnBeginDispatch(EventContext eventContext)
        {
            RaiseSafeEvent(BeginDispatch, new TransitionEventArgs<TState, TEvent, TArgs>(eventContext), ExceptionPolicy.RaiseExceptionEvent);
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent}.BeginTransition"/> event.
        /// </summary>
        /// <param name="eventContext">The event context.</param>
        protected virtual void OnBeginTransition(EventContext eventContext)
        {
            RaiseSafeEvent(BeginTransition, new TransitionEventArgs<TState, TEvent, TArgs>(eventContext), ExceptionPolicy.RaiseExceptionEvent);
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent}.ExceptionThrown"/> event.
        /// </summary>
        /// <param name="args">The <see cref="TransitionCompletedEventArgs{TState,TEvent}"/> instance containing the event data.</param>
        protected virtual void OnExceptionThrown(TransitionErrorEventArgs<TState, TEvent, TArgs> args)
        {
            RaiseSafeEvent(ExceptionThrown, args, ExceptionPolicy.Swallow);
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent,TArgs}.TransitionCompleted"/> event.
        /// </summary>
        /// <param name="args">The <see cref="TransitionCompletedEventArgs{TState,TEvent,TArgs}"/> instance 
        /// containing the event data.</param>
        protected virtual void OnTransitionCompleted(TransitionCompletedEventArgs<TState, TEvent, TArgs> args)
        {
            RaiseSafeEvent(TransitionCompleted, args, ExceptionPolicy.RaiseExceptionEvent);
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent,TArgs}.TransitionDeclined"/> event.
        /// </summary>
        /// <param name="eventContext">The event context.</param>
        protected virtual void OnTransitionDeclined(EventContext eventContext)
        {
            RaiseSafeEvent(TransitionDeclined, new TransitionEventArgs<TState, TEvent, TArgs>(eventContext), ExceptionPolicy.RaiseExceptionEvent);
        }

        /// <summary>
        /// Raises an event, without throwing out exceptions. 
        /// Optionally, exceptions could trigger the <see cref="ExceptionThrown"/> event.
        /// </summary>
        /// <typeparam name="TArgs">The type of the event arguments.</typeparam>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="args">The event arguments.</param>
        /// <param name="exceptionPolicy">if set to <see cref="ExceptionPolicy.RaiseExceptionEvent"/> 
        /// exceptions will trigger the <see cref="ExceptionThrown"/> event.</param>
        protected void RaiseSafeEvent<TEventArgs>(EventHandler<TEventArgs> eventHandler, TEventArgs args, ExceptionPolicy exceptionPolicy)
            where TEventArgs : EventArgs
        {
            try
            {
                if (eventHandler == null)
                {
                    return;
                }

                eventHandler(this, args);
            }
            catch (Exception ex)
            {
                if (exceptionPolicy == ExceptionPolicy.RaiseExceptionEvent)
                {
                    OnExceptionThrown(
                        new TransitionErrorEventArgs<TState, TEvent, TArgs>(CurrentEventContext, ex));
                }
            }
        }

        /// <summary>
        /// Sends an event to the state machine, that might trigger a transition.
        /// This event will have precedence over other pending events that were sent using
        /// the <see cref="Send"/> method.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="args">event arguments.</param>
        protected abstract void SendPriority(TEvent eventId, TArgs args);

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent,TArgs}.BeginTransition"/> event 
        /// on the currently running state machine.
        /// </summary>
        private static void currentStateMachineOnBeginTransition()
        {
            s_currentStateMachine.OnBeginTransition(s_currentStateMachine.CurrentEventContext);
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent,TArgs}.ExceptionThrown"/> event
        /// on the currently running state machine.
        /// </summary>
        /// <param name="ex">The exception that was thrown.</param>
        private static void currentStateMachineOnExceptionThrown(Exception ex)
        {
            s_currentStateMachine.OnExceptionThrown(
                new TransitionErrorEventArgs<TState, TEvent, TArgs>(
                    s_currentStateMachine.CurrentEventContext, ex));
        }

        #endregion

        #endregion

        /// <summary>
        /// A readonly mapping from <typeparamref name="TState"/> ID to 
        /// <see cref="Sanford.StateMachineToolkit.StateMachine{TState,TEvent,TArgs}.State"/> object.
        /// </summary>
        private sealed class StateMap
        {
            /// <summary>
            /// Maps state IDs to state objects.
            /// </summary>
            private readonly Dictionary<TState, State> m_map = new Dictionary<TState, State>();

            /// <summary>
            /// Gets the <see cref="Sanford.StateMachineToolkit.StateMachine{TState,TEvent,TArgs}.State"/>
            /// object with the specified <typeparamref name="TState"/> ID.
            /// </summary>
            /// <param name="state">The state ID.</param>
            /// <value>The <typeparamref name="TState"/> ID.</value>
            public State this[TState state]
            {
                get
                {
                    return lookupState(state);
                }
            }

            private State lookupState(TState state)
            {
                // lazy initialization
                if (!m_map.ContainsKey(state))
                {
                    m_map.Add(state, new State(state));
                }

                return m_map[state];
            }
        }

        /// <summary>
        /// A context with information about an event is being processed by the state machine.
        /// </summary>
        [DebuggerDisplay("{m_sourceState},{m_currentEvent},{m_args}")]
        public class EventContext : EventArgs
        {
            /// <summary>
            /// The source state ID.
            /// </summary>
            private readonly TState m_sourceState;

            /// <summary>
            /// The current event ID.
            /// </summary>
            private readonly TEvent m_currentEvent;

            /// <summary>
            /// The event arguments.
            /// </summary>
            private readonly TArgs m_args;

            /// <summary>
            /// Initializes a new instance of the <see cref="EventContext"/> class.
            /// </summary>
            /// <param name="sourceState">The source state.</param>
            /// <param name="currentEvent">The current event.</param>
            /// <param name="args">The event arguments.</param>
            public EventContext(TState sourceState, TEvent currentEvent, TArgs args)
            {
                m_sourceState = sourceState;
                m_currentEvent = currentEvent;
                m_args = args;
            }

            /// <summary>
            /// Gets the source state.
            /// </summary>
            /// <value>The state of the source.</value>
            public TState SourceState
            {
                [DebuggerStepThrough]
                get { return m_sourceState; }
            }

            /// <summary>
            /// Gets the event arguments.
            /// </summary>
            /// <value>The event arguments.</value>
            public TArgs Args
            {
                [DebuggerStepThrough]
                get { return m_args; }
            }

            /// <summary>
            /// Gets the current event.
            /// </summary>
            /// <value>The current event.</value>
            public TEvent CurrentEvent
            {
                [DebuggerStepThrough]
                get { return m_currentEvent; }
            }
        }
    }

    /// <summary>
    /// Policy when exception is caught
    /// </summary>
    public enum ExceptionPolicy
    {
        /// <summary>
        /// Ignore the exception
        /// </summary>
        Swallow,
        /// <summary>
        /// Raise an event
        /// </summary>
        RaiseExceptionEvent
    }
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore VirtualMemberNeverOverriden.Global
