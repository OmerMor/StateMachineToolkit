using System;
using System.ComponentModel;
using Sanford.StateMachineToolkit;
using Sanford.Threading;

namespace TrafficLightDemo
{
	public class TrafficLight : PassiveStateMachine<StateID, EventID>, IDisposable
	{
		private readonly State<StateID, EventID> disposed;
		private readonly State<StateID, EventID> off;
		private readonly State<StateID, EventID> on;
		private readonly State<StateID, EventID> green;
		private readonly State<StateID, EventID> yellow;
		private readonly State<StateID, EventID> red;
		private readonly AsyncOperation operation = AsyncOperationManager.CreateOperation(null);

		private readonly DelegateScheduler scheduler = new DelegateScheduler();

		private bool isDisposed;

		public TrafficLight()
		{
			on = CreateState(StateID.On, EntryOn, null);
			off = CreateState(StateID.Off, EntryOff, null);
			red = CreateState(StateID.Red, EntryRed, null);
			yellow = CreateState(StateID.Yellow, EntryYellow, null);
			green = CreateState(StateID.Green, EntryGreen, null);
			disposed = CreateState(StateID.Disposed, EntryDisposed, null);

			on.Substates.Add(red);
			on.Substates.Add(yellow);
			on.Substates.Add(green);

			on.InitialState = red;

			on.HistoryType = HistoryType.Shallow;

			on.Transitions.Add(EventID.TurnOff, off);

			off.Transitions.Add(EventID.TurnOn, off);

			red.Transitions.Add(EventID.TimerElapsed, green);

			green.Transitions.Add(EventID.TimerElapsed, yellow);

			yellow.Transitions.Add(EventID.TimerElapsed, red);

			off.Transitions.Add(EventID.Dispose, disposed);

			on.Transitions.Add(EventID.Dispose, disposed);

			Initialize(off);
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