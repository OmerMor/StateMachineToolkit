using System;
using System.Collections;
using System.ComponentModel;

namespace Sanford.StateMachineToolkit.StateMachineBuilder
{
	/// <summary>
	/// Summary description for ActionRowCollection.
	/// </summary>
	public class ActionRowCollection : CollectionBase, IBindingList
	{
		public int Add(ActionRow row)
		{
			return List.Add(row);
		}

		public int Add(string name)
		{
			ActionRow newRow = new ActionRow();

			newRow.Name = name;

			return Add(newRow);
		}

		protected override void OnClear()
		{
			foreach (ActionRow row in List)
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
			ActionRow newRow = (ActionRow) value;

			newRow.EditCancelled += EditCancelledHandler;

			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));

			base.OnInsertComplete(index, value);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			ActionRow oldRow = (ActionRow) value;

			oldRow.EditCancelled -= EditCancelledHandler;

			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));

			base.OnRemoveComplete(index, value);
		}

		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			if (oldValue != newValue)
			{
				ActionRow oldRow = (ActionRow) oldValue;
				ActionRow newRow = (ActionRow) newValue;

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

		public ActionRow this[int index]
		{
			get { return (ActionRow) List[index]; }
			set { List[index] = value; }
		}

		#region IBindingList Members

		public event ListChangedEventHandler ListChanged;

		public object AddNew()
		{
			ActionRow newRow = new ActionRow();

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