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
	public partial class FrameDragsPanel : Grid, INotifyPropertyChanged
	{
		public Border CurrentDrag { get; set; }
		public Point CurrentPivot { get; private set; }
		public Point CurrentDragInitPosition { get; private set; }

		public Size SelectedGroupInitSize { get; set; }
		public Point SelectedGroupInitPosition { get; private set; }
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

		public event PropertyChangedEventHandler PropertyChanged;

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

			this.LayoutUpdated += ( s, e ) =>
			{
				Vector offset = VisualTreeHelper.GetOffset(this);
				double XPosValue = offset.X;
				double YPosValue = offset.Y;
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

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
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
	}
}
