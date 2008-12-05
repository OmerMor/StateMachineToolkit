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
		private void onEditCancelled()
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

		/// <summary>
		/// Pushes changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/> or <see cref="M:System.ComponentModel.IBindingList.AddNew"/> call into the underlying object.
		/// </summary>
		public void EndEdit()
		{
			if (!isEditing) return;
			backupName = string.Empty;
			isEditing = false;
			isNew = false;
		}

		/// <summary>
		/// Discards changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/> call.
		/// </summary>
		public void CancelEdit()
		{
			if (!isEditing) return;
			name = backupName;
			isEditing = false;

			if (isNew)
			{
				onEditCancelled();
			}
		}

		/// <summary>
		/// Begins an edit on an object.
		/// </summary>
		public void BeginEdit()
		{
			if (isEditing) return;
			backupName = name;
			isEditing = true;
		}

		#endregion
	}
}