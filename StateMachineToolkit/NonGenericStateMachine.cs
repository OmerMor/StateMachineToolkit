using System;
using Sanford.StateMachineToolkit;

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
		public static Transition<TState, TEvent> Create<TState, TEvent>(State<TState, TEvent> target)
			where TState : struct, IComparable, IFormattable/*, IConvertible*/
			where TEvent : struct, IComparable, IFormattable/*, IConvertible*/
		{
			return new Transition<TState, TEvent>(target);
		}

		public static Transition<TState, TEvent> Create<TState, TEvent>(GuardHandler guard, State<TState, TEvent> target)
			where TState : struct, IComparable, IFormattable/*, IConvertible*/
			where TEvent : struct, IComparable, IFormattable/*, IConvertible*/
		{
			return new Transition<TState, TEvent>(guard, target);
		}
	}
}