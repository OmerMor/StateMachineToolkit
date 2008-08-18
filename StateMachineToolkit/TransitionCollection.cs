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
	public abstract partial class StateMachine<TState, TEvent>
	{
		/// <summary>
		/// The TransitionCollection represents a collection of Transitions. 
		/// Each <see cref="State"/> object has its own 
		/// TransitionCollection for holding its Transitions.<para/>
		/// When a Transition is added to a State's <see cref="State.Transitions"/>, 
		/// it is registered with an event ID. This event ID is a value identifying an event a 
		/// <see cref="State"/> can receive. When a <see cref="State"/> 
		/// receives an event, it uses the event's ID to check to see if it has any Transitions for that 
		/// event (as described above).
		/// </summary>
		public class TransitionCollection
		{
			#region TransitionCollection Members

			#region Fields

			// The owner of the collection.
			private readonly State owner;

			// The table of transitions.
			private readonly Dictionary<TEvent, List<Transition>> transitions
				= new Dictionary<TEvent, List<Transition>>();

			#endregion

			#region Construction

			/// <summary>
			/// Initializes a new instance of the TransitionCollection class with 
			/// the specified number of events.
			/// </summary>
			/// <param name="owner">
			/// The state that owns the TransitionCollection.
			/// </param>
			public TransitionCollection(State owner)
			{
				this.owner = owner;
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
			public void Add(TEvent eventID, Transition trans)
			{
				#region Preconditions

				if (trans.Source != null)
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
					transitions[eventID] = new List<Transition>();
				}

				// Add Transition.
				transitions[eventID].Add(trans);
			}

			/// <summary>
			/// Adds a Transition to the collection for the specified event ID.
			/// </summary>
			/// <param name="eventID">
			/// The event ID associated with the Transition.
			/// </param>
			/// <param name="targetState">
			/// The target state of the transtion.
			/// </param>
			/// <param name="actions">
			/// Optional array of actions, to be performed during the transition.
			/// </param>
			/// <remarks>
			/// When a Transition is added to the collection, it is associated with
			/// the specified event ID. When a State receives an event, it looks up
			/// the event ID in its TransitionCollection to see if there are any 
			/// Transitions for the specified event. 
			/// </remarks>
			public void Add(TEvent eventID, State targetState, params ActionHandler[] actions)
			{
				Add(eventID, null, targetState, actions);
			}
			/// <summary>
			/// Adds a Transition to the collection for the specified event ID.
			/// </summary>
			/// <param name="eventID">
			/// The event ID associated with the Transition.
			/// </param>
			/// <param name="guard">
			/// The guard to test to determine whether the transition should take 
			/// place.
			/// </param>
			/// <param name="targetState">
			/// The target state of the transtion.
			/// </param>
			/// <param name="actions">
			/// Optional array of actions, to be performed during the transition.
			/// </param>
			/// <remarks>
			/// When a Transition is added to the collection, it is associated with
			/// the specified event ID. When a State receives an event, it looks up
			/// the event ID in its TransitionCollection to see if there are any 
			/// Transitions for the specified event. 
			/// </remarks>
			public void Add(TEvent eventID, GuardHandler guard, State targetState, params ActionHandler[] actions)
			{
				Transition trans = new Transition(guard, targetState);
				foreach (ActionHandler action in actions)
				{
					if(action == null) continue;
					trans.Actions.Add(action);
				}
				Add(eventID, trans);
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
			public void Remove(TEvent eventID, Transition trans)
			{
				if (!transitions.ContainsKey(eventID))
					return;

				// If there are Transitions at the specified event id.
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
			public IEnumerable<Transition> this[TEvent eventID]
			{
				get { return transitions.ContainsKey(eventID) ? transitions[eventID] : null; }
			}

			#endregion

			#endregion
		}
	}
}