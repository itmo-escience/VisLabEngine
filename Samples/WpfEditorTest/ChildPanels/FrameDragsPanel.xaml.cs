using Fusion.Engine.Frames2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfEditorTest.FrameSelection;
using WpfEditorTest.UndoRedo;
using WpfEditorTest.Utility;
using CommandManager = WpfEditorTest.UndoRedo.CommandManager;
using Frame = Fusion.Engine.Frames.Frame;
using RectangleF = Fusion.Core.Mathematics.RectangleF;

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for FrameDragsPanel.xaml
	/// </summary>
	public partial class FrameDragsPanel : Grid
	{
		public Border CurrentDrag { get; set; }
		public Point CurrentPivot { get; private set; }
		public Point CurrentDragInitPosition { get; private set; }

		public Size SelectedGroupInitSize { get; set; }
		public Point SelectedGroupInitPosition { get; set; }
		public Point CenteredPivot { get; private set; }

		public bool DragMousePressed {
			get => _dragMousePressed;
			set {
				_dragMousePressed = value;
			}
		}

		public Transform PreviousDragTransform { get; set; }
		public List<Border> Drags { get; set; }

		public Dictionary<UIComponent, RectangleF> InitialFramesRectangles { get; private set; }

		private bool _dragMousePressed;

		private int StickingCoordsSensitivity = ApplicationConfig.FrameStickingSensitivity;

		public Dictionary<Border, Border> DragPivots { get; private set; }
        public Point NewDeziredPosition { get; private set; }

        public FrameDragsPanel()
		{
			InitializeComponent();

			Drags = new List<Border>
			{
				TopLeftDrag, TopDrag, TopRightDrag,
				LeftDrag, RightDrag,
				BottomLeftDrag, BottomDrag, BottomRightDrag
			};

			DragPivots = new Dictionary<Border, Border>
			{
				{ TopLeftDrag, BottomRightDrag },
				{ TopDrag, BottomDrag },
				{ TopRightDrag, BottomLeftDrag },
				{ LeftDrag, RightDrag },
				{ RightDrag, LeftDrag },
				{ BottomLeftDrag, TopRightDrag },
				{ BottomDrag, TopDrag },
				{ BottomRightDrag, TopLeftDrag }
			};

			SelectionManager.Instance.FrameSelected += ( s, e ) =>
			{
				this.UpdateBoundingBox();
			};

			SelectionManager.Instance.FrameUpdated += ( s, e ) =>
			{
				this.UpdateBoundingBox();
			};
		}

		private void UpdateBoundingBox()
		{
			var frames = SelectionManager.Instance.SelectedFrames;
            if (frames.Count > 1)
            {
                Visibility = Visibility.Visible;
                int minX = (int)(frames.Select(f => f.BoundingBox.X).Min() + 0.5f);
                int minY = (int)(frames.Select(f => f.BoundingBox.Y).Min() + 0.5f);
                int maxX = (int)(frames.Select(f => f.BoundingBox.X + f.BoundingBox.Width).Max() + 0.5f);
                int maxY = (int)(frames.Select(f => f.BoundingBox.Y + f.BoundingBox.Height).Max() + 0.5f);
                Width = Math.Max(maxX - minX, double.Epsilon);
                Height = Math.Max(maxY - minY, double.Epsilon);
                var delta = new TranslateTransform();
                RenderTransform = delta;
                delta.X = minX;
                delta.Y = minY;
            }
            else if (frames.Count == 1)
            {
                Visibility = Visibility.Visible;
                UIComponent frame = frames.First();
                Width = frame.Width;
                Height = frame.Height;
                var transform = new MatrixTransform(frame.GlobalTransform.M11, frame.GlobalTransform.M12,
                                                    frame.GlobalTransform.M21, frame.GlobalTransform.M22,
                                                    frame.GlobalTransform.M31, frame.GlobalTransform.M32);
                RenderTransform = transform;
            }
            else
			{
				this.Visibility = Visibility.Collapsed;
			}
		}

		private void RememberSizeAndPosition( List<UIComponent> selectedFrames )
		{
			InitialFramesRectangles = new Dictionary<UIComponent, RectangleF>();
			foreach (UIComponent frame in selectedFrames)
			{
				InitialFramesRectangles.Add(
					frame,
					new RectangleF(
						frame.X - (int)RenderTransform.Value.OffsetX,
						frame.Y - (int)RenderTransform.Value.OffsetY,
						frame.Width,
						frame.Height
					)
				);
			}
		}

		private void Drag_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			e.Handled = false;

			CurrentDrag = sender as Border;
			CurrentPivot = GetPosition(DragPivots[CurrentDrag]);
			CurrentDragInitPosition = GetPosition(CurrentDrag);

			SelectedGroupInitSize = new Size(Width, Height);
			SelectedGroupInitPosition = new Point(RenderTransform.Value.OffsetX, RenderTransform.Value.OffsetY);
			CenteredPivot = new Point(SelectedGroupInitPosition.X + SelectedGroupInitSize.Width / 2, SelectedGroupInitPosition.Y + SelectedGroupInitSize.Height / 2);

			DragMousePressed = true;
			PreviousDragTransform = CurrentDrag.RenderTransform;
			RememberSizeAndPosition(SelectionManager.Instance.SelectedFrames);
		}

		private Point GetPosition( Border border )
		{
			var offset = VisualTreeHelper.GetOffset(border);
			return new Point(
				RenderTransform.Value.OffsetX + offset.X + border.Width * border.RenderTransformOrigin.X ,
				RenderTransform.Value.OffsetY + offset.Y + border.Height * border.RenderTransformOrigin.Y
			);
		}

		private Point RelativeToPivot(Point pivotPoint, Point point)
		{
			return new Point(point.X - pivotPoint.X, point.Y - pivotPoint.Y);
		}

		private Point AbsolutePosition(Point pivotPoint, Point point)
		{
			return new Point(point.X + pivotPoint.X, point.Y + pivotPoint.Y);
		}

		private void Drag_MouseRightButtonDown( object sender, MouseButtonEventArgs e )
		{
			//var frames = SelectionManager.Instance.SelectedFrames;

			//if (frames.Count == 1)
			//{
			//	FrameAnchor changedAnchor = (FrameAnchor)Enum.Parse(typeof(FrameAnchor), (sender as Border).Tag as string);
			//	var initialAnchor = frames.First().Anchor;
			//	var command = new FramePropertyChangeCommand(frames.First(), "Anchor", initialAnchor ^= changedAnchor);
			//	CommandManager.Instance.Execute(command);
			//}
		}

		internal void Resize(double deltaX, double deltaY, bool isShiftPressed, bool isControlPressed, int gridSizeMultiplier, bool needSnapping,
			Visibility gridVisibility, List<StickCoordinateX> stickingCoordsX, List<StickCoordinateY> stickingCoordsY, out double heightMultiplier, out double widthMultiplier)
		{
			var pivot = isControlPressed ? CenteredPivot : CurrentPivot;

			var topLeftCorner = RelativeToPivot(pivot, SelectedGroupInitPosition);
			var bottomRightCorner = RelativeToPivot(
				pivot,
				new Point(
					SelectedGroupInitPosition.X + SelectedGroupInitSize.Width,
					SelectedGroupInitPosition.Y + SelectedGroupInitSize.Height)
				);
			var mouseInitPosition = RelativeToPivot(pivot, CurrentDragInitPosition);

			/*---*/
			bool gridEnabled = gridVisibility == Visibility.Visible;
			var step = (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * gridSizeMultiplier);
			double newX;
			double newY;
			var dX = deltaX;
			var dY = deltaY;
			if (needSnapping)
			{
				newX = mouseInitPosition.X + deltaX;
				newY = mouseInitPosition.Y + deltaY;
				dX -= -step / 2 + (newX + step / 2) % step;
				dY -= -step / 2 + (newY + step / 2) % step;
			}
			else if (!gridEnabled)
			{
				newX = CurrentDragInitPosition.X + deltaX;
				newY = CurrentDragInitPosition.Y + deltaY;

				var stickingDelta = 0;

				var newXHelper = (int)(newX + 0.5 * Math.Sign(newX));
				//var widthHelper = (int)(SelectedGroupInitSize.Width + 0.5);
				var newYHelper = (int)(newY + 0.5 * Math.Sign(newY));
				//var heightHelper = (int)(SelectedGroupInitSize.Height + 0.5);

				foreach (var x in stickingCoordsX)
				{
					x.IsActive = false;
				}
				foreach (var y in stickingCoordsY)
				{
					y.IsActive = false;
				}

				var closestStickX1 =
					stickingCoordsX.Where(scX => Math.Abs(newXHelper - scX.X) == stickingCoordsX.Select(x => Math.Abs(newXHelper - x.X)).Min()).FirstOrDefault();
				//var closestStickX2 =
				//	stickingCoordsX.Where(scX => Math.Abs(newXHelper + widthHelper - scX.X) == stickingCoordsX.Select(x => Math.Abs(newXHelper + widthHelper - x.X)).Min()).FirstOrDefault();
				//var closestStickX3 =
				//	stickingCoordsX.Where(scX => Math.Abs(newXHelper + widthHelper / 2 - scX.X) == stickingCoordsX.Select(x => Math.Abs(newXHelper + widthHelper / 2 - x.X)).Min()).FirstOrDefault();
				var closestStickY1 =
					stickingCoordsY.Where(scY => Math.Abs(newYHelper - scY.Y) == stickingCoordsY.Select(y => Math.Abs(newYHelper - y.Y)).Min()).FirstOrDefault();
				//var closestStickY2 =
				//	stickingCoordsY.Where(scY => Math.Abs(newYHelper + heightHelper - scY.Y) == stickingCoordsY.Select(y => Math.Abs(newYHelper + heightHelper - y.Y)).Min()).FirstOrDefault();
				//var closestStickY3 =
				//	stickingCoordsY.Where(scY => Math.Abs(newYHelper + heightHelper / 2 - scY.Y) == stickingCoordsY.Select(y => Math.Abs(newYHelper + heightHelper / 2 - y.Y)).Min()).FirstOrDefault();

				if (closestStickX1 != null && //closestStickX2 != null &&
					closestStickY1 != null //&& closestStickY2 != null &&
										   //closestStickX3 != null && closestStickY3 != null)
					)
				{

					var rangeX1 = Enumerable.Range(closestStickX1.X - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
					//var rangeX2 = Enumerable.Range(closestStickX2.X - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
					//var rangeX3 = Enumerable.Range(closestStickX3.X - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
					var rangeY1 = Enumerable.Range(closestStickY1.Y - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
					//var rangeY2 = Enumerable.Range(closestStickY2.Y - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
					//var rangeY3 = Enumerable.Range(closestStickY3.Y - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);

					if (rangeX1.Contains(newXHelper) && mouseInitPosition.X > double.Epsilon)
					{
						stickingDelta = closestStickX1.X - newXHelper;
						closestStickX1.IsActive = true;
					}
					//if (rangeX2.Contains(newXHelper + widthHelper))
					//{
					//	stickingDelta = closestStickX2.X - (newXHelper + widthHelper);
					//	closestStickX2.IsActive = true;
					//}
					//if (rangeX3.Contains(newXHelper + widthHelper / 2))
					//{
					//	stickingDelta = closestStickX3.X - (newXHelper + widthHelper / 2);
					//	closestStickX3.IsActive = true;
					//}
					dX += stickingDelta;

					stickingDelta = 0;
					if (rangeY1.Contains(newYHelper) && mouseInitPosition.Y > double.Epsilon)
					{
						stickingDelta = closestStickY1.Y - newYHelper;
						closestStickY1.IsActive = true;
					}
					//if (rangeY2.Contains(newYHelper + heightHelper))
					//{
					//	stickingDelta = closestStickY2.Y - (newYHelper + heightHelper);
					//	closestStickY2.IsActive = true;
					//}
					//if (rangeY3.Contains(newYHelper + heightHelper / 2))
					//{
					//	stickingDelta = closestStickY3.Y - (newYHelper + heightHelper / 2);
					//	closestStickY3.IsActive = true;
					//}
					dY += stickingDelta;
				}
			}
			/*---*/

			var scaleX = Math.Abs(mouseInitPosition.X) > double.Epsilon ? ((mouseInitPosition.X + dX) / mouseInitPosition.X) : 1.0d;
			var scaleY = Math.Abs(mouseInitPosition.Y) > double.Epsilon ? ((mouseInitPosition.Y + dY) / mouseInitPosition.Y) : 1.0d;

			if (isShiftPressed)
			{
				if(Math.Abs(mouseInitPosition.X) < double.Epsilon)
				{
					scaleX = scaleY;
				} else if (Math.Abs(mouseInitPosition.Y) < double.Epsilon)
				{
					scaleY = scaleX;
				} else if ((dX * mouseInitPosition.Y > mouseInitPosition.X * dY) ^(mouseInitPosition.X < 0) ^ (mouseInitPosition.Y < 0))
				{
					scaleY = scaleX;
				} else
				{
					scaleX = scaleY;
				}
			}

			var topLeftNew = new Point(topLeftCorner.X * scaleX, topLeftCorner.Y * scaleY);
			var bottomRightNew = new Point(bottomRightCorner.X * scaleX, bottomRightCorner.Y * scaleY);

			var newWidth = Math.Max(bottomRightNew.X - topLeftNew.X, float.Epsilon);
			var newHeight = Math.Max(bottomRightNew.Y - topLeftNew.Y, float.Epsilon);

			var topLeftAbsolutePosition = AbsolutePosition(pivot, topLeftNew);

            this.NewDeziredPosition = topLeftAbsolutePosition;

            //RenderTransform = new TranslateTransform(topLeftAbsolutePosition.X, topLeftAbsolutePosition.Y);

			widthMultiplier = newWidth / SelectedGroupInitSize.Width;
			heightMultiplier = newHeight / SelectedGroupInitSize.Height;
		}

		internal void Reposition( int deltaX, int deltaY, bool isShiftPressed, bool isControlPressed, int gridSizeMultiplier, bool needSnapping,
			Visibility gridVisibility, List<StickCoordinateX> stickingCoordsX, List<StickCoordinateY> stickingCoordsY, out double dX, out double dY )
		{
			bool gridEnabled = gridVisibility == Visibility.Visible;
			var step = (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * gridSizeMultiplier);
			var newX =  SelectedGroupInitPosition.X + deltaX;
			var newY =  SelectedGroupInitPosition.Y + deltaY;
			dX = deltaX;
			dY = deltaY;
			if (needSnapping)
			{
				dX -= -step / 2 + (newX + step / 2) % step;
				dY -= -step / 2 + (newY + step / 2) % step;
			}
			else if(!gridEnabled)
			{
				var stickingDelta = 0;

				var newXHelper = (int)(newX + 0.5 * Math.Sign(newX));
				var widthHelper = (int)(SelectedGroupInitSize.Width + 0.5);
				var newYHelper = (int)(newY + 0.5 * Math.Sign(newY));
				var heightHelper = (int)(SelectedGroupInitSize.Height + 0.5);

				foreach (var x in stickingCoordsX)
				{
					x.IsActive = false;
				}
				foreach (var y in stickingCoordsY)
				{
					y.IsActive = false;
				}

				var closestStickX1 =
					stickingCoordsX.Where(scX => Math.Abs(newXHelper - scX.X) == stickingCoordsX.Select(x => Math.Abs(newXHelper - x.X)).Min()).FirstOrDefault();
				var closestStickX2 =
					stickingCoordsX.Where(scX => Math.Abs(newXHelper + widthHelper - scX.X) == stickingCoordsX.Select(x => Math.Abs(newXHelper + widthHelper - x.X)).Min()).FirstOrDefault();
				var closestStickX3 =
					stickingCoordsX.Where(scX => Math.Abs(newXHelper + widthHelper/2 - scX.X) == stickingCoordsX.Select(x => Math.Abs(newXHelper + widthHelper/2 - x.X)).Min()).FirstOrDefault();
				var closestStickY1 =
					stickingCoordsY.Where(scY => Math.Abs(newYHelper - scY.Y) == stickingCoordsY.Select(y => Math.Abs(newYHelper - y.Y)).Min()).FirstOrDefault();
				var closestStickY2 =
					stickingCoordsY.Where(scY => Math.Abs(newYHelper + heightHelper - scY.Y) == stickingCoordsY.Select(y => Math.Abs(newYHelper + heightHelper - y.Y)).Min()).FirstOrDefault();
				var closestStickY3 =
					stickingCoordsY.Where(scY => Math.Abs(newYHelper + heightHelper/2 - scY.Y) == stickingCoordsY.Select(y => Math.Abs(newYHelper + heightHelper/2 - y.Y)).Min()).FirstOrDefault();

				if (closestStickX1 == null || closestStickX2 == null ||
					closestStickY1 == null || closestStickY2 == null ||
					closestStickX3 == null || closestStickY3 == null)
					return;

				var rangeX1 = Enumerable.Range(closestStickX1.X - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
				var rangeX2 = Enumerable.Range(closestStickX2.X - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
				var rangeX3 = Enumerable.Range(closestStickX3.X - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
				var rangeY1 = Enumerable.Range(closestStickY1.Y - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
				var rangeY2 = Enumerable.Range(closestStickY2.Y - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
				var rangeY3 = Enumerable.Range(closestStickY3.Y - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);

				if (rangeX1.Contains(newXHelper))
				{
					stickingDelta = closestStickX1.X - newXHelper;
					closestStickX1.IsActive = true;
				}
				if (rangeX2.Contains(newXHelper + widthHelper))
				{
					stickingDelta = closestStickX2.X - (newXHelper + widthHelper);
					closestStickX2.IsActive = true;
				}
				if (rangeX3.Contains(newXHelper + widthHelper/2))
				{
					stickingDelta = closestStickX3.X - (newXHelper + widthHelper/2);
					closestStickX3.IsActive = true;
				}
				dX += stickingDelta;

				stickingDelta = 0;
				if (rangeY1.Contains(newYHelper))
				{
					stickingDelta = closestStickY1.Y - newYHelper;
					closestStickY1.IsActive = true;
				}
				if (rangeY2.Contains(newYHelper + heightHelper))
				{
					stickingDelta = closestStickY2.Y - (newYHelper + heightHelper);
					closestStickY2.IsActive = true;
				}
				if (rangeY3.Contains(newYHelper + heightHelper/2))
				{
					stickingDelta = closestStickY3.Y - (newYHelper + heightHelper/2);
					closestStickY3.IsActive = true;
				}
				dY += stickingDelta; 
			}
		}
	}
}
