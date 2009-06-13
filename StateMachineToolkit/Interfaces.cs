using System;
using Sanford.Threading;

namespace Sanford.StateMachineToolkit
{
    /// <summary>
    /// Represents the base class for all state machines. You do not derive your state machine classes from this 
    /// class but rather from one  of its derived classes, either the <see cref="T:Sanford.StateMachineToolkit.ActiveStateMachine`2"/> 
    /// class or the <see cref="T:Sanford.StateMachineToolkit.PassiveStateMachine`2"/> class.
    /// </summary>
    /// <typeparam name="TState">The state enumeration type.</typeparam>
    /// <typeparam name="TEvent">The event enumeration type.</typeparam>
    public interface IStateMachine<TState, TEvent, TArgs>
        //where TState : struct, IComparable, IFormattable
        //where TEvent : struct, IComparable, IFormattable
        //where TArgs : EventArgs
    {
        /// <summary>
        /// Occurs before a dispatch starts.
        /// </summary>
        event EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> BeginDispatch;

        /// <summary>
        /// Occurs before a transition starts.
        /// </summary>
        event EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> BeginTransition;

        /// <summary>
        /// Occurs when an exception is thrown.
        /// </summary>
        event EventHandler<TransitionErrorEventArgs<TState, TEvent, TArgs>> ExceptionThrown;

        /// <summary>
        /// Occurs after a transition is completed.
        /// </summary>
        event EventHandler<TransitionCompletedEventArgs<TState, TEvent, TArgs>> TransitionCompleted;

        /// <summary>
        /// Occurs when a transition is declined.
        /// </summary>
        event EventHandler<TransitionEventArgs<TState, TEvent, TArgs>> TransitionDeclined;

        /// <summary>
        /// Gets the ID of the current state.
        /// </summary>
        TState CurrentStateID { get; }

        /// <summary>
        /// Sends an event to the state machine, that might trigger a transition.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="args">Optional event arguments.</param>
        void Send(TEvent eventId, TArgs args);

        void Send(TEvent eventId);
    }

    /// <summary>
    /// Represents a passive state machine.
    /// </summary>
    public interface IPassiveStateMachine
    {
        /// <summary>
        /// Executes pending events.
        /// </summary>
        void Execute();
    }

    /// <summary>
    /// Unlike the <see cref="ActiveStateMachine{TState,TEvent}"/> class, 
    /// the PassiveStateMachine class does not run in its own thread. Sometimes using an active 
    /// object is overkill. In those cases, it is  appropriate to derive your state machine from 
    /// the PassiveStateMachine class.<para/>
    /// Because the PassiveStateMachine is, well, passive, it has to be prodded to 
    /// fire its transitions. You do this by calling its <see cref="IPassiveStateMachine.Execute"/> method. After sending a 
    /// PassiveStateMachine derived class one or more events, you then call <see cref="IPassiveStateMachine.Execute"/>. 
    /// The state machine responds by dequeueing all of the events in its event queue, 
    /// dispatching them one right after the other. 
    /// </summary>
    /// <typeparam name="TState">The state enumeration type.</typeparam>
    /// <typeparam name="TEvent">The event enumeration type.</typeparam>
    public interface IPassiveStateMachine<TState, TEvent, TArgs> : IStateMachine<TState, TEvent, TArgs>, IPassiveStateMachine
        //where TArgs : EventArgs
        //where TState : struct, IComparable, IFormattable /*, IConvertible*/
        //where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
    {
    }

    /// <summary>
    /// Represents an active state machine.
    /// </summary>
    public interface IActiveStateMachine
    {
        /// <summary>
        /// Waits for pending events.
        /// </summary>
        void WaitForPendingEvents();
    }

    /// <summary>
    /// The ActiveStateMachine class uses the Active Object design pattern. 
    /// What this means is that an ActiveStateMachine object runs in its own thread. 
    /// Internally, ActiveStateMachines use <see cref="DelegateQueue"/> objects for handling 
    /// and dispatching events. 
    /// </summary>
    /// <typeparam name="TState">The state enumeration type.</typeparam>
    /// <typeparam name="TEvent">The event enumeration type.</typeparam>
    public interface IActiveStateMachine<TState, TEvent, TArgs> : IStateMachine<TState, TEvent, TArgs>, IActiveStateMachine
        //where TArgs : EventArgs
        //where TState : struct, IComparable, IFormattable
        //where TEvent : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Sends an event to the StateMachine, and blocks until it processing ends.
        /// </summary>
        /// <param name="eventId">
        /// The event ID.
        /// </param>
        /// <param name="args">
        /// The data accompanying the event.
        /// </param>
        void SendSynchronously(TEvent eventId, TArgs args);

        void SendSynchronously(TEvent eventId);
    }
}