using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Managing;
using WpfEditorTest.FrameSelection;
using WpfEditorTest.Utility;
using WpfEditorTest.Commands;
using WpfEditorTest.Utility;
using CommandManager = WpfEditorTest.Commands.CommandManager;
using Matrix3x2 = Fusion.Core.Mathematics.Matrix3x2;
using Vector2 = Fusion.Core.Mathematics.Vector2;
using Fusion.Engine.Frames2.Controllers;

namespace WpfEditorTest
{
	/// <summary>
	/// Interaction logic for WPFSelectionUILayer.xaml
	/// </summary>
	public partial class WPFSelectionUILayer : Grid
	{
		public Dictionary<UIComponent, FrameSelectionPanel> FrameSelectionPanelList = new Dictionary<UIComponent, FrameSelectionPanel>();
		private List<StickCoordinateY> StickingCoordsY = new List<StickCoordinateY>();
		private List<StickCoordinateX> StickingCoordsX = new List<StickCoordinateX>();

		private List<Line> StickLines = new List<Line>();

		private float _deltaX = 0;
		private float _deltaY = 0;
		private Point _initMousePosition;

		private Rectangle _selectionRectangle;

		public int GridSizeMultiplier { get => _gridSizeMultiplier; set { _gridSizeMultiplier = value; DrawGridLines(); } }
		private int _gridSizeMultiplier = ApplicationConfig.DefaultVisualGridSizeMultiplier;

		public IUIModifiableContainer<ISlot> DragFieldFrame => Window.DragFieldFrame;
		public IUIModifiableContainer<ISlot> SceneFrame => Window.SceneFrame;
		public IUIModifiableContainer<ISlot> RootFrame => Window.RootFrame;
		public FramePalette PaletteWindow;

		public InterfaceEditor Window { get; set; }
		public bool AreaSelectionEnabled { get; private set; } = false;
		public Rectangle SelectionRectangleBlack { get; private set; }

		public struct FrameSelectionPair
		{
			public UIComponent Frame;
			public FrameSelectionPanel Panel;
		}
		public event EventHandler<FrameSelectionPair> FrameSelected;
		public event EventHandler FramesDeselected;

		private Stack<FrameSelectionPanel> _selectionPanelPool = new Stack<FrameSelectionPanel>();
		private UIManager _uiManager;
		private ParentHighlightPanel _parentHighlightPanel;
		public FrameDragsPanel FrameDragsPanel;

		public bool IsScrollExpected { get; set; }
		private const float StickLinesTolerance = 0.0001f;

		public WPFSelectionUILayer( UIManager uiManager )
		{
			InitializeComponent();

			_uiManager = uiManager;
			_parentHighlightPanel = new ParentHighlightPanel(_uiManager);
			Children.Insert(3, _parentHighlightPanel);
			FrameDragsPanel = new FrameDragsPanel(_uiManager);
			Children.Add(FrameDragsPanel);

			this.SizeChanged += ( s, e ) =>
			{
				DrawGridLines();
			};

			SelectionManager.Instance.FrameSelected += ( s, selectedFrames ) =>
			{
				foreach (var frameAndPanel in FrameSelectionPanelList)
				{
					var frame = frameAndPanel.Key;
					var selectionPanel = frameAndPanel.Value;
					var bb = _uiManager.BoundingBox(frame.Placement);

					var commands = this.ResetSelectedFrame(new Point(bb.X, bb.Y), selectionPanel);
					CommandManager.Instance.ExecuteWithoutMemorising(commands);

					selectionPanel.SelectedFrame = null;
					_selectionPanelPool.Push(selectionPanel);
				}

				HighlightPanelsContainer.Children.Clear();
				FrameSelectionPanelList.Clear();

				if (selectedFrames.Count > 0)
				{
					foreach (var frame in selectedFrames)
					{
						var frameSelectionPanel = _selectionPanelPool.Count > 0 ? _selectionPanelPool.Pop() : new FrameSelectionPanel(_uiManager);

						FrameSelectionPanelList.Add(frame, frameSelectionPanel);

						frameSelectionPanel.SelectedFrame = frame;
						frameSelectionPanel.Visibility = Visibility.Visible;

						HighlightPanelsContainer.Children.Add(frameSelectionPanel);
					}

					SelectFrame(selectedFrames[selectedFrames.Count - 1]);
				}
			};

			FrameDragsPanel.BoundingBoxUpdated += ( s, e ) =>
			{
				CheckForStickingLines();
			};
		}

		private void CheckForStickingLines()
		{
			foreach (var x in StickingCoordsX)
			{
				x.IsActive = false;
			}
			foreach (var y in StickingCoordsY)
			{
				y.IsActive = false;
			}

			if (NeedSnapping())
			{

				ActivateStickingXLines(StickingCoordsX, FrameDragsPanel.SelectedGroupMinX, Math.Abs(FrameDragsPanel.SelectedGroupMaxX - FrameDragsPanel.SelectedGroupMinX));
				ActivateStickingYLines(StickingCoordsY, FrameDragsPanel.SelectedGroupMinY, Math.Abs(FrameDragsPanel.SelectedGroupMaxY - FrameDragsPanel.SelectedGroupMinY));
			}
		}

		private void DrawGridLines()
		{
			if (GridSizeMultiplier != 0)
			{
				ToggleGridLines(true);
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
			else
			{
				ToggleGridLines(false);
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
			VisualGrid.Children.Add(PrepareLine(x1, x2, y1, y2, thickness, brush));
		}

		private Line PrepareLine( double x1, double x2, double y1, double y2, double thickness, Brush brush )
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
			return myLine;
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

		private void SelectFrame( UIComponent frame )
		{
			if (FrameSelectionPanelList.TryGetValue(frame, out var panel))
			{
				FrameSelected?.Invoke(this, new FrameSelectionPair { Frame = frame, Panel = panel });
			}
		}

		private void RecalcMouseDelta( MouseEventArgs e )
		{
			var currentMousePosition = GetBoundedMousePosition(e);
			_deltaX = (float)(currentMousePosition.X - _initMousePosition.X);
			_deltaY = (float)(currentMousePosition.Y - _initMousePosition.Y);
		}

        private Point GetBoundedMousePosition(MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(this);
            mousePosition.X = Math.Max(Math.Min(mousePosition.X, Width), 0);
            mousePosition.Y = Math.Max(Math.Min(mousePosition.Y, Height), 0);
            return mousePosition;
        }

		public IEditorCommand ResetSelectedFrame( Point point, FrameSelectionPanel panel )
		{
			FramesDeselected?.Invoke(this, null);

            var group = this.ReleaseFrame(point, panel);
			var component = panel.SelectedFrame;

			panel.SelectedFrame = null;
			panel.Visibility = Visibility.Collapsed;

			if (panel.IsInDragField)
			{
				panel.IsInDragField = false;
				//CommandManager.Instance.ExecuteWithoutMemorising(
				//	new UIComponentParentChangeCommand(component, null, component.Parent() as IUIModifiableContainer<ISlot>)
				//);
				//(component.Parent() as IUIModifiableContainer<ISlot>)?.Remove(component); 
			}

			FrameDragsPanel.DragMousePressed = false;
			FrameDragsPanel.CurrentDrag = null;

			return group;
		}

		public UIComponent GetHoveredFrameOnScene( Point mousePos, bool ignoreScene, bool ignoreSelection )
		{
			UIComponent hoveredFrame;
			if (ignoreSelection)
			{
				hoveredFrame = UIHelper.GetLowestComponentInHierarchy(_uiManager, SceneFrame, new Vector2((float)mousePos.X, (float)mousePos.Y), SelectionManager.Instance.SelectedFrames);
				if (hoveredFrame != null && SelectionManager.Instance.SelectedFrames.Contains(hoveredFrame))
				{
					hoveredFrame = hoveredFrame.Parent() as IUIModifiableContainer<ISlot>;
				}
			}
			else
				hoveredFrame = UIHelper.GetLowestComponentInHierarchy(_uiManager, SceneFrame, new Vector2((float)mousePos.X, (float)mousePos.Y));

			if (ignoreScene)
			{
				if (hoveredFrame != SceneFrame) // if something is there and it's not SceneFrame
				{
					return hoveredFrame;
				}

				return null;
			}
			else
			{
				if (hoveredFrame != null) // if something is there
				{
					return hoveredFrame;
				}
				return null;
			}
		}

		public UIComponent GetChildFrameOnScene( Point mousePos, bool ignoreScene, bool ignoreSelection )
		{
			UIComponent hoveredFrame;
			if (ignoreSelection)
			{
				hoveredFrame = UIHelper.GetComponentInChildren(_uiManager, SceneFrame, new Vector2((float)mousePos.X, (float)mousePos.Y), SelectionManager.Instance.SelectedFrames);
				if (hoveredFrame != null && SelectionManager.Instance.SelectedFrames.Contains(hoveredFrame))
				{
					hoveredFrame = hoveredFrame.Parent() as IUIModifiableContainer<ISlot>;
				}
			}
			else
				hoveredFrame = UIHelper.GetComponentInChildren(_uiManager, SceneFrame, new Vector2((float)mousePos.X, (float)mousePos.Y));

			if (ignoreScene)
			{
				if (hoveredFrame != SceneFrame) // if something is there and it's not SceneFrame
				{
					return hoveredFrame;
				}

				return null;
			}
			else
			{
				if (hoveredFrame != null) // if something is there
				{
					return hoveredFrame;
				}
				return null;
			}
		}

		private bool HasFrameChangedSize( FrameSelectionPanel panel )
		{
			return (panel.SelectedFrame.Placement.Width != panel.InitialFrameSize.Width) || (panel.SelectedFrame.Placement.Height != panel.InitialFrameSize.Height);
		}

		public void MoveFrameToDragField( UIComponent frame )
		{
			//(frame.Parent() as IUIModifiableContainer<ISlot>)?.Remove(frame);

			//DragFieldFrame.Insert(frame,int.MaxValue);

			CommandManager.Instance.ExecuteWithoutMemorising(
				new UIComponentParentChangeCommand(frame, DragFieldFrame, frame.Parent() as IUIModifiableContainer<ISlot>)
				);

			var panel = FrameSelectionPanelList[frame];

			panel.UpdateSelectedFramePosition();
		}


		private CommandGroup ReleaseFrame( Point point, FrameSelectionPanel panel, bool needToChangeParent = false )
		{
			var group = new CommandGroup();

			if (panel.SelectedFrame != null)
			{
				if (panel.IsInDragField)
				{
					_parentHighlightPanel.SelectedFrame = null;

					var commandX = panel.SelectedFrame.Placement.X;
					var commandY = panel.SelectedFrame.Placement.Y;

					if (needToChangeParent)
					{
						var hoveredFrame = GetHoveredFrameOnScene(point, false, true) ?? SceneFrame;

						if (!(hoveredFrame is IUIModifiableContainer<ISlot>))
						{
							hoveredFrame = hoveredFrame.Parent();
						}

						if (hoveredFrame is UIController<IControllerSlot>)
						{
							hoveredFrame = SceneFrame;
						}

						IUIModifiableContainer<ISlot> container = hoveredFrame as IUIModifiableContainer<ISlot>;

						var mouseRelativeToSelected = RelativeToPoint(new Point(panel.SelectedFrame.Placement.X, panel.SelectedFrame.Placement.Y), point);
						Matrix3x2 GlobalFrameMatrix = new Matrix3x2(_uiManager.GlobalTransform(hoveredFrame.Placement).ToArray());
						GlobalFrameMatrix.Invert();
						var vectorHelper = Matrix3x2.TransformPoint(GlobalFrameMatrix, new Vector2((float)point.X, (float)point.Y));
						point = new Point(vectorHelper.X, vectorHelper.Y);

						commandX = (float)(point.X - mouseRelativeToSelected.X);
						commandY = (float)(point.Y - mouseRelativeToSelected.Y);

						group.Append(new UIComponentParentChangeCommand(panel.SelectedFrame, container, panel.InitFrameParent)); 
					}

				    group.Append(new TransformChangeCommand(panel.SelectedFrame.Placement,
						panel.SelectedFrame.Placement.Transform(),
				        panel.InitialTransform)
				    );
				    group.Append(new SlotPropertyChangeCommand(panel.SelectedFrame, "X",
						commandX,
				        (float) panel.InitFramePosition.X)
				    );
					group.Append(new SlotPropertyChangeCommand(panel.SelectedFrame, "Y",
						commandY,
					    (float)panel.InitFramePosition.Y)
					);
					
				}
				else if (this.HasFrameChangedSize(panel))
				{
				    group.Append(new TransformChangeCommand(panel.SelectedFrame.Placement,
				        panel.SelectedFrame.Placement.Transform(),
				        panel.InitialTransform)
				    );
				    group.Append(new UIComponentPropertyChangeCommand(panel.SelectedFrame, "DesiredWidth",
				        panel.SelectedFrame.DesiredWidth, (float) panel.InitialFrameSize.Width)
				    );
				    group.Append(new UIComponentPropertyChangeCommand(panel.SelectedFrame, "DesiredHeight",
				        panel.SelectedFrame.DesiredHeight, (float) panel.InitialFrameSize.Height)
				    );
				    group.Append(new SlotPropertyChangeCommand(panel.SelectedFrame, "X",
				        panel.SelectedFrame.Placement.X, (float) panel.InitFramePosition.X));
				    group.Append(new SlotPropertyChangeCommand(panel.SelectedFrame, "Y",
                        panel.SelectedFrame.Placement.Y, (float)panel.InitFramePosition.Y)
                    );
				}
			}
			panel.MousePressed = false;
			return group;
		}

		private Point RelativeToPoint( Point relativePoint, Point transformingPoint )
		{
			return new Point(transformingPoint.X - relativePoint.X, transformingPoint.Y - relativePoint.Y);
		}

		private void LocalGrid_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			UIComponent hovered;
			if (DeepClick())
				hovered = GetChildFrameOnScene(e.GetPosition(this), true, false);
			else
				hovered = GetHoveredFrameOnScene(e.GetPosition(this), true, false);


			var enableSelection = true;
			_initMousePosition = e.GetPosition(this);

			if (!FrameDragsPanel.DragMousePressed)
			{
				if (hovered != null)
				{
					IEditorCommand command = null;
					if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
					{
						var framesToSelect = new List<UIComponent>(SelectionManager.Instance.SelectedFrames);
						if (framesToSelect.Contains(hovered))
						{
							framesToSelect.Remove(hovered);
						}
						else
						{
							foreach (var frame in SelectionManager.Instance.SelectedFrames)
							{
								if (frame is IUIModifiableContainer<ISlot> container)
								{
									if (container.Contains(hovered))
									{
										enableSelection = false;
									}
								}
								if (frame.Parent() == hovered)
								{
									enableSelection = false;
								}
							}
							if (enableSelection)
								framesToSelect.Add(hovered);
						}
						command = new SelectFrameCommand(framesToSelect);
					}
					else
					{
						if (!SelectionManager.Instance.SelectedFrames.Contains(hovered) && !FrameSelectionPanelList.Any(fsp => fsp.Value.MousePressed))
						{
							if (hovered.Parent() is UIController<IControllerSlot>)
								hovered = hovered.Parent();

							command = new SelectFrameCommand(hovered);
						}
					}
					if (command != null)
					{
						CommandManager.Instance.ExecuteWithoutSettingDirty(command);
					}
				}
				else if(!FrameSelectionPanelList.Any(fsp => fsp.Value.MousePressed))
				{
					if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
					{
						var command = new SelectFrameCommand();
						CommandManager.Instance.ExecuteWithoutSettingDirty(command);
					}

					AreaSelectionEnabled = true;
				}
			}
			else
			{
				PrepareStickingCoords();
				CheckForStickingLines();
			}

			foreach (var frameAndPanel in FrameSelectionPanelList)
			{
				var frame = frameAndPanel.Key;
				var selectionPanel = frameAndPanel.Value;

                selectionPanel.InitialTransform = frame.Placement.Transform();
				selectionPanel.InitPanelPosition = new Point((float)selectionPanel.RenderTransform.Value.OffsetX, (float)selectionPanel.RenderTransform.Value.OffsetY);
				selectionPanel.InitFramePosition = new Point(frame.Placement.X, frame.Placement.Y);
				selectionPanel.InitFrameScale = new Point(frame.Placement.Transform().M11, frame.Placement.Transform().M22);
				selectionPanel.InitialFrameSize = new Size(frame.Placement.Width, frame.Placement.Height);
				selectionPanel.InitFrameParent = frame.Parent() as IUIModifiableContainer<ISlot>;
			}

			if (hovered != null && FrameSelectionPanelList.ContainsKey(hovered))
			{
				var panel = FrameSelectionPanelList[hovered];

				if (panel != null)
					panel.MousePressed = true;
				PrepareStickingCoords();
				CheckForStickingLines();
			}

			CaptureMouse();
            IsScrollExpected = true;
        }



		private void LocalGrid_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
            ReleaseMouseCapture();
            IsScrollExpected = false;
            EndMouseDrag(e.GetPosition(this));
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            //EndMouseDrag(e.GetPosition(this));
        }

        private void EndMouseDrag(Point mousePosition)
        {
            var group = new CommandGroup();

            foreach (var panel in FrameSelectionPanelList.Values)
            {
                var release = this.ReleaseFrame(mousePosition, panel);
                if(!release.IsEmpty)
                    group.Append(release);
            }

            FrameDragsPanel.DragMousePressed = false;

            ForgetStickingCoords();

            if (!group.IsEmpty)
            {
                CommandManager.Instance.Execute(group);
            }

            AreaSelectionEnd(SelectionManager.Instance.SelectedFrames);
        }

        private void LocalGrid_MouseMove( object sender, MouseEventArgs e )
		{
			RecalcMouseDelta(e);
			if (FrameDragsPanel.DragMousePressed)
			{
				RecalculateSelectionSize(e.MouseDevice.GetPosition(this));
			}
			else if (FrameSelectionPanelList.Any(fsp => fsp.Value.MousePressed))
			{
				var movedPanel = FrameSelectionPanelList.FirstOrDefault(fsp => fsp.Value.MousePressed).Value;
				if (!movedPanel.IsInDragField && (Math.Abs(_deltaX) > float.Epsilon || Math.Abs(_deltaY) > float.Epsilon))
				{
					foreach (var panel in FrameSelectionPanelList.Values)
					{
						//this.MoveFrameToDragField(panel.SelectedFrame);
						panel.IsInDragField = true;
					}
					FrameDragsPanel.SelectedGroupInitSize = new Size(FrameDragsPanel.Width, FrameDragsPanel.Height);
					FrameDragsPanel.SelectedGroupInitPosition = new Point(FrameDragsPanel.RenderTransform.Value.OffsetX, FrameDragsPanel.RenderTransform.Value.OffsetY);
				}
				else if (Math.Abs(_deltaX) > float.Epsilon || Math.Abs(_deltaY) > float.Epsilon)
				{
					RecalculateSelectionPosition(e.MouseDevice.GetPosition(this));
				}
				if (movedPanel.IsInDragField || PaletteWindow.SelectedFrameTemplate != null)
				{
					var hovered = GetHoveredFrameOnScene(e.GetPosition(this), true,true);
					_parentHighlightPanel.SelectedFrame = hovered;
				}
			}

            if (AreaSelectionEnabled)
                DrawSelectionRectangle(_initMousePosition, GetBoundedMousePosition(e));
		}

		public void PrepareStickingCoords()
		{
			ForgetStickingCoords();
				RememberStickingCoords(SceneFrame);


		}

		private void RememberStickingCoords( UIComponent frame )
		{
			if (!FrameSelectionPanelList.ContainsKey(frame))
			{
				var bBox = _uiManager.BoundingBox(frame.Placement);

				StickingCoordsX.Add(new StickCoordinateX(bBox.X, bBox.Y, (bBox.Y + bBox.Height))
				{
					ActiveChanged = DrawStickLine,
				});
				StickingCoordsX.Add(new StickCoordinateX((bBox.X + bBox.Width), bBox.Y, (bBox.Y + bBox.Height))
				{
					ActiveChanged = DrawStickLine,
				});
				StickingCoordsX.Add(new StickCoordinateX((bBox.X + bBox.Width/2), bBox.Y, (bBox.Y + bBox.Height))
				{
					ActiveChanged = DrawStickLine,
				});
				StickingCoordsY.Add(new StickCoordinateY(bBox.Y, bBox.X, (bBox.X + bBox.Width))
				{
					ActiveChanged = DrawStickLine,
				});
				StickingCoordsY.Add(new StickCoordinateY((bBox.Y + bBox.Height), bBox.X, (bBox.X + bBox.Width))
				{
					ActiveChanged = DrawStickLine,
				});
				StickingCoordsY.Add(new StickCoordinateY((bBox.Y + bBox.Height/2), bBox.X, (bBox.X + bBox.Width))
				{
					ActiveChanged = DrawStickLine,
				});

				if (frame is IUIContainer container)
				{
					foreach (var slot in container.Slots)
					{
						RememberStickingCoords(slot.Component);
					} 
				}
			}
		}

		private void DrawStickLine( object sender, EventArgs e )
		{
			if (sender is StickCoordinateX)
			{
				var coordX = sender as StickCoordinateX;
				if (coordX.IsActive)
				{
					var line =	PrepareLine(
						coordX.X, coordX.X,
						Math.Min(coordX.TopY, FrameDragsPanel.RenderTransform.Value.OffsetY + FrameDragsPanel.Height * Math.Min(FrameDragsPanel.RenderTransform.Value.M22,0)),
						Math.Max(coordX.BottomY, FrameDragsPanel.RenderTransform.Value.OffsetY + FrameDragsPanel.Height * FrameDragsPanel.RenderTransform.Value.M22),
						2, Brushes.MediumBlue
						);
					StickLinesGrid.Children.Add(line);
					line.Tag = coordX;
					StickLines.Add(line);
				}
				else
				{
					var line = StickLines.Where(l => l.Tag == coordX).FirstOrDefault();
					if (line!=null)
					{
						StickLinesGrid.Children.Remove(line);
						StickLines.Remove(line);
					}
				}
			}
			else
			{
				var coordY = sender as StickCoordinateY;
				if (coordY.IsActive)
				{
					var line = PrepareLine(
						Math.Min(coordY.LeftX, FrameDragsPanel.RenderTransform.Value.OffsetX + FrameDragsPanel.Width * Math.Min(FrameDragsPanel.RenderTransform.Value.M11,0)),
						Math.Max(coordY.RightX, FrameDragsPanel.RenderTransform.Value.OffsetX + FrameDragsPanel.Width * FrameDragsPanel.RenderTransform.Value.M11),
						coordY.Y, coordY.Y,
						2, Brushes.MediumBlue
						);
					StickLinesGrid.Children.Add(line);
					line.Tag = coordY;
					StickLines.Add(line);
				}
				else
				{
					var line = StickLines.Where(l => l.Tag == coordY).FirstOrDefault();
					if (line != null)
					{
						StickLinesGrid.Children.Remove(line);
						StickLines.Remove(line);
					}
				}
			}
		}

		private void ForgetStickingCoords()
		{
			foreach (var coord in StickingCoordsX)
			{
				coord.IsActive = false;
				coord.ActiveChanged = null;
			}
			foreach (var coord in StickingCoordsY)
			{
				coord.IsActive = false;
				coord.ActiveChanged = null;
			}
			StickingCoordsX.Clear();
			StickingCoordsY.Clear();

			foreach (var line in StickLines)
			{
				StickLinesGrid.Children.Remove(line);
			}
			StickLines.Clear();
		}

		public void RecalculateSelectionSize( Point currentLocation )
		{
			var isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
			var isControlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

			FrameDragsPanel.Resize(_deltaX, _deltaY, isShiftPressed, isControlPressed, GridSizeMultiplier, NeedSnapping(), VisualGrid.Visibility,
				StickingCoordsX, StickingCoordsY, out double heightMult, out double widthMult);

			var dragsPanelX = FrameDragsPanel.NewDeziredPosition.X;
			var dragsPanelY = FrameDragsPanel.NewDeziredPosition.Y;
			foreach (var panel in FrameSelectionPanelList.Values)
			{
				var initRect = FrameDragsPanel.InitialFramesRectangles[panel.SelectedFrame];

				panel.WidthBuffer = initRect.Width * widthMult * panel.InitFrameScale.X;
				panel.HeightBuffer = initRect.Height * heightMult * panel.InitFrameScale.Y;

				TranslateTransform multedTransform = new TranslateTransform
				{
					X = (float)dragsPanelX + initRect.X * widthMult,
					Y = (float)dragsPanelY + initRect.Y * heightMult
				};

                var transform = new TransformGroup();
				//TODO
				//Get GlobalAngle back
				//transform.Children.Add(new RotateTransform() { Angle = panel.SelectedFrame.Placement.GlobalAngle * (180 / Math.PI), CenterX = 0, CenterY = 0 });
				transform.Children.Add(new ScaleTransform(Math.Sign(panel.WidthBuffer), Math.Sign(panel.HeightBuffer),0,0));
				transform.Children.Add(multedTransform);

				var globaltransform = _uiManager.GlobalTransform(panel.SelectedFrame.Placement);

				panel.RenderTransform = new MatrixTransform(globaltransform.M11, globaltransform.M12,
															globaltransform.M21, globaltransform.M22,
															multedTransform.X, multedTransform.Y);// transform;
				panel.UpdateLayout();
			}
		}

		public void RecalculateSelectionPosition( Point currentLocation )
		{
			var isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
			var isControlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

			FrameDragsPanel.Reposition(_deltaX, _deltaY, isShiftPressed, isControlPressed, GridSizeMultiplier, NeedSnapping(), VisualGrid.Visibility,
				StickingCoordsX, StickingCoordsY, out float dX, out float dY);

			foreach (var panel in FrameSelectionPanelList.Values)
			{
				var transformDelta = new TranslateTransform((float)panel.InitPanelPosition.X + dX, (float)panel.InitPanelPosition.Y + dY);
                var transform = new TransformGroup();
				//TODO
				//Get GlobalAngle back
                //transform.Children.Add(new RotateTransform() { Angle= panel.SelectedFrame.GlobalAngle * (180/Math.PI), CenterX = 0, CenterY = 0 });
				transform.Children.Add(new ScaleTransform(Math.Sign(panel.WidthBuffer), Math.Sign(panel.HeightBuffer)));
				transform.Children.Add(transformDelta);
                panel.RenderTransform = transform;
				panel.UpdateLayout();
			}
			FrameDragsPanel.UpdateLayout();
		}

		private void ActivateStickingYLines(List<StickCoordinateY> stickingCoordsY, float minValue, float additionalValue )
		{
			var closestStickY1 = stickingCoordsY.Where(scY => Math.Abs(minValue - scY.Y) <= StickLinesTolerance).FirstOrDefault();
			var closestStickY2 = stickingCoordsY.Where(scY => Math.Abs((minValue + additionalValue) - scY.Y) <= StickLinesTolerance).FirstOrDefault();
			var closestStickY3 = stickingCoordsY.Where(scY => Math.Abs((minValue + additionalValue / 2) - scY.Y) <= StickLinesTolerance).FirstOrDefault();

			if (closestStickY1 != null)
			{
				closestStickY1.IsActive = true;
			}
			if (closestStickY2 != null)
			{
				closestStickY2.IsActive = true;
			}
			if (closestStickY3 != null)
			{
				closestStickY3.IsActive = true;
			}
		}

		private void ActivateStickingXLines( List<StickCoordinateX> stickingCoordsX, float minValue, float additionalValue )
		{
			var closestStickX1 = stickingCoordsX.Where(scX => Math.Abs(minValue - scX.X) <= StickLinesTolerance).FirstOrDefault();
			var closestStickX2 = stickingCoordsX.Where(scX => Math.Abs((minValue + additionalValue) - scX.X) <= StickLinesTolerance).FirstOrDefault();
			var closestStickX3 = stickingCoordsX.Where(scX => Math.Abs((minValue + additionalValue / 2) - scX.X) <= StickLinesTolerance).FirstOrDefault();

			if (closestStickX1 != null)
			{
				closestStickX1.IsActive = true;
			}
			if (closestStickX2 != null)
			{
				closestStickX2.IsActive = true;
			}
			if (closestStickX3 != null)
			{
				closestStickX3.IsActive = true;
			}
		}

		private bool NeedSnapping()
		{
			return!(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));
		}

		private bool DeepClick()
		{
			return !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
		}

		private bool NeedReparenting()
		{
			return !(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));
		}

		private void AreaSelectionEnd( List<UIComponent> selectedFrames )
		{
			AreaSelectionEnabled = false;
			if (_selectionRectangle != null)
			{
				var selectedframes = new List<UIComponent>(selectedFrames);
				Fusion.Core.Mathematics.RectangleF selectedArea = new Fusion.Core.Mathematics.RectangleF(
						(int)Canvas.GetLeft(_selectionRectangle),
						(int)Canvas.GetTop(_selectionRectangle),
						(int)_selectionRectangle.Width,
						(int)_selectionRectangle.Height
					);
				foreach (UIComponent frame in SceneFrame.GetChildren())
				{
					bool contains;
					var bb = _uiManager.BoundingBox(frame.Placement);
					selectedArea.Contains(ref bb, out contains);
					if (contains && !selectedframes.Contains(frame))
					{
						selectedframes.Add(frame);
					}
				}
				ClearSelectionRectangle();
				var command = new SelectFrameCommand(selectedframes);
				CommandManager.Instance.ExecuteWithoutSettingDirty(command);
			}
		}

        private void Grid_PreviewDragOver( object sender, System.Windows.DragEventArgs e )
		{
			e.Effects = DragDropEffects.None;

			string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

			if (!string.IsNullOrEmpty(dataString) || e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
				var hovered = GetHoveredFrameOnScene(e.GetPosition(this), true,false);
				_parentHighlightPanel.SelectedFrame = hovered;
				return;
			}
		}

		private void Grid_PreviewDrop( object sender, System.Windows.DragEventArgs e )
		{
		    UIComponent createdFrame = null;
			if (e.Data.GetDataPresent(DataFormats.StringFormat))
			{
				string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
				if (!string.IsNullOrEmpty(dataString))
				{
					createdFrame = Window.CreateFrameFromFile(System.IO.Path.Combine(ApplicationConfig.TemplatesPath, dataString) + ".xml");
                }
			}
			else if (e.Data.GetDataPresent(DataFormats.FileDrop))
		    {
		        Type dataType = (Type) e.Data.GetData(DataFormats.FileDrop);
		        if (dataType != null)
		        {
		            createdFrame = Activator.CreateInstance(dataType) as UIComponent;
		        }
		    }

		    if (createdFrame != null)
		    {
		        var commands = Window.AddFrameToScene(createdFrame, e.GetPosition(this));
		        commands.Append(new SelectFrameCommand(createdFrame));

		        CommandManager.Instance.Execute(commands);

		        foreach (UIComponent component in UIHelper.BFSTraverse(createdFrame))
		        {
		            UIManager.MakeComponentNameValid(component, SceneFrame, component);
		        }
		    }
		    PaletteWindow.SelectedFrameTemplate = null;
		    _parentHighlightPanel.SelectedFrame = null;
		}
    }
}
