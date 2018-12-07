﻿using System.Collections.Generic;

namespace WpfEditorTest.UndoRedo
{
	public class CommandManager
	{
		public static CommandManager Instance { get; } = new CommandManager();

		private CommandManager()
		{

		}

		private Stack<IEditorCommand> _doCommands = new Stack<IEditorCommand>();
		private Stack<IEditorCommand> _undoCommands = new Stack<IEditorCommand>();

		public bool UndoStackIsNotEmpty { get { return _undoCommands.Count > 0; } }
		public bool RedoStackIsNotEmpty { get { return _doCommands.Count > 0; } }

		public bool TryUndoCommand()
		{
			if (_undoCommands.Count==0)
				return false;

			var movedCommand = _undoCommands.Pop();
			movedCommand.Undo();
			_doCommands.Push(movedCommand);
			return true;
		}

		public bool TryRedoCommand()
		{
			if (_doCommands.Count == 0)
				return false;

			var movedCommand = _doCommands.Pop();
			movedCommand.Do();
			_undoCommands.Push(movedCommand);
			return true;
		}

		public void Execute(IEditorCommand command)
		{
			command.Do();
			_undoCommands.Push(command);
			_doCommands.Clear();
		}

		public void Reset()
		{
			_undoCommands.Clear();
			_doCommands.Clear();
		}
	}
}
