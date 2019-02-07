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

				Width = WidthBuffer = _selectedFrame.Width;
				Height = HeightBuffer = _selectedFrame.Height;

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
		            case "UnitWidth":
		            {
		                WidthBuffer = selected/*.BoundingBox*/.Width;
		                break;
		            }
		            case "Height":
		            case "UnitHeight":
		            {
		                HeightBuffer = selected/*.BoundingBox*/.Height;
		                break;
		            }
		            case "GlobalRectangle":
		            {
		                var frameDelta = new TranslateTransform();
		                RenderTransform = frameDelta;
		                frameDelta.X = selected.BoundingBox.X;
		                frameDelta.Y = selected.BoundingBox.Y;

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
				this.Width = Math.Max(0, _widthBuffer);
			}
		}
		public double HeightBuffer
		{
			get => _heightBuffer;
			set
			{
				_heightBuffer = value;
				this.Height = Math.Max(0, _heightBuffer);
			}
		}

		public Fusion.Core.Mathematics.RectangleF InitialGlobalRectangle { get; internal set; }
		public UIContainer InitFrameParent { get; internal set; }
		public Point InitPanelPosition { get; internal set; }

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
				_selectedFrame.Width = (int)(Width + Math.Min(0, WidthBuffer)+0.5d);
				_selectedFrame.Height = (int)(Height + Math.Min(0, HeightBuffer)+0.5d);
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
			//_selectedFrame.ForEachAncestor(a => { sumX += a.X; sumY += a.Y; });

			foreach (var item in UIHelper.Ancestors(_selectedFrame))
			{
				sumX += (int)(item.X+0.5f); sumY += (int)(item.Y+0.5f);
			}

			_selectedFrame.X = (int)RenderTransform.Value.OffsetX - (sumX/* - _selectedFrame.X*/);
			_selectedFrame.Y = (int)RenderTransform.Value.OffsetY - (sumY/* - _selectedFrame.Y*/);

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
