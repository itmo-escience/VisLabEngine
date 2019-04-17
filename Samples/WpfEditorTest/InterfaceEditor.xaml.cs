using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Fusion.Engine.Common;
using Fusion.Engine.Client;
using Fusion.Engine.Input;
using Fusion.Engine.Server;
using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Frames2.Managing;
using FusionUI;
using WpfEditorTest.Utility;
using WpfEditorTest.ChildWindows;
using WpfEditorTest.Commands;
using WpfEditorTest.DialogWindows;
using WpfEditorTest.FrameSelection;
using WpfEditorTest.Utility;

using CommandManager = WpfEditorTest.Commands.CommandManager;

using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Bitmap = System.Drawing.Bitmap;
using Keyboard = System.Windows.Input.Keyboard;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using RadioButton = System.Windows.Controls.RadioButton;
using TabControl = System.Windows.Controls.TabControl;
using TreeView = System.Windows.Controls.TreeView;

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
		public static RoutedCommand MoveFrameCmd = new RoutedCommand();
		public static RoutedCommand DeleteFrameCmd = new RoutedCommand();
		public static RoutedCommand AlignFrameCmd = new RoutedCommand();
		public static RoutedCommand SceneConfigCmd = new RoutedCommand();
		public static RoutedCommand RemoveSelectionCmd = new RoutedCommand();

		private readonly SlotDetailsWindow _slotDetails;
		private readonly FrameDetails _details;
		private readonly FramePalette _palette;
		private readonly FrameTreeView _treeView;
		private readonly ConsoleWindow _consoleWindow;
		private WPFSelectionUILayer SelectionLayer;

		private Binding _childrenBinding;

		private readonly Game _engine;
		private UIManager _uiManager;

		public FreePlacement DragFieldFrame;
		public FreePlacement SceneFrame;
		public FreePlacement RootFrame;
		private List<Type> customUIComponentTypes;
		private string _currentSceneFile;
		private string _sceneChangedIndicator;
		private string _titleWithFileName = ApplicationConfig.BaseTitle + " - " + ApplicationConfig.BaseSceneName;
		private List<string> _xmlComponentsBuffer = new List<string>();
		private List<Vector> _componentsOffsetBuffer = new List<Vector>();

		public ObservableCollection<SceneDataContainer> LoadedScenes { get; set; } = new ObservableCollection<SceneDataContainer>();
		public SceneDataContainer CurrentScene {
			get => _currentScene;
			set
			{
				if (_currentScene != null && RootFrame.Slots.Contains(_currentScene.Scene.Placement))
				{
					_currentScene.SceneSelection = SelectionManager.Instance.SelectedFrames;
					_currentScene.SceneZoom = ZoomerSlider.Value;
					_currentScene.SceneSize = new Size(SelectionLayer.Width, SelectionLayer.Height);

					SelectionManager.Instance.SelectFrame(new List<UIComponent> { });
					CommandManager.Instance.ExecuteWithoutMemorising(
						new UIComponentParentChangeCommand(_currentScene.Scene, null, _currentScene.Scene.Parent() as IUIModifiableContainer<ISlot>)
					);
					//RootFrame.Remove(_currentScene.Scene);
					CurrentScene.ChangedDirty -= UpdateSceneIndicator;
					foreach (var panel in SelectionLayer.FrameSelectionPanelList.Values)
					{
						var command = SelectionLayer.ResetSelectedFrame(new Point(0, 0), panel);
						CommandManager.Instance.ExecuteWithoutMemorising(command);
					}
					BindingOperations.DisableCollectionSynchronization(_currentScene.Scene.Slots);
				}


				_currentScene = value;

				CommandManager.Instance.ObservableScene = _currentScene;
				CurrentScene.ChangedDirty += UpdateSceneIndicator;

				CommandManager.Instance.ExecuteWithoutMemorising(
					new UIComponentParentChangeCommand(_currentScene.Scene, RootFrame, _currentScene.Scene.Parent() as IUIModifiableContainer<ISlot>)
				);
				//RootFrame.Add(_currentScene.Scene);
				SceneFrame = _currentScene.Scene;
				ZoomerSlider.Value = _currentScene.SceneZoom;
				SelectionLayer.Width = SceneFrame.DesiredWidth;
				SelectionLayer.Height = SceneFrame.DesiredHeight;

			    CurrentSceneFile = _currentScene.SceneFileFullPath;

				CommandManager.Instance.CheckForCommandStacks();
			}
		}

		private void UpdateSceneIndicator( object sender, bool isDirty )
		{
			this.SceneChangedIndicator = isDirty ? "*" : "";
		}

		public string CurrentSceneFile { get => _currentSceneFile; set { _currentSceneFile = value; this.UpdateTitle(); } }
		public string SceneChangedIndicator { get => _sceneChangedIndicator; set { _sceneChangedIndicator = value; this.UpdateTitle(); } }

		public double DefaultSceneWidth { get; private set; }
		public double DefaultSceneHeight { get; private set; }
		private double sceneScale = 1;

		public Dictionary<string, Point> KeyboardArrowsSteps = new Dictionary<string, Point>
		{
			{ "up", new Point(0,-1) },
			{ "down", new Point(0,1) },
			{ "left", new Point(-1,0) },
			{ "right", new Point(1,0) },
		};
		private SceneDataContainer _currentScene;

		public InterfaceEditor()
		{
			InitializeComponent();

			Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 40 });

			_engine = new Game("TestGame");
			_engine.Mouse = new DummyMouse(_engine);
			_engine.Keyboard = new DummyKeyboard(_engine);
			_engine.Touch = new DummyTouch(_engine);


			OpenFileDialog openFileDialog1 = new OpenFileDialog();

			openFileDialog1.InitialDirectory = ConfigurationManager.AppSettings.Get("InitialAssemblySearchDirectory");
			openFileDialog1.Filter = "Assembly files (*.exe, *.dll)|*.exe;*.dll";
			openFileDialog1.FilterIndex = 0;
			openFileDialog1.RestoreDirectory = true;

			var fileName = @"";

			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				fileName = openFileDialog1.FileName;
				var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				configFile.AppSettings.Settings["InitialAssemblySearchDirectory"].Value = Path.GetDirectoryName(fileName);
				configFile.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
			}
			else
			{
				System.Windows.Application.Current.Shutdown();
				return;
			}

			if (String.IsNullOrEmpty(fileName) || !File.Exists(fileName)) {
				throw new FileNotFoundException("File not found", fileName);
			}

			var assem = Assembly.LoadFrom(fileName);

			try
			{
				foreach (var type in assem.GetTypes())
				{
					if (type.IsSubclassOf(typeof(GameServer)) && _engine.GameServer == null)
					{
						_engine.GameServer = (GameServer)Activator.CreateInstance(type, _engine);
						continue;
					}
					if (type.IsSubclassOf(typeof(GameClient)) && _engine.GameClient == null)
					{
						_engine.GameClient = (GameClient)Activator.CreateInstance(type, _engine);
						continue;
					}
					if (type.IsSubclassOf(typeof(UserInterface)) && _engine.GameInterface == null)
					{
						_engine.GameInterface = (UserInterface)Activator.CreateInstance(type, _engine);
						continue;
					}
				}

				if (_engine.GameServer == null || _engine.GameClient == null || _engine.GameInterface == null)
				{
					throw new Exception("No classes found in Assembly: " + fileName);
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK);
				System.Windows.Application.Current.Shutdown();
				return;
				//throw;
			}
			Directory.SetCurrentDirectory(Path.GetDirectoryName(fileName));

			_engine.LoadConfiguration("Config.ini");

			DefaultSceneWidth = int.Parse(ConfigurationManager.AppSettings.Get("SceneSizeWidth"));
			DefaultSceneHeight = int.Parse(ConfigurationManager.AppSettings.Get("SceneSizeHeight"));

			_engine.RenderSystem.StereoMode = Fusion.Engine.Graphics.StereoMode.WpfEditor;
			_engine.RenderSystem.Width = (int)DefaultSceneWidth;
			_engine.RenderSystem.Height = (int)DefaultSceneHeight;
			_engine.RenderSystem.VSyncInterval = 1;

			var tokenSource = new CancellationTokenSource();

			var fusionThread = new Thread(() => _engine.RunExternal(tokenSource.Token));
			fusionThread.Name = "Fusion";

			Closing += ( sender, args ) => { if (args.Cancel == false) tokenSource.Cancel(); };

			this.customUIComponentTypes = (_engine.GameInterface as ICustomizableUI)?.CustomUIComponentTypes;

			_slotDetails = new SlotDetailsWindow();
			_details = new FrameDetails();
			_treeView = new FrameTreeView();
			_palette = new FramePalette();
			_consoleWindow = new ConsoleWindow(_engine.Invoker);

			miSlotDetails.Tag = _slotDetails;
			miFrameProperties.Tag = _details;
			miFrameTemplates.Tag = _palette;
			miSceneTreeView.Tag = _treeView;
			miConsole.Tag = _consoleWindow;

			SourceInitialized += ( _, args ) =>
			{
				_slotDetails.Owner = this;
				_slotDetails.Show();
				if (!bool.Parse(ConfigurationManager.AppSettings.Get("SlotDetailsWindowVisibility")))
					_slotDetails.Hide();
				_details.Owner = this;
				_details.Show();
				if (!bool.Parse(ConfigurationManager.AppSettings.Get("DetailsPanelVisibility")))
					_details.Hide();
				_treeView.Owner = this;
				_treeView.Show();
				if (!bool.Parse(ConfigurationManager.AppSettings.Get("TreeViewPanelVisibility")))
					_treeView.Hide();
				_palette.Owner = this;
				_palette.Show();
				if (!bool.Parse(ConfigurationManager.AppSettings.Get("PalettePanelVisibility")))
					_palette.Hide();
				_consoleWindow.Owner = this;
				_consoleWindow.Show();
				if (!bool.Parse(ConfigurationManager.AppSettings.Get("ConsoleWindowVisibility")))
					_consoleWindow.Hide();
			};

			_treeView.SelectedFrameChangedInUI += ( _, frame ) =>
			{
				var command = new SelectFrameCommand(frame);
				CommandManager.Instance.ExecuteWithoutSettingDirty(command);
			};
			_treeView.ControllerSlotSelected += ( _, slot ) =>
			{
				_slotDetails.SetSelectFrame(slot);
			};
			_treeView.RequestFrameDeletionInUI += ( _, __ ) => TryDeleteSelectedFrame();

			LoadPalettes();


			_palette.AvailableComponents.ItemsSource = customUIComponentTypes;



			_palette.BaseComponents.ItemsSource = Assembly.GetAssembly(typeof(UIComponent)).GetTypes()
				.Where(t => t.GetInterfaces().Contains(typeof(UIComponent)) && !t.IsAbstract/* && !t.IsAbstract*//* && t != typeof(StartClippingFlag) && t != typeof(EndClippingFlag)*/);





			_engine.OnInitialized += () =>
			{
				Application.Current.Dispatcher.InvokeAsync(() =>
				{
					_uiManager = (_engine.GameInterface as ICustomizableUI)?.GetUIManager();
					DebugCheckBox.IsChecked = _uiManager.DebugEnabled;

					#region SelectionLayer
					SelectionLayer = new WPFSelectionUILayer(_uiManager);
					Zoomer.Child = SelectionLayer;
					SelectionLayer.Window = this;
					SelectionLayer.PaletteWindow = _palette;
					SelectionLayer.FrameSelected += ( s, selection ) =>
					{
						_details.SetSelectFrame(selection.Frame);
						_treeView.SelectedFrame = selection.Frame;
					};
					SelectionManager.Instance.PlacementRecreated += ( s, component ) =>
					{
						_details.SetSelectFrame(component);
					};
					SelectionLayer.FramesDeselected += ( s, e ) =>
					{
						_details.DetailsScroll.DataContext = null;
						_details.DetailsScroll.Visibility = Visibility.Collapsed;
					};
					SelectionLayer.SizeChanged += ( s, e ) =>
					{
						this.DefaultSceneWidth = e.NewSize.Width;
						this.DefaultSceneHeight = e.NewSize.Height;
						if (SceneFrame != null)
						{
							SceneFrame.DesiredWidth = (float)e.NewSize.Width;
							SceneFrame.DesiredHeight = (float)e.NewSize.Height;

							DragFieldFrame.DesiredWidth = (float)e.NewSize.Width;
							DragFieldFrame.DesiredHeight = (float)e.NewSize.Height;
						}
						ZoomScene(1);
					};

					var loadedGridSize = ConfigurationManager.AppSettings.Get("GridSize");

					foreach (RadioButton item in GridSizeMenuItem.Items)
					{
						if (item.Tag.ToString() == loadedGridSize)
						{
							item.IsChecked = true;
							break;
						}
					}
					#endregion


					RootFrame = _uiManager.Root;
					if (RootFrame == null) {
						throw new Exception("RootFrame is null, looks like GameInterface is not implementing ICustomizableUI");
					}
					var startScene = new SceneDataContainer(RootFrame.Placement.Width, RootFrame.Placement.Height);
					SceneFrame = startScene.Scene;
					SceneFrame.Name = "SceneFrame";
					foreach (var child in RootFrame.Slots.Select(s=>s.Component))
					{
						RootFrame.Remove(child);
						SceneFrame.Insert(child,int.MaxValue);
					}
					DragFieldFrame = new FreePlacement() { Name = "DRAG_FIELD", DesiredWidth = RootFrame.Placement.Width, DesiredHeight = RootFrame.Placement.Height };

					RootFrame.Add(DragFieldFrame);

					LoadedScenes.Add(startScene);
					LoadedScenesTabs.SelectedIndex = LoadedScenes.IndexOf(startScene);

					SelectionLayer.DxElem.Renderer = _engine;
					SelectionLayer.DxElem.HandleInput(this);

					UndoButton.DataContext = CommandManager.Instance;
					RedoButton.DataContext = CommandManager.Instance;

					RearrangeWorkspaceSize();
				});
			};


			SaveTemplateButton.DataContext = SelectionManager.Instance;
			LoadedScenesTabs.DataContext = LoadedScenes;


			this.UpdateTitle();

			fusionThread.Start();
		}

		protected override void OnSourceInitialized( EventArgs e )
		{
			base.OnSourceInitialized(e);
			//SelectionLayer.DxElem.HandleInput(this);
		}

		private void UpdateTitle()
		{
			this.Title = ApplicationConfig.BaseTitle + " - " + (!string.IsNullOrEmpty(_currentSceneFile) ? CurrentSceneFile : ApplicationConfig.BaseSceneName) + SceneChangedIndicator;
		}

		private void TryDeleteSelectedFrame()
		{
			if (SelectionLayer.FrameSelectionPanelList.Count >= 0)
			{
			    var group = new CommandGroup();
                group.Append(new SelectFrameCommand());
				foreach (var frame in SelectionLayer.FrameSelectionPanelList.Keys)
				{
					group.Append(new UIComponentParentChangeCommand(frame, null));
				}
				CommandManager.Instance.Execute(group);
			}
		}

		#region Save/load stuff

		internal void TryLoadSceneAsTemplate()
		{

			var startPath = Path.GetFullPath(Path.Combine(ApplicationConfig.TemplatesPath, ".."));
			var filter = "XML files(*.xml)| *.xml";
			using (var openDialog = new System.Windows.Forms.OpenFileDialog() { InitialDirectory = startPath, Multiselect = false, Filter = filter })
			{
				if (openDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

				var createdFrame = CreateFrameFromFile(openDialog.FileName);
				if (createdFrame != null && createdFrame.GetType() == typeof(FreePlacement))
				{
					var loadedScene = new SceneDataContainer(createdFrame as FreePlacement) { SceneFileFullPath = openDialog.FileName, SceneName = openDialog.FileName.Split('\\').Last().Split('.').First() };

					SceneFrame = loadedScene.Scene;
					LoadedScenes.Add(loadedScene);
					LoadedScenesTabs.SelectedIndex = LoadedScenes.IndexOf(loadedScene);

					CommandManager.Instance.SetNotDirty();
					CommandManager.Instance.Reset();
				}
			}
		}

		private void TrySetNewScene()
		{

			var newScene = new SceneDataContainer(RootFrame.Placement.Width, RootFrame.Placement.Height);

			SceneFrame = newScene.Scene;
			LoadedScenes.Add(newScene);

			LoadedScenesTabs.SelectedIndex = LoadedScenes.IndexOf(newScene);

			CommandManager.Instance.SetNotDirty();
			CommandManager.Instance.Reset();
		}

		internal bool TrySaveSceneAsTemplate()
		{
			var startPath = Path.GetFullPath(Path.Combine(ApplicationConfig.TemplatesPath, ".."));
			var filter = "XML files(*.xml)| *.xml";
			using (var saveDialog = new System.Windows.Forms.SaveFileDialog() { InitialDirectory = startPath, Filter = filter })
			{
				if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					Fusion.Core.Utils.UIComponentSerializer.Write(SceneFrame, saveDialog.FileName);

					this.CurrentSceneFile = saveDialog.FileName;
					this.CurrentScene.SceneFileFullPath = this.CurrentSceneFile;
					CommandManager.Instance.SetNotDirty();
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		internal bool TrySaveSceneAsTemplate( SceneDataContainer sceneData )
		{
			var startPath = Path.GetFullPath(Path.Combine(ApplicationConfig.TemplatesPath, ".."));
			var filter = "XML files(*.xml)| *.xml";
			using (var saveDialog = new System.Windows.Forms.SaveFileDialog() { InitialDirectory = startPath, Filter = filter })
			{
				if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					Fusion.Core.Utils.UIComponentSerializer.Write(sceneData.Scene, saveDialog.FileName);

					this.CurrentSceneFile = saveDialog.FileName;
					sceneData.SceneFileFullPath = this.CurrentSceneFile;
					CommandManager.Instance.SetNotDirty();
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		internal CommandGroup AddFrameToScene( UIComponent createdFrame, Point point, bool dropDeeper = false )
		{
			var hoveredFrame = SelectionLayer.GetHoveredFrameOnScene(point, false,false) ?? SceneFrame;

			if (!(hoveredFrame is IUIModifiableContainer<ISlot>))
			{
				hoveredFrame = hoveredFrame.Parent() as IUIModifiableContainer<ISlot>;
			}

			IUIModifiableContainer<ISlot> container = hoveredFrame as IUIModifiableContainer<ISlot>;

			var commands = new CommandGroup(
				new UIComponentParentChangeCommand(createdFrame, container),
				new SlotPropertyChangeCommand(createdFrame, "X", (int)point.X - _uiManager.BoundingBox(hoveredFrame.Placement).X),
				new SlotPropertyChangeCommand(createdFrame, "Y", (int)point.Y - _uiManager.BoundingBox(hoveredFrame.Placement).Y)
			);

			UIManager.MakeComponentNameValid(createdFrame, SceneFrame);

			return commands;
		}

		internal bool TrySaveScene()
		{
			if (!String.IsNullOrEmpty(this.CurrentSceneFile))
			{
				Fusion.Core.Utils.UIComponentSerializer.Write(SceneFrame, this.CurrentSceneFile);
				CommandManager.Instance.SetNotDirty();
				return true;
			}
			else
			{
				return TrySaveSceneAsTemplate();
			}
		}

		internal bool TrySaveScene( SceneDataContainer sceneData )
		{
			if (!String.IsNullOrEmpty(sceneData.SceneFileFullPath))
			{
				Fusion.Core.Utils.UIComponentSerializer.Write(sceneData.Scene, sceneData.SceneFileFullPath);
				CommandManager.Instance.SetNotDirty();
				return true;
			}
			else
			{
				return TrySaveSceneAsTemplate(sceneData);
			}
		}

		internal void TrySaveFrameAsTemplate()
		{
			if (SelectionLayer.FrameSelectionPanelList.Count == 1)
			{
				if (!Directory.Exists(ApplicationConfig.TemplatesPath))
				{
					Directory.CreateDirectory(ApplicationConfig.TemplatesPath);
				}
				var selectedFrame = SelectionLayer.FrameSelectionPanelList.FirstOrDefault().Value.SelectedFrame;

				Fusion.Core.Utils.UIComponentSerializer.Write(selectedFrame, ApplicationConfig.TemplatesPath + "\\" + (selectedFrame.Name ?? selectedFrame.GetType().ToString()) + ".xml");
				this.LoadPalettes();
			}
		}

		public void LoadPalettes()
		{
			if (Directory.Exists(ApplicationConfig.TemplatesPath))
			{
				var templates = Directory.GetFiles(ApplicationConfig.TemplatesPath, "*.xml").ToList();
				_palette.AvailablePrefabs.ItemsSource = templates.Select(t => t.Split('\\').Last().Split('.').First());
			}
		}

		public UIComponent CreateFrameFromFile( string filePath )
		{
			UIComponent createdFrame = null;
			try
			{
			    Fusion.Core.Utils.UIComponentSerializer.Read(filePath, out createdFrame);
			}
			catch (Exception ex)
			{
				Fusion.Log.Error($"---------------ERROR---------------");
				Fusion.Log.Error($"Could not deserialize file \"{filePath}\".\n");
				Fusion.Log.Error($"Next exception is thrown:\n{ex.Message}\n");
				Fusion.Log.Error($"Exception stack trace:\n{ex.StackTrace}");
				Fusion.Log.Error($"---------------ERROR---------------\n");
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

		private void ExecutedDeleteFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
			this.TryDeleteSelectedFrame();
		}

		private void ExecutedMoveFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
			var delta = KeyboardArrowsSteps[e.Parameter.ToString()];
			if (SelectionManager.Instance.SelectedFrames.Count>0)
			{
				var step = SelectionLayer.VisualGrid.Visibility == Visibility.Visible ?
					(int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * SelectionLayer.GridSizeMultiplier) : 1;
				var group = new CommandGroup();
				foreach (UIComponent frame in SelectionLayer.FrameSelectionPanelList.Keys)
				{
					group.Append(new CommandGroup(
						new SlotPropertyChangeCommand(frame, "X", frame.Placement.X + (int)delta.X * step),
						new SlotPropertyChangeCommand(frame, "Y", frame.Placement.Y + (int)delta.Y * step))
					);
				}
				CommandManager.Instance.Execute(group);
			}
			else
			{
				var scroll = sender as ScrollViewer;

				if (delta.X>0)
				{
					scroll.LineRight();
				}
				if (delta.X < 0)
				{
					scroll.LineLeft();
				}
				if (delta.Y > 0)
				{
					scroll.LineDown();
				}
				if (delta.Y < 0)
				{
					scroll.LineUp();
				}
			}
		}

		private void ExecutedAlignFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
		    if (SelectionLayer.FrameSelectionPanelList.Count == 0)
		        return;

			var frames = SelectionLayer.FrameSelectionPanelList.Keys;
			int minX, maxX, minY, maxY;
			minX = int.MaxValue;
			minY = int.MaxValue;
			maxX = int.MinValue;
			maxY = int.MinValue;

			switch (e.Parameter.ToString())
			{
				case "up":
					{
						minY = (int)(frames.Select(f => f.Placement.Y).Min() + 0.5f);
						break;
					}
				case "down":
					{
						maxY = (int)(frames.Select(f => f.Placement.Y + f.Placement.Height).Max() + 0.5f);
						break;
					}
				case "left":
					{
						minX = (int)(frames.Select(f => f.Placement.X).Min() + 0.5f);
						break;
					}
				case "right":
					{
						maxX = (int)(frames.Select(f => f.Placement.X + f.Placement.Width).Max() + 0.5f);
						break;
					}
				case "horizontal":
					{
						minX = (int)(frames.Select(f => f.Placement.X).Min() + 0.5f);
						maxX = (int)(frames.Select(f => f.Placement.X + f.Placement.Width).Max() + 0.5f);
						break;
					}
				case "vertical":
					{
						minY = (int)(frames.Select(f => f.Placement.Y).Min() + 0.5f);
						maxY = (int)(frames.Select(f => f.Placement.Y + f.Placement.Height).Max() + 0.5f);
						break;
					}
			}

			var commands = new CommandGroup();
			foreach (UIComponent frame in SelectionLayer.FrameSelectionPanelList.Keys)
			{
				switch (e.Parameter.ToString())
				{
					case "up":
						{
							commands.Append(new SlotPropertyChangeCommand(frame, "Y", minY));
							break;
						}
					case "down":
						{
							commands.Append(new SlotPropertyChangeCommand(frame, "Y", maxY - frame.Placement.Height));
							break;
						}
					case "left":
						{
							commands.Append(new SlotPropertyChangeCommand(frame, "X", minX));
							break;
						}
					case "right":
						{
							commands.Append(new SlotPropertyChangeCommand(frame, "X", maxX - frame.Placement.Width));
							break;
						}
					case "horizontal":
						{
							commands.Append(new SlotPropertyChangeCommand(frame, "X", (minX + maxX - frame.Placement.Width) / 2));
							break;
						}
					case "vertical":
						{
							commands.Append(new SlotPropertyChangeCommand(frame, "Y", (minY + maxY - frame.Placement.Height) / 2));
							break;
						}
				}
			}

            if(!commands.IsEmpty)
    			CommandManager.Instance.Execute(commands);
		}

		private void ExecutedCopyFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
			_xmlComponentsBuffer.Clear();
			_componentsOffsetBuffer.Clear();
			Point selectionLayerOffset = new Point(SelectionLayer.FrameDragsPanel.RenderTransform.Value.OffsetX,
												   SelectionLayer.FrameDragsPanel.RenderTransform.Value.OffsetY);
			foreach (UIComponent component in SelectionLayer.FrameSelectionPanelList.Keys)
			{
				_xmlComponentsBuffer.Add(Fusion.Core.Utils.UIComponentSerializer.WriteToString(component));
				var globalTransform = _uiManager.GlobalTransform(component.Placement);
				_componentsOffsetBuffer.Add(new Point(globalTransform.M31,globalTransform.M32) - selectionLayerOffset);
			}
		}

		private void ExecutedCutFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
			ExecutedCopyFrameCommand(sender, e);
			TryDeleteSelectedFrame();
		}

		private void ExecutedPasteFrameCmdCommand( object sender, ExecutedRoutedEventArgs e )
		{
			List<UIComponent> pastedComponents = new List<UIComponent>();

			for (int i = 0; i < _xmlComponentsBuffer.Count; i++)
			{
				string componentXml = _xmlComponentsBuffer[i];
				Vector componentOffset = _componentsOffsetBuffer[i];

				if (!string.IsNullOrEmpty(componentXml))
				{
					UIComponent component = null;
					try
					{
						component = Fusion.Core.Utils.UIComponentSerializer.ReadFromString(componentXml);
					}
					catch (Exception ex)
					{
						Fusion.Log.Error($"---------------ERROR---------------");
						Fusion.Log.Error($"Could not deserialize string.\n");
						Fusion.Log.Error($"Next exception is thrown:\n{ex.Message}\n");
						Fusion.Log.Error($"Exception stack trace:\n{ex.StackTrace}");
						Fusion.Log.Error($"---------------ERROR---------------\n");
					}
					if (component != null)
					{
						pastedComponents.Add(component);

						var command = AddFrameToScene(component,
							System.Windows.Input.Mouse.GetPosition(SelectionLayer) + componentOffset);

						CommandManager.Instance.Execute(command);
					}
				}
			}

			CommandManager.Instance.Execute(new SelectFrameCommand(pastedComponents));

			foreach (UIComponent pasted in pastedComponents)
			{
				foreach (UIComponent component in UIHelper.BFSTraverse(pasted))
				{
					UIManager.MakeComponentNameValid(component, SceneFrame, component);
				}
			}
		}

		private void ExecutedSceneConfigCommand( object sender, ExecutedRoutedEventArgs e )
		{
			// Instantiate the dialog box
			SceneConfigWindow dlg = new SceneConfigWindow(SelectionLayer.ActualWidth, SelectionLayer.ActualHeight);

			// Configure the dialog box
			dlg.Owner = this;

			// Open the dialog box modally
			if (dlg.ShowDialog().Value)
			{
				SelectionLayer.Width = dlg.SceneWidth;
				SelectionLayer.Height = dlg.SceneHeight;
				DefaultSceneWidth = dlg.SceneWidth;
				DefaultSceneHeight = dlg.SceneHeight;
			};
		}

		private void ExecutedRemoveSelectionCommand( object sender, ExecutedRoutedEventArgs e )
		{
			CommandManager.Instance.Execute(new SelectFrameCommand());
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
			e.Cancel = false;
			if (LoadedScenes.Count>0)
			{
				foreach (var scene in LoadedScenes.ToList())
				{
					e.Cancel |= TryUnloadScene(scene);
					if (e.Cancel)
					{
						return;
					}
				}
			}
			else
			{
				return;
			}

			var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var settings = configFile.AppSettings.Settings;
			settings["SlotDetailsWindowX"].Value = _slotDetails.Left.ToString();
			settings["SlotDetailsWindowY"].Value = _slotDetails.Top.ToString();
			settings["SlotDetailsWindowHeight"].Value = _slotDetails.Height.ToString();
			settings["DetailsPanelX"].Value = _details.Left.ToString();
			settings["DetailsPanelY"].Value = _details.Top.ToString();
			settings["DetailsPanelHeight"].Value = _details.Height.ToString();
			settings["TreeViewPanelX"].Value = _treeView.Left.ToString();
			settings["TreeViewPanelY"].Value = _treeView.Top.ToString();
			settings["TreeViewPanelHeight"].Value = _treeView.Height.ToString();
			settings["PalettePanelX"].Value = _palette.Left.ToString();
			settings["PalettePanelY"].Value = _palette.Top.ToString();
			settings["PalettePanelHeight"].Value = _palette.Height.ToString();

			settings["ConsoleWindowX"].Value = _consoleWindow.Left.ToString();
			settings["ConsoleWindowY"].Value = _consoleWindow.Top.ToString();
			settings["ConsoleWindowWidth"].Value = _consoleWindow.Width.ToString();
			settings["ConsoleWindowHeight"].Value = _consoleWindow.Height.ToString();

			settings["DetailsPanelVisibility"].Value = _details.Visibility == Visibility.Visible ? true.ToString() : false.ToString();
			settings["PalettePanelVisibility"].Value = _palette.Visibility == Visibility.Visible ? true.ToString() : false.ToString();
			settings["TreeViewPanelVisibility"].Value = _treeView.Visibility == Visibility.Visible ? true.ToString() : false.ToString();
			settings["ConsoleWindowVisibility"].Value = _consoleWindow.Visibility == Visibility.Visible ? true.ToString() : false.ToString();
			settings["SlotDetailsWindowVisibility"].Value = _slotDetails.Visibility == Visibility.Visible ? true.ToString() : false.ToString();


			settings["MainWindowX"].Value = this.Left.ToString();
			settings["MainWindowY"].Value = this.Top.ToString();
			settings["MainWindowWidth"].Value = this.Width.ToString();
			settings["MainWindowHeight"].Value = this.Height.ToString();
			settings["MainWindowFullscreen"].Value = this.WindowState.ToString();

			settings["SceneZoom"].Value = this.ZoomerSlider.Value.ToString();
			settings["SceneSizeWidth"].Value = this.DefaultSceneWidth.ToString();
			settings["SceneSizeHeight"].Value = this.DefaultSceneHeight.ToString();

			configFile.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
		}

		//private bool CheckForChanges()
		//{
		//	if (CommandManager.Instance.IsDirty)
		//	{
		//		var result = MessageBox.Show(@"There are unsaved changes in the current scene.
		//						Do you want to save them?", "Attention", MessageBoxButton.YesNoCancel);
		//		switch (result)
		//		{
		//			case MessageBoxResult.Yes:
		//				{
		//					return this.TrySaveScene();
		//				}
		//			case MessageBoxResult.No:
		//				{
		//					return true;
		//				}
		//			case MessageBoxResult.Cancel:
		//				{
		//					return false;
		//				}
		//			default:
		//				{
		//					return true;
		//				}
		//		}
		//	}
		//	else
		//	{
		//		return true;
		//	}
		//}

		private bool CheckForChanges(SceneDataContainer sceneData)
		{
			if (sceneData.IsDirty)
			{
				var result = MessageBox.Show(
					$"There are unsaved changes in the scene: {sceneData.SceneName} Do you want to save them?",
					"Attention",
					MessageBoxButton.YesNoCancel
					);
				switch (result)
				{
					case MessageBoxResult.Yes:
						{
							return this.TrySaveScene(sceneData);
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

		private bool TryUnloadScene(SceneDataContainer sceneData)
		{
			int index = LoadedScenes.IndexOf(sceneData);
			bool unloadingConfirmed = this.CheckForChanges(sceneData);
			if (!unloadingConfirmed)
			{
				return !unloadingConfirmed;
			}

			if (index == 0)
			{
				if (LoadedScenes.Count > 0)
				{
					LoadedScenesTabs.SelectedIndex = index + 1;
				}
			}
			else
			{
				LoadedScenesTabs.SelectedIndex = index - 1;
			}

			LoadedScenes.Remove(sceneData);

			return !unloadingConfirmed;
		}

		private void MenuItem_Click( object sender, RoutedEventArgs e )
		{
			Window panel = (sender as MenuItem).Tag as Window;
			panel.Show();
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
			SelectionLayer.GridSizeMultiplier = int.Parse(rBtn.Tag.ToString());

			var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configFile.AppSettings.Settings["GridSize"].Value = rBtn.Tag.ToString();
			configFile.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
		}

		private void Window_KeyUp( object sender, KeyEventArgs e )
		{
			if (e.Key == Key.System && e.OriginalSource is ScrollViewer)
			{
				e.Handled = true;
			}
		}

		private void Window_Loaded( object sender, RoutedEventArgs e )
		{


            Activate();
        }

		private void RearrangeWorkspaceSize()
		{
			Height = double.Parse(ConfigurationManager.AppSettings.Get("MainWindowHeight"));
			Width = double.Parse(ConfigurationManager.AppSettings.Get("MainWindowWidth"));

			Left = double.Parse(ConfigurationManager.AppSettings.Get("MainWindowX"));
			Top = double.Parse(ConfigurationManager.AppSettings.Get("MainWindowY"));
			WindowState = (WindowState)Enum.Parse(typeof(WindowState), ConfigurationManager.AppSettings.Get("MainWindowFullscreen"));

			DefaultSceneWidth = double.Parse(ConfigurationManager.AppSettings.Get("SceneSizeWidth"));
			DefaultSceneHeight = double.Parse(ConfigurationManager.AppSettings.Get("SceneSizeHeight"));

			SelectionLayer.Width = DefaultSceneWidth;
			SelectionLayer.Height = DefaultSceneHeight;

			Zoomer.Width = SelectionLayer.Width;
			Zoomer.Height = SelectionLayer.Height;
			Zoomer.Stretch = Stretch.Uniform;
			ZoomerSlider.Value = double.Parse(ConfigurationManager.AppSettings.Get("SceneZoom"));
		}

		private void Window_PreviewMouseWheel( object sender, MouseWheelEventArgs e )
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				e.Handled = true;
				this.sceneScale = Math.Min(Math.Max(this.sceneScale + (double)e.Delta / 2000, ZoomerSlider.Minimum), ZoomerSlider.Maximum);
				ZoomerSlider.Value = sceneScale;
			}
		}

        public static readonly DependencyProperty MouseXProperty =
            DependencyProperty.Register("MouseX", typeof(double), typeof(InterfaceEditor), new UIPropertyMetadata((double)0));

        public double MouseX {
            get => (double)GetValue(MouseXProperty);
            set => SetValue(MouseXProperty, value);
        }

        public static readonly DependencyProperty MouseYProperty =
            DependencyProperty.Register("MouseY", typeof(double), typeof(InterfaceEditor), new UIPropertyMetadata((double)0));

        public double MouseY {
            get => (double)GetValue(MouseYProperty);
            set => SetValue(MouseYProperty, value);
        }

		private void Window_PreviewMouseMove( object sender, MouseEventArgs e )
        {
            MouseX = Math.Round(e.GetPosition(SelectionLayer).X);
            MouseY = Math.Round(e.GetPosition(SelectionLayer).Y);

            if (SelectionLayer!=null && SelectionLayer.IsScrollExpected)
            {
                Point mousePosition = e.GetPosition(ZoomerScroll);
                if (mousePosition.X < 0)
                {
                    ZoomerScroll.ScrollToHorizontalOffset(ZoomerScroll.ContentHorizontalOffset - 1);
                }
                if (mousePosition.Y < 0)
                {
                    ZoomerScroll.ScrollToVerticalOffset(ZoomerScroll.ContentVerticalOffset - 1);
                }
                if (mousePosition.X > ZoomerScroll.ViewportWidth)
                {
                    ZoomerScroll.ScrollToHorizontalOffset(ZoomerScroll.ContentHorizontalOffset + 1);
                }
                if (mousePosition.Y > ZoomerScroll.ViewportHeight)
                {
                    ZoomerScroll.ScrollToVerticalOffset(ZoomerScroll.ContentVerticalOffset + 1);
                }
            }
		}

		private void Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			var sign = Math.Sign(e.NewValue - e.OldValue);
			this.sceneScale = e.NewValue;
			ZoomScene(sign);
		}

		private void ZoomScene( int sign )
		{
			if (Zoomer != null)
			{
				Zoomer.Width = DefaultSceneWidth * sceneScale;
				Zoomer.Height = DefaultSceneHeight * sceneScale;

				var mousePosition = System.Windows.Input.Mouse.GetPosition(ZoomerScroll);
				Point scaledTopLeftCornerPosition;
				if (mousePosition.X >= 0 && mousePosition.Y >= 0)
					scaledTopLeftCornerPosition = new Point(MouseX * sceneScale - mousePosition.X, MouseY * sceneScale - mousePosition.Y);
				else
				{
					scaledTopLeftCornerPosition = new Point(ZoomerScroll.ViewportWidth / 2, ZoomerScroll.ViewportHeight / 2);

					mousePosition = ZoomerScroll.TransformToVisual(SelectionLayer).Transform(scaledTopLeftCornerPosition);
					scaledTopLeftCornerPosition = new Point(mousePosition.X * sceneScale - ZoomerScroll.ViewportWidth / 2, mousePosition.Y * sceneScale - ZoomerScroll.ViewportHeight / 2);
				}

				ZoomerScroll.ScrollToHorizontalOffset(scaledTopLeftCornerPosition.X);
				ZoomerScroll.ScrollToVerticalOffset(scaledTopLeftCornerPosition.Y);

			}
		}

		private void ZoomerScroll_PreviewKeyDown( object sender, KeyEventArgs e )
		{
			switch (e.Key)
			{
				case Key.Up:
				case Key.Down:
				case Key.Left:
				case Key.Right:
					{
						e.Handled = false;
						break;
					}
			}
		}

		private void LoadedScenesTabs_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if (e.Source is TabControl tc)
			{
				if (tc.SelectedIndex >= 0)
				{
					CurrentScene = LoadedScenes[tc.SelectedIndex];

				    if (_childrenBinding != null)
				    {
                        BindingOperations.ClearBinding(_treeView.ElementHierarchyView, TreeView.ItemsSourceProperty);
				    }

				    BindingOperations.EnableCollectionSynchronization(CurrentScene.Scene.Slots, CurrentScene.Scene.ChildrenAccessLock);

				    _childrenBinding = new Binding(nameof(CurrentScene.Scene.Slots))
				    {
				        Source = CurrentScene.Scene,
				    };
				    _treeView.ElementHierarchyView.SetBinding(TreeView.ItemsSourceProperty, _childrenBinding);

					_treeView.AttachScene(SceneFrame);
					SelectionManager.Instance.SelectFrame(CurrentScene.SceneSelection);
				}
				else
				{
					TrySetNewScene();
				}
			}
		}

		private void CloseTabButton_Click( object sender, RoutedEventArgs e )
		{
			Button btn = sender as Button;
			SceneDataContainer sceneData = btn.Tag as SceneDataContainer;
			TryUnloadScene(sceneData);
		}

		private void DebugCheckBox_Checked( object sender, RoutedEventArgs e )
		{
			var chbx = sender as CheckBox;

			EnableDebug((chbx.IsChecked != null && chbx.IsChecked == true) ? true : false);
		}

		private void DebugCheckBox_Unchecked( object sender, RoutedEventArgs e )
		{
			var chbx = sender as CheckBox;

			EnableDebug((chbx.IsChecked != null && chbx.IsChecked == true) ? true : false);
		}

		private void EnableDebug( bool isEnabled )
		{
			_uiManager.DebugEnabled = isEnabled;
		}
	}
}
