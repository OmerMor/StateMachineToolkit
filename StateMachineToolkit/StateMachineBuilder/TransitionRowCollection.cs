/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 10/07/2005
 */

using System;
using System.Collections;
using System.ComponentModel;

namespace Sanford.StateMachineToolkit.StateMachineBuilder
{
	/// <summary>
	/// Represents a collection of TransitionRows.
	/// </summary>
	public sealed class TransitionRowCollection : CollectionBase, IBindingList
	{
		#region TransitionRowCollection Members

		#region Construction

		#endregion

		#region Methods

		/// <summary>
		/// Adds a TransitionRow to the TransitionRowCollection.
		/// </summary>
		/// <param name="row">
		/// The TransitionRow to add to the TransitionRowCollection.
		/// </param>
		/// <returns>
		/// The position into which the TransitionRow was inserted into the 
		/// TransitionRowCollection.
		/// </returns>
		public int Add(TransitionRow row)
		{
			return List.Add(row);
		}

		/// <summary>
		/// Creates a TransitionRow with the specified event, guard,
		/// and target and adds it to the TransitionRowCollection.
		/// </summary>
		/// <param name="event">
		/// The event that raised the transition.
		/// </param>
		/// <param name="guard">
		/// The guard to evaluate whether or not the transition should fire.
		/// </param>
		/// <param name="target">
		/// The target state of the transition.
		/// </param>
		/// <returns>
		/// The position into which the TransitionRow was inserted into the 
		/// TransitionRowCollection.
		/// </returns>
		public int Add(string @event, string guard, string target)
		{
			TransitionRow newRow = new TransitionRow();

			newRow.Event = @event;
			newRow.Guard = guard;
			newRow.Target = target;

			return Add(newRow);
		}

		protected override void OnClear()
		{
			foreach (TransitionRow row in List)
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
			TransitionRow newRow = (TransitionRow) value;

			newRow.EditCancelled += EditCancelledHandler;

			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));

			base.OnInsertComplete(index, value);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			TransitionRow oldRow = (TransitionRow) value;

			oldRow.EditCancelled -= EditCancelledHandler;

			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));

			base.OnRemoveComplete(index, value);
		}

		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			if (oldValue != newValue)
			{
				TransitionRow oldRow = (TransitionRow) oldValue;
				TransitionRow newRow = (TransitionRow) newValue;

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
		/// Gets or sets the TransitionRow at the specified index.
		/// </summary>
		public TransitionRow this[int index]
		{
			get { return (TransitionRow) List[index]; }
			set { List[index] = value; }
		}

		#endregion

		#endregion

		#region IBindingList Members

		public event ListChangedEventHandler ListChanged;

		public object AddNew()
		{
			TransitionRow newRow = new TransitionRow();

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