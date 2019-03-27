using System.ComponentModel;
using WpfEditorTest.Utility;

namespace WpfEditorTest.Commands
{
	internal class CommandManager : INotifyPropertyChanged
	{
		public static CommandManager Instance { get; } = new CommandManager();

		private CommandManager()
		{
		}

		//private Stack<IEditorCommand> _doCommands = new Stack<IEditorCommand>();
		//private Stack<IEditorCommand> _undoCommands = new Stack<IEditorCommand>();

		public event PropertyChangedEventHandler PropertyChanged;
		//public event EventHandler<bool> ChangedDirty;

		public SceneDataContainer ObservableScene { get; internal set; }

		public bool UndoStackIsNotEmpty { get { return ObservableScene.UndoCommands.Count > 0; } }
		public bool RedoStackIsNotEmpty { get { return ObservableScene.DoCommands.Count > 0; } }

		//private bool _isDirty = false;
		//public bool IsDirty { get { return _isDirty; } private set { _isDirty = value; ChangedDirty?.Invoke(this, _isDirty); } }

		public bool TryUndoCommand()
		{
			if (ObservableScene.UndoCommands.Count == 0)
				return false;

			var movedCommand = ObservableScene.UndoCommands.Pop();
			movedCommand.Undo();
			ObservableScene.DoCommands.Push(movedCommand);
			ObservableScene.IsDirty |= movedCommand.IsDirty;
			CheckForCommandStacks();
			return true;
		}

		public bool TryRedoCommand()
		{
			if (ObservableScene.DoCommands.Count == 0)
				return false;

			var movedCommand = ObservableScene.DoCommands.Pop();
			movedCommand.Do();
			ObservableScene.UndoCommands.Push(movedCommand);
			ObservableScene.IsDirty |= movedCommand.IsDirty;
			CheckForCommandStacks();
			return true;
		}

		public void Execute(IEditorCommand command)
		{
			command.Do();
			ObservableScene.UndoCommands.Push(command);
			ObservableScene.DoCommands.Clear();
			ObservableScene.IsDirty |= command.IsDirty;
			CheckForCommandStacks();
		}

		public void ExecuteWithoutMemorising( IEditorCommand command )
		{
			command.Do();
		}

		public void ExecuteWithoutSettingDirty( IEditorCommand command )
		{
			command.Do();
			ObservableScene.UndoCommands.Push(command);
			ObservableScene.DoCommands.Clear();
			CheckForCommandStacks();
		}

		public void Reset()
		{
			ObservableScene.UndoCommands.Clear();
			ObservableScene.DoCommands.Clear();
			CheckForCommandStacks();
		}

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}

		public void SetNotDirty()
		{
			ObservableScene.IsDirty = false;
		}

		internal void CheckForCommandStacks()
		{
			OnPropertyChanged(nameof(UndoStackIsNotEmpty));
			OnPropertyChanged(nameof(RedoStackIsNotEmpty));
		}
	}
}
