using System;
using Sanford.StateMachineToolkit;

namespace LightSwitchDemo
{
	public class LightSwitch : PassiveStateMachine<StateID, EventID>
	{
		private readonly State<StateID, EventID> on;

		private readonly State<StateID, EventID> off;

		public LightSwitch()
		{
			off = new State<StateID, EventID>(StateID.Off, EnterOff, ExitOff);
			on = new State<StateID, EventID>(StateID.On, EnterOn, ExitOn);

			off.Transitions.Add(EventID.TurnOn, on, TurnOn);

			on.Transitions.Add(EventID.TurnOff, off, TurnOff);

			Initialize(off);
		}

		#region Entry/Exit Methods

		private static void EnterOn()
		{
			Console.WriteLine("Entering On state.");
		}

		private static void ExitOn()
		{
			Console.WriteLine("Exiting On state.");
		}

		private static void EnterOff()
		{
			Console.WriteLine("Entering Off state.");
		}

		private static void ExitOff()
		{
			Console.WriteLine("Exiting Off state.");
		}

		#endregion

		#region Action Methods

		private void TurnOn(object[] args)
		{
			Console.WriteLine("Light switch turned on.");

			ActionResult = "Turned on the light switch.";
		}

		private void TurnOff(object[] args)
		{
			Console.WriteLine("Light switch turned off.");

			ActionResult = "Turned off the light switch.";
		}

		#endregion
	}

	public enum EventID
	{
		TurnOn,
		TurnOff
	}

	public enum StateID
	{
		On,
		Off
	}
}