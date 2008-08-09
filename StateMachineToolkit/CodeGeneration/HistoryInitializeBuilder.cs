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
	/// Builds the method responsible for initializing states' history type.
	/// </summary>
	internal class HistoryInitializeBuilder
	{
		#region HistoryInitializeBuilder Members

		#region Fields

		// The state machine's states and their history types.
		private readonly IDictionary stateHistoryTypes;

		// The built method.
		private CodeMemberMethod result = new CodeMemberMethod();

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the HistoryInitializeBuilder class.
		/// </summary>
		/// <param name="stateHistoryTypes">
		/// The states and their history types.
		/// </param>
		public HistoryInitializeBuilder(IDictionary stateHistoryTypes)
		{
			this.stateHistoryTypes = stateHistoryTypes;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Builds the method.
		/// </summary>
		public void Build()
		{
			result = new CodeMemberMethod();
			result.Name = "InitializeHistoryTypes";
			result.Attributes = MemberAttributes.Private;

			CodeThisReferenceExpression thisReference =
				new CodeThisReferenceExpression();
			CodeTypeReferenceExpression historyType = new CodeTypeReferenceExpression(typeof (HistoryType));

			foreach (DictionaryEntry entry in stateHistoryTypes)
			{
				CodeFieldReferenceExpression stateField = new CodeFieldReferenceExpression(
					thisReference, "state" + entry.Key);

				CodePropertyReferenceExpression historyTypeProperty = new CodePropertyReferenceExpression(
					stateField, "HistoryType");

				CodeFieldReferenceExpression historyTypeField;
				if (string.IsNullOrEmpty(entry.Value.ToString()))
				{
					historyTypeField = new CodeFieldReferenceExpression(
						historyType, "None");
				}
				else
				{
					historyTypeField = new CodeFieldReferenceExpression(
						historyType, entry.Value.ToString());
				}

				CodeAssignStatement historyTypeAssign = new CodeAssignStatement(
					historyTypeProperty, historyTypeField);

				result.Statements.Add(historyTypeAssign);
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