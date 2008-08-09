/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 09/30/2005
 */

using System.CodeDom;
using System.Collections;

namespace Sanford.StateMachineToolkit.CodeGeneration
{
	/// <summary>
	/// Builds the method responsible for initializing the guard delegates.
	/// </summary>
	internal class GuardInitializeBuilder
	{
		#region GuardInitializeBuilder Members

		#region Fields

		// The state machine's guards.
		private readonly ICollection guards;

		// The built method.
		private CodeMemberMethod result = new CodeMemberMethod();

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the GuardInitializeBuilder class.
		/// </summary>
		/// <param name="guards">
		/// The guards from which the GuardHandler delegates are initialized.
		/// </param>
		public GuardInitializeBuilder(ICollection guards)
		{
			this.guards = guards;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Builds the method.
		/// </summary>
		public void Build()
		{
			result = new CodeMemberMethod();
			result.Name = "InitializeGuards";
			result.Attributes = MemberAttributes.Private;

			CodeThisReferenceExpression thisReference = new CodeThisReferenceExpression();

			foreach (string name in guards)
			{
				CodeFieldReferenceExpression guardField =
					new CodeFieldReferenceExpression(thisReference, "guard" + name);

				CodeDelegateCreateExpression delegateCreate =
					new CodeDelegateCreateExpression(
						new CodeTypeReference(typeof (GuardHandler)),
						thisReference, name);

				result.Statements.Add(new CodeAssignStatement(guardField, delegateCreate));
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