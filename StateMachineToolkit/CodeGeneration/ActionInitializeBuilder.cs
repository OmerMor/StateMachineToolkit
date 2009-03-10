/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 10/01/2005
 */

using System.CodeDom;
using System.Collections;

namespace Sanford.StateMachineToolkit.CodeGeneration
{
    /// <summary>
    /// Builds the method responsible for initializing the action delegates.
    /// </summary>
    internal class ActionInitializeBuilder
    {
        #region ActionInitializeBuilder Members

        #region Fields

        // The state machine's actions.
        private readonly ICollection actions;

        // The built method.
        private CodeMemberMethod result = new CodeMemberMethod();

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the ActionInitializeBuilder with the
        /// specified action table.
        /// </summary>
        /// <param name="actions">
        /// The actions from which the ActionHandler delegates are initialized.
        /// </param>
        public ActionInitializeBuilder(ICollection actions)
        {
            this.actions = actions;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the method.
        /// </summary>
        public void Build()
        {
            result = new CodeMemberMethod();
            result.Name = "InitializeActions";
            result.Attributes = MemberAttributes.Private;

            CodeThisReferenceExpression thisReference =
                new CodeThisReferenceExpression();
            CodeTypeReference actionHandlerReference =
                new CodeTypeReference(typeof (ActionHandler));

            foreach (string name in actions)
            {
                CodeFieldReferenceExpression actionField =
                    new CodeFieldReferenceExpression(thisReference, "action" + name);

                CodeDelegateCreateExpression delegateCreate =
                    new CodeDelegateCreateExpression(actionHandlerReference, thisReference, name);

                result.Statements.Add(
                    new CodeAssignStatement(actionField, delegateCreate));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the built method.
        /// </summary>
        public CodeMemberMethod Result
        {
            get { return result; }
        }

        #endregion

        #endregion
    }
}