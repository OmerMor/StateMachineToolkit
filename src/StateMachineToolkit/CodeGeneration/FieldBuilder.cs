/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 10/13/2005
 */

using System;
using System.CodeDom;
using System.Collections;

namespace Sanford.StateMachineToolkit.CodeGeneration
{
    /// <summary>
    /// Builds the fields belonging to the state machine.
    /// </summary>
    internal class FieldBuilder
    {
        #region FieldBuilder Members

        #region Fields

        // The state machine's states.
        private readonly ICollection states;

        // The state machine's guards.
        private readonly ICollection guards;

        // The state machine's actions.
        private readonly ICollection actions;

        // The list of built fields.
        private ArrayList fields = new ArrayList();

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the FieldBuilder class with the 
        /// specified state, guard, and action tables.
        /// </summary>
        /// <param name="states">
        /// The states from which to declare the State object fields.
        /// </param>
        /// <param name="events">
        /// The events from which to declare the event ID fields.
        /// </param>
        /// <param name="guards">
        /// The guards from which to declare the GuardHandler delegate fields.
        /// </param>
        /// <param name="actions">
        /// The actions from which to declare the ActionHandler delegate fields.
        /// </param>
        public FieldBuilder(ICollection states, ICollection events, ICollection guards,
                            ICollection actions)
        {
            this.states = states;
            this.guards = guards;
            this.actions = actions;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the fields.
        /// </summary>
        public void Build()
        {
            fields = new ArrayList();

            BuildFields(states, "Name", "state", typeof(StateMachine<,>.State));
            BuildFields(guards, "Name", "guard", typeof (GuardHandler));
            BuildFields(actions, "Name", "action", typeof (ActionHandler));
        }

        // Does the actual field generation.
        private void BuildFields(ICollection collection, string columnName,
                                 string fieldPrefix, Type fieldType)
        {
            foreach (string name in collection)
            {
                CodeMemberField field = new CodeMemberField(fieldType, fieldPrefix + name);
                fields.Add(field);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of built fields.
        /// </summary>
        public ICollection Result
        {
            get { return fields; }
        }

        #endregion

        #endregion
    }
}