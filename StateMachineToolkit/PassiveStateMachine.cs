using System;
using Sanford.Collections.Generic;

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// Unlike the <see cref="ActiveStateMachine{TState,TEvent}"/> class, 
	/// the PassiveStateMachine class does not run in its  own thread. Sometimes using an active 
	/// object is overkill. In those cases, it is  appropriate to derive your state machine from 
	/// the PassiveStateMachine class.<para/>
	/// Because the PassiveStateMachine is, well, passive, it has to be prodded to 
	/// fire its transitions. You do this by calling its <see cref="Execute"/> method. After sending a 
	/// PassiveStateMachine derived class one or more events, you then call <see cref="Execute"/>. 
	/// The state machine responds by dequeueing all of the events in its event queue, 
	/// dispatching them one right after the other. 
	/// </summary>
	/// <typeparam name="TState">The state enumeration type.</typeparam>
	/// <typeparam name="TEvent">The event enumeration type.</typeparam>
	public abstract class PassiveStateMachine<TState, TEvent> : StateMachine<TState, TEvent>
		where TState : struct, IComparable, IFormattable /*, IConvertible*/
		where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
	{
		private readonly Deque<StateMachineEvent> eventDeque = new Deque<StateMachineEvent>();

		/// <summary>
		/// Initializes the StateMachine's initial state.
		/// </summary>
		/// <param name="initialState">The state that will initially receive events from the StateMachine.</param>
		protected override void Initialize(State initialState)
		{
			InitializeStateMachine(initialState);
		}

		/// <summary>
		/// Executes pending events.
		/// </summary>
		public void Execute()
		{
			StateMachineEvent e;

			while (eventDeque.Count > 0)
			{
				e = eventDeque.PopFront();

				Dispatch(e.EventID, e.GetArgs());
			}
		}

		/// <summary>
		/// Sends an event to the state machine, that might trigger a transition.
		/// </summary>
		/// <param name="eventID">The event.</param>
		/// <param name="args">Optional event arguments.</param>
		public override void Send(TEvent eventID, object[] args)
		{
			AssertMachineIsValid();
			eventDeque.PushBack(new StateMachineEvent(eventID, args));
		}

		/// <summary>
		/// Sends an event to the state machine, that might trigger a transition.
		/// This event will have precedence over other pending events that were sent using
		/// the <see cref="Send"/> method.
		/// </summary>
		/// <param name="eventID">The event.</param>
		/// <param name="args">Optional event arguments.</param>
		protected override void SendPriority(TEvent eventID, object[] args)
		{
			AssertMachineIsValid();
			eventDeque.PushFront(new StateMachineEvent(eventID, args));
		}

		/// <summary>
		/// Template method for handling dispatch exceptions.
		/// </summary>
		/// <param name="ex">The exception.</param>
		protected override void HandleDispatchException(Exception ex)
		{
			OnExceptionThrown(new TransitionErrorEventArgs<TState, TEvent>(m_currentEventContext, ex));
			//throw new InvalidOperationException("Exception was thrown during dispatch.", ex);
		}

		/// <summary>
		/// Gets the state machine type: active or passive.
		/// </summary>
		public override StateMachineType StateMachineType
		{
			get { return StateMachineType.Passive; }
		}

		/// <summary>
		/// Encapsulates an event that was sent to the state machine.
		/// </summary>
		private class StateMachineEvent
		{
			private readonly TEvent eventID;

			private readonly object[] args;

			/// <summary>
			/// Initializes a new instance of the <see cref="StateMachineEvent"/> class.
			/// </summary>
			/// <param name="eventID">The event ID.</param>
			/// <param name="args">The event arguments.</param>
			public StateMachineEvent(TEvent eventID, object[] args)
			{
				this.eventID = eventID;
				this.args = args;
			}

			/// <summary>
			/// Gets the event arguments.
			/// </summary>
			/// <returns>The event arguments.</returns>
			public object[] GetArgs()
			{
				return args;
			}

			/// <summary>
			/// Gets the event ID.
			/// </summary>
			/// <value>The event ID.</value>
			public TEvent EventID
			{
				get { return eventID; }
			}
		}
	}
}