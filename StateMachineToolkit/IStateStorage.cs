using System;

namespace Sanford.StateMachineToolkit
{
    public delegate R Func<R>();

    public interface IStateStorage<T>
    {
        T Value { get; set; }
    }

    public sealed class InternalStateStorage<T> : IStateStorage<T>
    {
        public InternalStateStorage(T value)
        {
            m_value = value;
        }

        public InternalStateStorage()
        {
        }

        private T m_value;
        public T Value
        {
            get { return m_value; }
            set { m_value = value; }
        }
    }

    public sealed class ExternalStateStorage<T> : IStateStorage<T>
    {
        private readonly Func<T> m_getState;
        private readonly Action<T> m_setState;

        public ExternalStateStorage(Func<T> getState, Action<T> setState)
        {
            m_getState = getState;
            m_setState = setState;
        }

        public T Value
        {
            get { return m_getState(); }
            set { m_setState(value); }
        }
    }


}