using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using Sanford.StateMachineToolkit;

namespace StateMachineToolkit.Tests.Active
{
    ///<summary>
    ///  The error-handling behavior of the SM should be as follows: A standard sequence is: Event --> Guard* --> BeginTransition --> State.Exit* --> Transition.Action* --> State.Enter* --> TransitionCompleted Another might be: Event --> Guard* --> TransitionDeclined
    ///</summary>
    [TestFixture]
    public class TestActiveStateMachine
    {
        [TearDown]
        public void TearDown()
        {
        }

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

        private void registerMachineEvents<TState, TEvent, TArgs>(IStateMachine<TState, TEvent, TArgs> machine)
            //where TArgs : EventArgs
        {
            m_beginDispatchEvent = new EventTester();
            m_beginTransitionEvent = new EventTester();
            m_transitionDeclinedEvent = new EventTester();
            m_transitionCompletedEvent = new EventTester();
            m_exceptionThrownEvent = new EventTester();
            m_lastException = null;
            machine.BeginDispatch += (sender, e) => m_beginDispatchEvent.Set();
            machine.BeginTransition += (sender, e) => m_beginTransitionEvent.Set();
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
        public void SendSynchronously_should_not_hang_when_raising_BeginDispach_event()
        {
            var finishedEvent = new AutoResetEvent(false);
            EventHandler onLoad = ((sender, e) =>
            {
                using (var machine = new TestMachine<State, Event, EventArgs>())
                {
                    machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);
                    machine.BeginDispatch += (sender1, e1) => { };
                    machine.Start(State.S1);
                    machine.SendSynchronously(Event.S1_to_S2);
                    finishedEvent.Set();
                }
            });
            Form form = null;
            ThreadPool.QueueUserWorkItem(state =>
            {
                // The form must be instantiated in the future ui thread.
                // Otherwise, a WindowsFormSynchronizationContext would be installed
                // on the test thread (which has no message-pump), and will cause hangups.
                form = new Form();
                form.Load += onLoad;
                Application.Run(form);
            });
            var finished = finishedEvent.WaitOne(TimeSpan.FromSeconds(10));
            Assert.IsTrue(finished);
            form.Invoke((MethodInvoker) form.Dispose);
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
        public void Test_machine_with_String_state_and_Int32_event_types()
        {
            using (var machine = new TestMachine<string, int, EventArgs>())
            {
                machine.AddTransition("S1", 12, "S2");
                registerMachineEvents(machine);
                machine.Start("S1");
                machine.Send(12);
                machine.WaitForPendingEvents();
                assertMachineEvents(true, false, true, false);
                Assert.AreEqual("S2", machine.CurrentStateID);
            }
        }

        [Test]
        public void SimpleTransitionTest2()
        {
            using (var machine = new TestMachine<State, Event, EventArgs>())
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
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                machine.AddTransition(State.S1, Event.S1_to_S2, guardException, State.S2);

                registerMachineEvents(machine);
                machine.Start(State.S1);
                machine.Send(Event.S1_to_S2);
                machine.WaitForPendingEvents();
                assertMachineEvents(true, true, false, true);
                Assert.AreEqual(State.S1, machine.CurrentStateID);
                Assert.IsInstanceOf<GuardException>(m_lastException);
            }
        }

        [Test]
        public void GuardException_should_not_prevent_machine_from_checking_other_guards()
        {
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                machine.AddTransition(State.S1, Event.S1_to_S2, guardException, State.S2);
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
            TransitionErrorEventArgs<State, Event, EventArgs> args;
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                machine[State.S1].EntryHandler += throwException;
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
            Assert.IsInstanceOf<EntryException>(m_lastException);
        }

        [Test]
        public void EntryExceptionOnSend()
        {
            TransitionErrorEventArgs<State, Event, EventArgs> errorEventArgs;
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                machine[State.S2].EntryHandler += throwException;
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
            Assert.IsInstanceOf<EntryException>(m_lastException);
        }

        [Test]
        public void ExitException()
        {
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                machine[State.S1].ExitHandler += throwException;
                machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);
                registerMachineEvents(machine);
                machine.Start(State.S1);
                machine.Send(Event.S1_to_S2);
                machine.WaitForPendingEvents();
                assertMachineEvents(true, false, true, true);
                Assert.AreEqual(State.S2, machine.CurrentStateID);
                Assert.IsInstanceOf<ExitException>(m_lastException);
            }
        }

        [Test]
        public void TransitionActions_ThrowsException()
        {
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                var count = 0;
                EventHandler<TransitionEventArgs<State, Event, EventArgs>> actionHandler =
                    (sender, e) =>
                    {
                        count++;
                        throwException(sender, e);
                    };
                machine.AddTransition(State.S1, Event.S1_to_S2, State.S2,
                                      actionHandler);
                registerMachineEvents(machine);
                machine.Start(State.S1);
                machine.Send(Event.S1_to_S2);
                machine.WaitForPendingEvents();
                assertMachineEvents(true, false, true, true);
                Assert.AreEqual(State.S2, machine.CurrentStateID);
                Assert.AreEqual(1, count);
                Assert.IsInstanceOf<ActionException>(m_lastException);
            }
        }

        [Test]
        public void TransitionActions_ThrowsExceptionTwice()
        {
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                var count = 0;
                EventHandler<TransitionEventArgs<State, Event, EventArgs>> actionHandler =
                    (sender, e) =>
                    {
                        count++;
                        throwException(sender, e);
                    };
                machine.AddTransition(State.S1, Event.S1_to_S2, State.S2, actionHandler, actionHandler);

                registerMachineEvents(machine);
                machine.Start(State.S1);
                machine.Send(Event.S1_to_S2);
                machine.WaitForPendingEvents();
                assertMachineEvents(true, false, true, true);
                Assert.AreEqual(State.S2, machine.CurrentStateID);
                Assert.AreEqual(2, count);
                Assert.IsInstanceOf<ActionException>(m_lastException);
            }
        }

        [Test]
        public void TransitionDeclined()
        {
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                machine[State.S1].ExitHandler += throwException;
                machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);

                registerMachineEvents(machine);
                machine.Start(State.S1);
                machine.Send(Event.S2_to_S1);
                machine.WaitForPendingEvents();
                assertMachineEvents(true, true, false, false);
                Assert.AreEqual(State.S1, machine.CurrentStateID);
            }
        }

        private static void throwException(object sender, TransitionEventArgs<State, Event, EventArgs> args)
        {
            throw new Exception();
        }

        private static bool guardException(object sender, TransitionEventArgs<State, Event, EventArgs> args)
        {
            throw new Exception();
        }

        [Test]
        public void TransitionDeclined_ThrowsError()
        {
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                machine[State.S1].ExitHandler += throwException;
                machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);

                registerMachineEvents(machine);
                machine.TransitionDeclined += throwException;
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
            using (var machine = new TestMachine<State, Event, int>())
            {
                machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);
                machine.BeginDispatch += (sender, e) =>
                {
                    Assert.AreEqual(State.S1, machine.CurrentStateID);
                    Assert.AreEqual(Event.S1_to_S2, e.EventID);
                    Assert.AreEqual(State.S1, e.SourceStateID);
                    Assert.AreEqual(123, e.EventArgs);
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
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);

                registerMachineEvents(machine);
                machine.BeginDispatch += throwException;
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
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);

                registerMachineEvents(machine);
                machine.TransitionCompleted += throwException;
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
            using (var machine = new TestMachine<State, Event, EventArgs>())
            {
                machine.SetupSubstates(State.S1, HistoryType.None, State.S1_1, State.S1_2);
                machine.AddTransition(State.S1_1, Event.E1, State.S1_2);
                machine.AddTransition(State.S1_2, Event.E1, (sender, args) => false, State.S1_2);
                machine.AddTransition(State.S1, Event.E1, State.S2);

                IActiveStateMachine<State, Event, EventArgs> m = machine;
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
            TestMachine<State, Event, EventArgs> machine = null;
            var loadedEvent = new EventTester(TimeSpan.FromSeconds(1000));
            var calledInUiThread = false;
            EventHandler onLoad =
                (sender, e) =>
                {
                    machine = new TestMachine<State, Event, EventArgs>();
                    machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);
                    var uiThreadId = Thread.CurrentThread.ManagedThreadId;
                    machine.BeginDispatch +=
                        (sender1, e1) => { calledInUiThread = Thread.CurrentThread.ManagedThreadId == uiThreadId; };
                };
            Form form = null;
            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    using (form = new Form())
                    {
                        form.Load += onLoad;
                        form.Load += (sender, e) => loadedEvent.Set();
                        Application.Run(form);
                    }
                });
            loadedEvent.AssertWasCalled("Form was not loaded.");
            Assert.IsNotNull(machine);
            machine.Start(State.S1);
            machine.Send(Event.S1_to_S2);
            machine.WaitForPendingEvents();
            Assert.IsTrue(calledInUiThread);
            form.Invoke((MethodInvoker) (() => form.Close()));
        }
    }

    public sealed class EventTester
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

    public sealed class TestMachine<TState, TEvent, TArgs> : ActiveStateMachine<TState, TEvent, TArgs>
        //where TArgs : EventArgs
        //where TEvent : struct, IComparable, IFormattable
        //where TState : struct, IComparable, IFormattable
    {
        public void Start(TState initialState)
        {
            Initialize(initialState);
        }
    }

    public sealed class TestMachine : ActiveStateMachine<State, Event, EventArgs>
    {
        public TestMachine()
        {
            AddTransition(State.S1, Event.S1_to_S2, State.S2);
            AddTransition(State.S2, Event.S2_to_S1, State.S1);
// ReSharper disable DoNotCallOverridableMethodsInConstructor
            Initialize(State.S1);
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
}