using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Sanford.StateMachineToolkit;

namespace StateMachineToolkit.Tests.Passive
{
	/// <summary>
	/// The error-handling behavior of the SM should be as follows:
	/// A standard sequence is:
	///		Event --> Guard* --> BeginTransition --> State.Exit* --> 
	///			Transition.Action* --> State.Enter* --> TransitionCompleted
	/// Another might be:
	///		Event --> Guard* --> TransitionDeclined
	/// </summary>
	[TestFixture]
	public class TestPassiveStateMachine
	{
		private EventTester m_beginDispatchEvent;
		private EventTester m_transitionDeclinedEvent;
		private EventTester m_transitionCompletedEvent;
		private EventTester m_exceptionThrownEvent;

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

		private void registerMachineEvents(StateMachine<StateID, EventID> machine)
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
		public void SimpleTransitionTest()
		{
			var machine = new TestMachine();
			registerMachineEvents(machine);
			machine.Send(EventID.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, false);
			Assert.AreEqual(StateID.S2, machine.CurrentStateID);
		}

		[Test]
		public void SimpleTransitionTest2()
		{
			var machine = new TestMachine<StateID, EventID>();
			machine.AddTransition(StateID.S1, EventID.S1_to_S2, StateID.S2);

			registerMachineEvents(machine);
			machine.Start(StateID.S1);
			machine.Send(EventID.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, false);
			Assert.AreEqual(StateID.S2, machine.CurrentStateID);
		}

		[Test]
		public void GuardException()
		{
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1);
			var s2 = machine.CreateState(StateID.S2);
			s1.Transitions.Add(EventID.S1_to_S2, args => { throw new Exception(); }, s2);

			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(EventID.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, true, false, true);
			Assert.AreEqual(StateID.S1, machine.CurrentStateID);
		}

		[Test]
		public void EntryExceptionOnInit()
		{
			TransitionErrorEventArgs<StateID, EventID> args;
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1, () => { throw new Exception(); }, null);
			var s2 = machine.CreateState(StateID.S2);
			s1.Transitions.Add(EventID.S1_to_S2, s2);
			registerMachineEvents(machine);
			args = null;
			machine.ExceptionThrown += (sender, e) => args = e;
			machine.Start(s1);
			assertMachineEvents(false, false, false, true);
			Assert.AreEqual(StateID.S1, machine.CurrentStateID);
			Assert.AreEqual(false, args.MachineInitialized);
		}

		[Test]
		public void EntryExceptionOnSend()
		{
			TransitionErrorEventArgs<StateID, EventID> errorEventArgs;
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1);
			var s2 = machine.CreateState(StateID.S2, () => { throw new Exception(); }, null);
			s1.Transitions.Add(EventID.S1_to_S2, s2);
			errorEventArgs = null;
			machine.ExceptionThrown += (sender, args) => errorEventArgs = args;
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(EventID.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);

			Assert.AreEqual(StateID.S1, errorEventArgs.SourceStateID);
			Assert.AreEqual(StateID.S2, machine.CurrentStateID);
			Assert.AreEqual(EventID.S1_to_S2, errorEventArgs.EventID);
		}

		[Test]
		public void ExitException()
		{
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1, null, () => { throw new Exception(); });
			var s2 = machine.CreateState(StateID.S2);
			s1.Transitions.Add(EventID.S1_to_S2, s2);
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(EventID.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);
			Assert.AreEqual(StateID.S2, machine.CurrentStateID);
		}

		[Test]
		public void TransitionActions_ThrowsException()
		{
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1);
			var s2 = machine.CreateState(StateID.S2);
			var count = 0;
			s1.Transitions.Add(EventID.S1_to_S2, s2, args =>
			                                        	{
			                                        		count++;
			                                        		throw new Exception();
			                                        	});
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(EventID.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);
			Assert.AreEqual(StateID.S2, machine.CurrentStateID);
			Assert.AreEqual(1, count);
		}

		[Test]
		public void TransitionActions_ThrowsExceptionTwice()
		{
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1);
			var s2 = machine.CreateState(StateID.S2);
			var count = 0;
			ActionHandler actionHandler = args =>
			                              	{
			                              		count++;
			                              		throw new Exception();
			                              	};
			s1.Transitions.Add(EventID.S1_to_S2, s2, actionHandler, actionHandler);
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(EventID.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);
			Assert.AreEqual(StateID.S2, machine.CurrentStateID);
			Assert.AreEqual(2, count);
		}

		[Test]
		public void TransitionDeclined()
		{
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1, null, () => { throw new Exception(); });
			var s2 = machine.CreateState(StateID.S2);
			s1.Transitions.Add(EventID.S1_to_S2, s2);
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(EventID.S2_to_S1);
			machine.Execute();
			assertMachineEvents(true, true, false, false);
			Assert.AreEqual(StateID.S1, machine.CurrentStateID);
		}

		[Test]
		public void TransitionDeclined_ThrowsError()
		{
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1, null, () => { throw new Exception(); });
			var s2 = machine.CreateState(StateID.S2);
			s1.Transitions.Add(EventID.S1_to_S2, s2);
			registerMachineEvents(machine);
			machine.TransitionDeclined += (sender, e) => { throw new Exception(); };
			machine.Start(s1);
			machine.Send(EventID.S2_to_S1);
			machine.Execute();
			assertMachineEvents(true, true, false, true);
			Assert.AreEqual(StateID.S1, machine.CurrentStateID);
		}

		[Test]
		public void BeginDispatch()
		{
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1);
			var s2 = machine.CreateState(StateID.S2);
			s1.Transitions.Add(EventID.S1_to_S2, s2);
			machine.BeginDispatch += (sender, e) =>
			                         	{
			                         		Assert.AreEqual(StateID.S1, machine.CurrentStateID);
			                         		Assert.AreEqual(EventID.S1_to_S2, e.EventID);
			                         		Assert.AreEqual(StateID.S1, e.SourceStateID);
			                         		Assert.AreEqual(123, e.EventArgs[0]);
			                         	};

			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(EventID.S1_to_S2, 123);
			machine.Execute();
			assertMachineEvents(true, false, true, false);
			Assert.AreEqual(StateID.S2, machine.CurrentStateID);
		}

		[Test]
		public void BeginDispatch_ThrowsError()
		{
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1);
			var s2 = machine.CreateState(StateID.S2);
			s1.Transitions.Add(EventID.S1_to_S2, s2);

			registerMachineEvents(machine);
			machine.BeginDispatch += (sender, e) => { throw new Exception(); };
			machine.Start(s1);
			machine.Send(EventID.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);
			Assert.AreEqual(StateID.S2, machine.CurrentStateID);
		}

		[Test]
		public void TransitionCompleted_ThrowsError()
		{
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1);
			var s2 = machine.CreateState(StateID.S2);
			s1.Transitions.Add(EventID.S1_to_S2, s2);

			registerMachineEvents(machine);
			machine.TransitionCompleted += (sender, e) => { throw new Exception(); };
			machine.Start(s1);
			machine.Send(EventID.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);
			Assert.AreEqual(StateID.S2, machine.CurrentStateID);
		}

		[Test]
		public void Superstate_should_handle_event_when_guard_of_substate_does_not_pass()
		{
			var machine = new TestMachine<StateID, EventID>();
			var s1 = machine.CreateState(StateID.S1);
			var s1_1 = machine.CreateState(StateID.S1_1);
			var s1_2 = machine.CreateState(StateID.S1_2);
			var s2 = machine.CreateState(StateID.S2);

			s1.Substates.Add(s1_1);
			s1.Substates.Add(s1_2);
			s1.InitialState = s1_1;
			s1_1.Transitions.Add(EventID.E1, s1_2);
			s1_2.Transitions.Add(EventID.E1, args => false, s1_1);
			s1.Transitions.Add(EventID.E1, s2);

			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(EventID.E1);
			machine.Execute();
			Assert.AreEqual(StateID.S1_2, machine.CurrentStateID);
			machine.Send(EventID.E1);
			machine.Execute();
			Assert.AreEqual(StateID.S2, machine.CurrentStateID);
			assertMachineEvents(true, false, true, false);
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
			var timeout = Debugger.IsAttached
			                   	? TimeSpan.FromMinutes(1)
			                   	: TimeSpan.FromMilliseconds(50);
			return wasCalled(timeout);
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

	public class TestMachine<TState, TEvent> : PassiveStateMachine<TState, TEvent>
		where TEvent : struct, IComparable, IFormattable
		where TState : struct, IComparable, IFormattable
	{
		public void Start(TState initialState)
		{
			Initialize(initialState);
		}
		public void Start(State initialState)
		{
			Initialize(initialState);
		}
	}

	public class TestMachine : PassiveStateMachine<StateID, EventID>
	{
		public TestMachine()
		{
			AddTransition(StateID.S1, EventID.S1_to_S2, StateID.S2);
			AddTransition(StateID.S2, EventID.S2_to_S1, StateID.S1);
// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Initialize(StateID.S1);
// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}
	}

	public enum StateID
	{
		S1,
		S2,
		S1_1,
		S1_2,
	}

	public enum EventID
	{
		S1_to_S2,
		S2_to_S1,
		E1,
	}
}