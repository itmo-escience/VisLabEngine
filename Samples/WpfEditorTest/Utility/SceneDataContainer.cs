using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Containers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using WpfEditorTest.Commands;

namespace WpfEditorTest.Utility
{
	public class SceneDataContainer : INotifyPropertyChanged
	{
		private string _sceneName = ApplicationConfig.BaseSceneName;
		private bool _isDirty = false;
		private string _sceneFileFullPath = null;

		public event PropertyChangedEventHandler PropertyChanged;

		public string SceneName { get => _sceneName + (IsDirty ? "*" : ""); set { _sceneName = value; OnPropertyChanged(); } }
		public string SceneFileFullPath {
			get => _sceneFileFullPath;
			set {
				_sceneFileFullPath = value;
				SceneName = _sceneFileFullPath.Split('\\').Last().Split('.').First();
				OnPropertyChanged();
			}
		}
		public FreePlacement Scene { get; set; }
		public bool IsDirty { get => _isDirty; set { _isDirty = value; SceneName = _sceneName; OnPropertyChanged(); ChangedDirty?.Invoke(this, _isDirty); } }
		public double SceneZoom { get; set; }
		public List<UIComponent> SceneSelection { get; set; }
		public Size SceneSize { get; set; }

		#region UndoRedoStack
		public Stack<IEditorCommand> DoCommands = new Stack<IEditorCommand>();
		public Stack<IEditorCommand> UndoCommands = new Stack<IEditorCommand>();

		public event EventHandler<bool> ChangedDirty;
		#endregion

		public SceneDataContainer()
		{
			SceneZoom = 1.0f;
			SceneSelection = new List<UIComponent>();
		}

		public SceneDataContainer( float width, float height ) : this()
		{
			Scene = new FreePlacement() { Name = "SCENE" };
			//Scene.Placement.X = 0;
			//Scene.Placement.Y = 0;
			Scene.DesiredWidth = width;
			Scene.DesiredHeight = height;
			SceneSize = new Size(width, height);
		}

		public SceneDataContainer( FreePlacement scene ) : this()
		{
			Scene = scene;
			SceneSize = new Size(Scene.Placement.Width, Scene.Placement.Height);
		}

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}
}
