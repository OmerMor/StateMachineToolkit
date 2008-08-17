using System;

namespace TestExample
{
	/// <summary>
	/// Summary description for Example.
	/// </summary>
	public class Example : ExampleBase
	{
		private bool foo;

		public void SendA()
		{
			Send(EventID.A);
		}

		public void SendB()
		{
			Send(EventID.B);
		}

		public void SendC()
		{
			Send(EventID.C);
		}

		public void SendD()
		{
			Send(EventID.D);
		}

		public void SendE()
		{
			Send(EventID.E);
		}

		public void SendF()
		{
			Send(EventID.F);
		}

		public void SendG()
		{
			Send(EventID.G);
		}

		public void SendH()
		{
			Send(EventID.H);
		}

		protected override void EntryS0()
		{
			Console.Write("EntryS0 ");
		}

		protected override void EntryS1()
		{
			Console.Write("EntryS1 ");
		}

		protected override void EntryS11()
		{
			Console.Write("EntryS11 ");
		}

		protected override void EntryS2()
		{
			Console.Write("EntryS2 ");
		}

		protected override void EntryS21()
		{
			Console.Write("EntryS21 ");
		}

		protected override void EntryS211()
		{
			Console.Write("EntryS211 ");
		}

		protected override void ExitS0()
		{
			Console.Write("ExitS0 ");
		}

		protected override void ExitS1()
		{
			Console.Write("ExitS1 ");
		}

		protected override void ExitS11()
		{
			Console.Write("ExitS11 ");
		}

		protected override void ExitS2()
		{
			Console.Write("ExitS2 ");
		}

		protected override void ExitS21()
		{
			Console.Write("ExitS21 ");
		}

		protected override void ExitS211()
		{
			Console.Write("ExitS211 ");
		}

		protected override bool FooIsFalse(object[] args)
		{
			return foo == false;
		}

		protected override bool FooIsTrue(object[] args)
		{
			return foo;
		}

		protected override void SetFooToFalse(object[] args)
		{
			Console.Write("Setting foo to false... ");
			foo = false;
		}

		protected override void SetFooToTrue(object[] args)
		{
			Console.Write("Setting foo to true... ");
			foo = true;
		}
	}
}