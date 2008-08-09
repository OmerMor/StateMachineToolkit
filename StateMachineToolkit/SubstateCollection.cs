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
	/// <summary>
	/// Represents a collection of substates.
	/// </summary>
	public class SubstateCollection<TState, TEvent> : IEnumerable<State<TState, TEvent>>
		where TState : struct, IComparable, IFormattable/*, IConvertible*/
		where TEvent : struct, IComparable, IFormattable/*, IConvertible*/
	{
        #region SubstateCollection Members

        #region Fields

        // The owner of the collection. The States in the collection are 
        // substates to this State.
		private readonly State<TState, TEvent> owner;

        // The collection of substates.
		private readonly List<State<TState, TEvent>> substates = new List<State<TState, TEvent>>();

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the SubstateCollection with the 
        /// specified owner.
        /// </summary>
        /// <param name="owner">
        /// The owner of the collection.
        /// </param>
		public SubstateCollection(State<TState, TEvent> owner)
		{
            this.owner = owner;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the specified State to the collection.
        /// </summary>
        /// <param name="substate">
        /// The State to add to the collection.
        /// </param>
		public void Add(State<TState, TEvent> substate)
        {
            #region Preconditions

            if(owner == substate)
            {
                throw new ArgumentException(
                    "State cannot be a substate to itself.");
            }
        	if(substates.Contains(substate))
        	{
        		throw new ArgumentException(
        			"State is already a substate to this state.");
        	}
        	if(substate.Superstate != null)
        	{
        		throw new ArgumentException(
        			"State is already a substate to another State.");
        	}

        	#endregion

            substate.Superstate = owner;
            substates.Add(substate);
        }

        /// <summary>
        /// Removes the specified State from the collection.
        /// </summary>
        /// <param name="substate">
        /// The State to remove from the collection.
        /// </param>
		public void Remove(State<TState, TEvent> substate)
        {
        	if (!substates.Contains(substate)) return;
        	substate.Superstate = null;
        	substates.Remove(substate);

        	if(owner.InitialState == substate)
        	{
        		owner.InitialState = null;
        	}
        }

        #endregion

        #endregion


        #region IEnumerable Members

		public IEnumerator<State<TState, TEvent>> GetEnumerator()
        {
            return substates.GetEnumerator();
        }

        #endregion

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
