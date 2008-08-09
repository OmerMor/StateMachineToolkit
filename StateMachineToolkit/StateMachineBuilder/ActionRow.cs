using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Sanford.StateMachineToolkit.StateMachineBuilder
{
	/// <summary>
	/// Summary description for ActionRow.
	/// </summary>
	public class ActionRow : IEditableObject
	{
		private string name = string.Empty;

		private string backupName = string.Empty;

		// Indicates whether or not the ActionRow is being edited.
		private bool isEditing;

		private bool isNew = true;

		/// <summary>
		/// Raised when an edit has been cancelled.
		/// </summary>
		internal event EventHandler EditCancelled;

		// Raises the EditCancelled event.
		private void OnEditCancelled()
		{
			EventHandler handler = EditCancelled;

			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Gets or sets the action's name.
		/// </summary>
		[XmlAttribute("name")]
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		#region IEditableObject Members

		public void EndEdit()
		{
			if (!isEditing) return;
			backupName = string.Empty;
			isEditing = false;
			isNew = false;
		}

		public void CancelEdit()
		{
			if (!isEditing) return;
			name = backupName;
			isEditing = false;

			if (isNew)
			{
				OnEditCancelled();
			}
		}

		public void BeginEdit()
		{
			if (isEditing) return;
			backupName = name;
			isEditing = true;
		}

		#endregion
	}
}