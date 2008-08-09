/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 10/05/2005
 */

using System;
using System.Collections;
using System.ComponentModel;

namespace Sanford.StateMachineToolkit.StateMachineBuilder
{
	/// <summary>
	/// Represents a collection of StateRows.
	/// </summary>
	public class StateRowCollection : CollectionBase, IBindingList
	{
		#region StateRowCollection Members

		#region Construction

		#endregion

		#region Methods

		/// <summary>
		/// Adds a StateRow to the StateRowCollection.
		/// </summary>
		/// <param name="row">
		/// The StateRow to add to the StateRowCollection.
		/// </param>
		/// <returns>
		/// The position into which the StateRow was inserted into the 
		/// StareRowCollection.
		/// </returns>
		public int Add(StateRow row)
		{
			return List.Add(row);
		}

		/// <summary>
		/// Creates a StateRow based on the specified name and adds it to the
		/// StateRowCollection.
		/// </summary>
		/// <param name="name">
		/// The state name.
		/// </param>
		/// <returns>
		/// The position into which the StateRow was inserted into the 
		/// StareRowCollection.
		/// </returns>
		public int Add(string name)
		{
			StateRow newRow = new StateRow();

			newRow.Name = name;

			return Add(newRow);
		}

		/// <summary>
		/// Creates a StateRow based on the specified name and initial state 
		/// and adds it to the StateRowCollection.
		/// </summary>
		/// <param name="name">
		/// The state name.
		/// </param>
		/// <param name="initialState">
		/// The state's initial state.
		/// </param>
		/// <returns>
		/// The position into which the StateRow was inserted into the 
		/// StareRowCollection.
		/// </returns>
		public int Add(string name, string initialState)
		{
			StateRow newRow = new StateRow();

			newRow.Name = name;
			newRow.InitialState = initialState;

			return Add(newRow);
		}

		/// <summary>
		/// Creates a StateRow based on the specified name, initial state, and
		/// history type and adds it to the StateRowCollection.
		/// </summary>
		/// <param name="name">
		/// The state name.
		/// </param>
		/// <param name="initialState">
		/// The state's initial state.
		/// </param>
		/// <param name="historyType">
		/// The state's history type.
		/// </param>
		/// <returns>
		/// The position into which the StateRow was inserted into the 
		/// StareRowCollection.
		/// </returns>
		public int Add(string name, string initialState, HistoryType historyType)
		{
			StateRow newRow = new StateRow();

			newRow.Name = name;
			newRow.InitialState = initialState;
			newRow.HistoryType = historyType;

			return Add(newRow);
		}

		protected override void OnClear()
		{
			foreach (StateRow row in List)
			{
				row.EditCancelled -= EditCancelledHandler;
			}

			base.OnClear();
		}

		protected override void OnClearComplete()
		{
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));

			base.OnClearComplete();
		}

		protected override void OnInsertComplete(int index, object value)
		{
			StateRow newRow = (StateRow) value;

			newRow.EditCancelled += EditCancelledHandler;

			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));

			base.OnInsertComplete(index, value);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			StateRow oldRow = (StateRow) value;

			oldRow.EditCancelled -= EditCancelledHandler;

			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));

			base.OnRemoveComplete(index, value);
		}

		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			if (oldValue != newValue)
			{
				StateRow oldRow = (StateRow) oldValue;
				StateRow newRow = (StateRow) newValue;

				oldRow.EditCancelled -= EditCancelledHandler;
				newRow.EditCancelled += EditCancelledHandler;

				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
			}

			base.OnSetComplete(index, oldValue, newValue);
		}

		private void OnListChanged(ListChangedEventArgs e)
		{
			ListChangedEventHandler handler = ListChanged;

			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void EditCancelledHandler(object sender, EventArgs e)
		{
			List.Remove(sender);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the StateRow at the specified index.
		/// </summary>
		public StateRow this[int index]
		{
			get { return (StateRow) List[index]; }
			set { List[index] = value; }
		}

		#endregion

		#endregion

		#region IBindingList Members

		public event ListChangedEventHandler ListChanged;

		public object AddNew()
		{
			StateRow newRow = new StateRow();

			List.Add(newRow);

			return newRow;
		}

		public void AddIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException();
		}

		public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			throw new NotSupportedException();
		}

		public int Find(PropertyDescriptor property, object key)
		{
			throw new NotSupportedException();
		}

		public void RemoveSort()
		{
			throw new NotSupportedException();
		}

		public void RemoveIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException();
		}

		public bool AllowNew
		{
			get { return true; }
		}

		public bool AllowEdit
		{
			get { return true; }
		}

		public bool AllowRemove
		{
			get { return true; }
		}

		public PropertyDescriptor SortProperty
		{
			get { throw new NotSupportedException(); }
		}

		public bool SupportsChangeNotification
		{
			get { return true; }
		}

		public bool SupportsSorting
		{
			get { return false; }
		}

		public bool SupportsSearching
		{
			get { return false; }
		}

		public bool IsSorted
		{
			get { throw new NotSupportedException(); }
		}

		public ListSortDirection SortDirection
		{
			get { throw new NotSupportedException(); }
		}

		#endregion
	}
}