/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 09/30/2005
 */

using System;
using System.CodeDom;
using System.Collections;

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// Builds the method responsible for initializing states' initial state.
	/// </summary>
    internal class InitialStateInitializeBuilder
	{
        #region InitialStateInitializeBuilder Members

        #region Fields

        // The state machine's states and their initial states.
        private IDictionary stateInitialStates;

        // The built method.
        private CodeMemberMethod result = new CodeMemberMethod();

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the InitialStateInitializeBuilder 
        /// class.
        /// </summary>
        /// <param name="stateInitialStates">
        /// The states and their initial states. 
        /// </param>
		public InitialStateInitializeBuilder(IDictionary stateInitialStates)
		{
            this.stateInitialStates = stateInitialStates;
		}

        #endregion

        #region Methods

        /// <summary>
        /// Builds the method.
        /// </summary>
        public void Build()
        {
            result = new CodeMemberMethod();
            result.Name = "InitializeInitialStates";
            result.Attributes = MemberAttributes.Private;

            CodeThisReferenceExpression thisReference = new CodeThisReferenceExpression();
            CodeFieldReferenceExpression stateField;
            CodeFieldReferenceExpression initialStateField;
            CodePropertyReferenceExpression initialStateProperty;

            foreach(DictionaryEntry entry in stateInitialStates)
            {
                stateField = new CodeFieldReferenceExpression(
                    thisReference, "state" + entry.Key.ToString());

                initialStateField = new CodeFieldReferenceExpression(
                    thisReference, "state" + entry.Value.ToString());

                initialStateProperty = new CodePropertyReferenceExpression(
                    stateField, "InitialState");

                result.Statements.Add(new CodeAssignStatement(
                    initialStateProperty, initialStateField));
            }
        }

        #endregion

        #region Properties

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
