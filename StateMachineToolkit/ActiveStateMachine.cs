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
    public abstract class ActiveStateMachine<TState, TEvent> : StateMachine<TState, TEvent>, IDisposable, IActiveStateMachine<TState, TEvent> 
        //where TState : struct, IComparable, IFormattable /*, IConvertible*/
        //where TEvent : struct, IComparable, IFormattable /*, IConvertible*/
    {
        #region Fields

        /// <summary>
        /// The synchronization context, for executing callbacks on the origin thread.
        /// </summary>
        private readonly SynchronizationContext m_syncContext;

        /// <summary>
        /// Used for queuing events.
        /// </summary>
        private readonly DelegateQueue m_queue = new DelegateQueue();

        /// <summary>
        /// Indicates whether the instance was already disposed.
        /// </summary>
        private bool m_isDisposed;

        /// <summary>
        /// Indicates whether the current event was sent synchronously.
        /// </summary>
        private bool m_synchronousInvocation;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveStateMachine{TState, TEvent}"/> class.
        /// </summary>
        protected ActiveStateMachine()
            : this(SynchronizationContext.Current ?? new NullSynchronizationContext())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveStateMachine{TState, TEvent}"/> class.
        /// </summary>
        /// <param name="syncContext">The synchronization context.</param>
        protected ActiveStateMachine(SynchronizationContext syncContext)
        {
            m_syncContext = syncContext;
            m_queue.PostCompleted +=
                delegate(object sender, PostCompletedEventArgs args)
                    {
                        if (args.Error == null) return;
                        OnExceptionThrown(new TransitionErrorEventArgs<TState, TEvent>(
                                              CurrentEventContext, args.Error));
                    };
            m_queue.InvokeCompleted +=
                delegate(object sender, InvokeCompletedEventArgs args)
                    {
                        if (args.Error == null) return;
                        OnExceptionThrown(new TransitionErrorEventArgs<TState, TEvent>(
                                              CurrentEventContext, args.Error));
                    };
        }

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        protected bool IsDisposed
        {
            get { return m_isDisposed; }
        }

        /// <summary>
        /// The synchronization context, for executing callbacks on the origin thread.
        /// </summary>
        protected SynchronizationContext SyncContext
        {
            get { return m_syncContext; }
        }

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
        /// <param name="eventID">
        /// The event ID.
        /// </param>
        /// <param name="args">
        /// The data accompanying the event.
        /// </param>
        public override void Send(TEvent eventID, params object[] args)
        {
            AssertMachineIsValid();
            m_queue.Post(delegate { Dispatch(eventID, args); }, null);
        }

        /// <summary>
        /// Sends an event to the StateMachine, and blocks until it processing ends.
        /// </summary>
        /// <param name="eventID">
        /// The event ID.
        /// </param>
        /// <param name="args">
        /// The data accompanying the event.
        /// </param>
        public void SendSynchronously(TEvent eventID, params object[] args)
        {
            AssertMachineIsValid();
            m_queue.Send(
                delegate
                    {
                        m_synchronousInvocation = true;
                        try
                        {
                            Dispatch(eventID, args);
                        }
                        finally
                        {
                            m_synchronousInvocation = false;
                        }
                    },
                null);
        }

        /// <summary>
        /// Waits for pending events.
        /// </summary>
        public void WaitForPendingEvents()
        {
            SendOrPostCallback nop = delegate { };
            m_queue.Send(nop, null);
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

            m_isDisposed = true;
            m_queue.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the StateMachine's initial state.
        /// </summary>
        /// <param name="initialState">The state that will initially receive events from the StateMachine.</param>
        protected override void Initialize(State initialState)
        {
            Exception initException = null;
            m_queue.Send(
                delegate
                    {
                        try
                        {
                            InitializeStateMachine(initialState);
                        }
                        catch (Exception ex)
                        {
                            initException = ex;
                        }
                    },
                null);

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
            OnExceptionThrown(new TransitionErrorEventArgs<TState, TEvent>(CurrentEventContext, ex));
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent}.BeginDispatch"/> event.
        /// </summary>
        /// <param name="eventContext">The event context.</param>
        protected override void OnBeginDispatch(EventContext eventContext)
        {
            if (m_synchronousInvocation)
            {
                base.OnBeginDispatch(eventContext);
            }
            else
            {
                synchronizedSend(base.OnBeginDispatch, eventContext);
            }
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent}.BeginTransition"/> event.
        /// </summary>
        /// <param name="eventContext">The event context.</param>
        protected override void OnBeginTransition(EventContext eventContext)
        {
            if (m_synchronousInvocation)
            {
                base.OnBeginTransition(eventContext);
            }
            else
            {
                synchronizedSend(base.OnBeginTransition, eventContext);
            }
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent}.ExceptionThrown"/> event.
        /// </summary>
        /// <param name="args">The <see cref="TransitionErrorEventArgs{TState,TEvent}"/> instance containing the event data.</param>
        protected override void OnExceptionThrown(TransitionErrorEventArgs<TState, TEvent> args)
        {
            synchronizedPost(base.OnExceptionThrown, args);
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent}.TransitionCompleted"/> event.
        /// </summary>
        /// <param name="args">The <see cref="TransitionCompletedEventArgs{TState,TEvent}"/> instance
        /// containing the event data.</param>
        protected override void OnTransitionCompleted(TransitionCompletedEventArgs<TState, TEvent> args)
        {
            synchronizedPost(base.OnTransitionCompleted, args);
        }

        /// <summary>
        /// Raises the <see cref="StateMachine{TState,TEvent}.TransitionDeclined"/> event.
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
        /// <param name="eventID">The event.</param>
        /// <param name="args">Optional event arguments.</param>
        protected override void SendPriority(TEvent eventID, params object[] args)
        {
            AssertMachineIsValid();
            m_queue.PostPriority(delegate { Dispatch(eventID, args); }, null);
        }

        private void synchronizedSend<T>(Action<T> action, T arg)
        {
            SyncContext.Send(delegate { action(arg); }, null);
        }
        private void synchronizedPost<T>(Action<T> action, T arg)
        {
            SyncContext.Post(delegate { action(arg); }, null);
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