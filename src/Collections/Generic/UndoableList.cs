using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sanford.Collections.Generic
{
	public class UndoableList<T> : IList<T>
	{
		private readonly List<T> theList;
		private readonly UndoManager undoManager;

		public UndoableList()
		{
			undoManager = new UndoManager();
			theList = new List<T>();
		}

		public UndoableList(IEnumerable<T> collection)
		{
			undoManager = new UndoManager();
			theList = new List<T>(collection);
		}

		public UndoableList(int capacity)
		{
			undoManager = new UndoManager();
			theList = new List<T>(capacity);
		}

		public void Add(T item)
		{
			InsertCommand command = new InsertCommand(theList, Count, item);
			undoManager.Execute(command);
		}

		public void AddRange(IEnumerable<T> collection)
		{
			InsertRangeCommand command = new InsertRangeCommand(theList, theList.Count, collection);
			undoManager.Execute(command);
		}

		public int BinarySearch(T item)
		{
			return theList.BinarySearch(item);
		}

		public int BinarySearch(T item, IComparer<T> comparer)
		{
			return theList.BinarySearch(item, comparer);
		}

		public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
		{
			return theList.BinarySearch(index, count, item, comparer);
		}

		public void Clear()
		{
			if (Count == 0) return;
			ClearCommand command = new ClearCommand(theList);
			undoManager.Execute(command);
		}

		public void ClearHistory()
		{
			undoManager.ClearHistory();
		}

		public bool Contains(T item)
		{
			return theList.Contains(item);
		}

		public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
		{
			return theList.ConvertAll(converter);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			theList.CopyTo(array, arrayIndex);
		}

		public bool Exists(Predicate<T> match)
		{
			return theList.Exists(match);
		}

		public T Find(Predicate<T> match)
		{
			return theList.Find(match);
		}

		public List<T> FindAll(Predicate<T> match)
		{
			return theList.FindAll(match);
		}

		public int FindIndex(Predicate<T> match)
		{
			return theList.FindIndex(match);
		}

		public int FindIndex(int startIndex, Predicate<T> match)
		{
			return theList.FindIndex(startIndex, match);
		}

		public int FindIndex(int startIndex, int count, Predicate<T> match)
		{
			return theList.FindIndex(startIndex, count, match);
		}

		public T FindLast(Predicate<T> match)
		{
			return theList.FindLast(match);
		}

		public int FindLastIndex(Predicate<T> match)
		{
			return theList.FindLastIndex(match);
		}

		public int FindLastIndex(int startIndex, Predicate<T> match)
		{
			return theList.FindLastIndex(startIndex, match);
		}

		public int FindLastIndex(int startIndex, int count, Predicate<T> match)
		{
			return theList.FindLastIndex(startIndex, count, match);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return theList.GetEnumerator();
		}

		public int IndexOf(T item)
		{
			return theList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			InsertCommand command = new InsertCommand(theList, index, item);
			undoManager.Execute(command);
		}

		public void InsertRange(int index, IEnumerable<T> collection)
		{
			InsertRangeCommand command = new InsertRangeCommand(theList, index, collection);
			undoManager.Execute(command);
		}

		public int LastIndexOf(T item)
		{
			return theList.LastIndexOf(item);
		}

		public int LastIndexOf(T item, int index)
		{
			return theList.LastIndexOf(item, index);
		}

		public int LastIndexOf(T item, int index, int count)
		{
			return theList.LastIndexOf(item, index, count);
		}

		[Conditional("DEBUG")]
		private static void PopulateLists(ICollection<int> a, ICollection<int> b, int count)
		{
			Random random = new Random();
			for (int i = 0; i < count; i++)
			{
				int item = random.Next();
				a.Add(item);
				b.Add(item);
			}
		}

		public bool Redo()
		{
			return undoManager.Redo();
		}

		public bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index >= 0)
			{
				RemoveAtCommand command = new RemoveAtCommand(theList, index);
				undoManager.Execute(command);
				return true;
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			RemoveAtCommand command = new RemoveAtCommand(theList, index);
			undoManager.Execute(command);
		}

		public void RemoveRange(int index, int count)
		{
			RemoveRangeCommand command = new RemoveRangeCommand(theList, index, count);
			undoManager.Execute(command);
		}

		public void Reverse()
		{
			ReverseCommand command = new ReverseCommand(theList);
			undoManager.Execute(command);
		}

		public void Reverse(int index, int count)
		{
			ReverseCommand command = new ReverseCommand(theList, index, count);
			undoManager.Execute(command);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return theList.GetEnumerator();
		}

		[Conditional("DEBUG")]
		public static void Test()
		{
			const int capacity = 10;
			List<int> a = new List<int>(capacity);
			UndoableList<int> b = new UndoableList<int>(capacity);
			PopulateLists(a, b, capacity);
			TestAdd(a, b);
			TestClear(a, b);
			TestInsert(a, b);
			TestInsertRange(a, b);
			TestRemove(a, b);
			TestRemoveAt(a, b);
			TestRemoveRange(a, b);
			TestReverse(a, b);
		}

		[Conditional("DEBUG")]
		private static void TestAdd(IList<int> comparisonList, UndoableList<int> undoList)
		{
			TestEquals(comparisonList, undoList);
			Stack<int> stack = new Stack<int>();
			while (comparisonList.Count > 0)
			{
				stack.Push(comparisonList[comparisonList.Count - 1]);
				comparisonList.RemoveAt(comparisonList.Count - 1);
				Debug.Assert(undoList.Undo());
				TestEquals(comparisonList, undoList);
			}
			while (stack.Count > 0)
			{
				comparisonList.Add(stack.Pop());
				Debug.Assert(undoList.Redo());
				TestEquals(comparisonList, undoList);
			}
		}

		[Conditional("DEBUG")]
		private static void TestClear(ICollection<int> comparisonList, UndoableList<int> undoList)
		{
			TestEquals(comparisonList, undoList);
			undoList.Clear();
			Debug.Assert(undoList.Undo());
			TestEquals(comparisonList, undoList);
		}

		[Conditional("DEBUG")]
		private static void TestEquals(ICollection<int> a, ICollection<int> b)
		{
			bool condition = true;
			if (a.Count != b.Count)
			{
				condition = false;
			}
			IEnumerator<int> enumerator = a.GetEnumerator();
			IEnumerator<int> enumerator2 = b.GetEnumerator();
			while ((condition && enumerator.MoveNext()) && enumerator2.MoveNext())
			{
				condition = enumerator.Current.Equals(enumerator2.Current);
			}
			Debug.Assert(condition);
		}

		[Conditional("DEBUG")]
		private static void TestInsert(IList<int> comparisonList, UndoableList<int> undoList)
		{
			TestEquals(comparisonList, undoList);
			int index = comparisonList.Count/2;
			comparisonList.Insert(index, 0x3e7);
			undoList.Insert(index, 0x3e7);
			comparisonList.RemoveAt(index);
			Debug.Assert(undoList.Undo());
			TestEquals(comparisonList, undoList);
			comparisonList.Insert(index, 0x3e7);
			Debug.Assert(undoList.Redo());
			TestEquals(comparisonList, undoList);
		}

		[Conditional("DEBUG")]
		private static void TestInsertRange(List<int> comparisonList, UndoableList<int> undoList)
		{
			TestEquals(comparisonList, undoList);
			int[] collection = new int[] {1, 2, 3, 4, 5};
			int index = comparisonList.Count/2;
			comparisonList.InsertRange(index, collection);
			undoList.InsertRange(index, collection);
			TestEquals(comparisonList, undoList);
			comparisonList.RemoveRange(index, collection.Length);
			Debug.Assert(undoList.Undo());
			TestEquals(comparisonList, undoList);
			comparisonList.InsertRange(index, collection);
			Debug.Assert(undoList.Redo());
			TestEquals(comparisonList, undoList);
		}

		[Conditional("DEBUG")]
		private static void TestRemove(IList<int> comparisonList, UndoableList<int> undoList)
		{
			TestEquals(comparisonList, undoList);
			int index = comparisonList.Count/2;
			int item = comparisonList[index];
			comparisonList.Remove(item);
			undoList.Remove(item);
			TestEquals(comparisonList, undoList);
			comparisonList.Insert(index, item);
			Debug.Assert(undoList.Undo());
			TestEquals(comparisonList, undoList);
		}

		[Conditional("DEBUG")]
		private static void TestRemoveAt(IList<int> comparisonList, UndoableList<int> undoList)
		{
			TestEquals(comparisonList, undoList);
			int index = comparisonList.Count/2;
			int item = comparisonList[index];
			comparisonList.RemoveAt(index);
			undoList.RemoveAt(index);
			TestEquals(comparisonList, undoList);
			comparisonList.Insert(index, item);
			Debug.Assert(undoList.Undo());
			TestEquals(comparisonList, undoList);
		}

		[Conditional("DEBUG")]
		private static void TestRemoveRange(List<int> comparisonList, UndoableList<int> undoList)
		{
			TestEquals(comparisonList, undoList);
			int index = comparisonList.Count/2;
			int count = comparisonList.Count - index;
			List<int> range = comparisonList.GetRange(index, count);
			comparisonList.RemoveRange(index, count);
			undoList.RemoveRange(index, count);
			TestEquals(comparisonList, undoList);
			comparisonList.InsertRange(index, range);
			Debug.Assert(undoList.Undo());
			TestEquals(comparisonList, undoList);
		}

		[Conditional("DEBUG")]
		private static void TestReverse(List<int> comparisonList, UndoableList<int> undoList)
		{
			TestEquals(comparisonList, undoList);
			comparisonList.Reverse();
			undoList.Reverse();
			TestEquals(comparisonList, undoList);
			comparisonList.Reverse();
			Debug.Assert(undoList.Undo());
			TestEquals(comparisonList, undoList);
			comparisonList.Reverse();
			Debug.Assert(undoList.Redo());
			TestEquals(comparisonList, undoList);
			int count = comparisonList.Count/2;
			comparisonList.Reverse(0, count);
			undoList.Reverse(0, count);
			TestEquals(comparisonList, undoList);
			comparisonList.Reverse(0, count);
			Debug.Assert(undoList.Undo());
			TestEquals(comparisonList, undoList);
			comparisonList.Reverse(0, count);
			Debug.Assert(undoList.Redo());
			TestEquals(comparisonList, undoList);
		}

		public T[] ToArray()
		{
			return theList.ToArray();
		}

		public void TrimExcess()
		{
			theList.TrimExcess();
		}

		public bool TrueForAll(Predicate<T> match)
		{
			return theList.TrueForAll(match);
		}

		public bool Undo()
		{
			return undoManager.Undo();
		}

		public int Count
		{
			get { return theList.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public T this[int index]
		{
			get { return theList[index]; }
			set
			{
				SetCommand command = new SetCommand(theList, index, value);
				undoManager.Execute(command);
			}
		}

		public int RedoCount
		{
			get { return undoManager.RedoCount; }
		}

		public int UndoCount
		{
			get { return undoManager.UndoCount; }
		}

		private class ClearCommand : ICommand
		{
			private readonly IList<T> theList;
			private IList<T> undoList;
			private bool undone;

			public ClearCommand(IList<T> theList)
			{
				undone = true;
				this.theList = theList;
			}

			public void Execute()
			{
				if (!undone) return;
				undoList = new List<T>(theList);
				theList.Clear();
				undone = false;
			}

			public void Undo()
			{
				if (undone) return;
				Debug.Assert(theList.Count == 0);
				foreach (T local in undoList)
				{
					theList.Add(local);
				}
				undoList.Clear();
				undone = true;
			}
		}

		private class InsertCommand : ICommand
		{
			private int count;
			private readonly int index;
			private readonly T item;
			private readonly IList<T> theList;
			private bool undone;

			public InsertCommand(IList<T> theList, int index, T item)
			{
				undone = true;
				this.theList = theList;
				this.index = index;
				this.item = item;
			}

			public void Execute()
			{
				if (!undone) return;
				Debug.Assert((index >= 0) && (index <= theList.Count));
				count = theList.Count;
				theList.Insert(index, item);
				undone = false;
			}

			public void Undo()
			{
				if (undone) return;
				Debug.Assert((index >= 0) && (index <= theList.Count));
				T local = theList[index];
				Debug.Assert(local.Equals(item));
				theList.RemoveAt(index);
				undone = true;
				Debug.Assert(theList.Count == count);
			}
		}

		private class InsertRangeCommand : ICommand
		{
			private readonly int index;
			private readonly List<T> insertList;
			private readonly List<T> theList;
			private bool undone;

			public InsertRangeCommand(List<T> theList, int index, IEnumerable<T> collection)
			{
				undone = true;
				this.theList = theList;
				this.index = index;
				insertList = new List<T>(collection);
			}

			public void Execute()
			{
				if (!undone) return;
				Debug.Assert((index >= 0) && (index <= theList.Count));
				theList.InsertRange(index, insertList);
				undone = false;
			}

			public void Undo()
			{
				if (undone) return;
				Debug.Assert((index >= 0) && (index <= theList.Count));
				theList.RemoveRange(index, insertList.Count);
				undone = true;
			}
		}

		private class RemoveAtCommand : ICommand
		{
			private int count;
			private readonly int index;
			private T item;
			private readonly IList<T> theList;
			private bool undone;

			public RemoveAtCommand(IList<T> theList, int index)
			{
				undone = true;
				this.theList = theList;
				this.index = index;
			}

			public void Execute()
			{
				if (!undone) return;
				Debug.Assert((index >= 0) && (index < theList.Count));
				item = theList[index];
				count = theList.Count;
				theList.RemoveAt(index);
				undone = false;
			}

			public void Undo()
			{
				if (undone) return;
				Debug.Assert((index >= 0) && (index < theList.Count));
				theList.Insert(index, item);
				undone = true;
				Debug.Assert(theList.Count == count);
			}
		}

		private class RemoveRangeCommand : ICommand
		{
			private readonly int count;
			private readonly int index;
			private List<T> rangeList;
			private readonly List<T> theList;
			private bool undone;

			public RemoveRangeCommand(List<T> theList, int index, int count)
			{
				rangeList = new List<T>();
				undone = true;
				this.theList = theList;
				this.index = index;
				this.count = count;
			}

			public void Execute()
			{
				if (!undone) return;
				Debug.Assert((index >= 0) && (index < theList.Count));
				Debug.Assert((index + count) <= theList.Count);
				rangeList = new List<T>(theList.GetRange(index, count));
				theList.RemoveRange(index, count);
				undone = false;
			}

			public void Undo()
			{
				if (undone) return;
				theList.InsertRange(index, rangeList);
				undone = true;
			}
		}

		private class ReverseCommand : ICommand
		{
			private readonly int count;
			private readonly int index;
			private readonly bool reverseRange;
			private readonly List<T> theList;
			private bool undone;

			public ReverseCommand(List<T> theList)
			{
				undone = true;
				this.theList = theList;
				reverseRange = false;
			}

			public ReverseCommand(List<T> theList, int index, int count)
			{
				undone = true;
				this.theList = theList;
				this.index = index;
				this.count = count;
				reverseRange = true;
			}

			public void Execute()
			{
				if (!undone) return;
				if (reverseRange)
				{
					theList.Reverse(index, count);
				}
				else
				{
					theList.Reverse();
				}
				undone = false;
			}

			public void Undo()
			{
				if (undone) return;
				if (reverseRange)
				{
					theList.Reverse(index, count);
				}
				else
				{
					theList.Reverse();
				}
				undone = true;
			}
		}

		private class SetCommand : ICommand
		{
			private readonly int index;
			private readonly T newItem;
			private T oldItem;
			private readonly IList<T> theList;
			private bool undone;

			public SetCommand(IList<T> theList, int index, T item)
			{
				undone = true;
				this.theList = theList;
				this.index = index;
				newItem = item;
			}

			public void Execute()
			{
				if (!undone) return;
				Debug.Assert((index >= 0) && (index < theList.Count));
				oldItem = theList[index];
				theList[index] = newItem;
				undone = false;
			}

			public void Undo()
			{
				if (undone) return;
				Debug.Assert((index >= 0) && (index < theList.Count));
				T local = theList[index];
				Debug.Assert(local.Equals(newItem));
				theList[index] = oldItem;
				undone = true;
			}
		}
	}
}