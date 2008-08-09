using System.CodeDom;
using System.Collections;

namespace Sanford.StateMachineToolkit.CodeGeneration
{
	/// <summary>
	/// Builds the event identifiers.
	/// </summary>
	internal class StateEnumeratorBuilder
	{
		#region StateEnumeratorBuilder Members

		#region Fields

		private readonly ICollection states;

		private CodeTypeDeclaration result = new CodeTypeDeclaration();

		#endregion

		#region Construction

		public StateEnumeratorBuilder(ICollection states)
		{
			this.states = states;
		}

		#endregion

		#region Methods

		public void Build()
		{
			result = new CodeTypeDeclaration("StateID");

			result.IsEnum = true;

			foreach (string s in states)
			{
				result.Members.Add(new CodeMemberField(typeof (int), s));
			}
		}

		#endregion

		#region Properties

		public CodeTypeDeclaration Result
		{
			get { return result; }
		}

		#endregion

		#endregion
	}
}