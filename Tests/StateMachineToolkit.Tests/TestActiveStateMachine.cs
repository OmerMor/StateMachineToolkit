using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Sanford.StateMachineToolkit;

namespace StateMachineToolkit.Tests
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
	public class TestActiveStateMachine
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
		public void SimpleTransitionTest()
		{
			using (TestMachine machine = new TestMachine())
			{
				registerMachineEvents(machine);
				machine.Send(Events.S1_to_S2);
				assertMachineEvents(true, false, true, false);
				Assert.AreEqual(States.S2, machine.CurrentStateID);
			}
		}

		[Test]
		public void SimpleTransitionTest2()
		{
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1);
				var s2 = machine.CreateState(States.S2);
				s1.Transitions.Add(Events.S1_to_S2, s2);
				registerMachineEvents(machine);
				machine.Start(s1);
				machine.Send(Events.S1_to_S2);
				assertMachineEvents(true, false, true, false);
				Assert.AreEqual(States.S2, machine.CurrentStateID);
			}
		}

		[Test]
		public void GuardException()
		{
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1);
				var s2 = machine.CreateState(States.S2);
				s1.Transitions.Add(Events.S1_to_S2, args => { throw new Exception(); }, s2);

				registerMachineEvents(machine);
				machine.Start(s1);
				machine.Send(Events.S1_to_S2);
				assertMachineEvents(true, false, false, true);
				Assert.AreEqual(States.S1, machine.CurrentStateID);
			}
		}

		[Test]
		public void EntryExceptionOnInit()
		{
			TransitionErrorEventArgs<States, Events> args;
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1, () => { throw new Exception(); }, null);
				var s2 = machine.CreateState(States.S2);
				s1.Transitions.Add(Events.S1_to_S2, s2);
				registerMachineEvents(machine);
				args = null;
				machine.ExceptionThrown += (sender, e) => args = e;
				machine.Start(s1);
				assertMachineEvents(false, false, false, true);
				Assert.AreEqual(States.S1, machine.CurrentStateID);
			}
			Assert.AreEqual(false, args.MachineInitialized);
		}

		[Test]
		public void EntryExceptionOnSend()
		{
			TransitionErrorEventArgs<States, Events> errorEventArgs;
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1);
				var s2 = machine.CreateState(States.S2, () => { throw new Exception(); }, null);
				s1.Transitions.Add(Events.S1_to_S2, s2);
				errorEventArgs = null;
				machine.ExceptionThrown += (sender, args) => errorEventArgs = args;
				registerMachineEvents(machine);
				machine.Start(s1);
				machine.Send(Events.S1_to_S2);
				assertMachineEvents(true, false, true, true);

				Assert.AreEqual(States.S1, errorEventArgs.SourceStateID);
				Assert.AreEqual(States.S2, machine.CurrentStateID);
			}
			Assert.AreEqual(Events.S1_to_S2, errorEventArgs.EventID);
		}

		[Test]
		public void ExitException()
		{
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1, null, () => { throw new Exception(); });
				var s2 = machine.CreateState(States.S2);
				s1.Transitions.Add(Events.S1_to_S2, s2);
				registerMachineEvents(machine);
				machine.Start(s1);
				machine.Send(Events.S1_to_S2);
				assertMachineEvents(true, false, true, true);
				Assert.AreEqual(States.S2, machine.CurrentStateID);
			}
		}
		[Test]
		public void TransitionActions_ThrowsException()
		{
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1);
				var s2 = machine.CreateState(States.S2);
				int count = 0;
				s1.Transitions.Add(Events.S1_to_S2, s2, args => { count++; throw new Exception(); });
				registerMachineEvents(machine);
				machine.Start(s1);
				machine.Send(Events.S1_to_S2);
				assertMachineEvents(true, false, true, true);
				Assert.AreEqual(States.S2, machine.CurrentStateID);
				Assert.AreEqual(1, count);
			}
		}
		[Test]
		public void TransitionActions_ThrowsExceptionTwice()
		{
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1);
				var s2 = machine.CreateState(States.S2);
				int count = 0;
				ActionHandler actionHandler = args => { count++; throw new Exception(); };
				s1.Transitions.Add(Events.S1_to_S2, s2, actionHandler, actionHandler);
				registerMachineEvents(machine);
				machine.Start(s1);
				machine.Send(Events.S1_to_S2);
				assertMachineEvents(true, false, true, true);
				Assert.AreEqual(States.S2, machine.CurrentStateID);
				Assert.AreEqual(2, count);
			}
		}

		[Test]
		public void TransitionDeclined()
		{
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1, null, () => { throw new Exception(); });
				var s2 = machine.CreateState(States.S2);
				s1.Transitions.Add(Events.S1_to_S2, s2);
				registerMachineEvents(machine);
				machine.Start(s1);
				machine.Send(Events.S2_to_S1);
				assertMachineEvents(true, true, false, false);
				Assert.AreEqual(States.S1, machine.CurrentStateID);
			}
		}
		[Test]
		public void TransitionDeclined_ThrowsError()
		{
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1, null, () => { throw new Exception(); });
				var s2 = machine.CreateState(States.S2);
				s1.Transitions.Add(Events.S1_to_S2, s2);
				registerMachineEvents(machine);
				machine.TransitionDeclined += (sender, e) => { throw new Exception(); };
				machine.Start(s1);
				machine.Send(Events.S2_to_S1);
				assertMachineEvents(true, true, false, true);
				Assert.AreEqual(States.S1, machine.CurrentStateID);
			}
		}
		[Test]
		public void BeginDispatch()
		{
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1);
				var s2 = machine.CreateState(States.S2);
				s1.Transitions.Add(Events.S1_to_S2, s2);
				machine.BeginDispatch += (sender, e) =>
				                         	{
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
		}

		[Test]
		public void BeginDispatch_ThrowsError()
		{
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1);
				var s2 = machine.CreateState(States.S2);
				s1.Transitions.Add(Events.S1_to_S2, s2);

				registerMachineEvents(machine);
				machine.BeginDispatch += (sender, e) => { throw new Exception(); };
				machine.Start(s1);
				machine.Send(Events.S1_to_S2);
				assertMachineEvents(true, false, true, true);
				Assert.AreEqual(States.S2, machine.CurrentStateID);
			}
		}
		[Test]
		public void TransitionCompleted_ThrowsError()
		{
			using (TestMachine<States, Events> machine = new TestMachine<States, Events>())
			{
				var s1 = machine.CreateState(States.S1);
				var s2 = machine.CreateState(States.S2);
				s1.Transitions.Add(Events.S1_to_S2, s2);

				registerMachineEvents(machine);
				machine.TransitionCompleted += (sender, e) => { throw new Exception(); };
				machine.Start(s1);
				machine.Send(Events.S1_to_S2);
				assertMachineEvents(true, false, true, true);
				Assert.AreEqual(States.S2, machine.CurrentStateID);
			}
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
			TimeSpan timeout = Debugger.IsAttached 
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
			var s1 = CreateState(States.S1);
			var s2 = CreateState(States.S2);
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

	public class _<T>
	{
		public class SM
		{
			public void Init(State s)
			{
			}

			public void Send(T t)
			{
			}
		}

		public class State
		{
			public static void Foo()
			{
				_<int>.SM sm = new _<int>.SM();
				_<int>.State state = new _<int>.State();
				sm.Init(state);
				sm.Send(3);
			}
		}
		public class Transition
		{
			
		}
	}
}