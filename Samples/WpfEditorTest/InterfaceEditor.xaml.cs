using Fusion.Engine.Common;
using FusionUI;
using GISTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Fusion.Engine.Frames;
using Fusion.Engine.Input;
using WpfEditorTest.ChildPanels;
using Frame = Fusion.Engine.Frames.Frame;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using CommandManager = WpfEditorTest.UndoRedo.CommandManager;
using WpfEditorTest.UndoRedo;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;
using System.Windows.Interop;
using System.Configuration;
using WpfEditorTest.FrameSelection;
using ZWpfLib;

namespace WpfEditorTest
{
	/// <summary>
	/// Interaction logic for InterfaceEditor.xaml
	/// </summary>
	public partial class InterfaceEditor : Window
	{
		public static RoutedCommand SaveFrameCmd = new RoutedCommand();
		public static RoutedCommand SaveSceneCmd = new RoutedCommand();
		public static RoutedCommand QuickSaveSceneCmd = new RoutedCommand();
		public static RoutedCommand NewSceneCmd = new RoutedCommand();
		public static RoutedCommand LoadSceneCmd = new RoutedCommand();
		public static RoutedCommand RedoChangeCmd = new RoutedCommand();
		public static RoutedCommand UndoChangeCmd = new RoutedCommand();
		public static RoutedCommand CopyFrameCmd = new RoutedCommand();
		public static RoutedCommand PasteFrameCmd = new RoutedCommand();
		public static RoutedCommand CutFrameCmd = new RoutedCommand();

		//private Point InitFramePosition;
		//private Frame InitFrameParent;

		private readonly FrameDetails _details;
		private readonly FramePalette _palette;
		private readonly FrameTreeView _treeView;
		//private readonly FrameSelectionPanel _frameSelectionPanel;

		Binding childrenBinding;
		private readonly Game _engine;

		public FusionUI.UI.ScalableFrame DragFieldFrame;
		public FusionUI.UI.ScalableFrame SceneFrame;
		public FusionUI.UI.MainFrame RootFrame;
		private string currentSceneFile;
		private string sceneChangedIndicator;
		private string titleWithFileName = ApplicationConfig.BaseTitle + " - " + ApplicationConfig.BaseSceneName;
		private string xmlFrameBuffer;
		private Frame frameBuffer;

		public string CurrentSceneFile { get => currentSceneFile; set { currentSceneFile = value; this.UpdateTitle(); } }
		public string SceneChangedIndicator { get => sceneChangedIndicator; set { sceneChangedIndicator = value; this.UpdateTitle(); } }


        public InterfaceEditor()
		{
			InitializeComponent();

			_engine = new Game("TestGame");
			_engine.Mouse = new DummyMouse(_engine);
			_engine.Keyboard = new DummyKeyboard(_engine);
			_engine.Touch = new DummyTouch(_engine);

			_engine.GameServer = new CustomGameServer(_engine);
			_engine.GameClient = new CustomGameClient(_engine);
			_engine.GameInterface = new CustomGameInterface(_engine);
			_engine.LoadConfiguration("Config.ini");

			_engine.RenderSystem.StereoMode = Fusion.Engine.Graphics.StereoMode.WpfEditor;
			_engine.RenderSystem.Width = 1920;
			_engine.RenderSystem.Height = 1080;
			_engine.RenderSystem.VSyncInterval = 1;

			Directory.SetCurrentDirectory(@"..\..\..\..\GISTest\bin\x64\Debug");
			_engine.InitExternal();


            //_frameSelectionPanel = new FrameSelectionPanel(this);
            //LocalGrid.Children.Add(_frameSelectionPanel);

            _details = new FrameDetails();
			_treeView = new FrameTreeView();
			_palette = new FramePalette();

			miFrameProperties.Tag = _details;
			miFrameTemplates.Tag = _palette;
			miSceneTreeView.Tag = _treeView;


			SourceInitialized += ( _, args ) =>
			{
				_details.Owner = this;
				_details.Show();
				_treeView.Owner = this;
				_treeView.Show();
				_palette.Owner = this;
				_palette.Show();
            };

			_treeView.SelectedFrameChangedInUI += ( _, frame ) =>
			{
				var command = new SelectFrameCommand(new List<Frame> { frame });
				CommandManager.Instance.Execute(command);
			}; //SelectFrame(frame);
			_treeView.RequestFrameDeletionInUI += ( _, __ ) => TryDeleteSelectedFrame();

			var templates = Directory.GetFiles(ApplicationConfig.TemplatesPath, "*.xml").ToList();
			_palette.AvailableFrames.ItemsSource = templates.Select(t => t.Split('\\').Last().Split('.').First());

			RootFrame = ApplicationInterface.Instance.rootFrame;
			SceneFrame = (FusionUI.UI.ScalableFrame)RootFrame.Children.FirstOrDefault();
			DragFieldFrame = new FusionUI.UI.ScalableFrame(0, 0, RootFrame.UnitWidth, RootFrame.UnitHeight, "DragFieldFrame", Fusion.Core.Mathematics.Color.Zero)
			{
				Anchor = FrameAnchor.All,
				ZOrder = 1000000,
			};
			RootFrame.Add(DragFieldFrame);

			SelectionLayer.Window = this;
			SelectionLayer.paletteWindow = _palette;
		    SelectionLayer.DxElem.Renderer = _engine;

            var b = new Binding("Children")
			{
				Source = SceneFrame,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
				Mode = BindingMode.OneWay
			};
			_treeView.ElementHierarchyView.SetBinding(TreeView.ItemsSourceProperty, b);

			UndoButton.DataContext = CommandManager.Instance;
			RedoButton.DataContext = CommandManager.Instance;


			CommandManager.Instance.ChangedDirty += ( s, e ) => {
				this.SceneChangedIndicator = e ? "*" : "";
			};

			SelectionLayer.FrameSelected += ( s, selection ) => {
				_details.SetSelectFrame(selection.Frame);
				_treeView.SelectedFrame = selection.Frame;
			};
			SelectionLayer.FramesDeselected += ( s, e ) => {
				_details.FrameDetailsControls.ItemsSource = null;
			};
			this.UpdateTitle();
			rb1.IsChecked = true;
		}


	    protected override void OnSourceInitialized( EventArgs e )
		{
			base.OnSourceInitialized(e);
			SelectionLayer.DxElem.HandleInput(this);
		}

		private void UpdateTitle()
		{
			this.Title = ApplicationConfig.BaseTitle + " - " + (!string.IsNullOrEmpty(currentSceneFile) ? CurrentSceneFile : ApplicationConfig.BaseSceneName) + SceneChangedIndicator;
		}

		private void Window_KeyDown( object sender, KeyEventArgs e )
		{
			if (e.Key == Key.Delete)
			{
				this.TryDeleteSelectedFrame();
			}
		}

		private void TryDeleteSelectedFrame()
		{
			if (SelectionLayer.frameSelectionPanelList.Count >= 0)
			{
				List<IEditorCommand> commands = new List<IEditorCommand>();
				commands.Add(new SelectFrameCommand(new List<Frame> { }));
				foreach (var frame in SelectionLayer.frameSelectionPanelList.Keys)
				{
					commands.Add(new FrameParentChangeCommand(frame, null));
				}
				var command = new CommandGroup(commands.ToArray());
				CommandManager.Instance.Execute(command);
			}
		}

		#region Save/load stuff

		internal void TryLoadSceneAsTemplate()
		{
			if (!this.CheckForChanges())
				return;

			var startPath = Path.GetFullPath(Path.Combine(ApplicationConfig.TemplatesPath, ".."));
			var filter = "XML files(*.xml)| *.xml";
			using (var dialog = new System.Windows.Forms.OpenFileDialog() { InitialDirectory = startPath, Multiselect = false, Filter = filter })
			{
				if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

				var createdFrame = CreateFrameFromFile(dialog.FileName);
				if (createdFrame != null && createdFrame.GetType() == typeof(FusionUI.UI.ScalableFrame))
				{
					RootFrame.Remove(SceneFrame);
					foreach (var panel in SelectionLayer.frameSelectionPanelList.Values)
					{
						var commands = SelectionLayer.ResetSelectedFrame(new Point(0, 0), panel);
						var command = new CommandGroup(commands.ToArray());
						CommandManager.Instance.ExecuteWithoutMemorising(command);
					}
					SceneFrame = (FusionUI.UI.ScalableFrame)createdFrame;
					RootFrame.Add(SceneFrame);
					DragFieldFrame.ZOrder = 1000000;

					childrenBinding = new Binding("Children")
					{
						Source = SceneFrame,
					};
					_treeView.ElementHierarchyView.SetBinding(TreeView.ItemsSourceProperty, childrenBinding);

					this.CurrentSceneFile = dialog.FileName;
					CommandManager.Instance.SetNotDirty();
					CommandManager.Instance.Reset();
				}
			}
		}

		private void TrySetNewScene()
		{
			if (!this.CheckForChanges())
				return;
			RootFrame.Remove(SceneFrame);
			foreach (var panel in SelectionLayer.frameSelectionPanelList.Values)
			{
				var commands = SelectionLayer.ResetSelectedFrame(new Point(0, 0), panel);
				var command = new CommandGroup(commands.ToArray());
				CommandManager.Instance.ExecuteWithoutMemorising(command);
			}
			SceneFrame = new FusionUI.UI.ScalableFrame(0, 0, this.RootFrame.UnitWidth, this.RootFrame.UnitHeight, "Scene", Fusion.Core.Mathematics.Color.Zero) { Anchor = FrameAnchor.All };
			RootFrame.Add(SceneFrame);
			DragFieldFrame.ZOrder = 1000000;

			childrenBinding = new Binding("Children")
			{
				Source = SceneFrame,
			};
			_treeView.ElementHierarchyView.SetBinding(TreeView.ItemsSourceProperty, childrenBinding);
			this.CurrentSceneFile = null;
			CommandManager.Instance.SetNotDirty();
			CommandManager.Instance.Reset();
		}

		internal void TrySaveSceneAsTemplate()
		{
			var startPath = Path.GetFullPath(Path.Combine(ApplicationConfig.TemplatesPath, ".."));
			var filter = "XML files(*.xml)| *.xml";
			using (var dialog = new System.Windows.Forms.SaveFileDialog() { InitialDirectory = startPath, Filter = filter })
			{
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					Fusion.Core.Utils.FrameSerializer.Write(SceneFrame, dialog.FileName);
					this.CurrentSceneFile = dialog.FileName;
					CommandManager.Instance.SetNotDirty();
				}
			}
		}

		internal List<IEditorCommand> AddframeToScene( Frame createdFrame, Point point, List<IEditorCommand> commands )
		{
			var hoveredFrame = SelectionLayer.GetHoveredFrameOnScene(point, false) ?? SceneFrame;

			commands.Add(new CommandGroup(
				new FrameParentChangeCommand(createdFrame, hoveredFrame),
				new FramePropertyChangeCommand(createdFrame, "X", (int)point.X - hoveredFrame.GlobalRectangle.X - createdFrame.Width / 2),
				new FramePropertyChangeCommand(createdFrame, "Y", (int)point.Y - hoveredFrame.GlobalRectangle.Y - createdFrame.Height / 2),
				new SelectFrameCommand(new List<Frame> { createdFrame })
			));

			return commands;
		}

		internal void TrySaveScene()
		{
			if (!String.IsNullOrEmpty(this.CurrentSceneFile))
			{
				Fusion.Core.Utils.FrameSerializer.Write(SceneFrame, this.CurrentSceneFile);
				CommandManager.Instance.SetNotDirty();
			}
			else
			{
				TrySaveSceneAsTemplate();
			}
		}

		internal void TrySaveFrameAsTemplate()
		{
			if (SelectionLayer.frameSelectionPanelList.Count == 1)
			{
				if (!Directory.Exists(ApplicationConfig.TemplatesPath))
				{
					Directory.CreateDirectory(ApplicationConfig.TemplatesPath);
				}
				var selectedFrame = SelectionLayer.frameSelectionPanelList.FirstOrDefault().Value.SelectedFrame;

				Fusion.Core.Utils.FrameSerializer.Write(selectedFrame, ApplicationConfig.TemplatesPath + "\\" + (selectedFrame.Text ?? selectedFrame.GetType().ToString()) + ".xml");
				this.LoadPalettes();
			}
		}

		public void LoadPalettes()
		{
			if (Directory.Exists(ApplicationConfig.TemplatesPath))
			{
				var templates = Directory.GetFiles(ApplicationConfig.TemplatesPath, "*.xml").ToList();
				_palette.AvailableFrames.ItemsSource = templates.Select(t => t.Split('\\').Last().Split('.').First());
			}
		}

		public Frame CreateFrameFromFile( string filePath )
		{
			Frame createdFrame;
			try
			{
				Fusion.Core.Utils.FrameSerializer.Read(filePath, out createdFrame);
			}
			catch (Exception)
			{

				throw;
			}
			return createdFrame;
		}

		private void ExecutedSaveFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
			TrySaveFrameAsTemplate();
		}

		private void ExecutedSaveSceneCommand( object sender, ExecutedRoutedEventArgs e )
		{
			TrySaveSceneAsTemplate();
		}

		private void ExecutedQuickSaveSceneCommand( object sender, ExecutedRoutedEventArgs e )
		{
			TrySaveScene();
		}

		private void ExecutedNewSceneCommand( object sender, ExecutedRoutedEventArgs e )
		{
			TrySetNewScene();
		}

		private void ExecutedLoadSceneCommand( object sender, ExecutedRoutedEventArgs e )
		{
			TryLoadSceneAsTemplate();
		}

		private void ExecutedUndoChangeCommand( object sender, ExecutedRoutedEventArgs e )
		{
			CommandManager.Instance.TryUndoCommand();
		}

		private void ExecutedRedoChangeCommand( object sender, ExecutedRoutedEventArgs e )
		{
			CommandManager.Instance.TryRedoCommand();
		}

		private void ExecutedCopyFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
			if (SelectionLayer.frameSelectionPanelList.Count == 1)
			{
				var selectedFrame = SelectionLayer.frameSelectionPanelList.FirstOrDefault().Value.SelectedFrame;
				if (selectedFrame != null)
				{
					xmlFrameBuffer = Fusion.Core.Utils.FrameSerializer.WriteToString(selectedFrame);
					//Clipboard.SetData(DataFormats.Text, (Object)xmlFrame);
					var upperLeft = this.PointToScreen(new Point(selectedFrame.X, selectedFrame.Y));
					var lowerRight = this.PointToScreen(new Point(selectedFrame.X + selectedFrame.Width, selectedFrame.Y + selectedFrame.Height));

					var img = this.CopyScreen(
						(int)upperLeft.X,
						(int)upperLeft.Y + (int)SystemParameters.WindowCaptionHeight + 9,
						(int)lowerRight.X,
						(int)lowerRight.Y + (int)SystemParameters.WindowCaptionHeight + 9
						);
					Clipboard.SetData(DataFormats.Bitmap, (Object)img);
				}
			}
		}

		private void ExecutedCutFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
			ExecutedCopyFrameCommand(sender, e);
			TryDeleteSelectedFrame();
		}

		private void ExecutedPasteFrameCmdCommand( object sender, ExecutedRoutedEventArgs e )
		{
			if (!string.IsNullOrEmpty(xmlFrameBuffer))
			{
				List<IEditorCommand> commands = new List<IEditorCommand>();
				this.AddframeToScene(Fusion.Core.Utils.FrameSerializer.ReadFromString(xmlFrameBuffer),
					System.Windows.Input.Mouse.GetPosition(this), commands);
				var command = new CommandGroup(commands.ToArray());
				CommandManager.Instance.Execute(command);
			}
		}

		private void AlwaysCanExecute( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = true;
		}

		#endregion

		#region ImageDrawing
		private BitmapSource CopyScreen( int left, int top, int right, int bottom )
		{
			using (var screenBmp = new Bitmap(
				right - left,
				bottom - top
				))
			{
				using (var bmpGraphics = System.Drawing.Graphics.FromImage(screenBmp))
				{
					bmpGraphics.CopyFromScreen(left, top, 0, 0, screenBmp.Size);
					return Imaging.CreateBitmapSourceFromHBitmap(
						screenBmp.GetHbitmap(),
						IntPtr.Zero,
						Int32Rect.Empty,
						BitmapSizeOptions.FromEmptyOptions());
				}
			}
		}
		#endregion

		private void Window_Closing( object sender, CancelEventArgs e )
		{

			if (!this.CheckForChanges())
			{
				e.Cancel = true;
				return;
			}


			var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var settings = configFile.AppSettings.Settings;
			settings["DetailsPanelX"].Value = _details.Left.ToString();
			settings["DetailsPanelY"].Value = _details.Top.ToString();
			settings["TreeViewPanelX"].Value = _treeView.Left.ToString();
			settings["TreeViewPanelY"].Value = _treeView.Top.ToString();
			settings["PalettePanelX"].Value = _palette.Left.ToString();
			settings["PalettePanelY"].Value = _palette.Top.ToString();
			settings["DetailsPanelVisibility"].Value = _details.Visibility.ToString();
			settings["PalettePanelVisibility"].Value = _palette.Visibility.ToString();
			settings["TreeViewPanelVisibility"].Value = _treeView.Visibility.ToString();

			configFile.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
		}

		private bool CheckForChanges()
		{
			if (CommandManager.Instance.IsDirty)
			{
				var result = MessageBox.Show(@"There are unsaved changes in the current scene.
								Do you want to save them?", "Attention", MessageBoxButton.YesNoCancel);
				switch (result)
				{
					case MessageBoxResult.Yes:
						{
							this.TrySaveScene();
							return true;
						}
					case MessageBoxResult.No:
						{
							return true;
						}
					case MessageBoxResult.Cancel:
						{
							return false;
						}
					default:
						{
							return true;
						}
				}
			}
			else
			{
				return true;
			}
		}

		private void MenuItem_Click( object sender, RoutedEventArgs e )
		{
			Window panel = (sender as MenuItem).Tag as Window;
			panel.Visibility = Visibility.Visible;
			panel.Focus();
		}

		private void CheckBox_Checked( object sender, RoutedEventArgs e )
		{
			var chbx = sender as CheckBox;

			SelectionLayer.ToggleGridLines(chbx.IsChecked != null ? (bool)chbx.IsChecked : false);
		}

		private void rb_Checked( object sender, RoutedEventArgs e )
		{
			var rBtn = sender as RadioButton;
			SelectionLayer.GridSizeMultiplier = SelectionLayer.GridScaleNumbers[rBtn.Content.ToString()];
		}
	}
}
