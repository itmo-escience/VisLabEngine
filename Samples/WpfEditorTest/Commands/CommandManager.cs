using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WpfEditorTest.UndoRedo
{
	internal class CommandManager : INotifyPropertyChanged
	{
		public static CommandManager Instance { get; } = new CommandManager();

		private CommandManager()
		{
		}

		private Stack<IEditorCommand> _doCommands = new Stack<IEditorCommand>();
		private Stack<IEditorCommand> _undoCommands = new Stack<IEditorCommand>();

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler<bool> ChangedDirty;

		public bool UndoStackIsNotEmpty { get { return _undoCommands.Count > 0; } }
		public bool RedoStackIsNotEmpty { get { return _doCommands.Count > 0; } }

		private bool _isDirty = false;
		public bool IsDirty { get { return _isDirty; } private set { _isDirty = value; ChangedDirty?.Invoke(this, _isDirty); } }

		public bool TryUndoCommand()
		{
			if (_undoCommands.Count==0)
				return false;

			var movedCommand = _undoCommands.Pop();
			movedCommand.Undo();
			_doCommands.Push(movedCommand);
			IsDirty = true;
			OnPropertyChanged(nameof(UndoStackIsNotEmpty));
			OnPropertyChanged(nameof(RedoStackIsNotEmpty));
			return true;
		}

		public bool TryRedoCommand()
		{
			if (_doCommands.Count == 0)
				return false;

			var movedCommand = _doCommands.Pop();
			movedCommand.Do();
			_undoCommands.Push(movedCommand);
			IsDirty = true;
			OnPropertyChanged(nameof(UndoStackIsNotEmpty));
			OnPropertyChanged(nameof(RedoStackIsNotEmpty));
			return true;
		}

		public void Execute(IEditorCommand command)
		{
			command.Do();
			_undoCommands.Push(command);
			_doCommands.Clear();
			IsDirty = true;
			OnPropertyChanged(nameof(UndoStackIsNotEmpty));
			OnPropertyChanged(nameof(RedoStackIsNotEmpty));
		}

		public void ExecuteWithoutMemorising( IEditorCommand command )
		{
			command.Do();
		}

		public void ExecuteWithoutSettingDirty( IEditorCommand command )
		{
			command.Do();
			_undoCommands.Push(command);
			_doCommands.Clear();
			OnPropertyChanged(nameof(UndoStackIsNotEmpty));
			OnPropertyChanged(nameof(RedoStackIsNotEmpty));
		}

		public void Reset()
		{
			_undoCommands.Clear();
			_doCommands.Clear();
			OnPropertyChanged(nameof(UndoStackIsNotEmpty));
			OnPropertyChanged(nameof(RedoStackIsNotEmpty));
		}

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}

		public void SetNotDirty()
		{
			IsDirty = false;
		}
	}
}
