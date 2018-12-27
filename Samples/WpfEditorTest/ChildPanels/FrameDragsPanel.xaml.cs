using Fusion.Engine.Frames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using WpfEditorTest.FrameSelection;
using WpfEditorTest.UndoRedo;
using CommandManager = WpfEditorTest.UndoRedo.CommandManager;
using Frame = Fusion.Engine.Frames.Frame;

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for FrameDragsPanel.xaml
	/// </summary>
	public partial class FrameDragsPanel : Grid, INotifyPropertyChanged
	{
		public double XPosValue { get => _xPosValue; set { _xPosValue = value; OnPropertyChanged(); } }
		public double YPosValue { get => _yPosValue; set { _yPosValue = value; OnPropertyChanged(); } }

		public Border CurrentDrag { get; set; }
		public Size SelectedGroupInitSize { get; set; }
		public Point SelectedGroupInitPosition { get; private set; }

		public bool DragMousePressed {
			get => _dragMousePressed;
			set {
				_dragMousePressed = value;
				if (!_dragMousePressed)
				{
					InitMouseLocation = null;
				}
			}
		}

		public Point? InitMouseLocation { get; set; }
		public Transform PreviousDragTransform { get; set; }
		public List<Border> Drags { get; set; }
		public Dictionary<Frame, Tuple<Point, Size>> InitialFramesRectangles { get; private set; }

		public static readonly DependencyProperty XPosValueProperty =
			DependencyProperty.Register("XPosValue", typeof(double), typeof(FrameDragsPanel), null);
		public static readonly DependencyProperty YPosValueProperty =
			DependencyProperty.Register("YPosValue", typeof(double), typeof(FrameDragsPanel), null);
		private double _xPosValue;
		private double _yPosValue;
		private bool _dragMousePressed;

		public event PropertyChangedEventHandler PropertyChanged;

		public Dictionary<Border, MyDelegate> DragActions;

		public delegate void MyDelegate( double deltaX, double deltaY, out TranslateTransform transformDelta, out double heightDeltaMult, out double widthDeltaMult);

		public FrameDragsPanel()
		{
			InitializeComponent();

			Drags = new List<Border>
			{
				TopLeftDrag, TopDrag, TopRightDrag,
				LeftDrag, RightDrag,
				BottomLeftDrag, BottomDrag, BottomRightDrag
			};

			DragActions = new Dictionary<Border, MyDelegate>
		{
			{ TopLeftDrag, TopLeftResize },
			{ TopDrag, TopResize },
			{ TopRightDrag, TopRightResize },
			{ LeftDrag, LeftResize },
			{ RightDrag, RightResize },
			{ BottomLeftDrag, BottomLeftResize },
			{ BottomDrag, BottomResize },
			{ BottomRightDrag, BottomRightResize }

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

		private void BottomRightResize( double deltaX, double deltaY, out TranslateTransform transformDelta, out double heightDeltaMult, out double widthDeltaMult )
		{
			transformDelta = new TranslateTransform(0, 0);
			heightDeltaMult = 1.0 + deltaY / SelectedGroupInitSize.Height;
			widthDeltaMult = 1.0 + deltaX / SelectedGroupInitSize.Width;
		}

		private void BottomResize( double deltaX, double deltaY, out TranslateTransform transformDelta, out double heightDeltaMult, out double widthDeltaMult )
		{
			transformDelta = new TranslateTransform(0, 0);
			heightDeltaMult = 1.0 + deltaY / SelectedGroupInitSize.Height; 
			widthDeltaMult = 1.0;
		}

		private void BottomLeftResize( double deltaX, double deltaY, out TranslateTransform transformDelta, out double heightDeltaMult, out double widthDeltaMult )
		{
			transformDelta = new TranslateTransform(deltaX, 0);
			heightDeltaMult = 1.0 + deltaY / SelectedGroupInitSize.Height;
			widthDeltaMult = 1.0 -deltaX / SelectedGroupInitSize.Width;
		}

		private void RightResize( double deltaX, double deltaY, out TranslateTransform transformDelta, out double heightDeltaMult, out double widthDeltaMult )
		{
			transformDelta = new TranslateTransform(0, 0);
			widthDeltaMult = 1.0 + deltaX / SelectedGroupInitSize.Width;
			heightDeltaMult = 1.0;
		}

		private void LeftResize( double deltaX, double deltaY, out TranslateTransform transformDelta, out double heightDeltaMult, out double widthDeltaMult )
		{
			transformDelta = new TranslateTransform(deltaX, 0);
			widthDeltaMult = 1.0 + -deltaX / SelectedGroupInitSize.Width;
			heightDeltaMult = 1.0;
		}

		private void TopRightResize( double deltaX, double deltaY, out TranslateTransform transformDelta, out double heightDeltaMult, out double widthDeltaMult )
		{
			transformDelta = new TranslateTransform(0, deltaY);
			heightDeltaMult = 1.0 -deltaY / SelectedGroupInitSize.Height;
			widthDeltaMult = 1.0 + deltaX / SelectedGroupInitSize.Width;
		}

		private void TopResize( double deltaX, double deltaY, out TranslateTransform transformDelta, out double heightDeltaMult, out double widthDeltaMult )
		{
			transformDelta = new TranslateTransform(0, deltaY);
			heightDeltaMult = 1.0-deltaY / SelectedGroupInitSize.Height;
			widthDeltaMult = 1.0;
		}

		private void TopLeftResize( double deltaX, double deltaY, out TranslateTransform transformDelta, out double heightDeltaMult, out double widthDeltaMult )
		{
			transformDelta = new TranslateTransform(deltaX, deltaY);
			heightDeltaMult = 1.0 -deltaY / SelectedGroupInitSize.Height;
			widthDeltaMult = 1.0 -deltaX / SelectedGroupInitSize.Width;
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
			InitialFramesRectangles = new Dictionary<Frame, Tuple<Point, Size>>();
			foreach (Frame frame in selectedFrames)
			{
				InitialFramesRectangles.Add(
					frame,
					new Tuple<Point, Size>(
						new Point(frame.GlobalRectangle.X - RenderTransform.Value.OffsetX, frame.GlobalRectangle.Y - RenderTransform.Value.OffsetY),
						new Size(frame.Width, frame.Height))
					);
			}
		}

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}

		private void Drag_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			e.Handled = true;

			CurrentDrag = sender as Border;
			SelectedGroupInitSize = new Size(this.Width, this.Height);
			SelectedGroupInitPosition = new Point(this.RenderTransform.Value.OffsetX, this.RenderTransform.Value.OffsetY);
			DragMousePressed = true;
			PreviousDragTransform = CurrentDrag.RenderTransform;

			RememberSizeAndPosition(SelectionManager.Instance.SelectedFrames);
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
			//this.UpdateVisualAnchors(_selectedFrame.Anchor);
		}
	}
}
