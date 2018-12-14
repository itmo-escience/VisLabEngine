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


		private int DeltaX = 0;
		private int DeltaY = 0;

		private Point InitFramePosition;
		private Frame InitFrameParent;
		private Point InitMousePosition;

		private readonly FrameDetails _details;
		private readonly FramePalette _palette;
		private readonly FrameTreeView _treeView;
		private readonly FrameSelectionPanel _frameSelectionPanel;
		private readonly ParentHighlightPanel _parentHighlightPanel;

	    public string TemplatesPath = Path.GetFullPath(Path.Combine(Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName, "..\\..\\..\\FramesXML"));
	    Binding childrenBinding;
        private readonly Game _engine;

		public FusionUI.UI.ScalableFrame DragFieldFrame;
		public FusionUI.UI.ScalableFrame SceneFrame;
		public FusionUI.UI.MainFrame RootFrame;
		private string CurrentSceneFile;

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

            DxElem.Renderer = _engine;

			_parentHighlightPanel = new ParentHighlightPanel();
			LocalGrid.Children.Add(_parentHighlightPanel);

			_frameSelectionPanel = new FrameSelectionPanel(this);
			LocalGrid.Children.Add(_frameSelectionPanel);

			_details = new FrameDetails();
            _treeView = new FrameTreeView();
            _palette = new FramePalette();

			miFrameProperties.Tag = _details;
			miFrameTemplates.Tag = _palette;
			miSceneTreeView.Tag = _treeView;


			SourceInitialized += (_, args) =>
            {
                _details.Owner = this;
                _details.Show();
                _treeView.Owner = this;
                _treeView.Show();
				_palette.Owner = this;
				_palette.Show();
			};

			_treeView.SelectedFrameChangedInUI += ( _, frame ) => {
				var command = new SelectFrameCommand(frame);
				CommandManager.Instance.Execute(command);
			}; //SelectFrame(frame);
			_treeView.RequestFrameDeletionInUI += ( _, __ ) => TryDeleteSelectedFrame();

			var templates = Directory.GetFiles(TemplatesPath, "*.xml").ToList();
            _palette.AvailableFrames.ItemsSource = templates.Select(t => t.Split('\\').Last().Split('.').First());

            RootFrame = ApplicationInterface.Instance.rootFrame;
            SceneFrame = (FusionUI.UI.ScalableFrame)RootFrame.Children.FirstOrDefault();
            DragFieldFrame = new FusionUI.UI.ScalableFrame(0, 0, RootFrame.UnitWidth, RootFrame.UnitHeight, "DragFieldFrame", Fusion.Core.Mathematics.Color.Zero)
            {
                Anchor = FrameAnchor.All,
                ZOrder = 1000000,
            };
            RootFrame.Add(DragFieldFrame);

            var b = new Binding("Children")
            {
                Source = SceneFrame,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.OneWay
            };
            _treeView.ElementHierarchyView.SetBinding(TreeView.ItemsSourceProperty, b);

				UndoButton.DataContext = CommandManager.Instance;
				RedoButton.DataContext = CommandManager.Instance;

			SelectionManager.Instance.FrameSelected += ( s, e ) => {
				if (e!=null)
				{
					this.SelectFrame(e);
				}
				else
				{
					var releasedFrame = SelectionManager.Instance.SelectedFrame;
					this.ResetSelectedFrame(new Point(releasedFrame.GlobalRectangle.X, releasedFrame.GlobalRectangle.Y));
				}
			};
			//SelectionManager.Instance.FrameDeselected += ( s, e ) => {
			//	this.ResetSelectedFrame(new Point(e.GlobalRectangle.X,e.GlobalRectangle.Y));
			//};
		}

	    protected override void OnSourceInitialized(EventArgs e)
	    {
	        base.OnSourceInitialized(e);
	        DxElem.HandleInput(this);
	    }

		private void LocalGrid_MouseMove( object sender, MouseEventArgs e )
		{
			if (_frameSelectionPanel.DragMousePressed)
			{
				Point currentLocation = e.MouseDevice.GetPosition(this);
				var deltaX = currentLocation.X - _frameSelectionPanel.PreviousMouseLocation.X;
				var deltaY = currentLocation.Y - _frameSelectionPanel.PreviousMouseLocation.Y;
				TranslateTransform delta = null;// = new TranslateTransform (deltaX, deltaY);



				switch (_frameSelectionPanel.Drags.IndexOf(_frameSelectionPanel.CurrentDrag))
				{
					case 0:
						{
							delta = new TranslateTransform(deltaX, deltaY);
							_frameSelectionPanel.HeightBuffer -= deltaY;
							_frameSelectionPanel.WidthBuffer -= deltaX;
							break;
						}
					case 1:
						{
							delta = new TranslateTransform(0, deltaY);
							_frameSelectionPanel.HeightBuffer -= deltaY;
							break;
						}
					case 2:
						{
							delta = new TranslateTransform(0, deltaY);
							_frameSelectionPanel.HeightBuffer -= deltaY;
							_frameSelectionPanel.WidthBuffer += deltaX;
							break;
						}
					case 3:
						{
							delta = new TranslateTransform(deltaX, 0);
							_frameSelectionPanel.WidthBuffer -= deltaX;
							break;
						}
					case 4:
						{
							delta = new TranslateTransform(0, 0);
							_frameSelectionPanel.WidthBuffer += deltaX;
							break;
						}
					case 5:
						{
							delta = new TranslateTransform(deltaX, 0);
							_frameSelectionPanel.HeightBuffer += deltaY;
							_frameSelectionPanel.WidthBuffer -= deltaX;
							break;
						}
					case 6:
						{
							delta = new TranslateTransform(0, 0);
							_frameSelectionPanel.HeightBuffer += deltaY;
							break;
						}
					case 7:
						{
							delta = new TranslateTransform(0, 0);
							_frameSelectionPanel.HeightBuffer += deltaY;
							_frameSelectionPanel.WidthBuffer += deltaX;
							break;
						}
				}

				var group = new TransformGroup();
				group.Children.Add(_frameSelectionPanel.PreviousTransform);
				group.Children.Add(delta);
				_frameSelectionPanel.RenderTransform = group;
				_frameSelectionPanel.PreviousTransform = _frameSelectionPanel.RenderTransform;
				_frameSelectionPanel.PreviousMouseLocation = currentLocation;

			}
			else if (_frameSelectionPanel.MousePressed)
			{
				this.GetMouseDeltaAfterFrameMouseDown(e);
				if (!_frameSelectionPanel.IsMoved && (DeltaX!=0 || DeltaY!=0))
				{
					this.MoveFrameToDragField(_frameSelectionPanel.SelectedFrame);
					_frameSelectionPanel.IsMoved = true;
				}

				if (_frameSelectionPanel.IsMoved || _palette._selectedFrameTemplate != null)
				{
					var hovered = GetHoveredFrameOnScene(e.GetPosition(DxElem), true);
					_parentHighlightPanel.SelectedFrame = hovered;
				}

				Point currentLocation = e.MouseDevice.GetPosition(this);

				var delta = new TranslateTransform
				(currentLocation.X - _frameSelectionPanel.PreviousMouseLocation.X, currentLocation.Y - _frameSelectionPanel.PreviousMouseLocation.Y);

				var group = new TransformGroup();
				group.Children.Add(_frameSelectionPanel.PreviousTransform);
				group.Children.Add(delta);

				((UserControl)_frameSelectionPanel).RenderTransform = group;
				_frameSelectionPanel.PreviousTransform = ((UserControl)_frameSelectionPanel).RenderTransform;
				_frameSelectionPanel.PreviousMouseLocation = currentLocation;
			}
			else if (_palette._selectedFrameTemplate != null)
			{
				var hovered = GetHoveredFrameOnScene(e.GetPosition(DxElem), true);
				_parentHighlightPanel.SelectedFrame = hovered;
			}
		}

		private void GetMouseDeltaAfterFrameMouseDown( MouseEventArgs e )
		{
			var currentMousePosition = e.GetPosition(this);
			DeltaX = (int)InitMousePosition.X - (int)currentMousePosition.X;
			DeltaY = (int)InitMousePosition.Y - (int)currentMousePosition.Y;
		}

		private void LocalGrid_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			this.ReleaseFrame(e.GetPosition(this));

			if (_palette._selectedFrameTemplate != null)
			{

				var createdFrame = CreateFrameFromFile(Path.Combine(TemplatesPath, _palette._selectedFrameTemplate) + ".xml");

                if (createdFrame != null)
				{
					var hoveredFrame = GetHoveredFrameOnScene(e.GetPosition(this), false) ?? SceneFrame;

					var command = new CommandGroup(
						new FrameParentChangeCommand(createdFrame, hoveredFrame),
						new FramePropertyChangeCommand(createdFrame, "X", (int)e.MouseDevice.GetPosition(this).X - hoveredFrame.GlobalRectangle.X - createdFrame.Width / 2),
						new FramePropertyChangeCommand(createdFrame, "Y", (int)e.MouseDevice.GetPosition(this).Y - hoveredFrame.GlobalRectangle.Y - createdFrame.Height / 2),
						new SelectFrameCommand(createdFrame)
					);
					CommandManager.Instance.Execute(command);

					//createdFrame.X = (int)e.MouseDevice.GetPosition(this).X - createdFrame.Width / 2;
					//createdFrame.Y = (int)e.MouseDevice.GetPosition(this).Y - createdFrame.Height / 2;
					//LandFrameOnScene(createdFrame, e.GetPosition(this));
					//SelectFrame(createdFrame);

					var delta = new TranslateTransform();
					_frameSelectionPanel.RenderTransform = delta;
					delta.X = createdFrame.X;
					delta.Y = createdFrame.Y;
					_frameSelectionPanel.PreviousTransform = _frameSelectionPanel.RenderTransform;
					//_frameSelectionPanel.UpdateSelectedFramePosition();

					//createdFrame.X -= createdFrame.Parent.X;
					//createdFrame.Y -= createdFrame.Parent.Y;
				}
				_palette._selectedFrameTemplate = null;
				_parentHighlightPanel.SelectedFrame = null;
			}
		}

		private void LocalGrid_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			var hovered = GetHoveredFrameOnScene(e.GetPosition(DxElem), true);

		    if (hovered != null)
		    {
				//SelectFrame(hovered);
				var command = new SelectFrameCommand(hovered);
				CommandManager.Instance.Execute(command);

				InitFramePosition = new Point(hovered.X,hovered.Y);
				InitFrameParent = hovered.Parent;
				InitMousePosition = e.GetPosition(this);
				_frameSelectionPanel.StartFrameDragging(e.GetPosition(this));
				//if (hovered != _frameSelectionPanel.SelectedFrame)
				//{
				//	ResetSelectedFrame();
				//}
				//else
				//{
				//
				//}
			}
			else
			{
				//ResetSelectedFrame(e.GetPosition(this));
				var command = new SelectFrameCommand(null);
				CommandManager.Instance.Execute(command);
			}
		}

	    private void Window_KeyDown(object sender, KeyEventArgs e)
	    {
	        if (e.Key == Key.Delete)
	        {
				this.TryDeleteSelectedFrame();
	        }
	    }

		private void TryDeleteSelectedFrame()
		{
			if (_frameSelectionPanel.SelectedFrame != null)
			{
				var selected = _frameSelectionPanel.SelectedFrame;
				var command = new CommandGroup(
					new FrameParentChangeCommand(selected, null),
					new DeselectFrameCommand());
				CommandManager.Instance.Execute(command);

				//ResetSelectedFrame(new Point(0, 0));
			}
		}

		private void SelectFrame(Frame frame)
	    {
            _details.SetSelectFrame(frame);
			_treeView.SelectedFrame = frame;
			_frameSelectionPanel.SelectedFrame = frame;
            _frameSelectionPanel.Visibility = Visibility.Visible;
	    }

        private void ResetSelectedFrame( Point point )
		{
			this.ReleaseFrame(point);

			_details.FrameDetailsControls.ItemsSource = null;
			_frameSelectionPanel.SelectedFrame = null;
		    _frameSelectionPanel.DragMousePressed = false;
		    _frameSelectionPanel.CurrentDrag = null;
		    _frameSelectionPanel.Visibility = Visibility.Collapsed;
        }

		private void ReleaseFrame( Point point )
		{
			if (_frameSelectionPanel.SelectedFrame != null)
			{
				if (_frameSelectionPanel.DragMousePressed)
				{
					_frameSelectionPanel.DragMousePressed = false;
					if (_frameSelectionPanel.SelectedFrame.Width != (int)_frameSelectionPanel.SelectedFrameInitSize.Width ||
						_frameSelectionPanel.SelectedFrame.Height != (int)_frameSelectionPanel.SelectedFrameInitSize.Height ||
						_frameSelectionPanel.SelectedFrame.X != (int)_frameSelectionPanel.SelectedFrameInitPosition.X ||
						_frameSelectionPanel.SelectedFrame.Y != (int)_frameSelectionPanel.SelectedFrameInitPosition.Y)
					{
						var command = new CommandGroup(
					new FramePropertyChangeCommand(_frameSelectionPanel.SelectedFrame, "Width",
					_frameSelectionPanel.SelectedFrame.Width, (int)_frameSelectionPanel.SelectedFrameInitSize.Width),
					new FramePropertyChangeCommand(_frameSelectionPanel.SelectedFrame, "Height",
					_frameSelectionPanel.SelectedFrame.Height, (int)_frameSelectionPanel.SelectedFrameInitSize.Height),
					new FramePropertyChangeCommand(_frameSelectionPanel.SelectedFrame, "X",
					_frameSelectionPanel.SelectedFrame.X, (int)_frameSelectionPanel.SelectedFrameInitPosition.X),
					new FramePropertyChangeCommand(_frameSelectionPanel.SelectedFrame, "Y",
					_frameSelectionPanel.SelectedFrame.Y, (int)_frameSelectionPanel.SelectedFrameInitPosition.Y)
				);
						CommandManager.Instance.Execute(command);
					}
				}

				else if (_frameSelectionPanel.IsMoved)
				{
					_frameSelectionPanel.IsMoved = false;
					_parentHighlightPanel.SelectedFrame = null;

					var hoveredFrame = GetHoveredFrameOnScene(point, false) ?? SceneFrame;
					_frameSelectionPanel.SelectedFrame.Parent?.Remove(_frameSelectionPanel.SelectedFrame);

					var command = new CommandGroup(
						new FrameParentChangeCommand(_frameSelectionPanel.SelectedFrame, hoveredFrame, InitFrameParent),
						new FramePropertyChangeCommand(_frameSelectionPanel.SelectedFrame, "X",
						(int)point.X - hoveredFrame.GlobalRectangle.X - ((int)point.X-_frameSelectionPanel.SelectedFrame.GlobalRectangle.X),
						(int)InitFramePosition.X),
						new FramePropertyChangeCommand(_frameSelectionPanel.SelectedFrame, "Y",
						(int)point.Y - hoveredFrame.GlobalRectangle.Y - ((int)point.Y - _frameSelectionPanel.SelectedFrame.GlobalRectangle.Y),
						(int)InitFramePosition.Y)
					);
					CommandManager.Instance.Execute(command);
					//LandFrameOnScene(_frameSelectionPanel.SelectedFrame, point);
					SelectFrame(_frameSelectionPanel.SelectedFrame);
					//_frameSelectionPanel.UpdateSelectedFramePosition();
				}
			}
			_frameSelectionPanel.MousePressed = false;
		}

		public Frame GetHoveredFrameOnScene(Point mousePos, bool ignoreScene)
	    {
	        var hoveredFrames = FrameProcessor.GetFramesAt(SceneFrame, (int)mousePos.X, (int)mousePos.Y);

	        if (ignoreScene)
	        {
	            if (hoveredFrames.Count > 1) // if something is there and it's not SceneFrame
	            {
	                return hoveredFrames.Pop();
	            }

	            return null;
	        }
	        else
	        {
	            if (hoveredFrames.Any()) // if something is there
	            {
	                return hoveredFrames.Pop();
	            }

	            return null;
	        }
	    }

        public void MoveFrameToDragField(Frame frame)
		{
			frame.Parent?.Remove(frame);

			DragFieldFrame.Add(frame);
			_frameSelectionPanel.UpdateSelectedFramePosition();
		}

	    public void LandFrameOnScene(Frame frame, Point mousePosition)
	    {
	        frame.Parent?.Remove(frame);

            // If we can't find where to land it (that's weird) just try attach to the scene
	        var hoveredFrame = GetHoveredFrameOnScene(mousePosition, false) ?? SceneFrame;

	        hoveredFrame.Add(frame);

			//_treeView.ElementHierarcyView.SetSelectedItem(frame);
		}

        #region Save/load stuff

        internal void TryLoadSceneAsTemplate()
        {
			if (!this.CheckForChanges())
				return;

			var startPath = Path.GetFullPath(Path.Combine(TemplatesPath, ".."));
            var filter = "XML files(*.xml)| *.xml";
            using (var dialog = new System.Windows.Forms.OpenFileDialog() { InitialDirectory = startPath, Multiselect = false, Filter = filter })
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

				var createdFrame = CreateFrameFromFile(dialog.FileName);
                if (createdFrame != null && createdFrame.GetType() == typeof(FusionUI.UI.ScalableFrame))
                {
                    RootFrame.Remove(SceneFrame);
                    ResetSelectedFrame(new Point(0,0));
                    SceneFrame = (FusionUI.UI.ScalableFrame)createdFrame;
                    RootFrame.Add(SceneFrame);
                    DragFieldFrame.ZOrder = 1000000;

                    childrenBinding = new Binding("Children")
                    {
                        Source = SceneFrame,
                    };
                    _treeView.ElementHierarchyView.SetBinding(TreeView.ItemsSourceProperty, childrenBinding);
                }
				this.CurrentSceneFile = dialog.FileName;
				CommandManager.Instance.SetNotDirty();
				CommandManager.Instance.Reset();
			}
        }

		private void TrySetNewScene()
		{
			if (!this.CheckForChanges())
				return;
			RootFrame.Remove(SceneFrame);
			ResetSelectedFrame(new Point(0, 0));
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
            var startPath = Path.GetFullPath(Path.Combine(TemplatesPath, ".."));
            var filter = "XML files(*.xml)| *.xml";
            using (var dialog = new System.Windows.Forms.SaveFileDialog() { InitialDirectory = startPath, Filter = filter })
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Fusion.Core.Utils.FrameSerializer.Write(SceneFrame, dialog.FileName);
                }
				this.CurrentSceneFile = dialog.FileName;

			}
        }

		internal void TrySaveScene()
		{
			if (!String.IsNullOrEmpty(this.CurrentSceneFile))
			{
				Fusion.Core.Utils.FrameSerializer.Write(SceneFrame, this.CurrentSceneFile); 
			}
			else
			{
				TrySaveSceneAsTemplate();
			}
		}

		internal void TrySaveFrameAsTemplate()
        {
            if (_frameSelectionPanel.SelectedFrame != null)
            {
                if (!Directory.Exists(TemplatesPath))
                {
                    Directory.CreateDirectory(TemplatesPath);
                }
                var selectedFrame = _frameSelectionPanel.SelectedFrame;

                Fusion.Core.Utils.FrameSerializer.Write(selectedFrame, TemplatesPath + "\\" + (selectedFrame.Text ?? selectedFrame.GetType().ToString()) + ".xml");
                this.LoadPalettes();
            }
        }

        public void LoadPalettes()
        {
            if (Directory.Exists(TemplatesPath))
            {
                var templates = Directory.GetFiles(TemplatesPath, "*.xml").ToList();
                _palette.AvailableFrames.ItemsSource = templates.Select(t => t.Split('\\').Last().Split('.').First());
            }
        }

        private Frame CreateFrameFromFile(string filePath)
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

        private void ExecutedSaveFrameCommand(object sender, ExecutedRoutedEventArgs e)
	    {
	        TrySaveFrameAsTemplate();
	    }

	    private void ExecutedSaveSceneCommand(object sender, ExecutedRoutedEventArgs e)
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

		private void ExecutedLoadSceneCommand(object sender, ExecutedRoutedEventArgs e)
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
			var selectedFrame = _frameSelectionPanel.SelectedFrame;
			if (selectedFrame!=null)
			{
				//var xmlFrame = Fusion.Core.Utils.FrameSerializer.WriteToString(selectedFrame);
				//Clipboard.SetData(DataFormats.Text, (Object)xmlFrame); 
				var upperLeft = this.PointToScreen(new Point(selectedFrame.X, selectedFrame.Y));
				var lowerRight = this.PointToScreen(new Point(selectedFrame.X+ selectedFrame.Width, selectedFrame.Y+ selectedFrame.Height));
				_frameSelectionPanel.Visibility = Visibility.Collapsed;
				_frameSelectionPanel.UpdateLayout();

				var img = this.CopyScreen(
					(int)upperLeft.X,
					(int)upperLeft.Y + (int)SystemParameters.WindowCaptionHeight + 9,
					(int)lowerRight.X,
					(int)lowerRight.Y + (int)SystemParameters.WindowCaptionHeight + 9
					);
				_frameSelectionPanel.Visibility = Visibility.Visible;
				_frameSelectionPanel.UpdateLayout();
				Clipboard.SetData(DataFormats.Bitmap, (Object)img);
			}

		}

		private void ExecutedPasteFrameCmdCommand( object sender, ExecutedRoutedEventArgs e )
		{
			var t = 0;
		}

		private void AlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e)
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
	}
}
