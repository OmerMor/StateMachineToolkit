using System;
using Sanford.StateMachineToolkit;

namespace LightSwitchDemo
{
	internal class Class1
	{
		[STAThread]
		private static void Main()
		{
			LightSwitch ls = new LightSwitch();

			ls.TransitionCompleted += HandleTransitionCompleted;

			ls.Send(EventID.TurnOn);
			ls.Send(EventID.TurnOff);
			ls.Send(EventID.TurnOn);
			ls.Send(EventID.TurnOff);

			Console.ReadLine();
			ls.Execute();
			Console.ReadLine();
		}

		private static void HandleTransitionCompleted(object sender, TransitionCompletedEventArgs<StateID, EventID> e)
		{
			Console.WriteLine("Transition Completed:");
			Console.WriteLine("\tState ID: {0}", e.StateID);
			Console.WriteLine("\tEvent ID: {0}", e.EventID);

			if (e.Error != null)
			{
				Console.WriteLine("\tException: {0}", e.Error.Message);
			}
			else
			{
				Console.WriteLine("\tException: No exception was thrown.");
			}

			if (e.ActionResult != null)
			{
				Console.WriteLine("\tAction Result: {0}", e.ActionResult);
			}
			else
			{
				Console.WriteLine("\tAction Result: No action result.");
			}
		}
	}
}