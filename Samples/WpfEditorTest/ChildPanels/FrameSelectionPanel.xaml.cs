using Fusion.Engine.Frames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Fusion.Engine.Frames2;
using WpfEditorTest.UndoRedo;
using CommandManager = WpfEditorTest.UndoRedo.CommandManager;
using Fusion.Engine.Frames2.Managing;
using Matrix3x2 = Fusion.Core.Mathematics.Matrix3x2;

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for SelectedFramePanel.xaml
	/// </summary>
	public partial class FrameSelectionPanel : UserControl
	{
		private UIComponent _selectedFrame;
		private double _widthBuffer;
		private double _heightBuffer;

		private double _oldX;
		private double _oldY;
		private bool _locked = false;
		private bool _isInDragField;

		public UIComponent SelectedFrame
		{
			get => _selectedFrame;
			set
			{
				if (_selectedFrame != null)
					_selectedFrame.PropertyChanged -= FrameDimensionsChange;

				_selectedFrame = value;
				if (_selectedFrame == null) return;

				//Width = 
				WidthBuffer = _selectedFrame.Width * Math.Sign(_selectedFrame.Transform.M11);
				//Height =
				HeightBuffer = _selectedFrame.Height * Math.Sign(_selectedFrame.Transform.M22);

                var transform = new MatrixTransform(_selectedFrame.GlobalTransform.M11, _selectedFrame.GlobalTransform.M12,
                                                    _selectedFrame.GlobalTransform.M21, _selectedFrame.GlobalTransform.M22,
                                                    _selectedFrame.GlobalTransform.M31, _selectedFrame.GlobalTransform.M32);
                RenderTransform = transform;

                _oldX = RenderTransform.Value.OffsetX;
				_oldY = RenderTransform.Value.OffsetY;

				//UpdateVisualAnchors(_selectedFrame.Anchor);

				_selectedFrame.PropertyChanged += FrameDimensionsChange;
			}
		}

		private void FrameDimensionsChange( object sender, PropertyChangedEventArgs args )
		{
		    var selected = _selectedFrame;
            if (_locked || selected == null) return;

		    Application.Current.Dispatcher.InvokeAsync(() =>
		    {
		        switch (args.PropertyName)
		        {
		            case "Width":
		            {
		                WidthBuffer = selected.Width * Math.Sign(selected.GlobalTransform.M11);
		                break;
		            }
		            case "Height":
		            {
		                HeightBuffer = selected.Height * Math.Sign(selected.GlobalTransform.M22);
		                break;
		            }
		            case "Parent":
		            {
                        var transform = new MatrixTransform(_selectedFrame.GlobalTransform.M11, _selectedFrame.GlobalTransform.M12,
                                                    _selectedFrame.GlobalTransform.M21, _selectedFrame.GlobalTransform.M22,
                                                    _selectedFrame.GlobalTransform.M31, _selectedFrame.GlobalTransform.M32);
                        RenderTransform = transform;

                        _oldX = RenderTransform.Value.OffsetX;
		                _oldY = RenderTransform.Value.OffsetY;
		                break;
		            }
                    case "Angle":
                    {
                        var transform = new MatrixTransform(_selectedFrame.GlobalTransform.M11, _selectedFrame.GlobalTransform.M12,
                                                    _selectedFrame.GlobalTransform.M21, _selectedFrame.GlobalTransform.M22,
                                                    _selectedFrame.GlobalTransform.M31, _selectedFrame.GlobalTransform.M32);
                        RenderTransform = transform;
                        _oldX = RenderTransform.Value.OffsetX;
                        _oldY = RenderTransform.Value.OffsetY;
                        break;
                    }
					case "Transform":
						{
							var transform = new MatrixTransform(_selectedFrame.GlobalTransform.M11, _selectedFrame.GlobalTransform.M12,
														_selectedFrame.GlobalTransform.M21, _selectedFrame.GlobalTransform.M22,
														_selectedFrame.GlobalTransform.M31, _selectedFrame.GlobalTransform.M32);
							RenderTransform = transform;
							_oldX = RenderTransform.Value.OffsetX;
							_oldY = RenderTransform.Value.OffsetY;
							break;
						}
					case "X":
                    case "Y":
                    {
                        var transform = new TransformGroup();
                        var transformDelta = new TranslateTransform(selected.GlobalTransform.M31 - RenderTransform.Value.OffsetX, selected.GlobalTransform.M32 - RenderTransform.Value.OffsetY);
                        transform.Children.Add(RenderTransform);
                        transform.Children.Add(transformDelta);
                        RenderTransform = transform;
                        _oldX = RenderTransform.Value.OffsetX;
                        _oldY = RenderTransform.Value.OffsetY;
                        break;
                    }
		            case "Anchor":
		            {
		                //UpdateVisualAnchors(selected.Anchor);
		                break;
		            }
		        }
		    });
		}

		private void UpdateVisualAnchors( FrameAnchor anchor )
		{
			void UpdateAnchor( bool isActive, /*Border drag,*/ Border line )
			{
				//drag.Background = isActive ? Brushes.Black : Brushes.White;
				//drag.BorderBrush = isActive ? Brushes.White : Brushes.Black;
				line.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
			}

			UpdateAnchor((FrameAnchor.Top & anchor) != 0, /*TopDrag,*/ TopAnchorLine);
			UpdateAnchor((FrameAnchor.Bottom & anchor) != 0, /*BottomDrag,*/ BottomAnchorLine);
			UpdateAnchor((FrameAnchor.Left & anchor) != 0, /*LeftDrag,*/ LeftAnchorLine);
			UpdateAnchor((FrameAnchor.Right & anchor) != 0, /*RightDrag,*/ RightAnchorLine);
		}

		public bool MousePressed { get; set; }
		public bool IsInDragField {
			get => _isInDragField;
			set {
				_isInDragField = value;
				this.Cursor = value ? Cursors.Cross : Cursors.Arrow;
				if (value)
				{
					VisualAnchors.ForEach(a=>a.Visibility = Visibility.Collapsed);
				}
				else
				{
					//UpdateVisualAnchors(_selectedFrame.Anchor);
				}
			}
		}

		public List<Border> VisualAnchors { get; private set; }

		public double WidthBuffer
		{
			get => _widthBuffer;
			set
			{
				_widthBuffer = value;
				this.Width = Math.Abs(_widthBuffer);//Math.Max(0, _widthBuffer);
			}
		}
		public double HeightBuffer
		{
			get => _heightBuffer;
			set
			{
				_heightBuffer = value;
				this.Height = Math.Abs(_heightBuffer);// Math.Max(0, _heightBuffer);
			}
		}

		public Matrix3x2 InitialTransform { get; internal set; }
		public UIContainer InitFrameParent { get; internal set; }
		public Point InitPanelPosition { get; internal set; }
		public Point InitFramePosition { get; internal set; }
		public Size InitialFrameSize { get; internal set; }
		public Point InitFrameScale { get; internal set; }

		public FrameSelectionPanel()
		{
			InitializeComponent();

			Height = ApplicationConfig.OptionsWindowSize; Width = ApplicationConfig.OptionsWindowSize;

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
				_selectedFrame.Width = (float)Math.Max(0, Math.Abs(WidthBuffer));
				_selectedFrame.Height = (float)Math.Max(0, Math.Abs(HeightBuffer));

				_selectedFrame.Transform = new Matrix3x2(
					Math.Sign(WidthBuffer)<0?-1:1, _selectedFrame.Transform.M12,
					_selectedFrame.Transform.M21, Math.Sign(HeightBuffer) < 0 ? -1 : 1,
					_selectedFrame.Transform.M31, _selectedFrame.Transform.M32
					);
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

			var parent = _selectedFrame.Parent;
			var parentMatrixInvert = Matrix3x2.Invert(parent.GlobalTransform);
			var vectorHelper = Matrix3x2.TransformPoint(parentMatrixInvert, new Fusion.Core.Mathematics.Vector2((int)RenderTransform.Value.OffsetX + 0.5f, (int)RenderTransform.Value.OffsetY + 0.5f));

			_selectedFrame.X = (int)vectorHelper.X;
			_selectedFrame.Y = (int)vectorHelper.Y;

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
				TopAnchorLine.Margin = new Thickness(0, -measure, 0, 9);
			}
			if (BottomAnchorLine.Visibility == Visibility.Visible)
			{
				measure = _selectedFrame.Parent.Height - (_selectedFrame.Y + _selectedFrame.Height);
				BottomAnchorLine.Height = Math.Abs(measure);
				BottomAnchorLine.Margin = new Thickness(0, 9, 0, -measure);
			}
			if (LeftAnchorLine.Visibility == Visibility.Visible)
			{
				measure = _selectedFrame.X;
				LeftAnchorLine.Width = Math.Abs(measure);
				LeftAnchorLine.Margin = new Thickness(-measure, 0, 9, 0);
			}
			if (RightAnchorLine.Visibility == Visibility.Visible)
			{
				measure = _selectedFrame.Parent.Width - (_selectedFrame.X + _selectedFrame.Width);
				RightAnchorLine.Width = Math.Abs(measure);
				RightAnchorLine.Margin = new Thickness(9, 0, -measure, 0);
			}
		}

		private bool PositionChanged()
		{
			return (int)_oldX != (int)RenderTransform.Value.OffsetX || (int)_oldY != (int)RenderTransform.Value.OffsetY;
		}

		private void UserControl_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			e.Handled = false;
			MousePressed = true;
		}
	}
}
