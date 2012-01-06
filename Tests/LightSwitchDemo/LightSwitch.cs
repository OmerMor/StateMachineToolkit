using System;
using Sanford.StateMachineToolkit;

namespace LightSwitchDemo
{
	public class LightSwitch : PassiveStateMachine<StateID, EventID, EventArgs>
	{
		public LightSwitch()
		{
			this[StateID.Off].EntryHandler += EnterOff;
            this[StateID.Off].ExitHandler += ExitOff;
            this[StateID.On].EntryHandler += EnterOn;
            this[StateID.On].ExitHandler += ExitOn;
			AddTransition(StateID.Off, EventID.TurnOn, StateID.On, TurnOn);
			AddTransition(StateID.On, EventID.TurnOff, StateID.Off, TurnOff);

			Initialize(StateID.Off);
		}

		#region Entry/Exit Methods

		private static void EnterOn(object sender, TransitionEventArgs<StateID, EventID, EventArgs> e)
		{
			Console.WriteLine("Entering On state.");
		}

		private static void ExitOn(object sender, TransitionEventArgs<StateID, EventID, EventArgs> e)
		{
			Console.WriteLine("Exiting On state.");
		}

		private static void EnterOff(object sender, TransitionEventArgs<StateID, EventID, EventArgs> e)
		{
			Console.WriteLine("Entering Off state.");
		}

		private static void ExitOff(object sender, TransitionEventArgs<StateID, EventID, EventArgs> e)
		{
			Console.WriteLine("Exiting Off state.");
		}

		#endregion

		#region Action Methods

        private void TurnOn(object sender, EventArgs args)
		{
			Console.WriteLine("Light switch turned on.");

			ActionResult = "Turned on the light switch.";
		}

        private void TurnOff(object sender, EventArgs args)
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