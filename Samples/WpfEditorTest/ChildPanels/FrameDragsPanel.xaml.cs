using Fusion.Engine.Frames;
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
using CommandManager = WpfEditorTest.UndoRedo.CommandManager;
using Frame = Fusion.Engine.Frames.Frame;
using Rectangle = Fusion.Core.Mathematics.Rectangle;

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

		public Dictionary<Frame, Rectangle> InitialFramesRectangles { get; private set; }

		private bool _dragMousePressed;

		private int StickingCoordsSensitivity = ApplicationConfig.FrameStickingSensitivity;

		public Dictionary<Border, Border> DragPivots { get; private set; }

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
			if (frames.Count > 0)
			{
				this.Visibility = Visibility.Visible;
				int minX = frames.Select(f => f.GlobalRectangle.X).Min();
				int minY = frames.Select(f => f.GlobalRectangle.Y).Min();
				int maxX = frames.Select(f => f.GlobalRectangle.X + f.Width).Max();
				int maxY = frames.Select(f => f.GlobalRectangle.Y + f.Height).Max();
				Width = Math.Max(maxX - minX, double.Epsilon);
				Height = Math.Max(maxY - minY, double.Epsilon);
				var delta = new TranslateTransform();
				RenderTransform = delta;
				delta.X = minX;
				delta.Y = minY;
			}
			else
			{
				this.Visibility = Visibility.Collapsed;
			}
		}

		public void StickToX( int x ) //TODO: Delete this
		{
			var frames = SelectionManager.Instance.SelectedFrames;
			if (frames.Count > 0)
			{
				Frame minXFrame = frames.Where(fr => fr.GlobalRectangle.X == frames
					.Select(f => f.GlobalRectangle.X).Min()).FirstOrDefault();
				Frame maxXFrame = frames.Where(fr => fr.GlobalRectangle.X + fr.Width == frames
					.Select(f => f.GlobalRectangle.X + f.Width).Max()).FirstOrDefault();

				var rangeX = Enumerable.Range(x - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
				if (rangeX.Contains(minXFrame.GlobalRectangle.X))
				{
					var stickingDelta = x - minXFrame.GlobalRectangle.X;
					frames.ForEach(f => f.X += stickingDelta);
				}
				if (rangeX.Contains(maxXFrame.GlobalRectangle.X + maxXFrame.GlobalRectangle.Width))
				{
					var stickingDelta = x - (maxXFrame.GlobalRectangle.X + maxXFrame.GlobalRectangle.Width);
					frames.ForEach(f => f.X += stickingDelta);
				}
			}
		}

		public void StickToY( int y ) //TODO: Delete this
		{
			var frames = SelectionManager.Instance.SelectedFrames;
			if (frames.Count > 0)
			{
				Frame minYFrame = frames.Where(fr => fr.GlobalRectangle.Y == frames
					.Select(f => f.GlobalRectangle.Y).Min()).FirstOrDefault();
				Frame maxYFrame = frames.Where(fr => fr.GlobalRectangle.Y + fr.Height == frames
					.Select(f => f.GlobalRectangle.Y + f.Height).Max()).FirstOrDefault();

				var rangeY = Enumerable.Range(y - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
				if (rangeY.Contains(minYFrame.GlobalRectangle.Y))
				{
					var stickingDelta = y - minYFrame.GlobalRectangle.Y;
					frames.ForEach(f => f.Y += stickingDelta);
				}
				if (rangeY.Contains(maxYFrame.GlobalRectangle.Y + maxYFrame.GlobalRectangle.Height))
				{
					var stickingDelta = y - (maxYFrame.GlobalRectangle.Y + maxYFrame.GlobalRectangle.Height);
					frames.ForEach(f => f.Y += stickingDelta);
				}
			}
		}

		private void RememberSizeAndPosition( List<Frame> selectedFrames )
		{
			InitialFramesRectangles = new Dictionary<Frame, Rectangle>();
			foreach (Frame frame in selectedFrames)
			{
				InitialFramesRectangles.Add(
					frame,
					new Rectangle(
						frame.GlobalRectangle.X - (int)RenderTransform.Value.OffsetX,
						frame.GlobalRectangle.Y - (int)RenderTransform.Value.OffsetY,
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
			var frames = SelectionManager.Instance.SelectedFrames;

			if (frames.Count == 1)
			{
				FrameAnchor changedAnchor = (FrameAnchor)Enum.Parse(typeof(FrameAnchor), (sender as Border).Tag as string);
				var initialAnchor = frames.First().Anchor;
				var command = new FramePropertyChangeCommand(frames.First(), "Anchor", initialAnchor ^= changedAnchor);
				CommandManager.Instance.Execute(command);
			}
		}

		internal void Resize(double deltaX, double deltaY, bool isShiftPressed, bool isControlPressed, out double heightMultiplier, out double widthMultiplier)
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

			var scaleX = Math.Abs(mouseInitPosition.X) > double.Epsilon ? ((mouseInitPosition.X + deltaX) / mouseInitPosition.X) : 1.0d;
			var scaleY = Math.Abs(mouseInitPosition.Y) > double.Epsilon ? ((mouseInitPosition.Y + deltaY) / mouseInitPosition.Y) : 1.0d;

			if (isShiftPressed)
			{
				if(Math.Abs(mouseInitPosition.X) < double.Epsilon)
				{
					scaleX = scaleY;
				} else if (Math.Abs(mouseInitPosition.Y) < double.Epsilon)
				{
					scaleY = scaleX;
				} else if ((deltaX * mouseInitPosition.Y > mouseInitPosition.X * deltaY) ^(mouseInitPosition.X < 0) ^ (mouseInitPosition.Y < 0))
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

			RenderTransform = new TranslateTransform(topLeftAbsolutePosition.X, topLeftAbsolutePosition.Y);

			widthMultiplier = newWidth / SelectedGroupInitSize.Width;
			heightMultiplier = newHeight / SelectedGroupInitSize.Height;
		}

		internal void Reposition( int deltaX, int deltaY, bool isShiftPressed, bool isControlPressed, int gridSizeMultiplier, bool needSnapping,
			List<int> stickingCoordsX, List<int> stickingCoordsY, out double dX, out double dY )
		{
			var step = (int)(FusionUI.UI.ScalableFrame.ScaleMultiplier * gridSizeMultiplier);
			var newX = SelectedGroupInitPosition.X + deltaX;
			var newY = SelectedGroupInitPosition.Y + deltaY;
			dX = newX - SelectedGroupInitPosition.X;
			dY = newY - SelectedGroupInitPosition.Y;
			if (needSnapping)
			{
				dX -= -step / 2 + (newX + step / 2) % step;
				dY -= -step / 2 + (newY + step / 2) % step;
			}

			var stickingDelta = 0;

			var newXHelper = (int)(newX + 0.5*Math.Sign(newX));
			var widthHelper = (int)(SelectedGroupInitSize.Width + 0.5);
			var newYHelper = (int)(newY + 0.5 * Math.Sign(newY));
			var heightHelper = (int)(SelectedGroupInitSize.Height + 0.5);

			var closestStickX1 = stickingCoordsX.Where(scX => Math.Abs(newXHelper - scX) == stickingCoordsX.Select(x => Math.Abs(newXHelper-x)).Min()).FirstOrDefault();
			var closestStickX2 = stickingCoordsX.Where(scX => Math.Abs(newXHelper + widthHelper - scX) == stickingCoordsX.Select(x => Math.Abs(newXHelper + widthHelper - x)).Min()).FirstOrDefault();
			var closestStickY1 = stickingCoordsY.Where(scY => Math.Abs(newYHelper - scY) == stickingCoordsY.Select(y => Math.Abs(newYHelper - y)).Min()).FirstOrDefault();
			var closestStickY2 = stickingCoordsY.Where(scY => Math.Abs(newYHelper + heightHelper - scY) == stickingCoordsY.Select(y => Math.Abs(newYHelper + heightHelper - y)).Min()).FirstOrDefault();

			var rangeX1 = Enumerable.Range(closestStickX1 - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
			var rangeX2 = Enumerable.Range(closestStickX2 - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
			var rangeY1 = Enumerable.Range(closestStickY1 - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);
			var rangeY2 = Enumerable.Range(closestStickY2 - StickingCoordsSensitivity, StickingCoordsSensitivity * 2);

			if (rangeX1.Contains(newXHelper))
			{
				stickingDelta = closestStickX1 - newXHelper;
			}
			if (rangeX2.Contains(newXHelper + widthHelper))
			{
				stickingDelta = closestStickX2 - (newXHelper + widthHelper);
			}
			dX += stickingDelta;

			stickingDelta = 0;
			if (rangeY1.Contains(newYHelper))
			{
				stickingDelta = closestStickY1 - newYHelper;
			}
			if (rangeY2.Contains(newYHelper + heightHelper))
			{
				stickingDelta = closestStickY2 - (newYHelper + heightHelper);
			}
			dY += stickingDelta;
		}
	}
}
