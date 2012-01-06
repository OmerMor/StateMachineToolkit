using System;

namespace Sanford.StateMachineToolkit
{
    /// <summary>
    /// A base non-generic active state machine.
    /// </summary>
    public class ActiveStateMachine<TArgs> : ActiveStateMachine<int, int, TArgs> 
        //where TArgs : EventArgs
    {
    }

    /// <summary>
    /// A base non-generic passive state machine.
    /// </summary>
    public class PassiveStateMachine<TArgs> : PassiveStateMachine<int, int, TArgs> 
        //where TArgs : EventArgs
    {
    }
}