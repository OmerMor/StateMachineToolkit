using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using Sanford.StateMachineToolkit;
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

    public sealed class TraficLightStateMachine : ActiveStateMachine<TraficLightStates, TraficLightEvents>
    {
        private readonly Timer m_timer;
        private static readonly TimeSpan INTERVAL = TimeSpan.FromSeconds(2);
        public TraficLightStateMachine(IStateStorage<TraficLightStates> stateStorage)
            : base(stateStorage)
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

}