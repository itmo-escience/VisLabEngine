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

namespace WpfEditorTest
{
	/// <summary>
	/// Interaction logic for WPFSelectionUILayer.xaml
	/// </summary>
	public partial class WPFSelectionUILayer : Grid
	{
		public Dictionary<Frame, FrameSelectionPanel> frameSelectionPanelList = new Dictionary<Frame, FrameSelectionPanel>();


		private int DeltaX = 0;
		private int DeltaY = 0;
		private Point InitMousePosition;

		private Rectangle SelectionRectangle;

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
		public FramePalette paletteWindow;
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

			//_parentHighlightPanel = new ParentHighlightPanel();
			//Children.Add(_parentHighlightPanel);
			//_frameDragsPanel = new FrameDragsPanel();
			//Children.Add(_frameDragsPanel);

			DrawGridLines();

			SelectionManager.Instance.FrameSelected += ( s, selectedFrames ) =>
			{
				foreach (var frameAndPanel in frameSelectionPanelList)
				{
					var commands = this.ResetSelectedFrame(new Point(frameAndPanel.Key.GlobalRectangle.X, frameAndPanel.Key.GlobalRectangle.Y), frameAndPanel.Value);
					var command = new CommandGroup(commands.ToArray());
					CommandManager.Instance.ExecuteWithoutMemorising(command);

				    frameAndPanel.Value.SelectedFrame = null;
                    _selectionPanelPool.Push(frameAndPanel.Value);
				}

				HighlightPanelsContainer.Children.Clear();
                frameSelectionPanelList.Clear();

				if (selectedFrames.Count > 0)
				{
					foreach (var frame in selectedFrames)
					{
					    var frameSelectionPanel = _selectionPanelPool.Count > 0 ? _selectionPanelPool.Pop() : new FrameSelectionPanel(this);

						frameSelectionPanelList.Add(frame, frameSelectionPanel);

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
			for (int i = 0; i < SystemParameters.VirtualScreenWidth; i += (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * GridSizeMultiplier))
			{
				DrawLine(i, i, 0, SystemParameters.VirtualScreenHeight, ApplicationConfig.DefaultGridLinesThickness, ApplicationConfig.DefaultGridLinesBrush);
			}

			for (int j = 0; j < SystemParameters.VirtualScreenHeight; j += (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * GridSizeMultiplier))
			{
				DrawLine(0, SystemParameters.VirtualScreenWidth, j, j, ApplicationConfig.DefaultGridLinesThickness, ApplicationConfig.DefaultGridLinesBrush);
			}
		}

		private void DrawSelectionRectangle(Point startPoint, Point endPoint)
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
			return (int)width != (int)SelectionRectangle.Width &&
				(int)height != (int)SelectionRectangle.Height &&
				(int)top != (int)Canvas.GetTop(SelectionRectangle) &&
				(int)left != (int)Canvas.GetLeft(SelectionRectangle);
		}

		private void ClearGridLines()
		{
			VisualGrid.Children.Clear();
		}

		private void ClearSelectionRectangle()
		{
			if (AreaSelection.Children.Contains(SelectionRectangle))
			{
				AreaSelection.Children.Remove(SelectionRectangle);
				AreaSelection.Children.Remove(SelectionRectangleBlack);
				SelectionRectangle = null;
			}
		}

		private void DrawLine( double x1, double x2, double y1, double y2, double thickness, Brush brush )
		{
			// Add a Line Element
			var myLine = new Line() {
				IsHitTestVisible = false,
				Stroke = brush,
				X1 = x1, Y1 = y1,
				X2 = x2, Y2 = y2,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				StrokeThickness = thickness,
			};
			VisualGrid.Children.Add(myLine);
		}

		private void DrawSelectionRectangle( double width, double height, double top, double left, double thickness, Brush brush, Brush secondBrush )
		{
			// Add a Rectangle Element
			SelectionRectangle = new Rectangle() {
				IsHitTestVisible = false,
				Stroke = brush,
				StrokeDashArray = new DoubleCollection { 4, 4 },
				StrokeThickness = thickness,
				Width = width,
				Height = height,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
			};
			AreaSelection.Children.Add(SelectionRectangle);

			Canvas.SetLeft(SelectionRectangle, left);
			Canvas.SetTop(SelectionRectangle, top);
			
			SelectionRectangleBlack = new Rectangle() {
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
		    if (frameSelectionPanelList.TryGetValue(frame, out var panel))
		    {
		        FrameSelected?.Invoke(this, new FrameSelectionPair { Frame = frame, Panel = panel});
            }
		}

		private void RecalcMouseDelta( MouseEventArgs e )
		{
			var currentMousePosition = e.GetPosition(this);
			DeltaX = (int)(currentMousePosition.X - InitMousePosition.X);
			DeltaY = (int)(currentMousePosition.Y - InitMousePosition.Y);
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

			frameSelectionPanelList.TryGetValue(frame, out FrameSelectionPanel panel);

			panel.UpdateSelectedFramePosition();
		}

		public void LandFrameOnScene( Frame frame, Point mousePosition )
		{
			frame.Parent?.Remove(frame);

			// If we can't find where to land it (that's weird) just try attach to the scene
			var hoveredFrame = GetHoveredFrameOnScene(mousePosition, false) ?? SceneFrame;

			hoveredFrame.Add(frame);

			//_treeView.ElementHierarcyView.SetSelectedItem(frame);
		}

		private List<IEditorCommand> ReleaseFrame( Point point, FrameSelectionPanel panel )
		{
			List<IEditorCommand> commands = new List<IEditorCommand>();

			if (panel.SelectedFrame != null)
			{
				if (panel.IsInDragField)
				{
					panel.IsInDragField = false;
					_parentHighlightPanel.SelectedFrame = null;

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
			InitMousePosition = e.GetPosition(this);

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

			foreach (var frameAndPanel in frameSelectionPanelList)
			{
				var frame = frameAndPanel.Key;
				var selectionPanel = frameAndPanel.Value;

				selectionPanel.InitialGlobalRectangle = new Fusion.Core.Mathematics.Rectangle(frame.X, frame.Y, frame.Width, frame.Height);
				selectionPanel.InitPanelPosition = new Point(selectionPanel.RenderTransform.Value.OffsetX, selectionPanel.RenderTransform.Value.OffsetY);
				selectionPanel.InitFrameParent = frame.Parent;
			}

			if (hovered != null)
			{
				var panel = frameSelectionPanelList[hovered];

				if (panel != null)
					panel.StartFrameDragging(e.GetPosition(this));
			}
		}

		private void LocalGrid_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			List<IEditorCommand> commands = new List<IEditorCommand>();

			foreach (var panel in frameSelectionPanelList.Values)
			{
				commands.AddRange(this.ReleaseFrame(e.GetPosition(this), panel));
			}

			_frameDragsPanel.DragMousePressed = false;

			if (paletteWindow._selectedFrameTemplate != null)
			{

				var createdFrame = Window.CreateFrameFromFile(System.IO.Path.Combine(ApplicationConfig.TemplatesPath, paletteWindow._selectedFrameTemplate) + ".xml");

				if (createdFrame != null)
				{
					commands = Window.AddframeToScene(createdFrame, e.GetPosition(this), commands);

					var command = new CommandGroup(commands.ToArray());
					CommandManager.Instance.Execute(command);
				}
				paletteWindow._selectedFrameTemplate = null;
				_parentHighlightPanel.SelectedFrame = null;
			}
			else
			{
				if (commands.Count > 0)
				{
					var command = new CommandGroup(commands.ToArray());
					CommandManager.Instance.Execute(command);
				}
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
			else if (frameSelectionPanelList.Any(fsp => fsp.Value.MousePressed))
			{
				var movedPanel = frameSelectionPanelList.FirstOrDefault(fsp => fsp.Value.MousePressed).Value;				
				if (!movedPanel.IsInDragField && (DeltaX != 0 || DeltaY != 0))
				{
					foreach (var panel in frameSelectionPanelList.Values)
					{
						this.MoveFrameToDragField(panel.SelectedFrame);
						panel.IsInDragField = true;
					}
				}
				if (movedPanel.IsInDragField || paletteWindow._selectedFrameTemplate != null)
				{
					var hovered = GetHoveredFrameOnScene(e.GetPosition(this), true);
					_parentHighlightPanel.SelectedFrame = hovered;
				}

				Point currentLocation = e.MouseDevice.GetPosition(this);

				var step = (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * GridSizeMultiplier);
				var newX = movedPanel.InitPanelPosition.X + DeltaX;
				var newY = movedPanel.InitPanelPosition.Y + DeltaY;
				var dX = newX - movedPanel.InitPanelPosition.X;
				var dY = newY - movedPanel.InitPanelPosition.Y;
				if (NeedSnapping())
				{
					dX -= -step / 2 + (newX + step / 2) % step;
					dY -= -step / 2 + (newY + step / 2) % step;
				}
				foreach (var panel in frameSelectionPanelList.Values)
				{
					var delta2 = new TranslateTransform(panel.InitPanelPosition.X + dX, panel.InitPanelPosition.Y + dY);
					panel.RenderTransform = delta2;
				}
				movedPanel.PreviousMouseLocation = currentLocation;
			}
			else if (paletteWindow._selectedFrameTemplate != null)
			{
				var hovered = GetHoveredFrameOnScene(e.GetPosition(this), true);
				_parentHighlightPanel.SelectedFrame = hovered;
			}
		}

		public void RecalculateSelectionSize( Point currentLocation )
		{
			var isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
			var isControlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

			_frameDragsPanel.Resize(DeltaX, DeltaY, isShiftPressed, isControlPressed, out double heightMult, out double widthMult);

			var dragsPanelX = _frameDragsPanel.RenderTransform.Value.OffsetX;
			var dragsPanelY = _frameDragsPanel.RenderTransform.Value.OffsetY;
			foreach (var panel in frameSelectionPanelList.Values)
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
			if (SelectionRectangle != null)
			{
				var selectedframes = new List<Frame>(selectedFrames);
				Fusion.Core.Mathematics.Rectangle selectedArea = new Fusion.Core.Mathematics.Rectangle(
						(int)Canvas.GetLeft(SelectionRectangle),
						(int)Canvas.GetTop(SelectionRectangle),
						(int)SelectionRectangle.Width,
						(int)SelectionRectangle.Height
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
			if(AreaSelectionEnabled)
				DrawSelectionRectangle(InitMousePosition, e.GetPosition(this));
		}
	}
}
