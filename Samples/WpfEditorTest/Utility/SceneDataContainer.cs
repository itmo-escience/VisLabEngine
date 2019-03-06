using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Containers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfEditorTest.Utility
{
	public class SceneDataContainer : INotifyPropertyChanged
	{
		private string _sceneName = ApplicationConfig.BaseSceneName;
		private bool _isDirty = false;

		public event PropertyChangedEventHandler PropertyChanged;

		public string SceneName { get => _sceneName + (IsDirty ? "*" : ""); set { _sceneName = value; OnPropertyChanged(); } }
		public string SceneFileFullPath { get; set; } = null;
		public UIContainer Scene { get; set; }
		public bool IsDirty { get => _isDirty; set { _isDirty = value; SceneName = _sceneName; OnPropertyChanged(); } }
		public double SceneZoom { get; set; }
		public List<UIComponent> SceneSelection { get; set; }
		public Size SceneSize { get; set; }

		public SceneDataContainer()
		{
			SceneZoom = 1.0f;
			SceneSelection = new List<UIComponent>();
		}

		public SceneDataContainer( float width, float height ) : this()
		{
			Scene = new FreePlacement(0, 0, width, height) { Name = "SCENE" };
			SceneSize = new Size(width, height);
		}

		public SceneDataContainer( UIContainer scene ) : this()
		{
			Scene = scene;
			SceneSize = new Size(Scene.Width, Scene.Height);
		}

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}
}
