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
using System.Collections;
using System.Collections.Generic;

namespace Sanford.StateMachineToolkit
{
    public abstract partial class StateMachine<TState, TEvent>
    {
        /// <summary>
        /// The SubstateCollection class represents a collection of substates. 
        /// Each <see cref="State"/> has a <see cref="State.Substates"/> 
        /// property of the SubstateCollection type. 
        /// Substates are added and removed to a <see cref="State"/> via this property.<para/>
        /// Substates are not represented by their own class. The <see cref="State"/> class 
        /// performs double duty, playing the role of substates and superstates when necessary. 
        /// Whether or not a <see cref="State"/> is a substate depends on whether or not 
        /// it has been added to another State's <see cref="State.Substates"/> collection. 
        /// And whether or not a State is a superstate depends on whether or not any States have 
        /// been added to its <see cref="State.Substates"/> collection.<para/>
        /// There are some restrictions on which States can be added as substates to another <see cref="State"/>. 
        /// The most obvious one is that a <see cref="State"/> cannot be added to its own 
        /// <see cref="State.Substates"/> collection; 
        /// a <see cref="State"/> cannot be a substate to itself. 
        /// Also, a <see cref="State"/> can only be the direct substate of one other 
        /// <see cref="State"/>; you cannot add a <see cref="State"/> to the 
        /// <see cref="State.Substates"/> collection of more than one <see cref="State"/>.
        /// </summary>
        private sealed class SubstateCollection : IEnumerable<State>
        {
            #region SubstateCollection Members

            #region Fields

            // The owner of the collection. The States in the collection are 
            // substates to this State.
            private readonly State m_owner;

            // The collection of substates.
            private readonly List<State> m_substates = new List<State>();

            #endregion

            #region Construction

            /// <summary>
            /// Initializes a new instance of the SubstateCollection with the 
            /// specified owner.
            /// </summary>
            /// <param name="owner">
            /// The owner of the collection.
            /// </param>
            public SubstateCollection(State owner)
            {
                m_owner = owner;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Adds the specified State to the collection.
            /// </summary>
            /// <param name="substate">
            /// The State to add to the collection.
            /// </param>
            public void Add(State substate)
            {
                #region Preconditions

                if (m_owner == substate)
                {
                    throw new ArgumentException(
                        "State cannot be a substate to itself.");
                }
                if (m_substates.Contains(substate))
                {
                    throw new ArgumentException(
                        "State is already a substate to this state.");
                }
                if (substate.Superstate != null)
                {
                    throw new ArgumentException(
                        "State is already a substate to another State.");
                }

                #endregion

                substate.Superstate = m_owner;
                m_substates.Add(substate);
            }

            /// <summary>
            /// Removes the specified State from the collection.
            /// </summary>
            /// <param name="substate">
            /// The State to remove from the collection.
            /// </param>
            public void Remove(State substate)
            {
                if (!m_substates.Contains(substate)) return;
                substate.Superstate = null;
                m_substates.Remove(substate);

                if (m_owner.InitialState == substate)
                {
                    m_owner.InitialState = null;
                }
            }

            #endregion

            #endregion

            #region IEnumerable Members

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="IEnumerator{State}"/> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<State> GetEnumerator()
            {
                return m_substates.GetEnumerator();
            }

            #endregion

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Clear()
            {
                m_substates.Clear();
            }
        }
    }
}