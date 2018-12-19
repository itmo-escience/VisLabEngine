using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfEditorTest.UndoRedo;
using Frame = Fusion.Engine.Frames.Frame;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
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

		private readonly ParentHighlightPanel _parentHighlightPanel;
		private int permanentChildrenCount;

		private int DeltaX = 0;
		private int DeltaY = 0;
		private Point InitMousePosition;

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

			_parentHighlightPanel = new ParentHighlightPanel();
			Children.Add(_parentHighlightPanel);

			permanentChildrenCount = Children.Count; // DxElement + ParentHighLightPanel + GridCanvas

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

			    Children.RemoveRange(permanentChildrenCount, frameSelectionPanelList.Count);
                frameSelectionPanelList.Clear();

				if (selectedFrames.Count > 0)
				{
					foreach (var frame in selectedFrames)
					{
					    var frameSelectionPanel = _selectionPanelPool.Count > 0 ? _selectionPanelPool.Pop() : new FrameSelectionPanel(this);

						frameSelectionPanelList.Add(frame, frameSelectionPanel);

					    frameSelectionPanel.SelectedFrame = frame;
					    frameSelectionPanel.Visibility = Visibility.Visible;

                        Children.Add(frameSelectionPanel);
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

		private void ClearGridLines()
		{
			VisualGrid.Children.Clear();
		}

		private void DrawLine( double x1, double x2, double y1, double y2, double thickness, Brush brush )
		{
			// Add a Line Element
			var myLine = new Line();
			myLine.Stroke = brush;
			myLine.X1 = x1;
			myLine.X2 = x2;
			myLine.Y1 = y1;
			myLine.Y2 = y2;
			myLine.HorizontalAlignment = HorizontalAlignment.Left;
			myLine.VerticalAlignment = VerticalAlignment.Top;
			myLine.StrokeThickness = thickness;
			VisualGrid.Children.Add(myLine);
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

		private void GetMouseDeltaAfterFrameMouseDown( MouseEventArgs e )
		{
			var currentMousePosition = e.GetPosition(this);
			DeltaX = (int)InitMousePosition.X - (int)currentMousePosition.X;
			DeltaY = (int)InitMousePosition.Y - (int)currentMousePosition.Y;
		}

		public List<IEditorCommand> ResetSelectedFrame( Point point, FrameSelectionPanel panel )
		{

			FramesDeselected?.Invoke(this, null);

			List<IEditorCommand> commands = this.ReleaseFrame(point, panel);

			panel.SelectedFrame = null;
			panel.DragMousePressed = false;
			panel.CurrentDrag = null;
			panel.Visibility = Visibility.Collapsed;

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
			return panel.SelectedFrame.Width != (int)panel.SelectedFrameInitSize.Width ||
			panel.SelectedFrame.Height != (int)panel.SelectedFrameInitSize.Height ||
			panel.SelectedFrame.X != (int)panel.SelectedFrameInitPosition.X ||
			panel.SelectedFrame.Y != (int)panel.SelectedFrameInitPosition.Y;
		}

		public void MoveFrameToDragField( Frame frame )
		{
			frame.Parent?.Remove(frame);

			DragFieldFrame.Add(frame);

			FrameSelectionPanel panel;
			frameSelectionPanelList.TryGetValue(frame, out panel);

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
				if (panel.DragMousePressed)
				{
					panel.DragMousePressed = false;
					if (this.HasFrameChangedSize(panel))
					{
						commands.Add(new CommandGroup(
					new FramePropertyChangeCommand(panel.SelectedFrame, "Width",
					panel.SelectedFrame.Width, (int)panel.SelectedFrameInitSize.Width),
					new FramePropertyChangeCommand(panel.SelectedFrame, "Height",
					panel.SelectedFrame.Height, (int)panel.SelectedFrameInitSize.Height),
					new FramePropertyChangeCommand(panel.SelectedFrame, "X",
					panel.SelectedFrame.X, (int)panel.SelectedFrameInitPosition.X),
					new FramePropertyChangeCommand(panel.SelectedFrame, "Y",
					panel.SelectedFrame.Y, (int)panel.SelectedFrameInitPosition.Y)
				));
						//CommandManager.Instance.Execute(command);
					}
				}

				else if (panel.IsMoved)
				{
					panel.IsMoved = false;
					_parentHighlightPanel.SelectedFrame = null;

					var hoveredFrame = GetHoveredFrameOnScene(point, false) ?? SceneFrame;
					panel.SelectedFrame.Parent?.Remove(panel.SelectedFrame);

					commands.Add(new CommandGroup(
						new FrameParentChangeCommand(panel.SelectedFrame, hoveredFrame, panel.InitFrameParent),
						new FramePropertyChangeCommand(panel.SelectedFrame, "X",
						(int)point.X - hoveredFrame.GlobalRectangle.X - ((int)point.X - panel.SelectedFrame.GlobalRectangle.X),
						(int)panel.InitFramePosition.X),
						new FramePropertyChangeCommand(panel.SelectedFrame, "Y",
						(int)point.Y - hoveredFrame.GlobalRectangle.Y - ((int)point.Y - panel.SelectedFrame.GlobalRectangle.Y),
						(int)panel.InitFramePosition.Y)
					));
					//CommandManager.Instance.Execute(command);
					//LandFrameOnScene(_frameSelectionPanel.SelectedFrame, point);
					//SelectFrame(panel.SelectedFrame);
					//_frameSelectionPanel.UpdateSelectedFramePosition();
				}
			}
			panel.MousePressed = false;
			return commands;
		}

		private void LocalGrid_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			var hovered = GetHoveredFrameOnScene(e.GetPosition(this), true);

			if (hovered != null)
			{
				//SelectFrame(hovered);
				IEditorCommand command = null;
				if (Keyboard.IsKeyDown(Key.LeftShift))
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

				foreach (var frameAndPanel in frameSelectionPanelList)
				{
					frameAndPanel.Value.InitFramePosition = new Point(frameAndPanel.Key.X, frameAndPanel.Key.Y);
					frameAndPanel.Value.InitPanelPosition = 
						new Point(frameAndPanel.Value.RenderTransform.Value.OffsetX, frameAndPanel.Value.RenderTransform.Value.OffsetY);
					frameAndPanel.Value.InitFrameParent = frameAndPanel.Key.Parent;
				}

				InitMousePosition = e.GetPosition(this);

				FrameSelectionPanel panel;
				frameSelectionPanelList.TryGetValue(hovered, out panel);

				if (panel != null)
					panel.StartFrameDragging(e.GetPosition(this));

			}
			else
			{
				var command = new SelectFrameCommand(new List<Frame> { });
				CommandManager.Instance.Execute(command);
			}
		}

		private void LocalGrid_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			List<IEditorCommand> commands = new List<IEditorCommand>();

			foreach (var panel in frameSelectionPanelList.Values)
			{
				commands.AddRange(this.ReleaseFrame(e.GetPosition(this), panel));
			}

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
		}

		private void LocalGrid_MouseMove( object sender, MouseEventArgs e )
		{

			if (frameSelectionPanelList.Any(fsp => fsp.Value.DragMousePressed)  /*_frameSelectionPanel.DragMousePressed*/)
			{
				var draggedPanel = frameSelectionPanelList.FirstOrDefault(fsp => fsp.Value.DragMousePressed).Value;

				Point currentLocation = e.MouseDevice.GetPosition(this);
				var deltaX = currentLocation.X - draggedPanel.PreviousMouseLocation.X;
				var deltaY = currentLocation.Y - draggedPanel.PreviousMouseLocation.Y;
				TranslateTransform delta = null;// = new TranslateTransform (deltaX, deltaY);
				double HeightBufferDelta = 0;
				double WidthBufferDelta = 0;



				switch (draggedPanel.Drags.IndexOf(draggedPanel.CurrentDrag))
				{
					case 0:
						{
							delta = new TranslateTransform(deltaX, deltaY);
							HeightBufferDelta = -deltaY;
							WidthBufferDelta = -deltaX;
							break;
						}
					case 1:
						{
							delta = new TranslateTransform(0, deltaY);
							HeightBufferDelta = -deltaY;
							break;
						}
					case 2:
						{
							delta = new TranslateTransform(0, deltaY);
							HeightBufferDelta = -deltaY;
							WidthBufferDelta = deltaX;
							break;
						}
					case 3:
						{
							delta = new TranslateTransform(deltaX, 0);
							WidthBufferDelta = -deltaX;
							break;
						}
					case 4:
						{
							delta = new TranslateTransform(0, 0);
							WidthBufferDelta = deltaX;
							break;
						}
					case 5:
						{
							delta = new TranslateTransform(deltaX, 0);
							HeightBufferDelta = deltaY;
							WidthBufferDelta = -deltaX;
							break;
						}
					case 6:
						{
							delta = new TranslateTransform(0, 0);
							HeightBufferDelta = deltaY;
							break;
						}
					case 7:
						{
							delta = new TranslateTransform(0, 0);
							HeightBufferDelta = deltaY;
							WidthBufferDelta = deltaX;
							break;
						}
				}

				foreach (var panel in frameSelectionPanelList.Values)
				{
					panel.HeightBuffer += HeightBufferDelta;
					panel.WidthBuffer += WidthBufferDelta;

					var group = new TransformGroup();
					group.Children.Add(panel.PreviousTransform);
					group.Children.Add(delta);
					panel.RenderTransform = group;
					panel.PreviousTransform = panel.RenderTransform;

				}
				draggedPanel.PreviousMouseLocation = currentLocation;

			}
			else if (frameSelectionPanelList.Any(fsp => fsp.Value.MousePressed) /*_frameSelectionPanel.MousePressed*/)
			{
				var movedPanel = frameSelectionPanelList.FirstOrDefault(fsp => fsp.Value.MousePressed).Value;
				this.GetMouseDeltaAfterFrameMouseDown(e);
				if (!movedPanel.IsMoved && (DeltaX != 0 || DeltaY != 0))
				{
					foreach (var panel in frameSelectionPanelList.Values)
					{
						this.MoveFrameToDragField(panel.SelectedFrame);
						panel.IsMoved = true;
					}

				}

				if (movedPanel.IsMoved || paletteWindow._selectedFrameTemplate != null)
				{
					var hovered = GetHoveredFrameOnScene(e.GetPosition(this), true);
					_parentHighlightPanel.SelectedFrame = hovered;
				}

				Point currentLocation = e.MouseDevice.GetPosition(this);

				var step = (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * GridSizeMultiplier);
				var newX = movedPanel.InitPanelPosition.X - DeltaX;
				var newY = movedPanel.InitPanelPosition.Y - DeltaY;
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
					panel.PreviousTransform = panel.RenderTransform;

				}
				movedPanel.PreviousMouseLocation = currentLocation;
			}
			else if (paletteWindow._selectedFrameTemplate != null)
			{
				var hovered = GetHoveredFrameOnScene(e.GetPosition(this), true);
				_parentHighlightPanel.SelectedFrame = hovered;
			}
		}

		private bool NeedSnapping()
		{
			return VisualGrid.Visibility == Visibility.Visible && !Keyboard.IsKeyDown(Key.LeftAlt);
		}
	}
}
