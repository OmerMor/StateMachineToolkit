using System.Collections.Generic;

namespace Sanford.Collections.Generic
{
	internal class UndoManager
	{
		private readonly Stack<ICommand> redoStack = new Stack<ICommand>();
		private readonly Stack<ICommand> undoStack = new Stack<ICommand>();

		public void ClearHistory()
		{
			undoStack.Clear();
			redoStack.Clear();
		}

		public void Execute(ICommand command)
		{
			command.Execute();
			undoStack.Push(command);
			redoStack.Clear();
		}

		public bool Redo()
		{
			if (redoStack.Count == 0)
			{
				return false;
			}
			ICommand item = redoStack.Pop();
			item.Execute();
			undoStack.Push(item);
			return true;
		}

		public bool Undo()
		{
			if (undoStack.Count == 0)
			{
				return false;
			}
			ICommand item = undoStack.Pop();
			item.Undo();
			redoStack.Push(item);
			return true;
		}

		public int RedoCount
		{
			get { return redoStack.Count; }
		}

		public int UndoCount
		{
			get { return undoStack.Count; }
		}
	}
}