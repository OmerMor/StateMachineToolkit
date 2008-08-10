using System;
using System.Threading;
using NUnit.Framework;
using Sanford.StateMachineToolkit;

namespace StateMachineToolkit.Tests
{
	[TestFixture]
	public class TestActiveStateMachine
	{
		[Test]
		public void SimpleTransitionTest()
		{
			TestMachine machine = new TestMachine();
			var handle = new AutoResetEvent(false);
			machine.TransitionCompleted += (sender, e) => handle.Set();
			machine.Send(Events.S1_to_S2);
			bool signaled = handle.WaitOne(TimeSpan.FromMilliseconds(500), false);
			Assert.IsTrue(signaled);
			Assert.AreEqual(States.S2, machine.CurrentStateID);
		}

		[Test]
		public void SimpleTransitionTest2()
		{
			TestMachine<States, Events> machine = new TestMachine<States, Events>();
			var s1 = new State<States, Events>(States.S1);
			var s2 = new State<States, Events>(States.S2);
			var t1 = Transition.Create(s2);
			s1.Transitions.Add(Events.S1_to_S2, t1);
			machine.Start(s1);
			var handle = new AutoResetEvent(false);
			machine.TransitionCompleted += (sender, e) => handle.Set();
			machine.Send(Events.S1_to_S2);
			bool signaled = handle.WaitOne(TimeSpan.FromMilliseconds(500), false);
			Assert.IsTrue(signaled);
			Assert.AreEqual(States.S2, machine.CurrentStateID);
		}

		[Test]
		public void GuardException()
		{
			TestMachine<States, Events> machine = new TestMachine<States, Events>();
			var s1 = new State<States, Events>(States.S1);
			var s2 = new State<States, Events>(States.S2);
			var t1 = Transition.Create(args => { throw new Exception(); }, s2);
			s1.Transitions.Add(Events.S1_to_S2, t1);
			machine.Start(s1);
			var transitionEvent = new AutoResetEvent(false);
			var exceptionEvent = new AutoResetEvent(false);
			machine.TransitionCompleted += (sender, e) => transitionEvent.Set();
			machine.ExceptionThrown += (sender, e) => exceptionEvent.Set();
			machine.Send(Events.S1_to_S2);
			bool transitionCompleted = transitionEvent.WaitOne(TimeSpan.FromMilliseconds(500), false);
			Assert.IsFalse(transitionCompleted);
			Assert.AreEqual(States.S1, machine.CurrentStateID);
			bool exceptionThrown = exceptionEvent.WaitOne(TimeSpan.FromMilliseconds(500), false);
			Assert.IsTrue(exceptionThrown);
		}

		[Test]
		public void EntryException()
		{
			TestMachine<States, Events> machine = new TestMachine<States, Events>();
			var s1 = new State<States, Events>(States.S1, () => { throw new Exception(); }, null);
			var s2 = new State<States, Events>(States.S2);
			var t1 = Transition.Create(s2);
			s1.Transitions.Add(Events.S1_to_S2, t1);
			machine.Start(s1);
			var transitionEvent = new AutoResetEvent(false);
			var exceptionEvent = new AutoResetEvent(false);
			machine.TransitionCompleted += (sender, e) => transitionEvent.Set();
			machine.ExceptionThrown += (sender, e) => exceptionEvent.Set();
			machine.Send(Events.S1_to_S2);
			bool transitionCompleted = transitionEvent.WaitOne(TimeSpan.FromMilliseconds(500), false);
			Assert.IsFalse(transitionCompleted);
			bool exceptionThrown = exceptionEvent.WaitOne(TimeSpan.FromMilliseconds(500), false);
			Assert.IsTrue(exceptionThrown);
		}
		[Test]
		public void ExitException()
		{
			TestMachine<States, Events> machine = new TestMachine<States, Events>();
			var s1 = new State<States, Events>(States.S1, null, () => { throw new Exception(); });
			var s2 = new State<States, Events>(States.S2);
			var t1 = Transition.Create(s2);
			s1.Transitions.Add(Events.S1_to_S2, t1);
			machine.Start(s1);
			var transitionEvent = new AutoResetEvent(false);
			var exceptionEvent = new AutoResetEvent(false);
			machine.TransitionCompleted += (sender, e) => transitionEvent.Set();
			machine.ExceptionThrown += (sender, e) => exceptionEvent.Set();
			machine.Send(Events.S1_to_S2);
			bool transitionCompleted = transitionEvent.WaitOne(TimeSpan.FromMilliseconds(500), false);
			Assert.IsFalse(transitionCompleted);
			Assert.AreEqual(States.S1, machine.CurrentStateID);
			bool exceptionThrown = exceptionEvent.WaitOne(TimeSpan.FromMilliseconds(500), false);
			Assert.IsTrue(exceptionThrown);
		}
	}
	public class TestMachine<TState, TEvent> : ActiveStateMachine<TState, TEvent>
		where TEvent : struct, IComparable, IFormattable
		where TState : struct, IComparable, IFormattable
	{
		public void Start(State<TState,TEvent> initialState)
		{
			Initialize(initialState);
		}
	}
	
	public class TestMachine : ActiveStateMachine<States, Events>
	{
		public TestMachine()
		{
			var s1 = new State<States, Events>(States.S1);
			var s2 = new State<States, Events>(States.S2);
			var t1 = Transition.Create(s2);
			var t2 = Transition.Create(s1);
			s1.Transitions.Add(Events.S1_to_S2, t1);
			s2.Transitions.Add(Events.S2_to_S1, t2);
// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Initialize(s1);
// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}
	}

	public enum States
	{
		S1,
		S2
	}

	public enum Events
	{
		S1_to_S2,
		S2_to_S1
	}
}