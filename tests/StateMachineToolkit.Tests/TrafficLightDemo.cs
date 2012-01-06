using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using Sanford.StateMachineToolkit;
using StateMachineToolkit.Tests.Passive;
using Timer=System.Threading.Timer;

namespace StateMachineToolkit.Tests.Active
{
    [UsedImplicitly]
    public class TrafficLightApp
    {
        [UsedImplicitly]
        public void Main()
        {
            TraficLightStates traficLightState = TraficLightStates.Yellow;
            using(var form = new Form())
            using (var sm = new TraficLightStateMachine(
                new ExternalStateStorage<TraficLightStates>(
                    () => traficLightState,
                    newState => traficLightState = newState)))
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

    public sealed class TraficLightStateMachine : ActiveStateMachine<TraficLightStates, TraficLightEvents, EventArgs>
    {
        private readonly Timer m_timer;
        private static readonly TimeSpan INTERVAL = TimeSpan.FromSeconds(2);
        public TraficLightStateMachine(IStateStorage<TraficLightStates> stateStorage)
            : base(stateStorage: stateStorage)
        {
            AddTransition(TraficLightStates.Off, TraficLightEvents.Start, TraficLightStates.On);
            AddTransition(TraficLightStates.On, TraficLightEvents.Stop, TraficLightStates.Off);
            AddTransition(TraficLightStates.Red, TraficLightEvents.TimeEvent, TraficLightStates.RedYellow);
            AddTransition(TraficLightStates.RedYellow, TraficLightEvents.TimeEvent, TraficLightStates.Green);
            AddTransition(TraficLightStates.Green, TraficLightEvents.TimeEvent, TraficLightStates.Yellow);
            AddTransition(TraficLightStates.Yellow, TraficLightEvents.TimeEvent, TraficLightStates.Red);

            SetupSubstates(TraficLightStates.On, HistoryType.None, TraficLightStates.Red,
                           TraficLightStates.RedYellow, TraficLightStates.Green, TraficLightStates.Yellow);

            this[TraficLightStates.On].EntryHandler += start;
            this[TraficLightStates.On].ExitHandler += stop;

            m_timer = new Timer(s => Send(TraficLightEvents.TimeEvent));

            Initialize();
        }

        public TraficLightStateMachine()
            : this(new InternalStateStorage<TraficLightStates>(TraficLightStates.Yellow))
        {
        }

        private void start(object sender, TransitionEventArgs<TraficLightStates, TraficLightEvents, EventArgs> e)
        {
            m_timer.Change(INTERVAL, INTERVAL);
        }

        private void stop(object sender, TransitionEventArgs<TraficLightStates, TraficLightEvents, EventArgs> e)
        {
            m_timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        protected override void Dispose(bool disposing)
        {
            SendPriority(TraficLightEvents.Stop, null);
            WaitForPendingEvents();

            m_timer.Dispose();
            base.Dispose(disposing);
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

/*
namespace TelephoneCallExample
{
    class Program
    {
        enum Trigger
        {
            CallDialed,
            HungUp,
            CallConnected,
            LeftMessage,
            PlacedOnHold,
            TakenOffHold,
            PhoneHurledAgainstWall
        }

        enum State
        {
            OffHook,
            Ringing,
            Connected,
            OnHold,
            PhoneDestroyed
        }

        static void Main(string[] args)
        {
            var phoneCall = new StateMachine<State, Trigger>(State.OffHook);

            phoneCall.Configure(State.OffHook)
                    .Allow(Trigger.CallDialed, State.Ringing);
                
            phoneCall.Configure(State.Ringing)
                    .Allow(Trigger.HungUp, State.OffHook)
                    .Allow(Trigger.CallConnected, State.Connected);
             
            phoneCall.Configure(State.Connected)
                .OnEntry(t => StartCallTimer())
                .OnExit(t => StopCallTimer())
                    .Allow(Trigger.LeftMessage, State.OffHook)
                    .Allow(Trigger.HungUp, State.OffHook)
                    .Allow(Trigger.PlacedOnHold, State.OnHold);

            phoneCall.Configure(State.OnHold)
                .SubstateOf(State.Connected)
                .Allow(Trigger.TakenOffHold, State.Connected)
                .Allow(Trigger.HungUp, State.OffHook)
                .Allow(Trigger.PhoneHurledAgainstWall, State.PhoneDestroyed);

            Print(phoneCall);
            Fire(phoneCall, Trigger.CallDialed);
            Print(phoneCall);
            Fire(phoneCall, Trigger.CallConnected);
            Print(phoneCall);
            Fire(phoneCall, Trigger.PlacedOnHold);
            Print(phoneCall);
            Fire(phoneCall, Trigger.TakenOffHold);
            Print(phoneCall);
            Fire(phoneCall, Trigger.HungUp);
            Print(phoneCall);

            Console.WriteLine("Press any key...");
            Console.ReadKey(true);
        }

        static void StartCallTimer()
        {
            Console.WriteLine("[Timer:] Call started at {0}", DateTime.Now);
        }

        static void StopCallTimer()
        {
            Console.WriteLine("[Timer:] Call ended at {0}", DateTime.Now);
        }

        static void Fire(StateMachine<State, Trigger> phoneCall, Trigger trigger)
        {
            Console.WriteLine("[Firing:] {0}", trigger);
            phoneCall.Fire(trigger);
        }

        static void Print(StateMachine<State, Trigger> phoneCall)
        {
            Console.WriteLine("[Status:] {0}", phoneCall);
        }
    }
*/
}

namespace TelephoneCallExample
{
    public enum Trigger
    {
        CallDialed,
        HungUp,
        CallConnected,
        LeftMessage,
        PlacedOnHold,
        TakenOffHold,
        PhoneHurledAgainstWall
    }

    public enum State
    {
        OffHook,
        Ringing,
        Connected,
        Talking,
        OnHold,
        PhoneDestroyed
    }

    public class TelephoneStateMachine : PassiveStateMachine<State, Trigger, EventArgs>
    {
        private TelephoneStateMachine(State initialState)
        {
            AddTransition(State.OffHook, Trigger.CallDialed, State.Ringing);

            AddTransition(State.Ringing, Trigger.HungUp, State.OffHook);
            AddTransition(State.Ringing, Trigger.CallConnected, State.Connected);

            this[State.Connected].EntryHandler += StartCallTimer;
            AddTransition(State.Connected, Trigger.LeftMessage, State.OffHook);
            AddTransition(State.Connected, Trigger.HungUp, State.OffHook);
            this[State.Connected].ExitHandler += StopCallTimer;
            SetupSubstates(State.Connected, HistoryType.None, State.Talking, State.OnHold);

            AddTransition(State.Talking, Trigger.PlacedOnHold, State.OnHold);

            AddTransition(State.OnHold, Trigger.TakenOffHold, State.Talking);
            AddTransition(State.OnHold, Trigger.PhoneHurledAgainstWall, State.PhoneDestroyed);

            Initialize(initialState);
        }

        static void Main(string[] args)
        {
            var phoneCall = new TelephoneStateMachine(State.OffHook);

            Print(phoneCall);
            Fire(phoneCall, Trigger.CallDialed);
            Print(phoneCall);
            Fire(phoneCall, Trigger.CallConnected);
            Print(phoneCall);
            Fire(phoneCall, Trigger.PlacedOnHold);
            Print(phoneCall);
            Fire(phoneCall, Trigger.TakenOffHold);
            Print(phoneCall);
            Fire(phoneCall, Trigger.HungUp);
            Print(phoneCall);

            Console.WriteLine("Press any key...");
            //Console.ReadKey(true);
        }

        static void main2(string[] args)
        {
            var phoneCall = new StateMachineAdapter<State, Trigger>(State.OffHook);

            phoneCall.Configure(State.OffHook)
                    .Allow(Trigger.CallDialed, State.Ringing);

            phoneCall.Configure(State.Ringing)
                    .Allow(Trigger.HungUp, State.OffHook)
                    .Allow(Trigger.CallConnected, State.Connected);

            phoneCall.Configure(State.Connected)
                .OnEntry(StartCallTimer)
                .OnExit(StopCallTimer)
                .Allow(Trigger.LeftMessage, State.OffHook)
                .Allow(Trigger.HungUp, State.OffHook)
                .SuperstateOf(State.Talking, State.OnHold);

            phoneCall.Configure(State.Talking)
                //.SubstateOf(State.Connected)
                    .Allow(Trigger.PlacedOnHold, State.OnHold);

            phoneCall.Configure(State.OnHold)
                //.SubstateOf(State.Connected)
                .Allow(Trigger.TakenOffHold, State.Talking)
                .Allow(Trigger.PhoneHurledAgainstWall, State.PhoneDestroyed);

            Print(phoneCall);
            Fire(phoneCall, Trigger.CallDialed);
            Print(phoneCall);
            Fire(phoneCall, Trigger.CallConnected);
            Print(phoneCall);
            Fire(phoneCall, Trigger.PlacedOnHold);
            Print(phoneCall);
            Fire(phoneCall, Trigger.TakenOffHold);
            Print(phoneCall);
            Fire(phoneCall, Trigger.HungUp);
            Print(phoneCall);

            Console.WriteLine("Press any key...");
            //Console.ReadKey(true);
        }

        private static void Fire(StateMachineAdapter<State, Trigger> phoneCall, Trigger trigger)
        {
            Console.WriteLine("[Firing:] {0}", trigger);
            phoneCall.Fire(trigger);
        }

        private static void Print(StateMachineAdapter<State, Trigger> phoneCall)
        {
            Console.WriteLine("[Status:] {0}", phoneCall);
        }


        static void StartCallTimer(object sender, TransitionEventArgs<State, Trigger, EventArgs> e)
        {
            Console.WriteLine("[Timer:] Call started at {0}", DateTime.Now);
        }

        static void StopCallTimer(object sender, TransitionEventArgs<State, Trigger, EventArgs> e)
        {
            Console.WriteLine("[Timer:] Call ended at {0}", DateTime.Now);
        }

        static void Fire(IPassiveStateMachine<State, Trigger, EventArgs> phoneCall, Trigger trigger)
        {
            Console.WriteLine("[Firing:] {0}", trigger);
            phoneCall.Send(trigger);
            phoneCall.Execute();
        }

        static void Print(IStateMachine<State, Trigger, EventArgs> phoneCall)
        {
            Console.WriteLine("[Status:] {0}", phoneCall.CurrentStateID);
        }
    }

    public class StateMachineAdapter<TState, TTrigger>
    {
        private readonly TestMachine<TState, TTrigger, EventArgs> m_machine;
        public interface IStateConfiguration
        {
            IStateConfiguration Allow(TTrigger trigger, TState destinationState);
            IStateConfiguration OnEntry(EventHandler<TransitionEventArgs<TState,TTrigger,EventArgs>> entryAction);
            IStateConfiguration OnExit(EventHandler<TransitionEventArgs<TState, TTrigger, EventArgs>> exitAction);
            IStateConfiguration SuperstateOf(TState initialState, params TState[] substates);
        }

        public override string ToString()
        {
            return m_machine.CurrentStateID.ToString();
        }

        private sealed class StateConfiguration : IStateConfiguration
        {
            private readonly TestMachine<TState, TTrigger, EventArgs> m_machine;
            private readonly TState m_state;

            public StateConfiguration(TestMachine<TState, TTrigger, EventArgs> machine, TState state)
            {
                m_machine = machine;
                m_state = state;
            }

            public IStateConfiguration Allow(TTrigger trigger, TState destinationState)
            {
                m_machine.AddTransition(m_state, trigger, destinationState);
                return this;
            }

            public IStateConfiguration OnEntry(EventHandler<TransitionEventArgs<TState, TTrigger, EventArgs>> entryAction)
            {
                m_machine[m_state].EntryHandler += entryAction;
                return this;
            }

            public IStateConfiguration OnExit(EventHandler<TransitionEventArgs<TState, TTrigger, EventArgs>> exitAction)
            {
                m_machine[m_state].ExitHandler += exitAction;
                return this;
            }

            public IStateConfiguration SuperstateOf(TState initialState, params TState[] substates)
            {
                m_machine.SetupSubstates(m_state, HistoryType.None, initialState, substates);
                return this;
            }
        }

        public class Transition
        {
                
        }

        public StateMachineAdapter(TState state)
        {
            m_machine = new TestMachine<TState, TTrigger, EventArgs>();
            m_machine.Start(state);
        }

        public IStateConfiguration Configure(TState state)
        {
            return new StateConfiguration(m_machine, state);
        }

        public void Fire(TTrigger trigger)
        {
            m_machine.Send(trigger);
            m_machine.Execute();
        }
    }
}