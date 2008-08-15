using System;

namespace Sanford.StateMachineToolkit
{
	public abstract class ActiveStateMachine : ActiveStateMachine<int, int>
	{
	}

	public class PassiveStateMachine : PassiveStateMachine<int, int>
	{
	}

	public class State : State<int, int>
	{
		public State(int stateID)
			: base(stateID)
		{
		}

		public State(int stateID, EntryHandler entryHandler)
			: base(stateID, entryHandler)
		{
		}

		public State(int stateID, ExitHandler exitHandler)
			: base(stateID, exitHandler)
		{
		}

		public State(int stateID, EntryHandler entryHandler, ExitHandler exitHandler)
			: base(stateID, entryHandler, exitHandler)
		{
		}
	}

	public class Transition : Transition<int, int>
	{
		/// <summary>
		/// Initializes a new instance of the Transition class with the 
		/// specified target.
		/// </summary>
		/// <param name="target">
		/// The target state of the transition.
		/// </param>
		public static Transition<TState, TEvent> Create<TState, TEvent>(State<TState, TEvent> target)
			where TState : struct, IComparable, IFormattable /*, IConvertible*/
			where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
		{
			return new Transition<TState, TEvent>(target);
		}

		/// <summary>
		/// Initializes a new instance of the Transition class with the 
		/// specified guard and target.
		/// </summary>
		/// <param name="guard">
		/// The guard to test to determine whether the transition should take 
		/// place.
		/// </param>
		/// <param name="target">
		/// The target state of the transition.
		/// </param>
		public static Transition<TState, TEvent> Create<TState, TEvent>(GuardHandler guard, State<TState, TEvent> target)
			where TState : struct, IComparable, IFormattable /*, IConvertible*/
			where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
		{
			return new Transition<TState, TEvent>(guard, target);
		}
	}
}