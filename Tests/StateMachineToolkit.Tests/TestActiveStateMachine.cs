using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using Sanford.StateMachineToolkit;
using Timer=System.Threading.Timer;

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
        private EventTester m_beginTransitionEvent;
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

		private void registerMachineEvents(IStateMachine<State, Event> machine)
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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
				machine.WaitForPendingEvents();
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

			    IActiveStateMachine<State, Event> m = machine;
                registerMachineEvents(m);
				machine.Start(State.S1);
                m.SendSynchronously(Event.E1);
                Assert.AreEqual(State.S1_2, m.CurrentStateID);
				machine.SendSynchronously(Event.E1);
                Assert.AreEqual(State.S2, m.CurrentStateID);
                m.WaitForPendingEvents();
				assertMachineEvents(true, false, true, false);
			}
		}



		[Test]
		public void BeginDispach_event_should_raise_in_right_context()
		{
			TestMachine<State, Event> machine = null;
			var loadedEvent = new EventTester(TimeSpan.FromSeconds(1000));
			var calledInUiThread = false;
			EventHandler onLoad =
				(sender, e) =>
				{
					machine = new TestMachine<State, Event>();
					machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);
					var uiThreadId = Thread.CurrentThread.ManagedThreadId;
					machine.BeginDispatch +=
						(sender1, e1) =>
						{
							calledInUiThread = Thread.CurrentThread.ManagedThreadId == uiThreadId;
						};
				};
			Form form = null;
			try
			{
				ThreadPool.QueueUserWorkItem(
					state =>
					{
						form = new Form();
						form.Load += onLoad;
						form.Load += (sender, e) => loadedEvent.Set();
						Application.Run(form);
					});
				loadedEvent.AssertWasCalled("Form was not loaded.");
				Assert.IsNotNull(machine);
				machine.Start(State.S1);
				machine.Send(Event.S1_to_S2);
				machine.WaitForPendingEvents();
				Assert.IsTrue(calledInUiThread);
			}
			finally
			{
				form.Close();
			}
		}

		[Test]
		public void SendSynchronously_should_not_hang_when_raising_BeginDispach_event()
		{
			var finishedEvent = new AutoResetEvent(false);
			EventHandler onLoad = ((sender, e) =>
			{
				using (var machine = new TestMachine<State, Event>())
				{
					machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);
					machine.BeginDispatch += (sender1, e1) => { };
					machine.Start(State.S1);
					machine.SendSynchronously(Event.S1_to_S2);
					finishedEvent.Set();
				}
			});
			Form form = null;
			try
			{
				ThreadPool.QueueUserWorkItem(
					state =>
						{
							form = new Form();
							form.Load += onLoad;
							Application.Run(form);
						});
				var finished = finishedEvent.WaitOne(TimeSpan.FromSeconds(10));
				Assert.IsTrue(finished);
			}
			finally
			{
				form.Close();
			}
		}

	}

	public class EventTester
	{
		private readonly AutoResetEvent wasCalledEvent = new AutoResetEvent(false);
		private readonly TimeSpan m_timeout;

		public EventTester()
			: this(TimeSpan.FromMilliseconds(0))
		{
		}

		public EventTester(TimeSpan timeout)
		{
			m_timeout = timeout;
		}

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
			return wasCalled(m_timeout);
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
	
    public class TraficLightStateMachine : ActiveStateMachine<TraficLightStates,TraficLightEvents>
    {
        private readonly Timer m_timer;
        private static readonly TimeSpan INTERVAL = TimeSpan.FromSeconds(2);

        public TraficLightStateMachine()
        {
            AddTransition(TraficLightStates.Off,        TraficLightEvents.Start,        TraficLightStates.On,   x => start());
            AddTransition(TraficLightStates.On,         TraficLightEvents.Stop,         TraficLightStates.Off,  x => stop());
            AddTransition(TraficLightStates.Red,        TraficLightEvents.TimeEvent,    TraficLightStates.RedYellow);
            AddTransition(TraficLightStates.RedYellow,  TraficLightEvents.TimeEvent,    TraficLightStates.Green);
            AddTransition(TraficLightStates.Green,      TraficLightEvents.TimeEvent,    TraficLightStates.Yellow);
            AddTransition(TraficLightStates.Yellow,     TraficLightEvents.TimeEvent,    TraficLightStates.Red);

            SetupSubstates(TraficLightStates.On, HistoryType.None, TraficLightStates.Red,
                           TraficLightStates.RedYellow, TraficLightStates.Green, TraficLightStates.Yellow);

            m_timer = new Timer(state => Send(TraficLightEvents.TimeEvent));

            Initialize(TraficLightStates.Off);
        }

        private void start()
        {
            m_timer.Change(INTERVAL, INTERVAL);
        }
        private void stop()
        {
            m_timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        protected override void Dispose(bool disposing)
        {
            SendPriority(TraficLightEvents.Stop);
            WaitForPendingEvents();

            m_timer.Dispose();
            base.Dispose(disposing);
        }
    }

    public class TrafficLightApp
    {
        public void Main()
        {
            using(var form = new Form())
            using (var sm = new TraficLightStateMachine())
            {
                var state = new Label {Location = new Point {X = 20, Y = 20}};
                sm.TransitionCompleted += (sender, args) => state.Text = args.TargetStateID.ToString();
                var start = new Button {Text = "Start", Location = new Point {X = 20, Y = 60}};
                var stop = new Button {Text = "Stop", Location = new Point {X = 20, Y = 100}};

                start.Click += (sender, args) => sm.Send(TraficLightEvents.Start);
                stop.Click += (sender, args) => sm.SendSynchronously(TraficLightEvents.Stop);

                form.Controls.AddRange(new Control[] {state, start, stop});

                Application.Run(form);
            }
        }
    }

    public enum TraficLightStates
    {
        On,
        Off,
        Green,
        Yellow,
        Red,
        RedYellow
    }
    public enum TraficLightEvents
    {
        Start,
        Stop,
        TimeEvent
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