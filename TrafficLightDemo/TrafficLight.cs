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
			on = new State<StateID, EventID>(StateID.On, (EntryHandler)EntryOn);
			off = new State<StateID, EventID>(StateID.Off, (EntryHandler)EntryOff);
			red = new State<StateID, EventID>(StateID.Red, (EntryHandler)EntryRed);
			yellow = new State<StateID, EventID>(StateID.Yellow, (EntryHandler)EntryYellow);
			green = new State<StateID, EventID>(StateID.Green, (EntryHandler)EntryGreen);
			disposed = new State<StateID, EventID>(StateID.Disposed, (EntryHandler)EntryDisposed);

			on.Substates.Add(red);
			on.Substates.Add(yellow);
			on.Substates.Add(green);

			on.InitialState = red;

			on.HistoryType = HistoryType.Shallow;

			Transition<StateID, EventID> trans = Transition.Create(off);
			on.Transitions.Add(EventID.TurnOff, trans);

			trans = Transition.Create(on);
			off.Transitions.Add(EventID.TurnOn, trans);

			trans = Transition.Create(green);
			red.Transitions.Add(EventID.TimerElapsed, trans);

			trans = Transition.Create(yellow);
			green.Transitions.Add(EventID.TimerElapsed, trans);

			trans = Transition.Create(red);
			yellow.Transitions.Add(EventID.TimerElapsed, trans);

			trans = Transition.Create(disposed);
			off.Transitions.Add(EventID.Dispose, trans);
			trans = Transition.Create(disposed);
			on.Transitions.Add(EventID.Dispose, trans);

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

			Send((int) EventID.Dispose);
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