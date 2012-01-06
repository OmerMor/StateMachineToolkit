using System;
using Sanford.StateMachineToolkit;

namespace TestExample
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	internal class Class1
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			Example e = new Example();
            e.TransitionCompleted += 
                ((sender, eventArgs) =>
                 Console.WriteLine("\t{0} : {1} --> {2}", eventArgs.EventID, eventArgs.SourceStateID,
                                   eventArgs.TargetStateID));
			Console.WriteLine();
			e.SendA();

			Console.WriteLine();
			e.SendE();

			Console.WriteLine();
			e.SendE();

			Console.WriteLine();
			e.SendA();

			Console.WriteLine();
			e.SendH();

            Console.WriteLine();
            e.SendH();

            Console.WriteLine();
            e.SendF();

            Console.WriteLine();
            e.SendH();

            Console.WriteLine();
            e.SendH();

            //e.Dispose();

			Console.Read();
		}
	}
}