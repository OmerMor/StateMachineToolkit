using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Sanford.StateMachineToolkit;

namespace StateMachineToolkit.Tests.Active
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
		private Exception m_lastException;

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

		private void registerMachineEvents(StateMachine<State, Event> machine)
		{
			m_beginDispatchEvent = new EventTester();
			m_transitionDeclinedEvent = new EventTester();
			m_transitionCompletedEvent = new EventTester();
			m_exceptionThrownEvent = new EventTester();
			m_lastException = null;
			machine.BeginDispatch += (sender, e) => m_beginDispatchEvent.Set();
			machine.TransitionDeclined += (sender, e) => m_transitionDeclinedEvent.Set();
			machine.TransitionCompleted += (sender, e) => m_transitionCompletedEvent.Set();
			machine.ExceptionThrown += (sender, e) =>
			                           	{
											Debug.WriteLine("ExceptionThrown: " + e.Error);
			                           		m_lastException = e.Error;
			                           		m_exceptionThrownEvent.Set();
			                           	};
		}

		[Test]
		public void SimpleTransitionTest()
		{
			using (var machine = new TestMachine())
			{
				registerMachineEvents(machine);
				machine.Send(Event.S1_to_S2);
				assertMachineEvents(true, false, true, false);
				Assert.AreEqual(State.S2, machine.CurrentStateID);
			}
		}

		[Test]
		public void SimpleTransitionTest2()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);
				registerMachineEvents(machine);
				machine.Start(State.S1);
				machine.Send(Event.S1_to_S2);
				assertMachineEvents(true, false, true, false);
				Assert.AreEqual(State.S2, machine.CurrentStateID);
			}
		}

		[Test]
		public void GuardException()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				machine.AddTransition(State.S1, Event.S1_to_S2, throwException, State.S2);

				registerMachineEvents(machine);
				machine.Start(State.S1);
				machine.Send(Event.S1_to_S2);
				assertMachineEvents(true, true, false, true);
				Assert.AreEqual(State.S1, machine.CurrentStateID);
				Assert.IsInstanceOfType(typeof(GuardException), m_lastException);
			}
		}

		[Test]
		public void GuardException_should_not_prevent_machine_from_checking_other_guards()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				machine.AddTransition(State.S1, Event.S1_to_S2, throwException, State.S2);
				machine.AddTransition(State.S1, Event.S1_to_S2, delegate { return true; }, State.S1_1);

				registerMachineEvents(machine);
				machine.Start(State.S1);
				machine.Send(Event.S1_to_S2);
				assertMachineEvents(true, false, true, true);
				Assert.AreEqual(State.S1_1, machine.CurrentStateID);
			}
		}

		[Test]
		public void EntryExceptionOnInit()
		{
			TransitionErrorEventArgs<State, Event> args;
			using (var machine = new TestMachine<State, Event>())
			{
				machine.States[State.S1].EntryHandler += throwException;
				machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);
				registerMachineEvents(machine);
				args = null;
				machine.ExceptionThrown += (sender, e) => args = e;
				machine.Start(State.S1);
				assertMachineEvents(false, false, false, true);
				Assert.AreEqual(State.S1, machine.CurrentStateID);
			}
			Assert.AreEqual(false, args.MachineInitialized);
			Assert.IsInstanceOfType(typeof(EntryException), m_lastException);
		}

		[Test]
		public void EntryExceptionOnSend()
		{
			TransitionErrorEventArgs<State, Event> errorEventArgs;
			using (var machine = new TestMachine<State, Event>())
			{
				machine.States[State.S2].EntryHandler += throwException;
				machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);
				errorEventArgs = null;
				machine.ExceptionThrown += (sender, args) => errorEventArgs = args;
				registerMachineEvents(machine);
				machine.Start(State.S1);
				machine.Send(Event.S1_to_S2);
				assertMachineEvents(true, false, true, true);

				Assert.AreEqual(State.S1, errorEventArgs.SourceStateID);
				Assert.AreEqual(State.S2, machine.CurrentStateID);
			}
			Assert.AreEqual(Event.S1_to_S2, errorEventArgs.EventID);
			Assert.IsInstanceOfType(typeof(EntryException), m_lastException);
		}

		[Test]
		public void ExitException()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				machine.States[State.S1].ExitHandler += throwException;
				machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);
				registerMachineEvents(machine);
				machine.Start(State.S1);
				machine.Send(Event.S1_to_S2);
				assertMachineEvents(true, false, true, true);
				Assert.AreEqual(State.S2, machine.CurrentStateID);
				Assert.IsInstanceOfType(typeof(ExitException), m_lastException);
			}
		}
		[Test]
		public void TransitionActions_ThrowsException()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				var count = 0;
				machine.AddTransition(State.S1, Event.S1_to_S2, State.S2, args => { count++; throwException(); });
				registerMachineEvents(machine);
				machine.Start(State.S1);
				machine.Send(Event.S1_to_S2);
				assertMachineEvents(true, false, true, true);
				Assert.AreEqual(State.S2, machine.CurrentStateID);
				Assert.AreEqual(1, count);
				Assert.IsInstanceOfType(typeof(ActionException), m_lastException);
			}
		}
		[Test]
		public void TransitionActions_ThrowsExceptionTwice()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				var count = 0;
				ActionHandler actionHandler = args => { count++; throwException(); };
				machine.AddTransition(State.S1, Event.S1_to_S2, State.S2, actionHandler, actionHandler);

				registerMachineEvents(machine);
				machine.Start(State.S1);
				machine.Send(Event.S1_to_S2);
				assertMachineEvents(true, false, true, true);
				Assert.AreEqual(State.S2, machine.CurrentStateID);
				Assert.AreEqual(2, count);
				Assert.IsInstanceOfType(typeof(ActionException), m_lastException);
			}
		}

		[Test]
		public void TransitionDeclined()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				machine.States[State.S1].ExitHandler += throwException;
				machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);

				registerMachineEvents(machine);
				machine.Start(State.S1);
				machine.Send(Event.S2_to_S1);
				assertMachineEvents(true, true, false, false);
				Assert.AreEqual(State.S1, machine.CurrentStateID);
			}
		}

		private static void throwException()
		{
			throw new Exception();
		}
		private static bool throwException(object[] args)
		{
			throw new Exception();
		}

		[Test]
		public void TransitionDeclined_ThrowsError()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				machine.States[State.S1].ExitHandler += throwException;
				machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);

				registerMachineEvents(machine);
				machine.TransitionDeclined += (sender, e) => throwException();
				machine.Start(State.S1);
				machine.Send(Event.S2_to_S1);
				assertMachineEvents(true, true, false, true);
				Assert.AreEqual(State.S1, machine.CurrentStateID);
			}
		}
		[Test]
		public void BeginDispatch()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);
				machine.BeginDispatch += (sender, e) =>
				                         	{
				                         		Assert.AreEqual(State.S1, machine.CurrentStateID);
				                         		Assert.AreEqual(Event.S1_to_S2, e.EventID);
				                         		Assert.AreEqual(State.S1, e.SourceStateID);
				                         		Assert.AreEqual(123, e.EventArgs[0]);
				                         	};

				registerMachineEvents(machine);
				machine.Start(State.S1);
				machine.Send(Event.S1_to_S2, 123);
				assertMachineEvents(true, false, true, false);
				Assert.AreEqual(State.S2, machine.CurrentStateID);
			}
		}

		[Test]
		public void BeginDispatch_ThrowsError()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);

				registerMachineEvents(machine);
				machine.BeginDispatch += (sender, e) => throwException();
				machine.Start(State.S1);
				machine.Send(Event.S1_to_S2);
				assertMachineEvents(true, false, true, true);
				Assert.AreEqual(State.S2, machine.CurrentStateID);
			}
		}
		[Test]
		public void TransitionCompleted_ThrowsError()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);

				registerMachineEvents(machine);
				machine.TransitionCompleted += (sender, e) => throwException();
				machine.Start(State.S1);
				machine.Send(Event.S1_to_S2);
				assertMachineEvents(true, false, true, true);
				Assert.AreEqual(State.S2, machine.CurrentStateID);
			}
		}

		[Test]
		public void Superstate_should_handle_event_when_guard_of_substate_does_not_pass()
		{
			using (var machine = new TestMachine<State, Event>())
			{
				machine.SetupSubstates(State.S1, HistoryType.None, State.S1_1, State.S1_2);
				machine.AddTransition(State.S1_1, Event.E1, State.S1_2);
				machine.AddTransition(State.S1_2, Event.E1, args => false, State.S1_2);
				machine.AddTransition(State.S1, Event.E1, State.S2);

				registerMachineEvents(machine);
				machine.Start(State.S1);
				machine.SendSynchronously(Event.E1);
				Assert.AreEqual(State.S1_2, machine.CurrentStateID);
				machine.SendSynchronously(Event.E1);
				Assert.AreEqual(State.S2, machine.CurrentStateID);
				assertMachineEvents(true, false, true, false);
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
			var timeout = /*Debugger.IsAttached 
			                   	? TimeSpan.FromMinutes(1) 
			                   	:*/ TimeSpan.FromMilliseconds(50);
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
		public void Start(TState initialState)
		{
			Initialize(initialState);
		}
	}
	
	public class TestMachine : ActiveStateMachine<State, Event>
	{
		public TestMachine()
		{
			AddTransition(Active.State.S1, Event.S1_to_S2, Active.State.S2);
			AddTransition(Active.State.S2, Event.S2_to_S1, Active.State.S1);
// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Initialize(Active.State.S1);
// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}
	}

	public enum State
	{
		S1,
		S2,
		S1_1,
		S1_2,
	}

	public enum Event
	{
		S1_to_S2,
		S2_to_S1,
		E1,
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
				var sm = new _<int>.SM();
				var state = new _<int>.State();
				sm.Init(state);
				sm.Send(3);
			}
		}
		public class Transition
		{
			
		}
	}
}