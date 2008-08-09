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
using System.Collections.Generic;

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// Represents a collection of Transitions.
	/// </summary>
	public class TransitionCollection<TState, TEvent>
		where TState : struct, IComparable, IFormattable/*, IConvertible*/
		where TEvent : struct, IComparable, IFormattable/*, IConvertible*/
	{
        #region TransitionCollection Members

        #region Fields

        // The owner of the collection.
		private readonly State<TState, TEvent> owner;

        // The table of transitions.
		private readonly Dictionary<TEvent,List<Transition<TState, TEvent>>> transitions;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the TransitionCollection class with 
        /// the specified number of events.
        /// </summary>
        /// <param name="owner">
        /// The state that owns the TransitionCollection.
        /// </param>
		public TransitionCollection(State<TState, TEvent> owner)
		{
            this.owner = owner;
			transitions = new Dictionary<TEvent, List<Transition<TState, TEvent>>>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a Transition to the collection for the specified event ID.
        /// </summary>
        /// <param name="eventID">
        /// The event ID associated with the Transition.
        /// </param>
        /// <param name="trans">
        /// The Transition to add.
        /// </param>
        /// <remarks>
        /// When a Transition is added to the collection, it is associated with
        /// the specified event ID. When a State receives an event, it looks up
        /// the event ID in its TransitionCollection to see if there are any 
        /// Transitions for the specified event. 
        /// </remarks>
		public void Add(TEvent eventID, Transition<TState, TEvent> trans)
        {
            #region Preconditions

/*
            if(eventID < 0  || eventID >= transitions.Length)
            {
                throw new ArgumentOutOfRangeException("eventID",
                    "Event ID out of range.");
            }
*/
        	
			if(trans.Source != null)
        	{
        		throw new InvalidOperationException(
        			"This Transition has already been added to another State.");
        	}

        	#endregion            

            // Set the transition's source.
            trans.Source = owner;

            // If there are no Transitions for the specified event ID.
			if (!transitions.ContainsKey(eventID))
            {
                // Create new list of Transitions for the specified event ID.
				transitions[eventID] = new List<Transition<TState, TEvent>>();
            }            

            // Add Transition.
			transitions[eventID].Add(trans);
        }

        /// <summary>
        /// Removes the specified Transition at the specified event ID.
        /// </summary>
        /// <param name="eventID">
        /// The event ID associated with the Transition.
        /// </param>
        /// <param name="trans">
        /// The Transition to remove.
        /// </param>
		public void Remove(TEvent eventID, Transition<TState, TEvent> trans)
        {
            #region Preconditions

/*
            if(eventID < 0  || eventID >= transitions.Length)
            {
                throw new ArgumentOutOfRangeException("eventID",
                    "Event ID out of range.");
            }
*/

            #endregion

            // If there are Transitions at the specified event id.
			if (!transitions.ContainsKey(eventID)) 
				return;

			transitions[eventID].Remove(trans);

        	// If there are no more Transitions at the specified event id.
			if (transitions[eventID].Count == 0)
        	{
        		// Indicate that there are no Transitions at this event id.
				transitions.Remove(eventID);
        	}
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a collection of Transitions at the specified event ID.
        /// </summary>
        /// <remarks>
        /// If there are no Transitions at the specified event ID, the value
        /// of the collection will be null.
        /// </remarks>
		public IEnumerable<Transition<TState, TEvent>> this[TEvent eventID]
        {
            get
            {
                #region Preconditions

/*
                if(eventID < 0  || eventID >= transitions.Length)
                {
                    throw new ArgumentOutOfRangeException("eventID",
                        "Event ID out of range.");
                }
*/

                #endregion
                return transitions.ContainsKey(eventID) ? transitions[eventID] : null;
            }
        }

        #endregion

        #endregion
    }
}
