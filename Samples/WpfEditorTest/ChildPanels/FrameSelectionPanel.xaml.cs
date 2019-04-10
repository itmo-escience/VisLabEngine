using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Fusion.Engine.Frames2;
using Fusion.Engine.Frames;
using Matrix3x2 = Fusion.Core.Mathematics.Matrix3x2;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Frames2.Containers;
using WpfEditorTest.FrameSelection;

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
		private UIManager _uiManager;

		public UIComponent SelectedFrame
		{
			get => _selectedFrame;
			set
			{
				if (_selectedFrame != null)
				{
					SelectionManager.Instance.UIComponentUpdated -= ComponentDimensionsChange;
					_selectedFrame.Placement.PropertyChanged -= SlotDimensionsChange;
				}

				_selectedFrame = value;
				if (_selectedFrame == null) return;

				//Width =
				WidthBuffer = _selectedFrame.Placement.Width * Math.Sign(_selectedFrame.Placement.Transform().M11);
				//Height =
				HeightBuffer = _selectedFrame.Placement.Height * Math.Sign(_selectedFrame.Placement.Transform().M22);

				var globalTransform = _uiManager.GlobalTransform(_selectedFrame.Placement);

				var transform = new MatrixTransform(globalTransform.M11, globalTransform.M12,
                                                    globalTransform.M21, globalTransform.M22,
                                                    globalTransform.M31, globalTransform.M32);
                RenderTransform = transform;

                _oldX = RenderTransform.Value.OffsetX;
				_oldY = RenderTransform.Value.OffsetY;

				//UpdateVisualAnchors(_selectedFrame.Anchor);

				SelectionManager.Instance.UIComponentUpdated += ComponentDimensionsChange;
				_selectedFrame.Placement.PropertyChanged += SlotDimensionsChange;
			}
		}

		private void ComponentDimensionsChange( object sender, UIComponent component )
		{
			var selected = _selectedFrame;
			if (_locked || selected == null) return;

			Application.Current.Dispatcher.InvokeAsync(() =>
			{
				var globalTransform = _uiManager.GlobalTransform(selected.Placement);

				var transform = new MatrixTransform(globalTransform.M11, globalTransform.M12,
													globalTransform.M21, globalTransform.M22,
													globalTransform.M31, globalTransform.M32);
				RenderTransform = transform;

				_oldX = RenderTransform.Value.OffsetX;
				_oldY = RenderTransform.Value.OffsetY;

			});
		}

		private void SlotDimensionsChange( object sender, PropertyChangedEventArgs args )
		{
			var selected = _selectedFrame;
			if (_locked || selected == null) return;

			Application.Current.Dispatcher.InvokeAsync(() =>
			{
				var globalTransform = _uiManager.GlobalTransform(selected.Placement);

				WidthBuffer = selected.Placement.Width * Math.Sign(globalTransform.M11);
				HeightBuffer = selected.Placement.Height * Math.Sign(globalTransform.M22);

				var transform = new MatrixTransform(globalTransform.M11, globalTransform.M12,
													globalTransform.M21, globalTransform.M22,
													globalTransform.M31, globalTransform.M32);
				RenderTransform = transform;
				_oldX = RenderTransform.Value.OffsetX;
				_oldY = RenderTransform.Value.OffsetY;

				////var transform = new TransformGroup();
				////var transformDelta = new TranslateTransform(globalTransform.M31 - RenderTransform.Value.OffsetX, globalTransform.M32 - RenderTransform.Value.OffsetY);
				////transform.Children.Add(RenderTransform);
				////transform.Children.Add(transformDelta);
				//RenderTransform = transform;
				//_oldX = RenderTransform.Value.OffsetX;
				//_oldY = RenderTransform.Value.OffsetY;

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
		public IUIModifiableContainer<ISlot> InitFrameParent { get; internal set; }
		public Point InitPanelPosition { get; internal set; }
		public Point InitFramePosition { get; internal set; }
		public Size InitialFrameSize { get; internal set; }
		public Point InitFrameScale { get; internal set; }

		public FrameSelectionPanel( UIManager uiManager )
		{
			InitializeComponent();

			_uiManager = uiManager;

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
				_selectedFrame.DesiredWidth = (float)Math.Max(0, Math.Abs(WidthBuffer));
				_selectedFrame.DesiredHeight = (float)Math.Max(0, Math.Abs(HeightBuffer));

				var transform = _selectedFrame.Placement.Transform();

				if (_selectedFrame.Placement is FreePlacementSlot fpSlot)
				{
					fpSlot.SetTransform(new Matrix3x2(
					Math.Sign(WidthBuffer) < 0 ? -1 : 1, transform.M12,
					transform.M21, Math.Sign(HeightBuffer) < 0 ? -1 : 1,
					transform.M31, transform.M32
					)); 
				}
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

			var parent = _selectedFrame.Parent();
			var parentMatrixInvert = Matrix3x2.Invert(_uiManager.GlobalTransform(parent.Placement));
			var vectorHelper = Matrix3x2.TransformPoint(parentMatrixInvert, new Fusion.Core.Mathematics.Vector2((float)RenderTransform.Value.OffsetX, (float)RenderTransform.Value.OffsetY));

			if (_selectedFrame.Placement is FreePlacementSlot slot)
			{
				slot.X = vectorHelper.X;
				slot.Y = vectorHelper.Y;

				_oldX = RenderTransform.Value.OffsetX;
				_oldY = RenderTransform.Value.OffsetY; 
			}

			UpdateAnchorLines();
			_locked = false;
		}

		private void UpdateAnchorLines()
		{
			if (_selectedFrame == null) return;

			double measure;
			if (TopAnchorLine.Visibility == Visibility.Visible)
			{
				measure = _selectedFrame.Placement.Y;
				TopAnchorLine.Height = Math.Abs(measure);
				TopAnchorLine.Margin = new Thickness(0, -measure, 0, 9);
			}
			if (BottomAnchorLine.Visibility == Visibility.Visible)
			{
				measure = _selectedFrame.Parent().Placement.Height - (_selectedFrame.Placement.Y + _selectedFrame.Placement.Height);
				BottomAnchorLine.Height = Math.Abs(measure);
				BottomAnchorLine.Margin = new Thickness(0, 9, 0, -measure);
			}
			if (LeftAnchorLine.Visibility == Visibility.Visible)
			{
				measure = _selectedFrame.Placement.X;
				LeftAnchorLine.Width = Math.Abs(measure);
				LeftAnchorLine.Margin = new Thickness(-measure, 0, 9, 0);
			}
			if (RightAnchorLine.Visibility == Visibility.Visible)
			{
				measure = _selectedFrame.Parent().Placement.Width - (_selectedFrame.Placement.X + _selectedFrame.Placement.Width);
				RightAnchorLine.Width = Math.Abs(measure);
				RightAnchorLine.Margin = new Thickness(9, 0, -measure, 0);
			}
		}

		private bool PositionChanged()
		{
			return Math.Abs(_oldX - RenderTransform.Value.OffsetX) > double.Epsilon || Math.Abs(_oldY - RenderTransform.Value.OffsetY) > double.Epsilon;
		}

		private void UserControl_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			e.Handled = false;
			MousePressed = true;
		}
	}
}
