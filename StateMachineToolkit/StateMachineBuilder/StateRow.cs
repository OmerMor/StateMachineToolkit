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
	/// Represents a row of data describing a state in a StateRowCollection.
	/// </summary>
	public sealed class StateRow : IEditableObject
	{
        #region StateRow Members

        #region Structs

        // Represents the state's properties.
        private struct StateProperties
        {
            public string name;
            public string initialState;
            public HistoryType historyType;        
        }

        #endregion

        #region Fields

        // The state's current property values.
        private StateProperties state;

        // The state's pervious property values.
        private StateProperties backupState;

        // Indicates whether or not the StateRow is being edited.
        private bool isEditing = false;

        // Indicates whether or not the StateRow is new.
        private bool isNew = true;

        // The state's substates.
        private StateRowCollection substates = new StateRowCollection();

        // The state's transitions.
        private TransitionRowCollection transitions = new TransitionRowCollection();

        #endregion

        #region Events

        /// <summary>
        /// Raised when an edit has been cancelled.
        /// </summary>
        internal event EventHandler EditCancelled;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the StateRow class.
        /// </summary>
        public StateRow()
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
        /// Gets or sets the state's name.
        /// </summary>
        [XmlAttribute("name")]
        public string Name
        {
            get
            {
                return state.name;
            }
            set
            {
                state.name = value;
            }
        }

        /// <summary>
        /// Gets or sets the state's initial state.
        /// </summary>
        [XmlAttribute("initialState")]
        public string InitialState
        {
            get
            {
                return state.initialState;
            }
            set
            {
                state.initialState = value;
            }
        }

        /// <summary>
        /// Gets or sets the state's initial state.
        /// </summary>
        [XmlAttribute("historyType")]
        public HistoryType HistoryType
        {
            get
            {
                return state.historyType;
            }
            set
            {
                state.historyType = value;
            }
        }

        /// <summary>
        /// Gets the state's substates.
        /// </summary>
        [XmlElement("state", typeof(StateRow))]
        public StateRowCollection Substates
        {
            get
            {
                return substates;
            }
        }

        /// <summary>
        /// Gets the states transitions.
        /// </summary>
        [XmlElement("transition", typeof(TransitionRow))]
        public TransitionRowCollection Transitions
        {
            get
            {
                return transitions;
            }
        }

        #endregion

        #endregion

        #region IEditableObject Members

        /// <summary>
        /// Begins an edit on a StateRow.
        /// </summary>
        public void BeginEdit()
        {
            if(!isEditing)
            {
                backupState = state;
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
                state = backupState;
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
                backupState = new StateProperties();
                isEditing = false;
                isNew = false;
            }            
        }        

        #endregion
    }
}
