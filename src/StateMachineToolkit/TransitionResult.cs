#region License

/* Copyright (c) 2006 Leslie Sanford
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 */

#endregion

#region Contact

/*
 * Leslie Sanford
 * Email: jabberdabber@hotmail.com
 */

#endregion

using System;

namespace Sanford.StateMachineToolkit
{
    public abstract partial class StateMachine<TState, TEvent, TArgs>
    {
        /// <summary>
        /// Represents the result of a transition.
        /// </summary>
        private sealed class TransitionResult
        {
            #region TransitionResult Members

            #region Fields

            private readonly bool m_hasFired;

            private readonly TState m_newState;

            private readonly Exception m_error;

            #endregion

            #region Construction

            /// <summary>
            /// Initializes a new instance of the TransitionResult class.
            /// </summary>
            /// <param name="hasFired">
            /// Indicates whether or not the Transition fired.
            /// </param>
            /// <param name="newState">
            /// The resulting state of the Transition.
            /// </param>
            /// <param name="error">
            /// The resulting exception of the Transition if one was thrown.
            /// </param>
            public TransitionResult(bool hasFired, TState newState, Exception error)
            {
                m_hasFired = hasFired;
                m_newState = newState;
                m_error = error;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets a value indicating whether or not the transition fired.
            /// </summary>
            public bool HasFired
            {
                get { return m_hasFired; }
            }

            /// <summary>
            /// Gets the exception that was a result of firing the Transition.
            /// </summary>
            /// <remarks>
            /// This property will be null if the Transition did not fire or if it
            /// did fire but no exception took place.
            /// </remarks>
            public Exception Error
            {
                get { return m_error; }
            }

            /// <summary>
            /// Gets the state that is a result of firing the Transition.
            /// </summary>
            /// <remarks>
            /// This property will be null if the Transition did not fire.
            /// </remarks>
            public TState NewState
            {
                get { return m_newState; }
            }

            #endregion

            #endregion
        }
    }
}