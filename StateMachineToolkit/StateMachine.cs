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
using System.Collections.Generic;
using System.Diagnostics;

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// Represents the base class for all state machines.
	/// You do not derive your state machine classes from this class but rather from one 
	/// of its derived classes, either the <see cref="ActiveStateMachine{TState,TEvent}"/> 
	/// class or the <see cref="PassiveStateMachine{TState,TEvent}"/> class.
	/// </summary>
	public abstract partial class StateMachine<TState, TEvent>
		where TState : struct, IComparable, IFormattable /*, IConvertible*/
		where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
	{
		private readonly StateMap m_states = new StateMap();

		public State CreateState(TState stateID)
		{
			return new State(stateID);
		}
		public State CreateState(TState stateID, EntryHandler entryHandler, ExitHandler exitHandler)
		{
			return new State(stateID, entryHandler, exitHandler);
		}
		#region StateMachine Members

		#region Fields

		// The current state.
		protected State currentState;

		// The return value of the last action.
		private object actionResult;

		// Indicates whether the state machine has been initialized.
		private bool initialized;
		protected EventContext currentEventContext;

		[ThreadStatic]
		private static StateMachine<TState, TEvent> currentStateMachine;

		#endregion

		#region Events

		public event EventHandler<TransitionEventArgs<TState, TEvent>> BeginDispatch;
		public event EventHandler<TransitionCompletedEventArgs<TState, TEvent>> TransitionCompleted;
		public event EventHandler<TransitionEventArgs<TState, TEvent>> TransitionDeclined;
		public virtual event EventHandler<TransitionErrorEventArgs<TState, TEvent>> ExceptionThrown;

		#endregion

		#region Methods

		/// <summary>
		/// Initializes the StateMachine's initial state.
		/// </summary>
		/// <param name="initialState">
		/// The state that will initially receive events from the StateMachine.
		/// </param>
		protected abstract void Initialize(State initialState);

		/// <summary>
		/// Initializes the StateMachine's initial state.
		/// </summary>
		/// <param name="initialStateID">
		/// The state that will initially receive events from the StateMachine.
		/// </param>
		protected void Initialize(TState initialStateID)
		{
			Initialize(States[initialStateID]);
		}

		protected void InitializeStateMachine(State initialState)
		{
			#region Require

			if (initialState == null)
			{
				throw new ArgumentNullException("initialState");
			}
			if (initialized)
			{
				throw new InvalidOperationException("The state machine has already been initialized.");
			}

			#endregion

			initialized = true;
			currentStateMachine = this;

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
				superstate.Entry();
			}

			currentState = initialState.EnterByHistory();
			currentStateMachine = this;

		}

		public abstract void Send(TEvent eventID, params object[] args);

		protected abstract void SendPriority(TEvent eventID, params object[] args);

		protected virtual void OnBeginDispatch(EventContext eventContext)
		{
			raiseSafeEvent(BeginDispatch, new TransitionEventArgs<TState, TEvent>(eventContext), true);
		}

		protected virtual void OnTransitionCompleted(TransitionCompletedEventArgs<TState, TEvent> args)
		{
			raiseSafeEvent(TransitionCompleted, args, true);
		}
		protected virtual void OnTransitionDeclined(EventContext eventContext)
		{
			raiseSafeEvent(TransitionDeclined, new TransitionEventArgs<TState, TEvent>(eventContext), true);
		}

		protected virtual void OnExceptionThrown(TransitionErrorEventArgs<TState, TEvent> args)
		{
			raiseSafeEvent(ExceptionThrown, args, false);
		}

		internal static void OnExceptionThrown(Exception ex)
		{
			currentStateMachine.OnExceptionThrown(
				new TransitionErrorEventArgs<TState, TEvent>(
					currentStateMachine.currentEventContext, ex));
		}

		protected void raiseSafeEvent<TArgs>(EventHandler<TArgs> eventHandler, TArgs args, bool raiseEventOnException)
			where TArgs : EventArgs
		{
			try
			{
				if (eventHandler == null)
					return;
				eventHandler(this, args);
			}
			catch (Exception ex)
			{
				if (!raiseEventOnException) 
					return;
				OnExceptionThrown(
					new TransitionErrorEventArgs<TState, TEvent>(currentEventContext, ex));
			}
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
		protected virtual void Dispatch(TEvent eventID, object[] args)
		{
			// Reset action result.
			ActionResult = null;
			currentEventContext = new EventContext(CurrentStateID, eventID, args);
			currentStateMachine = this;
			try
			{
				OnBeginDispatch(currentEventContext);

				// Dispatch event to the current state.
				TransitionResult result = currentState.Dispatch(eventID, args);

/*
				// report errors
				if (result.Error != null)
					OnExceptionThrown(
						new TransitionErrorEventArgs(
							currentEventContext, result.Error));
*/

				// If a transition was fired as a result of this event.
				if (!result.HasFired)
				{
					OnTransitionDeclined(currentEventContext);
					return;
				}

				currentState = result.NewState;

				TransitionCompletedEventArgs<TState, TEvent> eventArgs =
					new TransitionCompletedEventArgs<TState, TEvent>(
						currentState.ID, currentEventContext, ActionResult, result.Error);

				OnTransitionCompleted(eventArgs);
			}
			catch (Exception ex)
			{
				handleDispatchException(ex);
			}
			finally
			{
				currentEventContext = null;
				currentStateMachine = null;
			}
		}

		protected abstract void handleDispatchException(Exception ex);

		protected virtual void assertMachineIsValid()
		{
			if (!IsInitialized)
			{
				throw new InvalidOperationException("State machine was not initialized yet.");
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the ID of the current State.
		/// </summary>
		public TState CurrentStateID
		{
			get
			{
				#region Require

				if (!initialized)
				{
					throw new InvalidOperationException();
				}

				#endregion

				Debug.Assert(currentState != null);

				return currentState.ID;
			}
		}

		/// <summary>
		/// Gets or sets the results of the action performed during the last transition.
		/// </summary>
		/// <remarks>
		/// This property should only be set during the execution of an action method.
		/// </remarks>
		protected object ActionResult
		{
			get { return actionResult; }
			set { actionResult = value; }
		}

		public abstract StateMachineType StateMachineType { get; }

		protected bool IsInitialized
		{
			get { return initialized && currentState != null; }
		}

		public StateMap States
		{
			get { return m_states; }
		}

		#endregion

		#endregion

		public class StateMap
		{
			private readonly Dictionary<TState, State> m_map = new Dictionary<TState, State>();

			public State this[TState state]
			{
				get
				{
					// lazy initialization
					if (!m_map.ContainsKey(state))
					{
						m_map.Add(state, new State(state));
					}
					return m_map[state];
				}
			}
		}

		public void AddTransition(TState source, TEvent eventID, TState target, params ActionHandler[] actions)
		{
			States[source].Transitions.Add(eventID, States[target], actions);
		}
		public void AddTransition(TState source, TEvent eventID, GuardHandler guard, TState target, params ActionHandler[] actions)
		{
			States[source].Transitions.Add(eventID, guard, States[target], actions);
		}

		public void SetupSubstates(TState superState, HistoryType historyType, TState initialSubstate, params TState[] additionalSubstates)
		{
			States[superState].Substates.Add(States[initialSubstate]);
			foreach (TState substate in additionalSubstates)
			{
				States[superState].Substates.Add(States[substate]);
			}
			States[superState].HistoryType = historyType;
			States[superState].InitialState = States[initialSubstate];
		}
		public class EventContext
		{
			private readonly TState sourceState;
			private readonly TEvent currentEvent;
			private readonly object[] args;

			public EventContext(TState sourceState, TEvent currentEvent, object[] args)
			{
				this.sourceState = sourceState;
				this.currentEvent = currentEvent;
				this.args = args;
			}

			public TState SourceState
			{
				[DebuggerStepThrough]
				get { return sourceState; }
			}

			public object[] Args
			{
				[DebuggerStepThrough]
				get { return args; }
			}

			public TEvent CurrentEvent
			{
				[DebuggerStepThrough]
				get { return currentEvent; }
			}
		}
	}

	public enum StateMachineType
	{
		Passive,
		Active
	}

}