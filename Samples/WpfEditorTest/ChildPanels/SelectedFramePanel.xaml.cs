using Fusion.Engine.Frames;
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

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for SelectedFramePanel.xaml
	/// </summary>
	public partial class SelectedFramePanel : UserControl, IDraggablePanel
	{
		private Fusion.Engine.Frames.Frame _selectedframe;
		private double _widthBuffer;
		private double _heightBuffer;

		double _oldX;
		double _oldY;
		private bool locked=false;

		public Fusion.Engine.Frames.Frame selectedframe
		{
			get => _selectedframe;
			set
			{
				_selectedframe = value;
				if (_selectedframe != null)
				{
					this.Width = WidthBuffer = _selectedframe.Width;
					this.Height = HeightBuffer = _selectedframe.Height;

					//var delta = new translatetransform
					//(_selectedframe.globalrectangle.x, _selectedframe.globalrectangle.y);

					//var group = new transformgroup();
					//group.children.add(delta);

					//this.rendertransform = group;
					//----------
					var delta = new TranslateTransform();
					this.RenderTransform = delta;
					delta.X = _selectedframe.GlobalRectangle.X;
					delta.Y = _selectedframe.GlobalRectangle.Y;
					this._previousTransform = this.RenderTransform;
					//----------

					//this.Margin = new Thickness(_selectedframe.GlobalRectangle.X, _selectedframe.GlobalRectangle.Y, 0, 0);

					foreach (var drag in drags)
					{
						drag.RenderTransform = new TranslateTransform(0, 0);
					}

					this.UpdateVisualAnchors(this._selectedframe.Anchor);

					_selectedframe.PropertyChanged += ( s, e ) => {
						if (!this.locked)
						{
							switch (e.PropertyName)
							{
								case "Width":
									{
										this.WidthBuffer = this._selectedframe.Width;
										break;
									}
								case "Height":
									{
										this.HeightBuffer = this._selectedframe.Height;
										break;
									}
								case "X":
								case "Y":
									{
										//var frameDelta = new TranslateTransform(this._selectedframe.GlobalRectangle.X - this.RenderTransform.Value.OffsetX, this._selectedframe.GlobalRectangle.Y - this.RenderTransform.Value.OffsetY);

										//var group = new TransformGroup();
										//group.Children.Add(this._previousTransform);
										//group.Children.Add(delta);

										//this.RenderTransform = group;
										//this._previousTransform = this.RenderTransform;
										var frameDelta = new TranslateTransform();
										this.RenderTransform = frameDelta;
										frameDelta.X = _selectedframe.GlobalRectangle.X;
										frameDelta.Y = _selectedframe.GlobalRectangle.Y;
										this._previousTransform = this.RenderTransform;

										_oldX = this.RenderTransform.Value.OffsetX;
										_oldY = this.RenderTransform.Value.OffsetY;
										break;
									}
								case "Anchor":
									{
										this.UpdateVisualAnchors(this._selectedframe.Anchor);
										break;
									}
							} 
						}
					};
				}


			}
		}

		private void UpdateVisualAnchors( FrameAnchor anchor )
		{
			if ((FrameAnchor.Top & anchor) != 0)
			{
				this.TopDrag.Background = Brushes.Black;
				this.TopDrag.BorderBrush = Brushes.White;
				this.TopAnchorLine.Visibility = Visibility.Visible;
			}
			else
			{
				this.TopDrag.Background = Brushes.White;
				this.TopDrag.BorderBrush = Brushes.Black;
				this.TopAnchorLine.Visibility = Visibility.Collapsed;
			}
			if ((FrameAnchor.Bottom & anchor) != 0)
			{
				this.BottomDrag.Background = Brushes.Black;
				this.BottomDrag.BorderBrush = Brushes.White;
				this.BottomAnchorLine.Visibility = Visibility.Visible;
			}
			else
			{
				this.BottomDrag.Background = Brushes.White;
				this.BottomDrag.BorderBrush = Brushes.Black;
				this.BottomAnchorLine.Visibility = Visibility.Collapsed;
			}
			if ((FrameAnchor.Left & anchor) != 0)
			{
				this.LeftDrag.Background = Brushes.Black;
				this.LeftDrag.BorderBrush = Brushes.White;
				this.LeftAnchorLine.Visibility = Visibility.Visible;
			}
			else
			{
				this.LeftDrag.Background = Brushes.White;
				this.LeftDrag.BorderBrush = Brushes.Black;
				this.LeftAnchorLine.Visibility = Visibility.Collapsed;
			}
			if ((FrameAnchor.Right & anchor) != 0)
			{
				this.RightDrag.Background = Brushes.Black;
				this.RightDrag.BorderBrush = Brushes.White;
				this.RightAnchorLine.Visibility = Visibility.Visible;
			}
			else
			{
				this.RightDrag.Background = Brushes.White;
				this.RightDrag.BorderBrush = Brushes.Black;
				this.RightAnchorLine.Visibility = Visibility.Collapsed;
			}
		}

		public Point _previousMouseLocation { get; set; }
		public Transform _previousDragTransform { get; set; }
		public Transform _previousTransform { get; set; }
		public bool _mousePressed { get; set; }
		public Border CurrentDrag { get; set; }
		public bool _dragMousePressed { get; set; }
		public Window _window { get; set; }
		public List<Border> drags { get; private set; }
		public List<Border> visualAnchors { get; private set; }

		public double WidthBuffer {
			get => _widthBuffer;
			set { _widthBuffer = value;
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
		public double HeightBuffer {
			get => _heightBuffer;
			set { _heightBuffer = value;
				this.Height = Math.Max(0, _heightBuffer);

				//((ScaleTransform)(this.RenderTransform)).ScaleY = Math.Sign(_heightBuffer) * 1;
			}
		}

		public SelectedFramePanel( InterfaceEditor interfaceEditor )
		{
			InitializeComponent();

			_previousTransform = this.RenderTransform;
			_window = interfaceEditor;

			Height = StaticData.OptionsWindowSize; Width = StaticData.OptionsWindowSize;

			this.drags = new List<Border>(new Border[]
			{
				TopLeftDrag, TopDrag, TopRightDrag,
				LeftDrag, RightDrag,
				BottomLeftDrag, BottomDrag, BottomRightDrag
			}
			);

			this.visualAnchors = new List<Border>(new Border[]
			{
				TopAnchorLine,
				LeftAnchorLine, RightAnchorLine,
				BottomAnchorLine,
			}
			);

			this.SizeChanged += ( s, e ) =>
			{

				if (this._selectedframe != null)
				{
					this.locked = true;
					this._selectedframe.Width = (int)(this.Width + Math.Min(0, WidthBuffer));
					this._selectedframe.Height = (int)(this.Height + Math.Min(0, HeightBuffer));
					this.locked = false;
				}

			};
			this.LayoutUpdated += ( s, e ) =>
			{
				if (this._selectedframe != null && this.PositionChanged())
				{
					this.locked = true;
					var sumX = 0;
					var sumY = 0;
					this._selectedframe.ForEachAncestor(a => { sumX += a.X; sumY += a.Y; });
					this._selectedframe.X = (int)this.RenderTransform.Value.OffsetX - (sumX - this._selectedframe.X);
					this._selectedframe.Y = (int)this.RenderTransform.Value.OffsetY - (sumY - this._selectedframe.Y);

					_oldX = this.RenderTransform.Value.OffsetX;
					_oldY = this.RenderTransform.Value.OffsetY;
					//this._selectedframe.Width = (int)(this.Width + Math.Min(0,WidthBuffer));
					//this._selectedframe.Height = (int)(this.Height + Math.Min(0, HeightBuffer));
					this.locked = false;
				}
				this.UpdateAnchorLines();
			};

			_oldX = this.RenderTransform.Value.OffsetX;
			_oldY = this.RenderTransform.Value.OffsetY;

			//this.selectedframe.PropertyChanged += ( s, e ) => {
			//	switch (e.PropertyName)
			//	{
			//		case "Width":
			//			{
			//				this.WidthBuffer = this._selectedframe.Width;
			//				break;
			//			}
			//		case "Height":
			//			{
			//				this.HeightBuffer = this._selectedframe.Height;
			//				break;
			//			}
			//		case "X":
			//		case "Y":
			//			{
			//				var delta = new TranslateTransform(this._selectedframe.X - this._oldX, this._selectedframe.Y - this._oldY);

			//				var group = new TransformGroup();
			//				group.Children.Add(this._previousTransform);
			//				group.Children.Add(delta);

			//				this.RenderTransform = group;
			//				this._previousTransform = this.RenderTransform;
			//				break;
			//			}
			//	}
			//};
		}

		private void UpdateAnchorLines()
		{
			if (this._selectedframe != null)
			{
				double measure;
				if (this.TopAnchorLine.Visibility == Visibility.Visible)
				{
					measure = this._selectedframe.Y;
					this.TopAnchorLine.Height = Math.Abs(measure);
					this.TopAnchorLine.Margin = new Thickness(0, -this.TopAnchorLine.Height * Math.Sign(measure), 0, 9);
				}
				if (this.BottomAnchorLine.Visibility == Visibility.Visible)
				{
					measure = this._selectedframe.Parent.Height - (this._selectedframe.Y + this._selectedframe.Height);
					this.BottomAnchorLine.Height = Math.Abs(measure);
					this.BottomAnchorLine.Margin = new Thickness(0, 9, 0, -this.BottomAnchorLine.Height * Math.Sign(measure));
				}
				if (this.LeftAnchorLine.Visibility == Visibility.Visible)
				{
					measure = this._selectedframe.X;
					this.LeftAnchorLine.Width = Math.Abs(measure);
					this.LeftAnchorLine.Margin = new Thickness(-this.LeftAnchorLine.Width * Math.Sign(measure), 0, 9, 0);
				}
				if (this.RightAnchorLine.Visibility == Visibility.Visible)
				{
					measure = this._selectedframe.Parent.Width - (this._selectedframe.X + this._selectedframe.Width);
					this.RightAnchorLine.Width = Math.Abs(measure);
					this.RightAnchorLine.Margin = new Thickness(9, 0, -this.RightAnchorLine.Width * Math.Sign(measure), 0);
				} 
			}
		}

		private bool PositionChanged()
		{
			return Math.Round(_oldX, 0) != Math.Round(this.RenderTransform.Value.OffsetX, 0) ||
				Math.Round(_oldY, 0) != Math.Round(this.RenderTransform.Value.OffsetY, 0);
		}

		public void Border_MouseDown( object sender, MouseButtonEventArgs e )
		{
			//throw new NotImplementedException();
		}

		private void UserControl_MouseDown( object sender, MouseButtonEventArgs e )
		{
			this._mousePressed = true;
			_previousMouseLocation = e.MouseDevice.GetPosition(_window);
			_previousTransform = this.RenderTransform;
		}

		private void UserControl_MouseMove( object sender, MouseEventArgs e )
		{
			//if (this._mousePressed)
			//{
			//	var sumX = 0;
			//	var sumY = 0;
			//	this._selectedframe.ForEachAncestor(a => { sumX += a.X; sumY += a.Y; });
			//	this._selectedframe.X = (int)this.RenderTransform.Value.OffsetX - (sumX - this._selectedframe.X);
			//	this._selectedframe.Y = (int)this.RenderTransform.Value.OffsetY - (sumY - this._selectedframe.Y);
			//}
		}

		private void Drag_MouseMove( object sender, MouseEventArgs e )
		{

		}

		private void Drag_MouseDown( object sender, MouseButtonEventArgs e )
		{
			Border drag = sender as Border;

			this.CurrentDrag = drag;
			this._dragMousePressed = true;
			_previousMouseLocation = e.MouseDevice.GetPosition(_window);
			_previousDragTransform = drag.RenderTransform;
		}
	}
}
