using System;
using System.Threading;
using NUnit.Framework;
using Sanford.StateMachineToolkit;

namespace StateMachineToolkit.Tests
{
	[TestFixture]
	public class TestActiveStateMachine
	{
		private EventTester m_beginDispatchEvent;
		private EventTester m_transitionDeclinedEvent;
		private EventTester m_transitionCompletedEvent;
		private EventTester m_exceptionThrownEvent;

		[Test]
		public void SimpleTransitionTest()
		{
			TestMachine machine = new TestMachine();
			registerMachineEvents(machine);
			machine.Send(Events.S1_to_S2);
			assertMachineEvents(true, false, true, false);
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
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Events.S1_to_S2);
			assertMachineEvents(true, false, true, false);
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

			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Events.S1_to_S2);
			assertMachineEvents(true, false, false, true);
			Assert.AreEqual(States.S1, machine.CurrentStateID);
		}

		[Test, ExpectedException(ExceptionType = typeof(InvalidOperationException))]
		public void EntryExceptionOnInit()
		{
			TestMachine<States, Events> machine = new TestMachine<States, Events>();
			var s1 = new State<States, Events>(States.S1, () => { throw new Exception(); }, null);
			var s2 = new State<States, Events>(States.S2);
			var t1 = Transition.Create(s2);
			s1.Transitions.Add(Events.S1_to_S2, t1);
			machine.Start(s1);
		}

		[Test]
		public void EntryExceptionOnSend()
		{
			TestMachine<States, Events> machine = new TestMachine<States, Events>();
			var s1 = new State<States, Events>(States.S1);
			var s2 = new State<States, Events>(States.S2, () => { throw new Exception(); }, null);
			var t1 = Transition.Create(s2);
			s1.Transitions.Add(Events.S1_to_S2, t1);
			TransitionErrorEventArgs<States, Events> errorEventArgs = null;
			machine.ExceptionThrown += (sender, args) => errorEventArgs = args;
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Events.S1_to_S2);
			assertMachineEvents(true, false, false, true);

			Assert.AreEqual(States.S1, errorEventArgs.SourceStateID);
			Assert.AreEqual(Events.S1_to_S2, errorEventArgs.EventID);
		}

		[Test]
		public void ExitException()
		{
			TestMachine<States, Events> machine = new TestMachine<States, Events>();
			var s1 = new State<States, Events>(States.S1, null, () => { throw new Exception(); });
			var s2 = new State<States, Events>(States.S2);
			var t1 = Transition.Create(s2);
			s1.Transitions.Add(Events.S1_to_S2, t1);
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Events.S1_to_S2);
			assertMachineEvents(true, false, false, true);
		}
		[Test]
		public void TransitionDeclined()
		{
			TestMachine<States, Events> machine = new TestMachine<States, Events>();
			var s1 = new State<States, Events>(States.S1, null, () => { throw new Exception(); });
			var s2 = new State<States, Events>(States.S2);
			var t1 = Transition.Create(s2);
			s1.Transitions.Add(Events.S1_to_S2, t1);
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Events.S2_to_S1);
			assertMachineEvents(true, true, false, false);
			Assert.AreEqual(States.S1, machine.CurrentStateID);
		}
		[Test]
		public void BeginDispatch()
		{
			TestMachine<States, Events> machine = new TestMachine<States, Events>();
			var s1 = new State<States, Events>(States.S1);
			var s2 = new State<States, Events>(States.S2);
			var t1 = Transition.Create(s2);
			s1.Transitions.Add(Events.S1_to_S2, t1);
			machine.BeginDispatch += (sender, e) =>
			 {
				 Thread.Sleep(100);
				 Assert.AreEqual(States.S1, machine.CurrentStateID);
				 Assert.AreEqual(Events.S1_to_S2, e.EventID);
				 Assert.AreEqual(States.S1, e.SourceStateID);
				 Assert.AreEqual(123, e.EventArgs[0]);
			 };

			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Events.S1_to_S2, 123);
			assertMachineEvents(true, false, true, false);
			Assert.AreEqual(States.S2, machine.CurrentStateID);
		}

		private void assertMachineEvents(bool beginDispatchCalled, bool transitionDeclinedCalled,
		                                 bool transitionCompletedCalled, bool exceptionThrownCalled)
		{
			if (beginDispatchCalled)
				m_beginDispatchEvent.AssertWasCalled("BeginDispatch was not called.");
			else
				m_beginDispatchEvent.AssertWasNotCalled("BeginDispatch was called.");

			if (transitionDeclinedCalled)
				m_transitionDeclinedEvent.AssertWasCalled("TransitionDeclined was not called.");
			else
				m_transitionDeclinedEvent.AssertWasNotCalled("TransitionDeclined was called.");

			if (transitionCompletedCalled)
				m_transitionCompletedEvent.AssertWasCalled("TransitionCompleted was not called.");
			else
				m_transitionCompletedEvent.AssertWasNotCalled("TransitionCompleted was called.");

			if (exceptionThrownCalled)
				m_exceptionThrownEvent.AssertWasCalled("ExceptionThrown was not called.");
			else
				m_exceptionThrownEvent.AssertWasNotCalled("ExceptionThrown was called.");
		}

		private void registerMachineEvents(StateMachine<States, Events> machine)
		{
			m_beginDispatchEvent = new EventTester();
			m_transitionDeclinedEvent = new EventTester();
			m_transitionCompletedEvent = new EventTester();
			m_exceptionThrownEvent = new EventTester();
			machine.BeginDispatch += (sender, e) => m_beginDispatchEvent.Set();
			machine.TransitionDeclined += (sender, e) => m_transitionDeclinedEvent.Set();
			machine.TransitionCompleted += (sender, e) => m_transitionCompletedEvent.Set();
			machine.ExceptionThrown += (sender, e) => m_exceptionThrownEvent.Set();
		}

		[Test]
		public void BeginDispatch_ThrowsError()
		{
			TestMachine<States, Events> machine = new TestMachine<States, Events>();
			var s1 = new State<States, Events>(States.S1);
			var s2 = new State<States, Events>(States.S2);
			var t1 = Transition.Create(s2);
			s1.Transitions.Add(Events.S1_to_S2, t1);

			registerMachineEvents(machine);
			machine.BeginDispatch += (sender, e) => {throw new Exception();};
			machine.Start(s1);
			machine.Send(Events.S1_to_S2);
			assertMachineEvents(true, false, false, true);
			Assert.AreEqual(States.S1, machine.CurrentStateID);
		}
	}

	public class EventTester
	{
		private readonly AutoResetEvent wasCalledEvent = new AutoResetEvent(false);

		public void Set()
		{
			wasCalledEvent.Set();
		}

		private bool wasCalled(TimeSpan timeout)
		{
			return wasCalledEvent.WaitOne(timeout, false);
		}

		private bool wasCalled()
		{
			return wasCalled(TimeSpan.FromMilliseconds(200));
		}

		public void AssertWasCalled(string message)
		{
			Assert.IsTrue(wasCalled(), message);
		}
		public void AssertWasNotCalled(string message)
		{
			Assert.IsFalse(wasCalled(), message);
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