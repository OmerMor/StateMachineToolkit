/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 10/01/2005
 */

using System;
using System.CodeDom;
using System.Collections;

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// Builds the method responsible for initializing the substate/superstate 
	/// relationships between states.
	/// </summary>
    internal class RelationshipInitializeBuilder
	{
        #region RelationshipInitializeBuilder Members

        #region Fields

        // The state machine's states and their superstates.
        private IDictionary stateRelationships;

        // The build method.
        private CodeMemberMethod result = new CodeMemberMethod();

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the RelationshipInitializeBuilder
        /// with the specified relationship table.
        /// </summary>
        /// <param name="stateRelationship">
        /// The relationships between substates and superstates.
        /// </param>
		public RelationshipInitializeBuilder(IDictionary stateRelationships)
		{
            this.stateRelationships = stateRelationships;
		}

        #endregion

        #region Methods

        /// <summary>
        /// Builds the method.
        /// </summary>
        public void Build()
        {
            result = new CodeMemberMethod();
            result.Name = "InitializeRelationships";
            result.Attributes = MemberAttributes.Private;

            CodeThisReferenceExpression thisReference = new CodeThisReferenceExpression();
            CodeFieldReferenceExpression substateField;
            CodeFieldReferenceExpression superstateField;
            CodePropertyReferenceExpression substatesProperty;
            CodeMethodInvokeExpression addInvoke;

            foreach(DictionaryEntry entry in stateRelationships)
            {
                substateField = new CodeFieldReferenceExpression(
                    thisReference, "state" + entry.Key.ToString());
                superstateField = new CodeFieldReferenceExpression(
                    thisReference, "state" + entry.Value.ToString());

                substatesProperty = new CodePropertyReferenceExpression(
                    superstateField, "Substates");

                addInvoke = new CodeMethodInvokeExpression(substatesProperty,
                    "Add", new CodeExpression[] { substateField });

                result.Statements.Add(addInvoke);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the built method.
        /// </summary>
        public CodeMemberMethod Result
        {
            get
            {
                return result;
            }
        }

        #endregion

        #endregion
	}
}
