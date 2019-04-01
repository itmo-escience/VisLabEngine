using Fusion.Engine.Frames2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfEditorTest.FrameSelection;
using WpfEditorTest.Utility;

using RectangleF = Fusion.Core.Mathematics.RectangleF;
using Vector2 = Fusion.Core.Mathematics.Vector2;
using Matrix3x2 = Fusion.Core.Mathematics.Matrix3x2;

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for FrameDragsPanel.xaml
	/// </summary>
	public partial class FrameDragsPanel : Grid
	{
		public EventHandler BoundingBoxUpdated;

		public Border CurrentDrag { get; set; }
		public Point CurrentPivot { get; private set; }
		public Point CurrentDragInitPosition { get; private set; }

		public Size SelectedGroupInitSize { get; set; }
		public Point SelectedGroupInitPosition { get; set; }
		public Point CenteredPivot { get; private set; }

		public bool DragMousePressed
		{
			get => _dragMousePressed;
			set
			{
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
		public Point TransformedCurrentDragInitPosition { get; private set; }
		public float SelectedGroupMinX { get; private set; }
		public float SelectedGroupMinY { get; private set; }
		public float SelectedGroupMaxX { get; private set; }
		public float SelectedGroupMaxY { get; private set; }

		public Matrix3x2 GlobalFrameMatrix;

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
				SelectedGroupMinX = frames.Select(f => f.BoundingBox.X).Min();
				SelectedGroupMinY = frames.Select(f => f.BoundingBox.Y).Min();
				SelectedGroupMaxX = frames.Select(f => f.BoundingBox.X + f.BoundingBox.Width).Max();
				SelectedGroupMaxY = frames.Select(f => f.BoundingBox.Y + f.BoundingBox.Height).Max();
				Width = Math.Max(SelectedGroupMaxX - SelectedGroupMinX, double.Epsilon);
				Height = Math.Max(SelectedGroupMaxY - SelectedGroupMinY, double.Epsilon);
				var delta = new TranslateTransform();
				RenderTransform = delta;
				delta.X = SelectedGroupMinX;
				delta.Y = SelectedGroupMinY;
			}
			else if (frames.Count == 1)
			{
				Visibility = Visibility.Visible;
				var component = SelectionManager.Instance.SelectedFrames.First();
				SelectedGroupMinX = component.BoundingBox.X;
				SelectedGroupMinY = component.BoundingBox.Y;
				SelectedGroupMaxX = component.BoundingBox.X + component.BoundingBox.Width;
				SelectedGroupMaxY = component.BoundingBox.Y + component.BoundingBox.Height;

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
				SelectedGroupMinX = 0;
				SelectedGroupMinY = 0;
				SelectedGroupMaxX = 0;
				SelectedGroupMaxY = 0;

				this.Visibility = Visibility.Collapsed;
			}

			BoundingBoxUpdated?.Invoke(this, null);
		}

		private void RememberSizeAndPosition( List<UIComponent> selectedFrames )
		{
			InitialFramesRectangles = new Dictionary<UIComponent, RectangleF>();
			foreach (UIComponent frame in selectedFrames)
			{


				InitialFramesRectangles.Add(
					frame,
					new RectangleF(
						frame.GlobalTransform.M31 - (float)RenderTransform.Value.OffsetX,
						frame.GlobalTransform.M32 - (float)RenderTransform.Value.OffsetY,
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
			CenteredPivot = new Point((CurrentPivot.X + CurrentDragInitPosition.X) / 2, (CurrentPivot.Y + CurrentDragInitPosition.Y) / 2);
			if (SelectionManager.Instance.SelectedFrames.Count == 1)
			{
				GlobalFrameMatrix = new Matrix3x2(SelectionManager.Instance.SelectedFrames.First().GlobalTransform.ToArray());
				GlobalFrameMatrix.Invert();

				var vectorHelper = Matrix3x2.TransformPoint(GlobalFrameMatrix, new Vector2((float)CurrentPivot.X, (float)CurrentPivot.Y));
				CurrentPivot = new Point(vectorHelper.X, vectorHelper.Y);
				vectorHelper = Matrix3x2.TransformPoint(GlobalFrameMatrix, new Vector2((float)CurrentDragInitPosition.X, (float)CurrentDragInitPosition.Y));
				TransformedCurrentDragInitPosition = new Point(vectorHelper.X, vectorHelper.Y);
				vectorHelper = Matrix3x2.TransformPoint(GlobalFrameMatrix, new Vector2((float)SelectedGroupInitPosition.X, (float)SelectedGroupInitPosition.Y));
				SelectedGroupInitPosition = new Point(vectorHelper.X, vectorHelper.Y);
				vectorHelper = Matrix3x2.TransformPoint(GlobalFrameMatrix, new Vector2((float)CenteredPivot.X, (float)CenteredPivot.Y));
				CenteredPivot = new Point(vectorHelper.X, vectorHelper.Y);

				GlobalFrameMatrix.Invert();
			}
			else
			{
				TransformedCurrentDragInitPosition = CurrentDragInitPosition;
			}


			DragMousePressed = true;
			PreviousDragTransform = CurrentDrag.RenderTransform;
			RememberSizeAndPosition(SelectionManager.Instance.SelectedFrames);
		}

		private Point GetPosition( Border border )
		{
			var offset = VisualTreeHelper.GetOffset(border);
			double angle = 0;

			var globalTransform = SelectionManager.Instance.SelectedFrames.First().GlobalTransform;

			if (SelectionManager.Instance.SelectedFrames.Count == 1)
			{
				angle = SelectionManager.Instance.SelectedFrames.First().GlobalAngle;
			}
			return new Point(
				RenderTransform.Value.OffsetX +
				(offset.X + border.Width * border.RenderTransformOrigin.X) * globalTransform.M11 + (offset.Y + border.Height * border.RenderTransformOrigin.Y) * globalTransform.M21,
				RenderTransform.Value.OffsetY +
				(offset.Y + border.Height * border.RenderTransformOrigin.Y) * globalTransform.M22 + (offset.X + border.Width * border.RenderTransformOrigin.X) * globalTransform.M12
			);
		}

		private Point RelativeToPivot( Point pivotPoint, Point point )
		{
			return new Point(/*Math.Round(*/point.X - pivotPoint.X,/* 3), Math.Round(*/point.Y - pivotPoint.Y/*, 3)*/);
		}

		private Point AbsolutePosition( Point pivotPoint, Point point )
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

		internal void Resize( double deltaX, double deltaY, bool isShiftPressed, bool isControlPressed, int gridSizeMultiplier, bool needSnapping,
			Visibility gridVisibility, List<StickCoordinateX> stickingCoordsX, List<StickCoordinateY> stickingCoordsY, out double heightMultiplier, out double widthMultiplier )
		{
			var pivot = isControlPressed ? CenteredPivot : CurrentPivot;

			var topLeftCorner = RelativeToPivot(pivot, SelectedGroupInitPosition);
			var bottomRightCorner = RelativeToPivot(
				pivot,
				new Point(
					SelectedGroupInitPosition.X + SelectedGroupInitSize.Width,
					SelectedGroupInitPosition.Y + SelectedGroupInitSize.Height)
				);
			var mouseInitPosition = RelativeToPivot(pivot, TransformedCurrentDragInitPosition);

			/*---*/
			bool gridEnabled = gridVisibility == Visibility.Visible;
			var step = (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * gridSizeMultiplier);
			double newX;
			double newY;
			if (SelectionManager.Instance.SelectedFrames.Count == 1)
			{

				var deltaVector = new Vector2((float)(deltaX + CurrentDragInitPosition.X), (float)(deltaY + CurrentDragInitPosition.Y));
				GlobalFrameMatrix.Invert();
				deltaVector = Matrix3x2.TransformPoint(GlobalFrameMatrix, deltaVector);
				GlobalFrameMatrix.Invert();
				deltaX = deltaVector.X - TransformedCurrentDragInitPosition.X;
				deltaY = deltaVector.Y - TransformedCurrentDragInitPosition.Y;
			}

			var dX = deltaX;
			var dY = deltaY;
			if (gridEnabled && needSnapping)
			{
				newX = mouseInitPosition.X + deltaX;
				newY = mouseInitPosition.Y + deltaY;
				dX -= -step / 2 + (newX + step / 2) % step;
				dY -= -step / 2 + (newY + step / 2) % step;
			}
			else if (needSnapping)
			{
				newX = CurrentDragInitPosition.X + deltaX;
				newY = CurrentDragInitPosition.Y + deltaY;

				float stickingDelta = 0;

				var newXHelper = (float)newX;
				var newYHelper = (float)newY;

				//foreach (var x in stickingCoordsX)
				//{
				//	x.IsActive = false;
				//}
				//foreach (var y in stickingCoordsY)
				//{
				//	y.IsActive = false;
				//}

				var closestStickX1 =
					stickingCoordsX.Where(scX => Math.Abs(newXHelper - scX.X) == stickingCoordsX.Select(x => Math.Abs(newXHelper - x.X)).Min()).FirstOrDefault();
				var closestStickY1 =
					stickingCoordsY.Where(scY => Math.Abs(newYHelper - scY.Y) == stickingCoordsY.Select(y => Math.Abs(newYHelper - y.Y)).Min()).FirstOrDefault();

				if (closestStickX1 != null && closestStickY1 != null)
				{
					if (Math.Abs(closestStickX1.X - newXHelper) <= StickingCoordsSensitivity)
					{
						stickingDelta = closestStickX1.X - newXHelper;
						dX += stickingDelta;
						//closestStickX1.IsActive = true;
					}


					stickingDelta = 0;
					if (Math.Abs(closestStickY1.Y - newYHelper) <= StickingCoordsSensitivity)
					{
						stickingDelta = closestStickY1.Y - newYHelper;
						dY += stickingDelta;
						//closestStickY1.IsActive = true;
					}

				}
			}
			/*---*/

			var scaleX = Math.Abs(mouseInitPosition.X) > double.Epsilon ? ((mouseInitPosition.X + dX) / mouseInitPosition.X) : 1.0d;
			var scaleY = Math.Abs(mouseInitPosition.Y) > double.Epsilon ? ((mouseInitPosition.Y + dY) / mouseInitPosition.Y) : 1.0d;

			if (isShiftPressed)
			{
				if (Math.Abs(mouseInitPosition.X) < double.Epsilon)
				{
					scaleX = scaleY;
				}
				else if (Math.Abs(mouseInitPosition.Y) < double.Epsilon)
				{
					scaleY = scaleX;
				}
				else if ((dX * mouseInitPosition.Y > mouseInitPosition.X * dY) ^ (mouseInitPosition.X < 0) ^ (mouseInitPosition.Y < 0))
				{
					scaleY = scaleX;
				}
				else
				{
					scaleX = scaleY;
				}
			}

			var topLeftNew = new Point(topLeftCorner.X * scaleX, topLeftCorner.Y * scaleY);
			var bottomRightNew = new Point(bottomRightCorner.X * scaleX, bottomRightCorner.Y * scaleY);

			var newWidth = Math.Max(Math.Abs(bottomRightNew.X - topLeftNew.X), float.Epsilon) * Math.Sign(bottomRightNew.X - topLeftNew.X);
			var newHeight = Math.Max(Math.Abs(bottomRightNew.Y - topLeftNew.Y), float.Epsilon) * Math.Sign(bottomRightNew.Y - topLeftNew.Y);

			var topLeftAbsolutePosition = AbsolutePosition(pivot, topLeftNew);

			if (SelectionManager.Instance.SelectedFrames.Count == 1)
			{
				var vectorHelper = Matrix3x2.TransformPoint(GlobalFrameMatrix,
					new Vector2((float)topLeftAbsolutePosition.X, (float)topLeftAbsolutePosition.Y));

				topLeftAbsolutePosition = new Point(vectorHelper.X, vectorHelper.Y);
			}

			this.NewDeziredPosition = topLeftAbsolutePosition;


			widthMultiplier = newWidth / SelectedGroupInitSize.Width;
			heightMultiplier = newHeight / SelectedGroupInitSize.Height;
		}

		internal void Reposition( float deltaX, float deltaY, bool isShiftPressed, bool isControlPressed, int gridSizeMultiplier, bool needSnapping,
			Visibility gridVisibility, List<StickCoordinateX> stickingCoordsX, List<StickCoordinateY> stickingCoordsY, out float dX, out float dY )
		{
			bool gridEnabled = gridVisibility == Visibility.Visible;
			var step = (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * gridSizeMultiplier);
			float newX = (float)SelectedGroupInitPosition.X + deltaX;
			float newY = (float)SelectedGroupInitPosition.Y + deltaY;
			dX = deltaX;
			dY = deltaY;
			if (gridEnabled && needSnapping)
			{
				dX -= -step / 2 + (newX + step / 2) % step;
				dY -= -step / 2 + (newY + step / 2) % step;
			}
			else if (needSnapping)
			{


				var newXHelper = (float)newX;
				var widthHelper = (float)SelectedGroupInitSize.Width;
				var newYHelper = (float)newY;
				var heightHelper = (float)SelectedGroupInitSize.Height;


				var closestStickX1 =
					stickingCoordsX.Where(scX => Math.Abs(Math.Abs(newXHelper - scX.X) - stickingCoordsX.Select(x => Math.Abs(newXHelper - x.X)).Min())<=float.Epsilon).FirstOrDefault();
				var closestStickX2 =
					stickingCoordsX.Where(scX => Math.Abs(Math.Abs(newXHelper + widthHelper - scX.X) - stickingCoordsX.Select(x => Math.Abs(newXHelper + widthHelper - x.X)).Min()) <= float.Epsilon).FirstOrDefault();
				var closestStickX3 =
					stickingCoordsX.Where(scX => Math.Abs(Math.Abs(newXHelper + widthHelper / 2 - scX.X) - stickingCoordsX.Select(x => Math.Abs(newXHelper + widthHelper / 2 - x.X)).Min()) <= float.Epsilon).FirstOrDefault();
				var closestStickY1 =
					stickingCoordsY.Where(scY => Math.Abs(Math.Abs(newYHelper - scY.Y) - stickingCoordsY.Select(y => Math.Abs(newYHelper - y.Y)).Min()) <= float.Epsilon).FirstOrDefault();
				var closestStickY2 =
					stickingCoordsY.Where(scY => Math.Abs(Math.Abs(newYHelper + heightHelper - scY.Y) - stickingCoordsY.Select(y => Math.Abs(newYHelper + heightHelper - y.Y)).Min()) <= float.Epsilon).FirstOrDefault();
				var closestStickY3 =
					stickingCoordsY.Where(scY => Math.Abs(Math.Abs(newYHelper + heightHelper / 2 - scY.Y) - stickingCoordsY.Select(y => Math.Abs(newYHelper + heightHelper / 2 - y.Y)).Min()) <= float.Epsilon).FirstOrDefault();

				if (closestStickX1 == null || closestStickX2 == null ||
					closestStickY1 == null || closestStickY2 == null ||
					closestStickX3 == null || closestStickY3 == null)
					return;

				float? stickingDelta = null;
				float? stickingDeltaHelper1 = null;
				float? stickingDeltaHelper2 = null;
				float? stickingDeltaHelper3 = null;
				if (Math.Abs(closestStickX1.X - newXHelper)<= StickingCoordsSensitivity)
				{
					stickingDeltaHelper1 = closestStickX1.X - newXHelper;
				}
				if (Math.Abs(closestStickX2.X - newXHelper - widthHelper) <= StickingCoordsSensitivity)
				{
					stickingDeltaHelper2 = closestStickX2.X - (newXHelper + widthHelper);
				}
				if (Math.Abs(closestStickX3.X - newXHelper - widthHelper / 2) <= StickingCoordsSensitivity)
				{
					stickingDeltaHelper3 = closestStickX3.X - (newXHelper + widthHelper / 2);
				}
				stickingDelta = ClosestToZero(ClosestToZero(stickingDeltaHelper1, stickingDeltaHelper2), stickingDeltaHelper3);
				if (stickingDelta != null)
				{
					dX += stickingDelta.GetValueOrDefault(0);
				}


				stickingDelta = null;
				stickingDeltaHelper1 = null;
				stickingDeltaHelper2 = null;
				stickingDeltaHelper3 = null;
				if (Math.Abs(closestStickY1.Y - newYHelper) <= StickingCoordsSensitivity)
				{
					stickingDeltaHelper1 = closestStickY1.Y - newYHelper;
				}
				if (Math.Abs(closestStickY2.Y - newYHelper - heightHelper) <= StickingCoordsSensitivity)
				{
					stickingDeltaHelper2 = closestStickY2.Y - (newYHelper + heightHelper);
				}
				if (Math.Abs(closestStickY3.Y - newYHelper - heightHelper/2) <= StickingCoordsSensitivity)
				{
					stickingDeltaHelper3 = closestStickY3.Y - (newYHelper + heightHelper / 2);
				}
				stickingDelta = ClosestToZero(ClosestToZero(stickingDeltaHelper1, stickingDeltaHelper2), stickingDeltaHelper3);

				dY += stickingDelta.GetValueOrDefault(0);
			}
		}

		private float? ClosestToZero( float? left, float? right )
		{

			if (left != null)
			{
				if (right != null && Math.Abs((float)left) >= Math.Abs((float)right))
				{
					return right;
				}
				else
				{
					return left;
				}
			}
			else
			{
				if (right != null)
				{
					return right;
				}
				else
				{
					return null;
				}
			}
		}
	}
}
