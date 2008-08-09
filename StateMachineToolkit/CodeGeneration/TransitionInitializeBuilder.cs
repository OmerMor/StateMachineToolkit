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

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// Builds the method responsible for initializing the transitions.
	/// </summary>
    internal class TransitionInitializeBuilder
	{
        #region TransitionInitializeBuilder Members

        #region Fields

        // The state machine's transitions.
        private IDictionary stateTransitions;

        private string stateMachineName = string.Empty;

        // The built method.
        private CodeMemberMethod result = new CodeMemberMethod();

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the TransitionInitializeBuilder class
        /// with the specified state transition table.
        /// </summary>
        /// <param name="stateTransitions">
        /// The state transitions. 
        /// </param>
		public TransitionInitializeBuilder(IDictionary stateTransitions)
		{
            this.stateTransitions = stateTransitions;
		}

        #endregion

        #region Methods

        /// <summary>
        /// Builds the method.
        /// </summary>
        public void Build()
        {
            result = new CodeMemberMethod();
            result.Name = "InitializeTransitions";
            result.Attributes = MemberAttributes.Private;

            CodeVariableDeclarationStatement transDeclaration = 
                new CodeVariableDeclarationStatement(typeof(Transition), "trans");

            result.Statements.Add(transDeclaration);
            
            CodeThisReferenceExpression thisReference = 
                new CodeThisReferenceExpression();
            CodeTypeReferenceExpression stateMachineReference = 
                new CodeTypeReferenceExpression(StateMachineName);
            CodeExpression sourceStateReference;
            CodeExpression guardReference;
            CodeExpression actionReference;            
            CodeExpression targetStateReference;
            CodeObjectCreateExpression transCreate;
            CodePropertyReferenceExpression actionProperty;
            CodePropertyReferenceExpression transProperty;
            CodeMethodInvokeExpression addInvoke;
            CodeVariableReferenceExpression transReference = new CodeVariableReferenceExpression("trans");
            CodeAssignStatement transAssign;

            actionProperty = new CodePropertyReferenceExpression(transReference, "Actions");

            TransitionRowCollection transRowCollection;

            foreach(DictionaryEntry entry in stateTransitions)
            {
                transRowCollection = (TransitionRowCollection)entry.Value;

                foreach(TransitionRow transRow in transRowCollection)
                {
                    sourceStateReference = new CodeFieldReferenceExpression(
                        thisReference, "state" + entry.Key.ToString());                    

                    // If there is a guard for this transition.
                    if(transRow.Guard != null && transRow.Guard != string.Empty)
                    {
                        // Create a reference to the guard.
                        guardReference = new CodeFieldReferenceExpression(
                            thisReference, "guard" + transRow.Guard);
                    }
                    // Else there is no guard for this transition.
                    else
                    {
                        // Create a null reference for the guard.
                        guardReference = new CodePrimitiveExpression(null);
                    }                    

                    // If there is a target state for this transition.
                    if(transRow.Target != null && transRow.Target != string.Empty)
                    {
                        // Create a reference to the state target.
                        targetStateReference = new CodeFieldReferenceExpression(
                            thisReference, "state" + transRow.Target);
                    }
                    // Else there is no target state for this transition (it is an 
                    // internal transition).
                    else
                    {
                        // Create a null reference for the target state.
                        targetStateReference = new CodePrimitiveExpression(null);
                    }

                    //
                    // Create and initialize the transition and add it to the collection
                    // of transitions for the specified state.
                    //

                    transCreate = new CodeObjectCreateExpression();

                    transCreate.CreateType = new CodeTypeReference(typeof(Transition));
                    transCreate.Parameters.Add(guardReference);
                    transCreate.Parameters.Add(targetStateReference);

                    transAssign = new CodeAssignStatement(transReference, transCreate);

                    result.Statements.Add(transAssign);

                    foreach(ActionRow actionRow in transRow.Actions)
                    {
                        actionReference = new CodeFieldReferenceExpression(thisReference, "action" + actionRow.Name);
                        addInvoke = new CodeMethodInvokeExpression(actionProperty, "Add", actionReference);
                        result.Statements.Add(addInvoke);
                    }

                    transProperty = new CodePropertyReferenceExpression(
                        sourceStateReference, "Transitions");

                    CodeFieldReferenceExpression enumFieldReference = new CodeFieldReferenceExpression(
                        new CodeTypeReferenceExpression("EventID"), transRow.Event);

                    CodeCastExpression enumCast = 
                        new CodeCastExpression(typeof(int), enumFieldReference);

                    addInvoke = new CodeMethodInvokeExpression(transProperty,
                        "Add", new CodeExpression[] { enumCast, transReference });
                
                    result.Statements.Add(addInvoke);
                }
            }
        }

        #endregion

        #region Properties

        public string StateMachineName
        {
            get
            {
                return stateMachineName;
            }
            set
            {
                stateMachineName = value;
            }
        }

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
