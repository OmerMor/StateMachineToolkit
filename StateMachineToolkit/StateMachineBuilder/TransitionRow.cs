/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 10/07/2005
 */

using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Sanford.StateMachineToolkit
{
	/// <summary>
	/// Represents a row of data describing a state transition in a 
	/// TransitionRowCollection.
	/// </summary>
	public sealed class TransitionRow : IEditableObject
	{
        #region TransitionRow Members

        #region Structs

        // Represents the transition's properties.
        private struct TransitionProperties
        {
            public string eventName;
            public string guard;
            public string target;
        }

        #endregion

        #region Fields

        // The transition's current property values.
        private TransitionProperties trans;

        // The transition's previous property values.
        private TransitionProperties backupTrans;

        // Indicates whether or not the TransitionRow is being edited.
        private bool isEditing = false;

        // Indicates whether or not the TransitionRow is new.
        private bool isNew = true;

        // The transition's actions.
        private ActionRowCollection actions = new ActionRowCollection();

        #endregion

        #region Events

        /// <summary>
        /// Raised when an edit has been cancelled.
        /// </summary>
        internal event EventHandler EditCancelled;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the TransitionRow class.
        /// </summary>
		public TransitionRow()
		{
		}

        #endregion

        #region Methods

        // Raises the EditCancelled event.
        private void OnEditCancelled()
        {
            EventHandler handler = EditCancelled;

            if(handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the event that triggered the transition.
        /// </summary>
        [XmlAttribute("event")]
        public string Event
        {
            get
            {
                return trans.eventName;
            }
            set
            {
                trans.eventName = value;
            }
        }

        /// <summary>
        /// Gets the guard that is evaluated to determine whether or not the 
        /// transition will fire.
        /// </summary>
        [XmlAttribute("guard")]
        public string Guard
        {
            get
            {
                return trans.guard;
            }
            set
            {
                trans.guard = value;
            }
        }

        /// <summary>
        /// Gets the target state of the transition.
        /// </summary>
        [XmlAttribute("target")]
        public string Target
        {
            get
            {
                return trans.target;
            }
            set
            {
                trans.target = value;
            }
        }

        [XmlElement("action", typeof(ActionRow))]
        public ActionRowCollection Actions
        {
            get
            {
                return actions;
            }
        }

        #endregion

        #endregion

        #region IEditableObject Members

        /// <summary>
        /// Begins an edit on a TransitionRow.
        /// </summary>
        public void BeginEdit()
        {
            if(!isEditing)
            {
                backupTrans = trans;
                isEditing = true;
            }
        }

        /// <summary>
        /// Discards changes since the last BeginEdit call.
        /// </summary>
        public void CancelEdit()
        {
            if(isEditing)
            {
                trans = backupTrans;
                isEditing = false;

                if(isNew)
                {
                    OnEditCancelled();
                }
            }
        }        

        /// <summary>
        /// Pushes changes since the last BeginEdit or IBindingList.AddNew call 
        /// into the underlying StateRow.
        /// </summary>
        public void EndEdit()
        {
            if(isEditing)
            {
                backupTrans = new TransitionProperties();
                isEditing = false;
                isNew = false;
            }            
        }        

        #endregion
    }
}
