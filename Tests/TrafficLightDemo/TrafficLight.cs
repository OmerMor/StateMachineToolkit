using System;
using System.ComponentModel;
using Sanford.StateMachineToolkit;
using Sanford.Threading;

namespace TrafficLightDemo
{
	public class TrafficLight : PassiveStateMachine<StateID, EventID>, IDisposable
	{
		private readonly AsyncOperation operation = AsyncOperationManager.CreateOperation(null);

		private readonly DelegateScheduler scheduler = new DelegateScheduler();

		private bool isDisposed;

		public TrafficLight()
		{
			States[StateID.On].EntryHandler += EntryOn;
			States[StateID.Off].EntryHandler += EntryOff;
			States[StateID.Red].EntryHandler += EntryRed;
			States[StateID.Yellow].EntryHandler += EntryYellow;
			States[StateID.Green].EntryHandler += EntryGreen;
			States[StateID.Disposed].EntryHandler += EntryDisposed;

			SetupSubstates(StateID.On, HistoryType.Shallow, StateID.Red, StateID.Yellow, StateID.Green);

			AddTransition(StateID.On, EventID.TurnOff, StateID.Off);
			AddTransition(StateID.Off, EventID.TurnOn, StateID.On);
			AddTransition(StateID.Red, EventID.TimerElapsed, StateID.Green);
			AddTransition(StateID.Green, EventID.TimerElapsed, StateID.Yellow);
			AddTransition(StateID.Yellow, EventID.TimerElapsed, StateID.Red);
			AddTransition(StateID.Off, EventID.Dispose, StateID.Disposed);
			AddTransition(StateID.On, EventID.Dispose, StateID.Disposed);

			Initialize(StateID.Off);
		}

		#region Entry/Exit Methods

		private void EntryOn()
		{
			scheduler.Start();
		}

		private void EntryOff()
		{
			scheduler.Stop();
			scheduler.Clear();
		}

		private void EntryRed()
		{
			scheduler.Add(1, 5000, new SendTimerDelegate(SendTimerEvent));
		}

		private void EntryYellow()
		{
			scheduler.Add(1, 2000, new SendTimerDelegate(SendTimerEvent));
		}

		private void EntryGreen()
		{
			scheduler.Add(1, 5000, new SendTimerDelegate(SendTimerEvent));
		}

		private void EntryDisposed()
		{
			scheduler.Dispose();

			operation.OperationCompleted();

			isDisposed = true;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			#region Guard

			if (isDisposed)
			{
				return;
			}

			#endregion

			Send(EventID.Dispose);
		}

		#endregion

		private void SendTimerEvent()
		{
			operation.Post(delegate
			               	{
			               		Send(EventID.TimerElapsed);
			               		Execute();
			               	}, null);
		}

		#region Nested type: SendTimerDelegate

		private delegate void SendTimerDelegate();

		#endregion
	}

	public enum StateID
	{
		On,
		Off,
		Red,
		Yellow,
		Green,
		Disposed
	}

	public enum EventID
	{
		Dispose,
		TurnOn,
		TurnOff,
		TimerElapsed
	}
}