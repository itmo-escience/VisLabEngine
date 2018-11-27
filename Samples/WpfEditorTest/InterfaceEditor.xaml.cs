﻿using Fusion.Engine.Common;
using FusionUI;
using GISTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Fusion.Engine.Frames;
using Fusion.Engine.Input;
using WpfEditorTest.ChildPanels;
using Frame = Fusion.Engine.Frames.Frame;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace WpfEditorTest
{
	/// <summary>
	/// Interaction logic for InterfaceEditor.xaml
	/// </summary>
	public partial class InterfaceEditor : Window
	{
		private readonly FrameDetails _details;
		private readonly FramePalette _palette;
		private readonly FrameTreeView _treeView;
		private readonly FrameSelectionPanel _frameSelectionPanel;
	    private readonly List<IDraggablePanel> _panels = new List<IDraggablePanel>();

	    public string TemplatesPath = Path.GetFullPath(Path.Combine(Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName, "..\\..\\..\\FramesXML"));

	    private readonly Game _engine;

		public FusionUI.UI.ScalableFrame DragFieldFrame;
		public FusionUI.UI.ScalableFrame SceneFrame;
		public FusionUI.UI.MainFrame RootFrame;

		public InterfaceEditor()
        {
            InitializeComponent();

            _engine = new Game("TestGame");
            _engine.Mouse = new DummyMouse(_engine);
            _engine.Keyboard = new DummyKeyboard(_engine);
            _engine.Touch = new DummyTouch(_engine);

            _engine.GameServer = new CustomGameServer(_engine);
            _engine.GameClient = new CustomGameClient(_engine);
            _engine.GameInterface = new CustomGameInterface(_engine);
            _engine.LoadConfiguration("Config.ini");

            _engine.RenderSystem.StereoMode = Fusion.Engine.Graphics.StereoMode.WpfEditor;
            _engine.RenderSystem.Width = 1920;
            _engine.RenderSystem.Height = 1080;
            _engine.RenderSystem.VSyncInterval = 1;

            Directory.SetCurrentDirectory(@"..\..\..\..\GISTest\bin\x64\Debug");
            _engine.InitExternal();

            DxElem.Renderer = _engine;

            _frameSelectionPanel = new FrameSelectionPanel(this);
            LocalGrid.Children.Add(_frameSelectionPanel);
            _panels.Add(_frameSelectionPanel);

            _details = new FrameDetails(this);
            LocalGrid.Children.Add(_details);
            _panels.Add(_details);

            _palette = new FramePalette(this);
            LocalGrid.Children.Add(_palette);
            _panels.Add(_palette);

            _treeView = new FrameTreeView(this, _details.FrameDetailsControls);
            LocalGrid.Children.Add(_treeView);
            _panels.Add(_treeView);

            _treeView.SelectedFrameChanged += SelectFrame;

            var templates = Directory.GetFiles(TemplatesPath, "*.xml").ToList();
            _palette.AvailableFrames.ItemsSource = templates.Select(t => t.Split('\\').Last().Split('.').First());

            RootFrame = ApplicationInterface.Instance.rootFrame;
            SceneFrame = (FusionUI.UI.ScalableFrame)RootFrame.Children.FirstOrDefault();
            DragFieldFrame = new FusionUI.UI.ScalableFrame(0, 0, RootFrame.UnitWidth, RootFrame.UnitHeight, "DragFieldFrame", Fusion.Core.Mathematics.Color.Zero)
            {
                Anchor = FrameAnchor.All
            };
            RootFrame.Add(DragFieldFrame);

            var b = new Binding("Children")
            {
                Source = SceneFrame,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.OneWay
            };
            _treeView.ElementHierarcyView.SetBinding(TreeView.ItemsSourceProperty, b);
        }

        private void SelectFrame(object sender, EventArgs args)
	    {
            _frameSelectionPanel.SelectedFrame = _treeView.SelectedFrame;
            MoveFrameToDragField(_treeView.SelectedFrame);
        }

        protected override void OnSourceInitialized( EventArgs e )
		{
			base.OnSourceInitialized(e);
			DxElem.HandleInput(this);
		}

		private void LocalGrid_MouseMove( object sender, MouseEventArgs e )
		{
			foreach (var panel in _panels)
			{
				if (panel.GetType() == typeof(FrameSelectionPanel) && _frameSelectionPanel.DragMousePressed)
				{
					var drag = _frameSelectionPanel.CurrentDrag;

					Point currentLocation = e.MouseDevice.GetPosition(this);
					var deltaX = currentLocation.X - _frameSelectionPanel.PreviousMouseLocation.X;
					var deltaY = currentLocation.Y - _frameSelectionPanel.PreviousMouseLocation.Y;
					TranslateTransform delta = null;// = new TranslateTransform (deltaX, deltaY);



					switch (_frameSelectionPanel.Drags.IndexOf(drag))
					{
						case 0:
							{
								delta = new TranslateTransform(deltaX, deltaY);
								_frameSelectionPanel.HeightBuffer -= deltaY;
								_frameSelectionPanel.WidthBuffer -= deltaX;
								break;
							}
						case 1:
							{
								delta = new TranslateTransform(0, deltaY);
								_frameSelectionPanel.HeightBuffer -= deltaY;
								break;
							}
						case 2:
							{
								delta = new TranslateTransform(0, deltaY);
								_frameSelectionPanel.HeightBuffer -= deltaY;
								_frameSelectionPanel.WidthBuffer += deltaX;
								break;
							}
						case 3:
							{
								delta = new TranslateTransform(deltaX, 0);
								_frameSelectionPanel.WidthBuffer -= deltaX;
								break;
							}
						case 4:
							{
								delta = new TranslateTransform(0, 0);
								_frameSelectionPanel.WidthBuffer += deltaX;
								break;
							}
						case 5:
							{
								delta = new TranslateTransform(deltaX, 0);
								_frameSelectionPanel.HeightBuffer += deltaY;
								_frameSelectionPanel.WidthBuffer -= deltaX;
								break;
							}
						case 6:
							{
								delta = new TranslateTransform(0, 0);
								_frameSelectionPanel.HeightBuffer += deltaY;
								break;
							}
						case 7:
							{
								delta = new TranslateTransform(0, 0);
								_frameSelectionPanel.HeightBuffer += deltaY;
								_frameSelectionPanel.WidthBuffer += deltaX;
								break;
							}
					}

					var group = new TransformGroup();
					group.Children.Add(_frameSelectionPanel.PreviousTransform);
					group.Children.Add(delta);
					_frameSelectionPanel.RenderTransform = group;
					_frameSelectionPanel.PreviousTransform = _frameSelectionPanel.RenderTransform;
					_frameSelectionPanel.PreviousMouseLocation = currentLocation;

				}
				else if (panel.MousePressed)
				{
					Point currentLocation = e.MouseDevice.GetPosition(this);

					var delta = new TranslateTransform
					(currentLocation.X - panel.PreviousMouseLocation.X, currentLocation.Y - panel.PreviousMouseLocation.Y);

					var group = new TransformGroup();
					group.Children.Add(panel.PreviousTransform);
					group.Children.Add(delta);

					((UserControl)panel).RenderTransform = group;
					panel.PreviousTransform = ((UserControl)panel).RenderTransform;
					panel.PreviousMouseLocation = currentLocation;
				}
			}
		}

		private void LocalGrid_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
            if(_frameSelectionPanel.SelectedFrame != null)
		    {
                TryLandSelectedFrameOnScene(e.GetPosition(this));
		        _frameSelectionPanel.SelectedFrame = null;
		    }

		    _frameSelectionPanel.DragMousePressed = false;
		    _frameSelectionPanel.CurrentDrag = null;

            foreach (var panel in _panels)
			{
				panel.MousePressed = false;
			}

			if (_palette._selectedFrameTemplate != null)
			{
				Fusion.Core.Utils.FrameSerializer.Read(Path.Combine(TemplatesPath, _palette._selectedFrameTemplate)+".xml", out var createdFrame);

				if (createdFrame != null)
				{
					createdFrame.X = (int)e.MouseDevice.GetPosition(this).X - createdFrame.Width / 2;
					createdFrame.Y = (int)e.MouseDevice.GetPosition(this).Y - createdFrame.Height / 2;
					SceneFrame.Add(createdFrame);
				}
				_palette._selectedFrameTemplate = null;

			    Cursor = Cursors.Arrow;
			}
		}

		private void LocalGrid_MouseDown( object sender, MouseButtonEventArgs e )
		{
			var hovered = GetHoveredFrameOnScene(e.GetPosition(DxElem), true);

            if(hovered != null)
                _treeView.SetSelectedFrame(hovered);
        }

	    private void Window_KeyDown(object sender, KeyEventArgs e)
	    {
	        if (e.Key == Key.Delete && _treeView.ElementHierarcyView.SelectedItem != null)
	        {
	            var selected = (Frame)_treeView.ElementHierarcyView.SelectedItem;
	            _details.FrameDetailsControls.ItemsSource = null;

	            selected.Parent?.Remove(selected);
	        }
	    }

	    public Frame GetHoveredFrameOnScene(Point mousePos, bool ignoreScene)
	    {
	        var hoveredFrames = FrameProcessor.GetFramesAt(SceneFrame, (int) mousePos.X, (int) mousePos.Y);

	        if (ignoreScene)
	        {
	            if (hoveredFrames.Count > 1) // if something is there and it's not SceneFrame
	            {
	                return hoveredFrames.Pop();
	            }

	            return null;
            }
	        else
	        {
	            if (hoveredFrames.Any()) // if something is there
	            {
	                return hoveredFrames.Pop();
	            }

	            return null;
            }
	    }

	    public void MoveFrameToDragField(Frame frame)
		{
			frame.Parent?.Remove(frame);

			DragFieldFrame.Add(frame);
			_frameSelectionPanel.UpdateSelectedFramePosition();
		}

	    public void TryLandSelectedFrameOnScene(Point pos)
	    {
	        _frameSelectionPanel.SelectedFrame.Parent.Remove(_frameSelectionPanel.SelectedFrame);

	        var hoveredFrame = GetHoveredFrameOnScene(pos, false) ?? SceneFrame;
	        hoveredFrame.Add(_frameSelectionPanel.SelectedFrame);

            _frameSelectionPanel.UpdateSelectedFramePosition();
	    }
	}

	public class Propsy : INotifyPropertyChanged
	{
		public Propsy( PropertyInfo prop, Frame obj )
		{
			Obj = obj;
			PropName = prop.Name;
			PropInfo = prop;
			PropType = prop.PropertyType;
			if (PropType.IsEnum)
			{
				EnumValues = Enum.GetValues(PropType).Cast<object>().ToList();
			}

			if (prop.PropertyType != typeof(string))
			{
			    _prop = Activator.CreateInstance(prop.PropertyType);
			    _prop = prop.GetValue(obj);
			}
			else
			{
				if (prop.GetValue(obj) != null)
				{
				    _prop = Activator.CreateInstance(prop.PropertyType, (prop.GetValue(obj) as string).ToCharArray());
				}
				else
				{
                    _prop = string.Empty;
				}
			}

			Obj.PropertyChanged += (s, e) => {
				if (e.PropertyName == PropName)
				{
					var val = PropInfo.GetValue(Obj);
					if (!Prop.Equals(val))
					{
                        _prop = PropInfo.GetValue(Obj);
					}
				}
			};
		}

		private object _prop;
		public object Prop
		{
			get => _prop;
		    set
			{
			    _prop = value;
                var convertedValue = Convert.ChangeType(value, PropInfo.PropertyType);
                PropInfo.SetValue(Obj, convertedValue);
                OnPropertyChanged();
			}
		}


	    public Fusion.Engine.Frames.Frame Obj { get; set; }

	    public PropertyInfo PropInfo { get; set; }
	    public Type PropType { get; set; }
		public string PropName { get; set; }

	    public IList<object> EnumValues { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}
}
