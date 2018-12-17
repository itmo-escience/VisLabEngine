using Fusion.Engine.Frames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfEditorTest.UndoRedo;
using CommandManager = WpfEditorTest.UndoRedo.CommandManager;

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for SelectedFramePanel.xaml
	/// </summary>
	public partial class FrameSelectionPanel : UserControl
	{
		private Fusion.Engine.Frames.Frame _selectedFrame;
		private double _widthBuffer;
		private double _heightBuffer;

		private double _oldX;
		private double _oldY;
		private bool _locked = false;
		private bool _isMoved;

		public Fusion.Engine.Frames.Frame SelectedFrame
		{
			get => _selectedFrame;
			set
			{
				if (_selectedFrame != null)
					_selectedFrame.PropertyChanged -= FrameDimensionsChange;

				_selectedFrame = value;
				if (_selectedFrame == null) return;

				Width = WidthBuffer = _selectedFrame.Width;
				Height = HeightBuffer = _selectedFrame.Height;

				var delta = new TranslateTransform();
				RenderTransform = delta;
				delta.X = _selectedFrame.GlobalRectangle.X;// == _selectedFrame.X ? _selectedFrame.GlobalRectangle.X : _selectedFrame.X;
				delta.Y = _selectedFrame.GlobalRectangle.Y;// == _selectedFrame.Y ? _selectedFrame.GlobalRectangle.Y : _selectedFrame.Y;
				PreviousTransform = RenderTransform;

				foreach (var drag in Drags)
				{
					drag.RenderTransform = new TranslateTransform(0, 0);
				}

				UpdateVisualAnchors(_selectedFrame.Anchor);

				_selectedFrame.PropertyChanged += FrameDimensionsChange;

				this.PositionInfo.DataContext = this.SizeInfo.DataContext = _selectedFrame;
			}
		}

		private void FrameDimensionsChange( object sender, PropertyChangedEventArgs args )
		{
			if (_locked) return;

			switch (args.PropertyName)
			{
				case "Width":
				case "UnitWidth":
					{
						WidthBuffer = _selectedFrame.Width;
						break;
					}
				case "Height":
				case "UnitHeight":
					{
						HeightBuffer = _selectedFrame.Height;
						break;
					}
				//case "X":
				//case "Y":
				case "GlobalRectangle":
					{
						var frameDelta = new TranslateTransform();
						RenderTransform = frameDelta;
						frameDelta.X = _selectedFrame.GlobalRectangle.X;
						frameDelta.Y = _selectedFrame.GlobalRectangle.Y;
						PreviousTransform = RenderTransform;

						_oldX = RenderTransform.Value.OffsetX;
						_oldY = RenderTransform.Value.OffsetY;
						break;
					}
				case "Anchor":
					{
						UpdateVisualAnchors(_selectedFrame.Anchor);
						break;
					}
			}
		}

		private void UpdateVisualAnchors( FrameAnchor anchor )
		{
			void UpdateAnchor( bool isActive, Border drag, Border line )
			{
				drag.Background = isActive ? Brushes.Black : Brushes.White;
				drag.BorderBrush = isActive ? Brushes.White : Brushes.Black;
				line.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
			}

			UpdateAnchor((FrameAnchor.Top & anchor) != 0, TopDrag, TopAnchorLine);
			UpdateAnchor((FrameAnchor.Bottom & anchor) != 0, BottomDrag, BottomAnchorLine);
			UpdateAnchor((FrameAnchor.Left & anchor) != 0, LeftDrag, LeftAnchorLine);
			UpdateAnchor((FrameAnchor.Right & anchor) != 0, RightDrag, RightAnchorLine);
		}

		public Point PreviousMouseLocation { get; set; }
		public Transform PreviousDragTransform { get; set; }
		public Transform PreviousTransform { get; set; }
		public bool MousePressed { get; set; }
		public bool IsMoved {
			get => _isMoved;
			set {
				_isMoved = value;
				this.Cursor = value ? Cursors.Cross : Cursors.Arrow;
				if (value)
				{
					VisualAnchors.ForEach(a=>a.Visibility = Visibility.Collapsed);
				}
				else
				{
					UpdateVisualAnchors(_selectedFrame.Anchor);
				}
			}
		}
		public Border CurrentDrag { get; set; }
		public Size SelectedFrameInitSize { get; private set; }
		public Point SelectedFrameInitPosition { get; private set; }
		public bool DragMousePressed { get; set; }
		public InterfaceEditor Window { get; set; }
		public List<Border> Drags { get; private set; }
		public List<Border> VisualAnchors { get; private set; }

		public double WidthBuffer
		{
			get => _widthBuffer;
			set
			{
				_widthBuffer = value;
				this.Width = Math.Max(0, _widthBuffer); //Math.Abs(_widthBuffer);

				//var group = new TransformGroup();
				//var scale = new ScaleTransform();
				//scale.ScaleX = Math.Sign(_widthBuffer) * 1;
				//scale.ScaleY = 1;
				//group.Children.Add(scale);
				//group.Children.Add(this.RenderTransform);
				//this.RenderTransform = group;
			}
		}
		public double HeightBuffer
		{
			get => _heightBuffer;
			set
			{
				_heightBuffer = value;
				this.Height = Math.Max(0, _heightBuffer);

				//((ScaleTransform)(this.RenderTransform)).ScaleY = Math.Sign(_heightBuffer) * 1;
			}
		}

		public Point InitFramePosition { get; internal set; }
		public Fusion.Engine.Frames.Frame InitFrameParent { get; internal set; }

		public FrameSelectionPanel( InterfaceEditor interfaceEditor )
		{
			InitializeComponent();

			PreviousTransform = RenderTransform;
			Window = interfaceEditor;

			Height = ApplicationConfig.OptionsWindowSize; Width = ApplicationConfig.OptionsWindowSize;

			Drags = new List<Border>
			{
				TopLeftDrag, TopDrag, TopRightDrag,
				LeftDrag, RightDrag,
				BottomLeftDrag, BottomDrag, BottomRightDrag
			};

			VisualAnchors = new List<Border>
			{
				TopAnchorLine,
				LeftAnchorLine, RightAnchorLine,
				BottomAnchorLine,
			};

			SizeChanged += ( s, e ) =>
			{
				if (_selectedFrame == null) return;

				_locked = true;
				_selectedFrame.Width = (int)(Width + Math.Min(0, WidthBuffer));
				_selectedFrame.Height = (int)(Height + Math.Min(0, HeightBuffer));
				_locked = false;

			};
			LayoutUpdated += ( s, e ) =>
			{
				if (_selectedFrame != null && PositionChanged())
				{
					UpdateSelectedFramePosition();
				}
				UpdateAnchorLines();
			};

			_oldX = RenderTransform.Value.OffsetX;
			_oldY = RenderTransform.Value.OffsetY;
		}

		public void UpdateSelectedFramePosition()
		{
			_locked = true;
			var sumX = 0;
			var sumY = 0;
			_selectedFrame.ForEachAncestor(a => { sumX += a.X; sumY += a.Y; });
			_selectedFrame.X = (int)RenderTransform.Value.OffsetX - (sumX - _selectedFrame.X);
			_selectedFrame.Y = (int)RenderTransform.Value.OffsetY - (sumY - _selectedFrame.Y);

			_oldX = RenderTransform.Value.OffsetX;
			_oldY = RenderTransform.Value.OffsetY;

			UpdateAnchorLines();
			_locked = false;
		}

		private void UpdateAnchorLines()
		{
			if (_selectedFrame == null) return;

			double measure;
			if (TopAnchorLine.Visibility == Visibility.Visible)
			{
				measure = _selectedFrame.Y;
				TopAnchorLine.Height = Math.Abs(measure);
				TopAnchorLine.Margin = new Thickness(0, -TopAnchorLine.Height * Math.Sign(measure), 0, 9);
			}
			if (BottomAnchorLine.Visibility == Visibility.Visible)
			{
				measure = _selectedFrame.Parent.Height - (_selectedFrame.Y + _selectedFrame.Height);
				BottomAnchorLine.Height = Math.Abs(measure);
				BottomAnchorLine.Margin = new Thickness(0, 9, 0, -BottomAnchorLine.Height * Math.Sign(measure));
			}
			if (LeftAnchorLine.Visibility == Visibility.Visible)
			{
				measure = _selectedFrame.X;
				LeftAnchorLine.Width = Math.Abs(measure);
				LeftAnchorLine.Margin = new Thickness(-LeftAnchorLine.Width * Math.Sign(measure), 0, 9, 0);
			}
			if (RightAnchorLine.Visibility == Visibility.Visible)
			{
				measure = _selectedFrame.Parent.Width - (_selectedFrame.X + _selectedFrame.Width);
				RightAnchorLine.Width = Math.Abs(measure);
				RightAnchorLine.Margin = new Thickness(9, 0, -RightAnchorLine.Width * Math.Sign(measure), 0);
			}
		}

		private bool PositionChanged()
		{
			return (int)_oldX != (int)RenderTransform.Value.OffsetX || (int)_oldY != (int)RenderTransform.Value.OffsetY;
		}

		private void UserControl_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			e.Handled = false;
			StartFrameDragging(e.MouseDevice.GetPosition(Window));
		}

		public void StartFrameDragging( Point mousePosition )
		{
			MousePressed = true;
			PreviousMouseLocation = mousePosition;
			PreviousTransform = RenderTransform;
			//Window.MoveFrameToDragField(_selectedFrame);
		}

		private void Drag_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			e.Handled = true;

			CurrentDrag = sender as Border;
			SelectedFrameInitSize = new Size(SelectedFrame.Width,SelectedFrame.Height);
			SelectedFrameInitPosition = new Point(SelectedFrame.X, SelectedFrame.Y);
			DragMousePressed = true;
			PreviousMouseLocation = e.MouseDevice.GetPosition(Window);
			PreviousDragTransform = CurrentDrag.RenderTransform;
		}

		private void Drag_MouseRightButtonDown( object sender, MouseButtonEventArgs e )
		{
			FrameAnchor changedAnchor = (FrameAnchor)Enum.Parse(typeof(FrameAnchor), (sender as Border).Tag as string);
			var initialAnchor = _selectedFrame.Anchor;
			var command = new FramePropertyChangeCommand(_selectedFrame, "Anchor", initialAnchor ^= changedAnchor);
			CommandManager.Instance.Execute(command);
			this.UpdateVisualAnchors(_selectedFrame.Anchor);
		}
	}
}
