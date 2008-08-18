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
	public abstract partial class StateMachine<TState, TEvent>
	{
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
		/// It also indicates whether or not an exception occurred during the  Transition's action 
		/// (if one was performed). State machines use this information to update their 
		/// current State, if necessary.
		/// </summary>
		[System.Diagnostics.DebuggerDisplay("{stateID}")]
		public class State
		{
			#region State Members

			#region Fields

			// The superstate.
			private State superstate;

			// The initial State.
			private State initialState;

			// The history State.
			private State historyState;

			// The collection of substates for the State.
			private SubstateCollection substates;

			// The collection of Transitions for the State.
			private TransitionCollection transitions;

			// The result if no transitions fired in response to an event.
			private static readonly TransitionResult notFiredResult =
				new TransitionResult(false, null, null);

			// Entry action.
			public event EntryHandler EntryHandler;

			// Exit action.
			public event ExitHandler ExitHandler;

			// The State's history type.
			private HistoryType historyType = HistoryType.None;

			// The level of the State within the State hierarchy.
			private int level;

			// A unique integer value representing the State's ID.
			private TState stateID;

			#endregion

			#region Construction

			/// <summary>
			/// Initializes a new instance of the State class with the specified
			/// number of events it will handle.
			/// </summary>
			/// <param name="stateID">
			/// The State's ID.
			/// </param>
			public State(TState stateID)
			{
				InitializeState(stateID);
			}

			/// <summary>
			/// Initializes a new instance of the State class with the specified
			/// number of events it will handle as well as its entry action.
			/// </summary>
			/// <param name="stateID">
			/// The State's ID.
			/// </param>
			/// <param name="entryHandler">
			/// The entry action.
			/// </param>
			public State(TState stateID, EntryHandler entryHandler)
			{
				EntryHandler = entryHandler;

				InitializeState(stateID);
			}

			/// <summary>
			/// Initializes a new instance of the State class with the specified
			/// number of events it will handle as well as its exit action.
			/// </summary>
			/// <param name="stateID">
			/// The State's ID.
			/// </param>
			/// <param name="exitHandler">
			/// The exit action.
			/// </param>
			public State(TState stateID, ExitHandler exitHandler)
			{
				ExitHandler = exitHandler;

				InitializeState(stateID);
			}

			/// <summary>
			/// Initializes a new instance of the State class with the specified
			/// number of events it will handle as well as its entry and exit 
			/// actions.
			/// </summary>
			/// <param name="stateID">
			/// The State's ID.
			/// </param>
			/// <param name="entryHandler">
			/// The entry action.
			/// </param>
			/// <param name="exitHandler">
			/// The exit action.
			/// </param>
			public State(TState stateID, EntryHandler entryHandler, ExitHandler exitHandler)
			{
				EntryHandler = entryHandler;
				ExitHandler = exitHandler;

				InitializeState(stateID);
			}

			#endregion

			#region Methods

			// Initializes the State.
			private void InitializeState(TState id)
			{
				stateID = id;

				substates = new SubstateCollection(this);
				transitions = new TransitionCollection(this);

				level = 1;
			}

			/// <summary>
			/// Dispatches an event to the StateMachine.
			/// </summary>
			/// <param name="eventID"></param>
			/// <param name="args">
			/// The arguments accompanying the event.
			/// </param>
			/// <returns>
			/// The results of the dispatch.
			/// </returns>
			internal TransitionResult Dispatch(TEvent eventID, object[] args)
			{
				return Dispatch(this, eventID, args);
			}

			// Recursively goes up the the state hierarchy until a state is found 
			// that will handle the event.
			private TransitionResult Dispatch(State origin, TEvent eventID, object[] args)
			{
				TransitionResult transResult = notFiredResult;

				// If there are any Transitions for this event.
				if (transitions[eventID] != null)
				{
					// Iterate through the Transitions until one of them fires.
					foreach (Transition trans in transitions[eventID])
					{
						transResult = trans.Fire(origin, args);
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
					transResult = Superstate.Dispatch(origin, eventID, args);
				}

				return transResult;
			}

			/// <summary>
			/// Enters the state.
			/// </summary>
			internal void Entry()
			{
				// If an entry action exists for this state.
				if (EntryHandler == null) return;
				// Execute entry action.
				try
				{
					EntryHandler();
				}
				catch (Exception ex)
				{
					EventContext context = currentStateMachine.currentEventContext;
					string message;
					if (context == null) // state machine initialization phase only
						message = string.Format("During the state machine initialization an exception was thrown inside the {0} state entry handler.",
											ID);
					else
					message = string.Format("During the transition {0}.{1} an exception was thrown inside the {2} state entry handler.",
											context.SourceState, context.CurrentEvent, ID);
					;
					EntryException entryException = new EntryException(message, ex);
					OnExceptionThrown(entryException);
				}
			}

			/// <summary>
			/// Exits the state.
			/// </summary>
			internal void Exit()
			{
				// If an exit action exists for this state.
				if (ExitHandler != null)
				{
					try
					{
						// Execute exit action.
						ExitHandler();
					}
					catch (Exception ex)
					{
						EventContext context = currentStateMachine.currentEventContext;
						string message = string.Format("During the transition {0}.{1} an exception was thrown inside the {2} state exit handler.",
													  context.SourceState, context.CurrentEvent, ID);
						ExitException exitException = new ExitException(message, ex);
						OnExceptionThrown(exitException);
					}
				}


				// If there is a superstate.
				if (superstate != null)
				{
					// Set the superstate's history state to this state. This lets
					// the superstate remember which of its substates was last 
					// active before exiting.
					superstate.historyState = this;
				}
			}

			// Enters the state by its history (assumes that the Entry method has 
			// already been called).
			internal State EnterByHistory()
			{
				State result = this;

				// If there is no history type.
				switch (HistoryType)
				{
					case HistoryType.None:
						if (initialState != null)
						{
							// Enter the initial state.
							result = initialState.EnterShallow();
						}
						break;
					case HistoryType.Shallow:
						if (historyState != null)
						{
							// Enter history state in shallow mode.
							result = historyState.EnterShallow();
						}
						break;
					case HistoryType.Deep:
						if (historyState != null)
						{
							// Enter history state in deep mode.
							result = historyState.EnterDeep();
						}
						break;
				}

				return result;
			}

			// Enters the state in via its history in shallow mode.
			private State EnterShallow()
			{
				Entry();

				State result = this;

				// If the lowest level has not been reached.
				if (initialState != null)
				{
					// Enter the next level initial state.
					result = initialState.EnterShallow();
				}

				return result;
			}

			// Enters the state via its history in deep mode.
			private State EnterDeep()
			{
				Entry();

				State result = this;

				// If the lowest level has not been reached.
				if (historyState != null)
				{
					// Enter the next level history state.
					result = historyState.EnterDeep();
				}

				return result;
			}

			#endregion

			#region Properties

			/// <summary>
			/// Gets the State's ID.
			/// </summary>
			public TState ID
			{
				get { return stateID; }
			}

			/// <summary>
			/// Gets the collection of substates.
			/// </summary>
			public SubstateCollection Substates
			{
				get { return substates; }
			}

			/// <summary>
			/// Gets the collection of transitions.
			/// </summary>
			public TransitionCollection Transitions
			{
				get { return transitions; }
			}

			/// <summary>
			/// Gets or sets the superstate.
			/// </summary>
			/// <remarks>
			/// If no superstate exists for this state, this property is null.
			/// </remarks>
			internal State Superstate
			{
				get { return superstate; }
				set
				{
					#region Preconditions

					if (this == value)
					{
						throw new ArgumentException(
							"The superstate cannot be the same as this state.");
					}

					#endregion

					superstate = value;

					if (superstate == null)
					{
						Level = 1;
					}
					else
					{
						Level = superstate.Level + 1;
					}
				}
			}

			/// <summary>
			/// Gets or sets the initial state.
			/// </summary>
			/// <remarks>
			/// If no initial state exists for this state, this property is null.
			/// </remarks>
			public State InitialState
			{
				get { return initialState; }
				set
				{
					#region Preconditions

					if (this == value)
					{
						throw new ArgumentException(
							"State cannot be an initial state to itself.");
					}

					if (value.Superstate != this)
					{
						throw new ArgumentException(
							"State is not a direct substate.");
					}

					#endregion

					initialState = historyState = value;
				}
			}

			/// <summary>
			/// Gets or sets the history type.
			/// </summary>
			public HistoryType HistoryType
			{
				get { return historyType; }
				set { historyType = value; }
			}

			/// <summary>
			/// Gets the State's level in the State hierarchy.
			/// </summary>
			internal int Level
			{
				get { return level; }
				set
				{
					level = value;

					foreach (State substate in Substates)
					{
						substate.Level = level + 1;
					}
				}
			}

			#endregion

			#endregion
		}
	}
	/// <summary>
	/// Represents the method that will perform an action during a state 
	/// transition.
	/// </summary>
	public delegate void ActionHandler(object[] args);

	/// <summary>
	/// Represents the method that is evaluated to determine whether the state
	/// transition should fire.
	/// </summary>
	public delegate bool GuardHandler(object[] args);

	/// <summary>
	/// Represents the method that is called when a state is entered.
	/// </summary>
	public delegate void EntryHandler();

	/// <summary>
	/// Represents the method that is called when a state is exited.
	/// </summary>
	public delegate void ExitHandler();

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