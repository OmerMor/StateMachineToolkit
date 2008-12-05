namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// A base non-generic active state machine.
	/// </summary>
	public abstract class ActiveStateMachine : ActiveStateMachine<int, int>
	{
	}

	/// <summary>
	/// A base non-generic passive state machine.
	/// </summary>
	public class PassiveStateMachine : PassiveStateMachine<int, int>
	{
	}
}