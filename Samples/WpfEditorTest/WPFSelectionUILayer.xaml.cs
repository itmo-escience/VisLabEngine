using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfEditorTest.UndoRedo;
using Frame = Fusion.Engine.Frames.Frame;
using CommandManager = WpfEditorTest.UndoRedo.CommandManager;
using WpfEditorTest.FrameSelection;
using WpfEditorTest.ChildPanels;
using Fusion.Engine.Frames;
using ZWpfLib;

namespace WpfEditorTest
{
	/// <summary>
	/// Interaction logic for WPFSelectionUILayer.xaml
	/// </summary>
	public partial class WPFSelectionUILayer : Grid
	{
		public Dictionary<Frame, FrameSelectionPanel> FrameSelectionPanelList = new Dictionary<Frame, FrameSelectionPanel>();

		private int _deltaX = 0;
		private int _deltaY = 0;
		private Point _initMousePosition;

		private Rectangle _selectionRectangle;

		public int GridSizeMultiplier { get => _gridSizeMultiplier; set { _gridSizeMultiplier = value; DrawGridLines(); } }
		public Dictionary<string, int> GridScaleNumbers = new Dictionary<string, int>
		{
			{ "1x", 1 },
			{ "2x", 2 },
			{ "3x", 3 },
			{ "4x", 4 },
			{ "5x", 5 },
			{ "10x", 10 }
		};

		public FusionUI.UI.ScalableFrame DragFieldFrame => Window.DragFieldFrame;
		public FusionUI.UI.ScalableFrame SceneFrame => Window.SceneFrame;
		public FusionUI.UI.MainFrame RootFrame => Window.RootFrame;
		public FramePalette PaletteWindow;
		private int _gridSizeMultiplier = ApplicationConfig.DefaultVisualGridSizeMultiplier;

		public InterfaceEditor Window { get; set; }
		public bool AreaSelectionEnabled { get; private set; } = false;
		public Rectangle SelectionRectangleBlack { get; private set; }

		public struct FrameSelectionPair
		{
			public Frame Frame;
			public FrameSelectionPanel Panel;
		}
		public event EventHandler<FrameSelectionPair> FrameSelected;
		public event EventHandler FramesDeselected;

		private Stack<FrameSelectionPanel> _selectionPanelPool = new Stack<FrameSelectionPanel>();

		public WPFSelectionUILayer()
		{
			InitializeComponent();

			this.SizeChanged += ( s, e ) => {
				DrawGridLines();
			};

			SelectionManager.Instance.FrameSelected += ( s, selectedFrames ) =>
			{
				foreach (var frameAndPanel in FrameSelectionPanelList)
				{
					var frame = frameAndPanel.Key;
					var selectionPanel = frameAndPanel.Value;

					var commands = this.ResetSelectedFrame(new Point(frame.GlobalRectangle.X, frame.GlobalRectangle.Y), selectionPanel);
					var command = new CommandGroup(commands.ToArray());
					CommandManager.Instance.ExecuteWithoutMemorising(command);

					selectionPanel.SelectedFrame = null;
					_selectionPanelPool.Push(selectionPanel);
				}

				HighlightPanelsContainer.Children.Clear();
				FrameSelectionPanelList.Clear();

				if (selectedFrames.Count > 0)
				{
					foreach (var frame in selectedFrames)
					{
						var frameSelectionPanel = _selectionPanelPool.Count > 0 ? _selectionPanelPool.Pop() : new FrameSelectionPanel();

						FrameSelectionPanelList.Add(frame, frameSelectionPanel);

						frameSelectionPanel.SelectedFrame = frame;
						frameSelectionPanel.Visibility = Visibility.Visible;

						HighlightPanelsContainer.Children.Add(frameSelectionPanel);
					}

					SelectFrame(selectedFrames[selectedFrames.Count - 1]);
				}
			};
		}

		private void DrawGridLines()
		{
			ClearGridLines();
			for (int i = 0; i < this.ActualWidth; i += (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * GridSizeMultiplier))
			{
				DrawLine(i, i, 0, this.ActualHeight, ApplicationConfig.DefaultGridLinesThickness, ApplicationConfig.DefaultGridLinesBrush);
			}

			for (int j = 0; j < this.ActualHeight; j += (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * GridSizeMultiplier))
			{
				DrawLine(0, this.ActualWidth, j, j, ApplicationConfig.DefaultGridLinesThickness, ApplicationConfig.DefaultGridLinesBrush);
			}
		}

		private void DrawSelectionRectangle( Point startPoint, Point endPoint )
		{
			var width = Math.Abs(startPoint.X - endPoint.X);
			var height = Math.Abs(startPoint.Y - endPoint.Y);
			var top = Math.Min(startPoint.Y, endPoint.Y);
			var left = Math.Min(startPoint.X, endPoint.X);
			ClearSelectionRectangle();
			DrawSelectionRectangle(width, height, top, left, ApplicationConfig.DefaultGridLinesThickness,
			ApplicationConfig.DefaultSelectionRectanglePrimaryBrush, ApplicationConfig.DefaultSelectionRectangleSecondaryBrush);
		}

		private bool HasSelectionAreaChanged( double width, double height, double top, double left )
		{
			return (int)width != (int)_selectionRectangle.Width &&
				(int)height != (int)_selectionRectangle.Height &&
				(int)top != (int)Canvas.GetTop(_selectionRectangle) &&
				(int)left != (int)Canvas.GetLeft(_selectionRectangle);
		}

		private void ClearGridLines()
		{
			VisualGrid.Children.Clear();
		}

		private void ClearSelectionRectangle()
		{
			if (AreaSelection.Children.Contains(_selectionRectangle))
			{
				AreaSelection.Children.Remove(_selectionRectangle);
				AreaSelection.Children.Remove(SelectionRectangleBlack);
				_selectionRectangle = null;
			}
		}

		private void DrawLine( double x1, double x2, double y1, double y2, double thickness, Brush brush )
		{
			// Add a Line Element
			var myLine = new Line()
			{
				IsHitTestVisible = false,
				Stroke = brush,
				X1 = x1,
				Y1 = y1,
				X2 = x2,
				Y2 = y2,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				StrokeThickness = thickness,
				SnapsToDevicePixels = true,
			};
			VisualGrid.Children.Add(myLine);
		}

		private void DrawSelectionRectangle( double width, double height, double top, double left, double thickness, Brush brush, Brush secondBrush )
		{
			// Add a Rectangle Element
			_selectionRectangle = new Rectangle()
			{
				IsHitTestVisible = false,
				Stroke = brush,
				StrokeDashArray = new DoubleCollection { 4, 4 },
				StrokeThickness = thickness,
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
			};
			AreaSelection.Children.Add(_selectionRectangle);

			Canvas.SetLeft(_selectionRectangle, left);
			Canvas.SetTop(_selectionRectangle, top);

			SelectionRectangleBlack = new Rectangle()
			{
				IsHitTestVisible = false,
				Stroke = secondBrush,
				StrokeDashArray = new DoubleCollection { 4, 4 },
				StrokeDashOffset = 4,
				StrokeThickness = thickness,
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
			};
			AreaSelection.Children.Add(SelectionRectangleBlack);

			Canvas.SetLeft(SelectionRectangleBlack, left);
			Canvas.SetTop(SelectionRectangleBlack, top);
		}

		internal void ToggleGridLines( bool enable )
		{
			VisualGrid.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
		}

		private void SelectFrame( Frame frame )
		{
			if (FrameSelectionPanelList.TryGetValue(frame, out var panel))
			{
				FrameSelected?.Invoke(this, new FrameSelectionPair { Frame = frame, Panel = panel });
			}
		}

		private void RecalcMouseDelta( MouseEventArgs e )
		{
			var currentMousePosition = e.GetPosition(this);
			_deltaX = (int)(currentMousePosition.X - _initMousePosition.X);
			_deltaY = (int)(currentMousePosition.Y - _initMousePosition.Y);
		}

		public List<IEditorCommand> ResetSelectedFrame( Point point, FrameSelectionPanel panel )
		{
			FramesDeselected?.Invoke(this, null);

			List<IEditorCommand> commands = this.ReleaseFrame(point, panel);

			panel.SelectedFrame = null;
			panel.Visibility = Visibility.Collapsed;

			_frameDragsPanel.DragMousePressed = false;
			_frameDragsPanel.CurrentDrag = null;

			return commands;
		}

		public Frame GetHoveredFrameOnScene( Point mousePos, bool ignoreScene )
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

		private bool HasFrameChangedSize( FrameSelectionPanel panel )
		{
			return panel.SelectedFrame.GlobalRectangle != panel.InitialGlobalRectangle;
		}

		public void MoveFrameToDragField( Frame frame )
		{
			frame.Parent?.Remove(frame);

			DragFieldFrame.Add(frame);

			var panel = FrameSelectionPanelList[frame];

			panel.UpdateSelectedFramePosition();
		}

		public void LandFrameOnScene( Frame frame, Point mousePosition )
		{
			frame.Parent?.Remove(frame);

			// If we can't find where to land it (that's weird) just try attach to the scene
			var hoveredFrame = GetHoveredFrameOnScene(mousePosition, false) ?? SceneFrame;

			hoveredFrame.Add(frame);
		}

		private List<IEditorCommand> ReleaseFrame( Point point, FrameSelectionPanel panel )
		{
			List<IEditorCommand> commands = new List<IEditorCommand>();

			if (panel.SelectedFrame != null)
			{
				if (panel.IsInDragField)
				{
					panel.IsInDragField = false;
					ParentHighlightPanel.SelectedFrame = null;

					var hoveredFrame = GetHoveredFrameOnScene(point, false) ?? SceneFrame;
					panel.SelectedFrame.Parent?.Remove(panel.SelectedFrame);

					commands.Add(new CommandGroup(
						new FrameParentChangeCommand(panel.SelectedFrame, hoveredFrame, panel.InitFrameParent),
						new FramePropertyChangeCommand(panel.SelectedFrame, "X",
						(int)point.X - hoveredFrame.GlobalRectangle.X - ((int)point.X - panel.SelectedFrame.GlobalRectangle.X),
						panel.InitialGlobalRectangle.X),
						new FramePropertyChangeCommand(panel.SelectedFrame, "Y",
						(int)point.Y - hoveredFrame.GlobalRectangle.Y - ((int)point.Y - panel.SelectedFrame.GlobalRectangle.Y),
						panel.InitialGlobalRectangle.Y)
					));
				}
				else if (this.HasFrameChangedSize(panel))
				{
					commands.Add(new CommandGroup(
						new FramePropertyChangeCommand(panel.SelectedFrame, "Width",
						panel.SelectedFrame.Width, panel.InitialGlobalRectangle.Width),
						new FramePropertyChangeCommand(panel.SelectedFrame, "Height",
						panel.SelectedFrame.Height, panel.InitialGlobalRectangle.Height),
						new FramePropertyChangeCommand(panel.SelectedFrame, "X",
						panel.SelectedFrame.X, panel.InitialGlobalRectangle.X),
						new FramePropertyChangeCommand(panel.SelectedFrame, "Y",
						panel.SelectedFrame.Y, panel.InitialGlobalRectangle.Y)
					));
				}
			}
			panel.MousePressed = false;
			return commands;
		}

		private void LocalGrid_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			var hovered = GetHoveredFrameOnScene(e.GetPosition(this), true);
			_initMousePosition = e.GetPosition(this);

			if (!_frameDragsPanel.DragMousePressed)
			{
				if (hovered != null)
				{
					IEditorCommand command = null;
					if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
					{
						var framesToSelect = new List<Frame>(SelectionManager.Instance.SelectedFrames);
						if (framesToSelect.Contains(hovered))
						{
							framesToSelect.Remove(hovered);
						}
						else
						{
							framesToSelect.Add(hovered);
						}
						command = new SelectFrameCommand(framesToSelect);
					}
					else
					{
						if (!SelectionManager.Instance.SelectedFrames.Contains(hovered))
						{
							command = new SelectFrameCommand(new List<Frame> { hovered });
						}
					}
					if (command != null)
					{
						CommandManager.Instance.Execute(command);
					}
				}
				else
				{
					if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
					{
						var command = new SelectFrameCommand(new List<Frame> { });
						CommandManager.Instance.Execute(command);
					}

					AreaSelectionEnabled = true;
				}
			}

			foreach (var frameAndPanel in FrameSelectionPanelList)
			{
				var frame = frameAndPanel.Key;
				var selectionPanel = frameAndPanel.Value;

				selectionPanel.InitialGlobalRectangle = new Fusion.Core.Mathematics.Rectangle(frame.X, frame.Y, frame.Width, frame.Height);
				selectionPanel.InitPanelPosition = new Point(selectionPanel.RenderTransform.Value.OffsetX, selectionPanel.RenderTransform.Value.OffsetY);
				selectionPanel.InitFrameParent = frame.Parent;
			}

			if (hovered != null && FrameSelectionPanelList.ContainsKey(hovered))
			{
				var panel = FrameSelectionPanelList[hovered];

				if (panel != null)
					panel.MousePressed = true;
			}
		}

		private void LocalGrid_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			List<IEditorCommand> commands = new List<IEditorCommand>();

			foreach (var panel in FrameSelectionPanelList.Values)
			{
				commands.AddRange(this.ReleaseFrame(e.GetPosition(this), panel));
			}

			_frameDragsPanel.DragMousePressed = false;

			//if (PaletteWindow.SelectedFrameTemplate != null)
			//{
			//	//var createdFrame = Window.CreateFrameFromFile(System.IO.Path.Combine(ApplicationConfig.TemplatesPath, PaletteWindow.SelectedFrameTemplate) + ".xml");
			//	//if (createdFrame != null)
			//	//{
			//	//	commands = Window.AddFrameToScene(createdFrame, e.GetPosition(this), commands);
			//	//	var command = new CommandGroup(commands.ToArray());
			//	//	CommandManager.Instance.Execute(command);
			//	//}
			//	//PaletteWindow.SelectedFrameTemplate = null;
			//	//ParentHighlightPanel.SelectedFrame = null;
			//}
			//else
			//{
			//	//if (commands.Count > 0)
			//	//{
			//	//	var command = new CommandGroup(commands.ToArray());
			//	//	CommandManager.Instance.Execute(command);
			//	//}
			//}
			if (commands.Count > 0)
			{
				var command = new CommandGroup(commands.ToArray());
				CommandManager.Instance.Execute(command);
			}
			AreaSelectionEnd(SelectionManager.Instance.SelectedFrames);
		}

		private void LocalGrid_MouseMove( object sender, MouseEventArgs e )
		{
			RecalcMouseDelta(e);
			if (_frameDragsPanel.DragMousePressed)
			{
				RecalculateSelectionSize(e.MouseDevice.GetPosition(this));
			}
			else if (FrameSelectionPanelList.Any(fsp => fsp.Value.MousePressed))
			{
				var movedPanel = FrameSelectionPanelList.FirstOrDefault(fsp => fsp.Value.MousePressed).Value;
				if (!movedPanel.IsInDragField && (_deltaX != 0 || _deltaY != 0))
				{
					foreach (var panel in FrameSelectionPanelList.Values)
					{
						this.MoveFrameToDragField(panel.SelectedFrame);
						panel.IsInDragField = true;
					}
				}
				if (movedPanel.IsInDragField || PaletteWindow.SelectedFrameTemplate != null)
				{
					var hovered = GetHoveredFrameOnScene(e.GetPosition(this), true);
					ParentHighlightPanel.SelectedFrame = hovered;
				}

				Point currentLocation = e.MouseDevice.GetPosition(this);

				var step = (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * GridSizeMultiplier);
				var newX = movedPanel.InitPanelPosition.X + _deltaX;
				var newY = movedPanel.InitPanelPosition.Y + _deltaY;
				var dX = newX - movedPanel.InitPanelPosition.X;
				var dY = newY - movedPanel.InitPanelPosition.Y;
				if (NeedSnapping())
				{
					dX -= -step / 2 + (newX + step / 2) % step;
					dY -= -step / 2 + (newY + step / 2) % step;
				}
				foreach (var panel in FrameSelectionPanelList.Values)
				{
					var transformDelta = new TranslateTransform(panel.InitPanelPosition.X + dX, panel.InitPanelPosition.Y + dY);
					panel.RenderTransform = transformDelta;
				}
			}
			else if (PaletteWindow.SelectedFrameTemplate != null)
			{
				//var hovered = GetHoveredFrameOnScene(e.GetPosition(this), true);
				//ParentHighlightPanel.SelectedFrame = hovered;
			}
		}

		public void RecalculateSelectionSize( Point currentLocation )
		{
			var isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
			var isControlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

			_frameDragsPanel.Resize(_deltaX, _deltaY, isShiftPressed, isControlPressed, out double heightMult, out double widthMult);

			var dragsPanelX = _frameDragsPanel.RenderTransform.Value.OffsetX;
			var dragsPanelY = _frameDragsPanel.RenderTransform.Value.OffsetY;
			foreach (var panel in FrameSelectionPanelList.Values)
			{
				var initRect = _frameDragsPanel.InitialFramesRectangles[panel.SelectedFrame];
				panel.HeightBuffer = initRect.Height * heightMult;
				panel.WidthBuffer = initRect.Width * widthMult;
				TranslateTransform multedTransform = new TranslateTransform
				{
					X = dragsPanelX + initRect.X * widthMult,
					Y = dragsPanelY + initRect.Y * heightMult
				};
				panel.RenderTransform = multedTransform;
			}
		}

		private bool NeedSnapping()
		{
			return VisualGrid.Visibility == Visibility.Visible && !(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));
		}

		private void AreaSelectionEnd( List<Frame> selectedFrames )
		{
			AreaSelectionEnabled = false;
			if (_selectionRectangle != null)
			{
				var selectedframes = new List<Frame>(selectedFrames);
				Fusion.Core.Mathematics.Rectangle selectedArea = new Fusion.Core.Mathematics.Rectangle(
						(int)Canvas.GetLeft(_selectionRectangle),
						(int)Canvas.GetTop(_selectionRectangle),
						(int)_selectionRectangle.Width,
						(int)_selectionRectangle.Height
					);
				foreach (Frame frame in SceneFrame.Children)
				{
					if (selectedArea.Contains(frame.GlobalRectangle) && !selectedframes.Contains(frame))
					{
						selectedframes.Add(frame);
					}
				}
				ClearSelectionRectangle();
				var command = new SelectFrameCommand(selectedframes);
				CommandManager.Instance.Execute(command);
			}
		}

		private void VisualSelection_MouseMove( object sender, MouseEventArgs e )
		{
			if (AreaSelectionEnabled)
				DrawSelectionRectangle(_initMousePosition, e.GetPosition(this));
		}

		private void Grid_PreviewDragOver( object sender, System.Windows.DragEventArgs e )
		{
			//e.Handled = false;

			e.Effects = DragDropEffects.None;

			string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

			// If the string can be converted into a Brush, allow copying.
			if (!string.IsNullOrEmpty(dataString))
			{
				e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
				var hovered = GetHoveredFrameOnScene(e.GetPosition(this), true);
				ParentHighlightPanel.SelectedFrame = hovered;
			}
		}

		private void Grid_PreviewDrop( object sender, System.Windows.DragEventArgs e )
		{
			// If the DataObject contains string data, extract it.
			if (e.Data.GetDataPresent(DataFormats.StringFormat))
			{
				string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
				if (!string.IsNullOrEmpty(dataString))
				{
					var createdFrame = Window.CreateFrameFromFile(System.IO.Path.Combine(ApplicationConfig.TemplatesPath, dataString) + ".xml");

					if (createdFrame != null)
					{
						var command = new CommandGroup(
							Window.AddFrameToScene(createdFrame, e.GetPosition(this), new List<IEditorCommand>()).ToArray()
							);
						CommandManager.Instance.Execute(command);
					}
					PaletteWindow.SelectedFrameTemplate = null;
					ParentHighlightPanel.SelectedFrame = null;
				}
			}
		}
	}
}
