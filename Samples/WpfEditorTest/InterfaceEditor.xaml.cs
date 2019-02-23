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
using Fusion.Engine.Frames2;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using CommandManager = WpfEditorTest.UndoRedo.CommandManager;
using WpfEditorTest.UndoRedo;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;
using System.Windows.Interop;
using System.Configuration;
using System.Threading;
using System.Windows.Media.Animation;
using WpfEditorTest.FrameSelection;
using ZWpfLib;
using Keyboard = System.Windows.Input.Keyboard;
using WpfEditorTest.DialogWindows;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Frames2.Managing;
using WpfEditorTest.ChildWindows;

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

		private Binding _childrenBinding;
		private readonly Game _engine;

		public UIContainer DragFieldFrame;
		public UIContainer SceneFrame;
		public UIContainer RootFrame;
		private string _currentSceneFile;
		private string _sceneChangedIndicator;
		private string _titleWithFileName = ApplicationConfig.BaseTitle + " - " + ApplicationConfig.BaseSceneName;
		private List<string> _xmlComponentsBuffer = new List<string>();
        private List<Vector> _componentsOffsetBuffer = new List<Vector>();

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

		public InterfaceEditor()
		{
			InitializeComponent();

		    Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 40 });

            _engine = new Game("TestGame");
			_engine.Mouse = new DummyMouse(_engine);
			_engine.Keyboard = new DummyKeyboard(_engine);
			_engine.Touch = new DummyTouch(_engine);

			_engine.GameServer = new CustomGameServer(_engine);
			_engine.GameClient = new CustomGameClient(_engine);
			_engine.GameInterface = new CustomGameInterface(_engine);
			_engine.LoadConfiguration("Config.ini");

            DefaultSceneWidth = int.Parse(ConfigurationManager.AppSettings.Get("SceneSizeWidth"));
            DefaultSceneHeight = int.Parse(ConfigurationManager.AppSettings.Get("SceneSizeHeight"));

            _engine.RenderSystem.StereoMode = Fusion.Engine.Graphics.StereoMode.WpfEditor;
			_engine.RenderSystem.Width = (int)DefaultSceneWidth;
			_engine.RenderSystem.Height = (int)DefaultSceneHeight;
			_engine.RenderSystem.VSyncInterval = 1;

			Directory.SetCurrentDirectory(@"..\..\..\..\GISTest\bin\x64\Debug");

		    var tokenSource = new CancellationTokenSource();

            var fusionThread = new Thread(() => _engine.RunExternal(tokenSource.Token));
		    fusionThread.Name = "Fusion";

		    Closing += (sender, args) => { if (args.Cancel == false) tokenSource.Cancel(); };

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
				var command = new SelectFrameCommand(new List<UIComponent> { frame });
				CommandManager.Instance.Execute(command);
			};
			_treeView.ControllerSlotSelected += ( _, slot ) =>
			{
				_slotDetails.SetSelectFrame(slot);
			};
			_treeView.RequestFrameDeletionInUI += ( _, __ ) => TryDeleteSelectedFrame();

			var templates = Directory.GetFiles(ApplicationConfig.TemplatesPath, "*.xml").ToList();
			_palette.AvailableFrames.ItemsSource = templates.Select(t => t.Split('\\').Last().Split('.').First());

		    SelectionLayer.Window = this;
		    SelectionLayer.PaletteWindow = _palette;


            _engine.OnInitialized += () =>
		    {
		        Application.Current.Dispatcher.InvokeAsync(() =>
		        {
		            RootFrame = (_engine.GameInterface as CustomGameInterface).GetUIRoot();
					var rootChildren = new List<UIComponent>(RootFrame.Children);
		            SceneFrame = new FreePlacement(0, 0, RootFrame.Width, RootFrame.Height);//RootFrame.Children.FirstOrDefault() as UIContainer;
					foreach (var child in rootChildren)
					{
						RootFrame.Remove(child);
						SceneFrame.Add(child);
					}
					DragFieldFrame = new FreePlacement(0, 0, RootFrame.Width, RootFrame.Height);
					//    ,"DragFieldFrame", Fusion.Core.Mathematics.Color.Zero)
					//{
					//    Anchor = FrameAnchor.All,
					//    ZOrder = 1000000,
					//};
					RootFrame.Add(SceneFrame);
		            RootFrame.Add(DragFieldFrame);

		            SelectionLayer.DxElem.Renderer = _engine;

		            var binding = new Binding("Children")
		            {
		                Source = SceneFrame,
		                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
		                Mode = BindingMode.OneWay
		            };
		            _treeView.ElementHierarchyView.SetBinding(TreeView.ItemsSourceProperty, binding);
					_treeView.AttachScene(SceneFrame);
		        });
		    };

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
			SelectionLayer.SizeChanged += ( s, e ) =>
			{
				this.DefaultSceneWidth = e.NewSize.Width;
				this.DefaultSceneHeight = e.NewSize.Height;
                if (SceneFrame != null)
                {
                    SceneFrame.Width = (float)e.NewSize.Width;
                    SceneFrame.Height = (float)e.NewSize.Height;
                }
                ZoomScene();
			};
			this.UpdateTitle();


			var loadedGridSize = ConfigurationManager.AppSettings.Get("GridSize");

			foreach (RadioButton item in GridSizeMenuItem.Items)
			{
				if ((string)item.Content == loadedGridSize)
				{
					item.IsChecked = true;
					break;
				}
			}

            fusionThread.Start();
		}

	    protected override void OnSourceInitialized( EventArgs e )
		{
			base.OnSourceInitialized(e);
			SelectionLayer.DxElem.HandleInput(this);
		}

		private void UpdateTitle()
		{
			this.Title = ApplicationConfig.BaseTitle + " - " + (!string.IsNullOrEmpty(_currentSceneFile) ? CurrentSceneFile : ApplicationConfig.BaseSceneName) + SceneChangedIndicator;
		}

		private void TryDeleteSelectedFrame()
		{
			if (SelectionLayer.FrameSelectionPanelList.Count >= 0)
			{
				List<IEditorCommand> commands = new List<IEditorCommand>();
				commands.Add(new SelectFrameCommand(new List<UIComponent> { }));
				foreach (var frame in SelectionLayer.FrameSelectionPanelList.Keys)
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
			using (var openDialog = new System.Windows.Forms.OpenFileDialog() { InitialDirectory = startPath, Multiselect = false, Filter = filter })
			{
				if (openDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

				var createdFrame = CreateFrameFromFile(openDialog.FileName);
				if (createdFrame != null && createdFrame.GetType().BaseType == typeof(UIContainer))
				{
					SelectionManager.Instance.SelectFrame(new List<UIComponent> { });
					RootFrame.Remove(SceneFrame);
					foreach (var panel in SelectionLayer.FrameSelectionPanelList.Values)
					{
						var commands = SelectionLayer.ResetSelectedFrame(new Point(0, 0), panel);
						var command = new CommandGroup(commands.ToArray());
						CommandManager.Instance.ExecuteWithoutMemorising(command);
					}

					SceneFrame = createdFrame as UIContainer;
					RootFrame.Add(SceneFrame);
					_treeView.AttachScene(SceneFrame);
					//DragFieldFrame.ZOrder = 1000000;

					_childrenBinding = new Binding("Children")
					{
						Source = SceneFrame,
					};
					_treeView.ElementHierarchyView.SetBinding(TreeView.ItemsSourceProperty, _childrenBinding);

					this.CurrentSceneFile = openDialog.FileName;
					CommandManager.Instance.SetNotDirty();
					CommandManager.Instance.Reset();

					SelectionLayer.Width = SceneFrame.Width;
					SelectionLayer.Height = SceneFrame.Height;
				}
			}
		}

		private void TrySetNewScene()
		{
			if (!this.CheckForChanges())
				return;
			SelectionManager.Instance.SelectFrame(new List<UIComponent> { });
			RootFrame.Remove(SceneFrame);
			foreach (var panel in SelectionLayer.FrameSelectionPanelList.Values)
			{
				var commands = SelectionLayer.ResetSelectedFrame(new Point(0, 0), panel);
				var command = new CommandGroup(commands.ToArray());
				CommandManager.Instance.ExecuteWithoutMemorising(command);
			}

			SceneFrame = new FreePlacement(0, 0, RootFrame.Width, RootFrame.Height);

			//new FusionUI.UI.ScalableFrame(0, 0, this.RootFrame.UnitWidth, this.RootFrame.UnitHeight, "Scene", Fusion.Core.Mathematics.Color.Zero) { Anchor = FrameAnchor.All };
			RootFrame.Add(SceneFrame);
			_treeView.AttachScene(SceneFrame);
			//DragFieldFrame.ZOrder = 1000000;

			_childrenBinding = new Binding("Children")
			{
				Source = SceneFrame,
			};
			_treeView.ElementHierarchyView.SetBinding(TreeView.ItemsSourceProperty, _childrenBinding);
			this.CurrentSceneFile = null;
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
					CommandManager.Instance.SetNotDirty();
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		internal List<IEditorCommand> AddFrameToScene(UIComponent createdFrame, Point point)
		{
			var hoveredFrame = SelectionLayer.GetHoveredFrameOnScene(point, false) ?? SceneFrame;

			if (!(hoveredFrame is UIContainer))
			{
				hoveredFrame = hoveredFrame.Parent;
			}

			UIContainer container = hoveredFrame as UIContainer;

            List<IEditorCommand> commands = new List<IEditorCommand>
            {
                new CommandGroup(
                    new FrameParentChangeCommand(createdFrame, container),
                    new FramePropertyChangeCommand(createdFrame, "X", (int)point.X - hoveredFrame.BoundingBox.X),
                    new FramePropertyChangeCommand(createdFrame, "Y", (int)point.Y - hoveredFrame.BoundingBox.Y)
                )
            };

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
				_palette.AvailableFrames.ItemsSource = templates.Select(t => t.Split('\\').Last().Split('.').First());
			}
		}

		public UIComponent CreateFrameFromFile( string filePath )
		{
			UIComponent createdFrame;
			try
			{
				Fusion.Core.Utils.UIComponentSerializer.Read(filePath, out createdFrame);
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

		private void ExecutedDeleteFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
			this.TryDeleteSelectedFrame();
		}

		private void ExecutedMoveFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
			var step = GridToggle.IsChecked != null && (bool)GridToggle.IsChecked?
				(int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * SelectionLayer.GridSizeMultiplier) : 1;
			List<IEditorCommand> commands = new List<IEditorCommand>();
			var delta = KeyboardArrowsSteps[e.Parameter.ToString()];
			foreach (UIComponent frame in SelectionLayer.FrameSelectionPanelList.Keys)
			{
				commands.Add(new CommandGroup(
					new FramePropertyChangeCommand(frame, "X", frame.X + (int)delta.X* step),
					new FramePropertyChangeCommand(frame, "Y", frame.Y + (int)delta.Y* step)));
			}
			var command = new CommandGroup(commands.ToArray());
			CommandManager.Instance.Execute(command);
		}

		private void ExecutedAlignFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
			if (SelectionLayer.FrameSelectionPanelList.Count>0)
			{
				List<IEditorCommand> commands = new List<IEditorCommand>();
				var frames = SelectionLayer.FrameSelectionPanelList.Keys;
				int minX,maxX,minY,maxY;
				minX = int.MaxValue; /*SelectionLayer.FrameSelectionPanelList.First().Key.X;*/
				minY = int.MaxValue; /*SelectionLayer.FrameSelectionPanelList.First().Key.Y;*/
				maxX = int.MinValue; /*minX + SelectionLayer.FrameSelectionPanelList.First().Key.Width;*/
				maxY = int.MinValue; /*minY + SelectionLayer.FrameSelectionPanelList.First().Key.Height;*/

				switch (e.Parameter.ToString())
				{
					case "up":
						{
							minY = (int)(frames.Select(f => f.Y).Min()+0.5f);
							break;
						}
					case "down":
						{
							maxY = (int)(frames.Select(f => f.Y + f.Height).Max() + 0.5f);
							break;
						}
					case "left":
						{
							minX = (int)(frames.Select(f => f.X).Min() + 0.5f);
							break;
						}
					case "right":
						{
							maxX = (int)(frames.Select(f => f.X + f.Width).Max() + 0.5f);
							break;
						}
					case "horizontal":
						{
							minX = (int)(frames.Select(f => f.X).Min() + 0.5f);
							maxX = (int)(frames.Select(f => f.X + f.Width).Max() + 0.5f);
							break;
						}
					case "vertical":
						{
							minY = (int)(frames.Select(f => f.Y).Min() + 0.5f);
							maxY = (int)(frames.Select(f => f.Y + f.Height).Max() + 0.5f);
							break;
						}
				}
				foreach (UIComponent frame in SelectionLayer.FrameSelectionPanelList.Keys)
				{
					switch (e.Parameter.ToString())
					{
						case "up":
							{
								commands.Add(new FramePropertyChangeCommand(frame, "Y",minY));
								break;
							}
						case "down":
							{
								commands.Add(new FramePropertyChangeCommand(frame, "Y",maxY-frame.Height));
								break;
							}
						case "left":
							{
								commands.Add(new FramePropertyChangeCommand(frame, "X",minX));
								break;
							}
						case "right":
							{
								commands.Add(new FramePropertyChangeCommand(frame, "X", maxX - frame.Width));
								break;
							}
						case "horizontal":
							{
								commands.Add(new FramePropertyChangeCommand(frame, "X", (minX+maxX-frame.Width)/2));
								break;
							}
						case "vertical":
							{
								commands.Add(new FramePropertyChangeCommand(frame, "Y", (minY + maxY - frame.Height) / 2));
								break;
							}
					}
				}

				var command = new CommandGroup(commands.ToArray());
				CommandManager.Instance.Execute(command);
			}
		}

		private void ExecutedCopyFrameCommand( object sender, ExecutedRoutedEventArgs e )
		{
            _xmlComponentsBuffer.Clear();
            _componentsOffsetBuffer.Clear();
            Point selectionLayerOffset = new Point(SelectionLayer._frameDragsPanel.RenderTransform.Value.OffsetX, 
                                                   SelectionLayer._frameDragsPanel.RenderTransform.Value.OffsetY);
            foreach (UIComponent component in SelectionLayer.FrameSelectionPanelList.Keys)
            {
                _xmlComponentsBuffer.Add(Fusion.Core.Utils.UIComponentSerializer.WriteToString(component));
                _componentsOffsetBuffer.Add(new Point(component.GlobalTransform.M31, component.GlobalTransform.M32) - selectionLayerOffset);

                /*var upperLeft = this.PointToScreen(new Point(component.X, component.Y));
                var lowerRight = this.PointToScreen(new Point(component.X + component.Width, component.Y + component.Height));

                var img = this.CopyScreen(
                    (int)upperLeft.X,
                    (int)upperLeft.Y + (int)SystemParameters.WindowCaptionHeight + 9,
                    (int)lowerRight.X,
                    (int)lowerRight.Y + (int)SystemParameters.WindowCaptionHeight + 9
                    );
                Clipboard.SetData(DataFormats.Bitmap, (Object)img);*/
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
                    UIComponent component = Fusion.Core.Utils.UIComponentSerializer.ReadFromString(componentXml);
                    pastedComponents.Add(component);

                    List<IEditorCommand> commands = AddFrameToScene(component,
                        System.Windows.Input.Mouse.GetPosition(SelectionLayer) + componentOffset);

                    var command = new CommandGroup(commands.ToArray());
                    CommandManager.Instance.Execute(command);
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

        private void ExecutedRemoveSelectionCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CommandManager.Instance.Execute(new SelectFrameCommand(new List<UIComponent>()));
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
			settings["GridSize"].Value = SelectionLayer.GridSizeMultiplier.ToString() + "x";
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
							return this.TrySaveScene();
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
			SelectionLayer.GridSizeMultiplier = SelectionLayer.GridScaleNumbers[rBtn.Content.ToString()];
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
				this.sceneScale = Math.Min(Math.Max(this.sceneScale + (double)e.Delta/2000, ZoomerSlider.Minimum), ZoomerSlider.Maximum);
				ZoomerSlider.Value = sceneScale;
			}
		}

        private void Window_MouseMove( object sender, MouseEventArgs e)
        {
            SelectionLayer.FullMouseMove(sender, e);
        }

        private void Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			this.sceneScale = e.NewValue;
			ZoomScene();
		}

		private void ZoomScene()
		{
			if (Zoomer!=null)
			{
				Zoomer.Height = DefaultSceneHeight * sceneScale;
				Zoomer.Width = DefaultSceneWidth * sceneScale;
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
	}
}
