// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable VirtualMemberNeverOverriden.Global

using System.Collections.Generic;

#if NET40
using System.Threading.Tasks;
#endif

namespace Sanford.StateMachineToolkit
{
    using System;
    using System.Threading;
    using Threading;

    /// <summary>
    /// The ActiveStateMachine class uses the Active Object design pattern. 
    /// What this means is that an ActiveStateMachine object runs in its own thread. 
    /// Internally, ActiveStateMachines use <see cref="DelegateQueue"/> objects for handling 
    /// and dispatching events. 
    /// You derive your state machines from this class when you want them to be active objects.<para/>
    /// The ActiveStateMachine class implements the <see cref="IDisposable"/> interface. 
    /// Since it represents an  active object, it needs to be disposed of at some point to 
    /// shut its thread down. 
    /// The Dispose method was made virtual so that derived ActiveStateMachine classes can override it. 
    /// Typically, a derived ActiveStateMachine will override the Dispose method, and when it is called, 
    /// will send an event to itself using the <see cref="SendPriority"/> method telling it to dispose of itself. 
    /// In other words, disposing of an ActiveStateMachine is treated like an event. 
    /// How your state machine handles the disposing event depends on its current state. 
    /// However, at some point, your state machine will need to call the ActiveStateMachine's 
    /// <see cref="Dispose(bool)"/> base class method, passing it a true value. 
    /// This lets the base class dispose of its <see cref="DelegateQueue"/> object, thus shutting down the 
    /// thread in which it is running.
    /// </summary>
    /// <typeparam name="TState">The state enumeration type.</typeparam>
    /// <typeparam name="TEvent">The event enumeration type.</typeparam>
    /// <typeparam name="TArgs">The event arguments type.</typeparam>
    public abstract class ActiveStateMachine<TState, TEvent, TArgs> : StateMachine<TState, TEvent, TArgs>, IDisposable, IActiveStateMachine<TState, TEvent, TArgs> 
        //where TArgs : EventArgs 
        //where TState : struct, IComparable, IFormattable /*, IConvertible*/
        //where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
    {
        #region Fields

        /// <summary>
        /// Used for queuing events.
        /// </summary>
        private DelegateQueue queue { get; set; }

        /// <summary>
        /// Indicates whether the current event was sent synchronously.
        /// </summary>
        private bool synchronousInvocation { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveStateMachine{TState,TEvent,TArgs}"/> class.
        /// </summary>
        protected ActiveStateMachine(IEqualityComparer<TEvent> comparer = null, IStateStorage<TState> stateStorage = null)
            : this(defaultSynchronizationContext, comparer, stateStorage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveStateMachine{TState,TEvent,TArgs}"/> class.
        /// </summary>
        /// <param name="syncContext">The synchronization context.</param>
        /// <param name="comparer"> </param>
        /// <param name="stateStorage"> </param>
        protected ActiveStateMachine(SynchronizationContext syncContext, IEqualityComparer<TEvent> comparer = null, IStateStorage<TState> stateStorage = null)
            : base(comparer, stateStorage)
        {
            SyncContext = syncContext;
            queue = new DelegateQueue();
            queue.PostCompleted += raiseExceptionEventOnError;
            queue.InvokeCompleted += raiseExceptionEventOnError;
        }

        private void raiseExceptionEventOnError(object sender, CompletedEventArgs args)
        {
            if (args.Error == null) return;
            OnExceptionThrown(new TransitionErrorEventArgs<TState, TEvent, TArgs>(CurrentEventContext, args.Error));
        }

        #region Properties

        /// <summary>
        /// Gets the ID of the current state.
        /// </summary>
        /// <remarks>Thread safe.</remarks>
        /// <value></value>
        public override TState CurrentStateID
        {
            get 
            {
                var currentState = default(TState);
                SendOrPostCallback fetchState = state => currentState = base.CurrentStateID;
                queue.Send(fetchState, null);
                return currentState;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        protected bool IsDisposed { get; private set; }

        /// <summary>
        /// The synchronization context, for executing callbacks on the origin thread.
        /// </summary>
        protected SynchronizationContext SyncContext { get; private set; }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose(true);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends an event to the StateMachine.
        /// </summary>
        /// <param name="eventId">
        /// The event ID.
        /// </param>
        /// <param name="args">
        /// The data accompanying the event.
        /// </param>
        public override void Send(TEvent eventId, TArgs args = default (TArgs))
        {
            AssertMachineIsValid();
            queue.Post(state => Dispatch(eventId, args), null);
        }

        /// <summary>
        /// Sends an event to the StateMachine, and blocks until it processing ends.
        /// </summary>
        /// <param name="eventId">
        /// The event ID.
        /// </param>
        /// <param name="args">
        /// The data accompanying the event.
        /// </param>
        public void SendSynchronously(TEvent eventId, TArgs args)
        {
            AssertMachineIsValid();
            queue.Send(state =>
            {
                synchronousInvocation = true;
                try
                {
                    Dispatch(eventId, args);
                }
                finally
                {
                    synchronousInvocation = false;
                }
            }, null);
        }

#if NET40        
        public Task<TState> SendAsync(TEvent eventId, TArgs args)
        {
            AssertMachineIsValid();
            var tcs = new TaskCompletionSource<TState>();
            queue.Post(state =>
            {
                Dispatch(eventId, args);
                tcs.SetResult(CurrentStateID);
            }, null);
            return tcs.Task;
        }
#endif

        /// <summary>
        /// Sends an event to the StateMachine, and blocks until it processing ends.
        /// </summary>
        /// <param name="eventId">
        /// The event ID.
        /// </param>
        public void SendSynchronously(TEvent eventId)
        {
            SendSynchronously(eventId, default(TArgs));
        }


        /// <summary>
        /// Waits for pending events.
        /// </summary>
        public void WaitForPendingEvents()
        {
            SendOrPostCallback nop = delegate { };
            queue.Send(nop, null);
        }

        /// <summary>
        /// Asserts that the state machine was initialized and not disposed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the state machine was not initialized.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the state machine was already disposed.</exception>"
        protected override void AssertMachineIsValid()
        {
            base.AssertMachineIsValid();
            if (IsDisposed)
            {
                throw new ObjectDisposedException(
                    string.Format("ActiveStateMachine<{0},{1}>", typeof(TState).Name, typeof(TEvent).Name),
                    "State machine was already disposed.");
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            IsDisposed = true;
            queue.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the StateMachine's initial state.
        /// </summary>
        /// <param name="initialState">The state that will initially receive events from the StateMachine.</param>
        protected override void Initialize(TState initialState)
        {
            Exception initException = null;
            queue.Send(state =>
                {
                    try
                    {
                        InitializeStateMachine(initialState);
                    }
                    catch (Exception ex)
                    {
                        initException = ex;
                    }
                }, null);

            if (initException != null)
            {
                throw new InvalidOperationException("State machine failed to initialize.", initException);
            }
        }

        /// <summary>
        /// Template method for handling dispatch exceptions.
        /// </summary>
        /// <param name="ex">The exception.</param>
        protected override void HandleDispatchException(Exception ex)
        {
            OnExceptionThrown(new TransitionErrorEventArgs<TState, TEvent, TArgs>(CurrentEventContext, ex));
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent,TArgs}.BeginDispatch"/> event.
        /// </summary>
        /// <param name="eventContext">The event context.</param>
        protected override void OnBeginDispatch(EventContext eventContext)
        {
            if (synchronousInvocation)
            {
                base.OnBeginDispatch(eventContext);
            }
            else
            {
                synchronizedSend(base.OnBeginDispatch, eventContext);
            }
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent,TArgs}.BeginTransition"/> event.
        /// </summary>
        /// <param name="eventContext">The event context.</param>
        protected override void OnBeginTransition(EventContext eventContext)
        {
            if (synchronousInvocation)
            {
                base.OnBeginTransition(eventContext);
            }
            else
            {
                synchronizedSend(base.OnBeginTransition, eventContext);
            }
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent,TArgs}.ExceptionThrown"/> event.
        /// </summary>
        /// <param name="args">The <see cref="TransitionErrorEventArgs{TState,TEvent,TArgs}"/> instance containing the event data.</param>
        protected override void OnExceptionThrown(TransitionErrorEventArgs<TState, TEvent, TArgs> args)
        {
            synchronizedPost(base.OnExceptionThrown, args);
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent,TArgs}.TransitionCompleted"/> event.
        /// </summary>
        /// <param name="args">The <see cref="TransitionCompletedEventArgs{TState,TEvent,TArgs}"/> instance
        /// containing the event data.</param>
        protected override void OnTransitionCompleted(TransitionCompletedEventArgs<TState, TEvent, TArgs> args)
        {
            synchronizedPost(base.OnTransitionCompleted, args);
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent,TArgs}.TransitionDeclined"/> event.
        /// </summary>
        /// <param name="eventContext">The event context.</param>
        protected override void OnTransitionDeclined(EventContext eventContext)
        {
            synchronizedPost(base.OnTransitionDeclined, eventContext);
        }

        /// <summary>
        /// Sends an event to the state machine, that might trigger a transition.
        /// This event will have precedence over other pending events that were sent using
        /// the <see cref="Send"/> method.
        /// </summary>
        /// <param name="eventId">The event.</param>
        /// <param name="args">Optional event arguments.</param>
        protected override void SendPriority(TEvent eventId, TArgs args)
        {
            AssertMachineIsValid();
            queue.PostPriority(state => Dispatch(eventId, args), null);
        }

        private static SynchronizationContext defaultSynchronizationContext
        {
            get { return SynchronizationContext.Current ?? new NullSynchronizationContext(); }
        }

        private void synchronizedSend<T>(Action<T> action, T arg)
        {
            SyncContext.Send(state => action(arg), null);
        }
        private void synchronizedPost<T>(Action<T> action, T arg)
        {
            SyncContext.Post(state => action(arg), null);
        }

        #endregion
    }

    /// <summary>
    /// An empty implementation of <see cref="SynchronizationContext"/>
    /// which always invokes the delegates on the calling thread.
    /// </summary>
    public sealed class NullSynchronizationContext : SynchronizationContext
    {
        /// <summary>
        /// Creates a copy of the synchronization context.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Threading.SynchronizationContext"/> object.
        /// </returns>
        public override SynchronizationContext CreateCopy()
        {
            return this;
        }

        /// <summary>
        /// Dispatches a synchronous message to a synchronization context.
        /// </summary>
        /// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback"/> delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Send(SendOrPostCallback d, object state)
        {
            d(state);
        }

        /// <summary>
        /// Dispatches an asynchronous message to a synchronization context.
        /// </summary>
        /// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback"/> delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Post(SendOrPostCallback d, object state)
        {
            d(state);
        }
    }
}

// ReSharper enable MemberCanBePrivate.Global
// ReSharper enable VirtualMemberNeverOverriden.Global
