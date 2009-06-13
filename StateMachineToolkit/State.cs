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

namespace Sanford.StateMachineToolkit
{
    public abstract partial class StateMachine<TState, TEvent, TArgs>
    {
        /// <summary>
        /// The event handlers of a state.
        /// </summary>
        public interface IStateEventHandlers
        {
            /// <summary>
            /// Occurs when entering the state.
            /// </summary>
            event EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> EntryHandler;

            /// <summary>
            /// Occurs when leaving the state.
            /// </summary>
            event EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> ExitHandler;
        }

        /// <summary>
        /// The State class represents a state a <see cref="StateMachine{TState,TEvent}"/> can be in during 
        /// its lifecycle. 
        /// A State can be a substate and/or superstate to other States.<para/>
        /// When a State receives an event, it checks to see if it has any Transitions for that event. 
        /// If it does, it iterates through all of the Transitions for that event until one of them fires. 
        /// If no Transitions were found, the State passes the event up to its superstate, if it has one; 
        /// the process is repeated at the superstate level. 
        /// This process can continue indefinitely until either a Transition fires or the top of the 
        /// state hierarchy is reached.<para/>
        /// After processing an event, the State returns the results to the 
        /// <see cref="Dispatch(TEvent,object[])"/> method where the State originally received the event. 
        /// The results indicate whether or not a Transition fired, and if so, the resulting 
        /// State of the <see cref="Transition"/>. 
        /// It also indicates whether or not an exception occurred during the Transition's action 
        /// (if one was performed). State machines use this information to update their 
        /// current State, if necessary.
        /// </summary>
        [System.Diagnostics.DebuggerDisplay("{m_stateId}")]
        private sealed class State : IStateEventHandlers
        {
            #region State Members

            #region Fields

            // The superstate.
            private State m_superstate;

            // The initial State.
            private State m_initialState;

            // The history State.
            private State m_historyState;

            // The collection of substates for the State.
            private readonly SubstateCollection m_substates;

            // The collection of Transitions for the State.
            private readonly TransitionCollection m_transitions;

            // The result if no transitions fired in response to an event.
            private static readonly TransitionResult s_notFiredResult =
                new TransitionResult(false, default(TState), null);

            // Entry action.

            // The State's history type.
            private HistoryType m_historyType = HistoryType.None;

            // The level of the State within the State hierarchy.
            private int m_level;

            // A unique integer value representing the State's ID.
            private readonly TState m_stateId;

            #endregion

            #region Events

            /// <summary>
            /// Occurs when entering the state.
            /// </summary>
            public event EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> EntryHandler;

            /// <summary>
            /// Occurs when leaving the state.
            /// </summary>
            public event EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> ExitHandler;

            #endregion

            #region Construction

            /// <summary>
            /// Initializes a new instance of the State class with the specified
            /// number of events it will handle.
            /// </summary>
            /// <param name="stateId">The State's ID.</param>
            /// Lookup method for getting the internal State object of the given state ID.
            /// </param>
            public State(TState stateId)
                : this(stateId, null, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the State class with the specified
            /// number of events it will handle as well as its entry and exit 
            /// actions.
            /// </summary>
            /// <param name="stateId">
            /// The State's ID.
            /// </param>
            /// <param name="entryHandler">
            /// The entry action.
            /// </param>
            /// <param name="exitHandler">
            /// The exit action.
            /// </param>
            public State(TState stateId, 
                EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> entryHandler, 
                EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> exitHandler)
            {
                EntryHandler += entryHandler ?? delegate { };
                ExitHandler += exitHandler ?? delegate { };

                m_stateId = stateId;

                m_substates = new SubstateCollection(this);
                m_transitions = new TransitionCollection(this);

                m_level = 1;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Dispatches an event to the StateMachine.
            /// </summary>
            /// <param name="eventId"></param>
            /// <param name="args">
            /// The arguments accompanying the event.
            /// </param>
            /// <returns>
            /// The results of the dispatch.
            /// </returns>
            internal TransitionResult Dispatch(EventContext context)
            {
                return dispatch(context);
            }

            // Recursively goes up the the state hierarchy until a state is found 
            // that will handle the event.

            /// <summary>
            /// Enters the state.
            /// </summary>
            internal void Entry(EventContext context)
            {
                // Execute entry action.
                try
                {
                    EntryHandler(s_currentStateMachine,
                                 new TransitionEventArgs<TState, TEvent, TArgs>(context));
                }
                catch (Exception ex)
                {
                    string message;
                    if (context == null) // state machine initialization phase only
                        message = string.Format("During the state machine initialization an exception was thrown inside the {0} state entry handler.",
                                            ID);
                    else
                        message =
                            string.Format("During the transition {0}.{1} an exception was thrown inside the {2} state entry handler.",
                                          context.SourceState, context.CurrentEvent, ID);

                    EntryException entryException = new EntryException(message, ex);
                    currentStateMachineOnExceptionThrown(entryException);
                }
            }

            /// <summary>
            /// Exits the state.
            /// </summary>
            internal void Exit(EventContext context)
            {
                try
                {
                    // Execute exit action.
                    ExitHandler(s_currentStateMachine,
                                new TransitionEventArgs<TState, TEvent, TArgs>(context));
                }
                catch (Exception ex)
                {
                    string message = string.Format("During the transition {0}.{1} an exception was thrown inside the {2} state exit handler.",
                                                  context.SourceState, context.CurrentEvent, ID);
                    ExitException exitException = new ExitException(message, ex);
                    currentStateMachineOnExceptionThrown(exitException);
                }

                // If there is a superstate.
                if (m_superstate != null)
                {
                    // Set the superstate's history state to this state. This lets
                    // the superstate remember which of its substates was last 
                    // active before exiting.
                    m_superstate.m_historyState = this;
                }
            }

            // Enters the state by its history (assumes that the Entry method has 
            // already been called).
            internal State EnterByHistory(EventContext context)
            {
                switch (HistoryType)
                {
                    case HistoryType.None:
                        // If there is no history type
                        return m_initialState == null ? this : m_initialState.enterShallow(context);
                    case HistoryType.Shallow:
                        return m_historyState == null ? this : m_historyState.enterShallow(context);
                    case HistoryType.Deep:
                        return m_historyState == null ? this : m_historyState.enterDeep(context);
                    default:
                        throw new InvalidOperationException("Invalid HistoryType");
                }
            }

            // Enters the state in via its history in shallow mode.
            private TransitionResult dispatch(EventContext context)
            {
                TransitionResult transResult = s_notFiredResult;

                // If there are any Transitions for this event.
                if (m_transitions[context.CurrentEvent] != null)
                {
                    // Iterate through the Transitions until one of them fires.
                    foreach (Transition trans in m_transitions[context.CurrentEvent])
                    {
                        transResult = trans.fire(context);
                        if (transResult.HasFired)
                        {
                            // Break out of loop. We're finished.
                            return transResult;
                        }
                    }
                }
                // Else if there are no Transitions for this event and there is a 
                // superstate.
                if (Superstate != null)
                {
                    // Dispatch the event to the superstate.
                    transResult = Superstate.dispatch(context);
                }

                return transResult;
            }

            private State enterShallow(EventContext context)
            {
                Entry(context);

                State result = this;

                // If the lowest level has not been reached.
                if (m_initialState != null)
                {
                    // Enter the next level initial state.
                    result = m_initialState.enterShallow(context);
                }

                return result;
            }

            // Enters the state via its history in deep mode.
            private State enterDeep(EventContext context)
            {
                Entry(context);

                State result = this;

                // If the lowest level has not been reached.
                if (m_historyState != null)
                {
                    // Enter the next level history state.
                    result = m_historyState.enterDeep(context);
                }

                return result;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets or sets the history type.
            /// </summary>
            public HistoryType HistoryType
            {
                get { return m_historyType; }
                set { m_historyType = value; }
            }

            /// <summary>
            /// Gets the State's ID.
            /// </summary>
            public TState ID
            {
                get { return m_stateId; }
            }

            /// <summary>
            /// Gets or sets the initial state.
            /// </summary>
            /// <remarks>
            /// If no initial state exists for this state, this property is null.
            /// </remarks>
            public State InitialState
            {
                get { return m_initialState; }
                set
                {
                    #region Preconditions

                    if (this == value)
                    {
                        throw new ArgumentException(
                            "State cannot be an initial state to itself.", "value");
                    }

                    if (value != null && value.Superstate != this)
                    {
                        throw new ArgumentException(
                            "State is not a direct substate.", "value");
                    }

                    #endregion

                    m_initialState = m_historyState = value;
                }
            }

            /// <summary>
            /// Gets the collection of substates.
            /// </summary>
            internal SubstateCollection Substates
            {
                get { return m_substates; }
            }

            /// <summary>
            /// Gets the collection of transitions.
            /// </summary>
            internal TransitionCollection Transitions
            {
                get { return m_transitions; }
            }

            /// <summary>
            /// Gets or sets the superstate.
            /// </summary>
            /// <remarks>
            /// If no superstate exists for this state, this property is null.
            /// </remarks>
            internal State Superstate
            {
                get { return m_superstate; }
                set
                {
                    #region Preconditions

                    if (this == value)
                    {
                        throw new ArgumentException(
                            "The superstate cannot be the same as this state.", "value");
                    }

                    #endregion

                    m_superstate = value;

                    if (m_superstate == null)
                    {
                        Level = 1;
                    }
                    else
                    {
                        Level = m_superstate.Level + 1;
                    }
                }
            }

            /// <summary>
            /// Gets the State's level in the State hierarchy.
            /// </summary>
            internal int Level
            {
                get { return m_level; }
                set
                {
                    m_level = value;

                    foreach (State substate in Substates)
                    {
                        substate.Level = m_level + 1;
                    }
                }
            }

            #endregion

            #endregion
        }
    }


    /// <summary>
    /// Represents the method that is evaluated to determine whether the state
    /// transition should fire.
    /// </summary>
    public delegate bool GuardHandler<TState, TEvent, TArgs>(object sender, TransitionEventArgs<TState, TEvent, TArgs> args);

    /// <summary>
    /// Specifies constants defining the type of history a state uses.
    /// </summary>
    /// <remarks>
    /// A state's history type determines which of its nested states it enters 
    /// into when it is the target of a transition. If a state does not have 
    /// any nested states, its history type has no effect.
    /// </remarks>
    public enum HistoryType
    {
        /// <summary>
        /// The state enters into its initial state which in turn enters into
        /// its initial state and so on until the innermost nested state is 
        /// reached.
        /// </summary>
        None,

        /// <summary>
        /// The state enters into its last active state which in turn enters 
        /// into its initial state and so on until the innermost nested state
        /// is reached.
        /// </summary>
        Shallow,

        /// <summary>
        /// The state enters into its last active state which in turns enters
        /// into its last active state and so on until the innermost nested
        /// state is reached.
        /// </summary>
        Deep
    }
}