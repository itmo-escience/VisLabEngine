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
										var frameDelta = new TranslateTransform(this._selectedframe.X - this._oldX, this._selectedframe.Y - this._oldY);

										var group = new TransformGroup();
										group.Children.Add(this._previousTransform);
										group.Children.Add(delta);

										this.RenderTransform = group;
										this._previousTransform = this.RenderTransform;
										break;
									}
							} 
						}
					};
				}


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

		public double WidthBuffer {
			get => _widthBuffer;
			set { _widthBuffer = value;
				this.Width = Math.Max(0, _widthBuffer);
			}
		}
		public double HeightBuffer {
			get => _heightBuffer;
			set { _heightBuffer = value;
				this.Height = Math.Max(0, _heightBuffer);
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

			this.SizeChanged += ( s, e ) =>
			{
				this.locked = true;
				if (this._selectedframe != null)
				{
					this._selectedframe.Width = (int)(this.Width + Math.Min(0, WidthBuffer));
					this._selectedframe.Height = (int)(this.Height + Math.Min(0, HeightBuffer)); 
				}
				this.locked = false;
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
