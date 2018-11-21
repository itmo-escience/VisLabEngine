using Fusion.Engine.Common;
using FusionUI;
using GISTest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Fusion.Engine.Input;
using WpfEditorTest.ChildPanels;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace WpfEditorTest
{
	/// <summary>
	/// Interaction logic for InterfaceEditor.xaml
	/// </summary>
	public partial class InterfaceEditor : Window
	{
		FrameDetails details;
		FramePalette palette;
		FrameTreeView treeView;
		SelectedFramePanel frameHoverPanel;
		List<IDraggablePanel> panels = new List<IDraggablePanel>();

		Fusion.Engine.Frames.Frame selectedframe;
		Fusion.Engine.Frames.Frame lastSelectedframe;
		int lastSelectedframeBorder;
		Fusion.Core.Mathematics.Color lastSelectedframeBorderColor;

		string templatesPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName, "..\\..\\..\\FramesXML"));

		Game engine;

		public InterfaceEditor()
		{
			InitializeComponent();

			engine = new Game("TestGame");
            engine.Mouse = new DummyMouse(engine);
            engine.Keyboard = new DummyKeyboard(engine);
		    engine.Touch = new DummyTouch(engine);

			engine.GameServer = new CustomGameServer(engine);
			engine.GameClient = new CustomGameClient(engine);
			engine.GameInterface = new CustomGameInterface(engine);
			engine.LoadConfiguration("Config.ini");


			engine.RenderSystem.StereoMode = Fusion.Engine.Graphics.StereoMode.WpfEditor;
			engine.RenderSystem.Width = 1920;
			engine.RenderSystem.Height = 1080;
			engine.RenderSystem.VSyncInterval = 1;

			//var t = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(@"..\..\..\..\GISTest\bin\x64\Debug");
			engine.InitExternal();

			DxElem.Renderer = engine;

			frameHoverPanel = new SelectedFramePanel(this);
			this.LocalGrid.Children.Add(frameHoverPanel);
			this.panels.Add(frameHoverPanel);

			details = new FrameDetails(this);
			this.LocalGrid.Children.Add(details);
			this.panels.Add(details);

			palette = new FramePalette(this);
			this.LocalGrid.Children.Add(palette);
			this.panels.Add(palette);

			treeView = new FrameTreeView(this, details.FrameDetailsControls);
			this.LocalGrid.Children.Add(treeView);
			this.panels.Add(treeView);


			treeView.selectedFrameChanged += ( s, e ) =>
			{
				frameHoverPanel.selectedframe = treeView.Selectedframe;
			};

			var templates = Directory.GetFiles(templatesPath, "*.xml").ToList();
			palette.AvailableFrames.ItemsSource = templates.Select(t=>t.Split('\\').Last().Split('.').First());
				//StaticData.availableFrameElements;

			Binding b = new Binding("Children")
			{
				Source = (engine.GameInterface as ApplicationInterface).rootFrame,
			};
			treeView.ElementHierarcyView.SetBinding(TreeView.ItemsSourceProperty,b);
			//treeView.ElementHierarcyView.ItemsSource = (engine.GameInterface as ApplicationInterface).rootFrame.Children;
			(engine.GameInterface as ApplicationInterface).rootFrame.PropertyChanged += ( s, e ) => { };

			(engine.GameInterface as ApplicationInterface).rootFrame.ForEachChildren(
				c =>
				{
					DisableChildrenFree(c);
				}
				);

			//StaticData.availableFrameElements[0].Name;

			//this.Dispatcher.BeginInvoke(new Action(() => SetMouseLook()), DispatcherPriority.ApplicationIdle);

		}

		private void DisableChildrenFree(Fusion.Engine.Frames.Frame frame)
		{
			if (frame.GetType().GetField("DisableFree") != null)
			{
				((FusionUI.UI.FreeFrame)frame).DisableFree = true;
			}
			frame.ForEachChildren(
				c =>
				{
					DisableChildrenFree(c);
				}
				);
		}

		protected override void OnSourceInitialized( EventArgs e )
		{
			base.OnSourceInitialized(e);
			DxElem.HandleInput(this);
		}

		private void ElementHierarcyView_SelectedItemChanged( object sender, RoutedPropertyChangedEventArgs<object> e )
		{
		}

		private void TextBlock_MouseDown( object sender, MouseButtonEventArgs e )
		{

			if (lastSelectedframe!=null)
			{
				lastSelectedframe.Border = lastSelectedframeBorder;
				lastSelectedframe.BorderColor = lastSelectedframeBorderColor;
			}

			selectedframe = (Fusion.Engine.Frames.Frame)((sender as TextBlock).Tag);

			lastSelectedframe = selectedframe;
			lastSelectedframeBorder = selectedframe.BorderTop;
			lastSelectedframeBorderColor = selectedframe.BorderColor;



			selectedframe.Border = 2;
			selectedframe.BorderColor = Fusion.Core.Mathematics.Color.Yellow;

			var publicProperties = selectedframe.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			FramePropertyHolder holder = new FramePropertyHolder();

			List<Propsy> propsies = new List<Propsy>();
			foreach (var property in publicProperties)
			{
				if (property.GetMethod != null && property.SetMethod!=null && !property.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute"))
				{
					holder.Properties.Add(new PropertyDependant(property, selectedframe));

					propsies.Add(new Propsy(property, selectedframe));
					//holder.Properties.Last().Prop.Name
				}
			}
			//List<Fusion.Engine.Frames.Frame> frames = new List<Fusion.Engine.Frames.Frame>();
			//frames.Add(selectedframe);

			//List<IntNumbers> numbers = new List<IntNumbers>();
			//numbers.Add(new IntNumbers() { number = 5 });
			//numbers.Add(new IntNumbers() { number = 15 });
			//numbers.Add(new IntNumbers() { number = 25 });
			//numbers.Add(new IntNumbers() { number = 35 });
			//numbers.Add(new IntNumbers() { number = 45 });
			//numbers.Add(new IntNumbers() { number = 55 });

			/*this.FrameDetails.*/
			details.FrameDetailsControls.ItemsSource = propsies;//holder.Properties;
		}

		private void LocalGrid_MouseMove( object sender, MouseEventArgs e )
		{
			foreach (var panel in panels)
			{

				if (panel.GetType() == typeof(SelectedFramePanel) && frameHoverPanel._dragMousePressed)
				{
					var drag = frameHoverPanel.CurrentDrag;

					Point currentLocation = e.MouseDevice.GetPosition(this);
					var deltaX = currentLocation.X - frameHoverPanel._previousMouseLocation.X;
					var deltaY = currentLocation.Y - frameHoverPanel._previousMouseLocation.Y;
					TranslateTransform delta = null;// = new TranslateTransform (deltaX, deltaY);



					switch (frameHoverPanel.drags.IndexOf(drag))
					{
						case 0:
							{
								delta = new TranslateTransform(deltaX, deltaY);
								frameHoverPanel.HeightBuffer -= deltaY;
								frameHoverPanel.WidthBuffer -= deltaX;
								break;
							}
						case 1:
							{
								delta = new TranslateTransform(0, deltaY);
								frameHoverPanel.HeightBuffer -= deltaY;
								break;
							}
						case 2:
							{
								delta = new TranslateTransform(0, deltaY);
								frameHoverPanel.HeightBuffer -= deltaY;
								frameHoverPanel.WidthBuffer += deltaX;
								break;
							}
						case 3:
							{
								delta = new TranslateTransform(deltaX, 0);
								frameHoverPanel.WidthBuffer -= deltaX;
								break;
							}
						case 4:
							{
								delta = new TranslateTransform(0, 0);
								frameHoverPanel.WidthBuffer += deltaX;
								break;
							}
						case 5:
							{
								delta = new TranslateTransform(deltaX, 0);
								frameHoverPanel.HeightBuffer += deltaY;
								frameHoverPanel.WidthBuffer -= deltaX;
								break;
							}
						case 6:
							{
								delta = new TranslateTransform(0, 0);
								frameHoverPanel.HeightBuffer += deltaY;
								break;
							}
						case 7:
							{
								delta = new TranslateTransform(0, 0);
								frameHoverPanel.HeightBuffer += deltaY;
								frameHoverPanel.WidthBuffer += deltaX;
								break;
							}
					}

					var group = new TransformGroup();
					group.Children.Add(frameHoverPanel._previousTransform);
					group.Children.Add(delta);
					frameHoverPanel.RenderTransform = group;
					frameHoverPanel._previousTransform = frameHoverPanel.RenderTransform;
					frameHoverPanel._previousMouseLocation = currentLocation;

				}
				else if (panel._mousePressed)
				{
					Point currentLocation = e.MouseDevice.GetPosition(this);

					var delta = new TranslateTransform
					(currentLocation.X - panel._previousMouseLocation.X, currentLocation.Y - panel._previousMouseLocation.Y);

					var group = new TransformGroup();
					group.Children.Add(panel._previousTransform);
					group.Children.Add(delta);

					((UserControl)panel).RenderTransform = group;
					panel._previousTransform = ((UserControl)panel).RenderTransform;
					panel._previousMouseLocation = currentLocation;
				}
			}
		}

		private void LocalGrid_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			foreach (var panel in panels)
			{
				panel._mousePressed = false;
			}

			frameHoverPanel._dragMousePressed = false;
			frameHoverPanel.CurrentDrag = null;

			if (palette._selectedFrameTemplate!=null)
			{
				var createdFrame = (Fusion.Engine.Frames.Frame)null;
					Fusion.Core.Utils.FrameSerializer.Read(System.IO.Path.Combine(templatesPath, palette._selectedFrameTemplate)+".xml", out createdFrame);
				//Activator.CreateInstance(palette._selectedFrameTemplate, (engine.GameInterface as ApplicationInterface).rootFrame.ui);
				if (createdFrame!=null)
				{
					createdFrame.X = (int)e.MouseDevice.GetPosition(this).X - createdFrame.Width / 2;
					createdFrame.Y = (int)e.MouseDevice.GetPosition(this).Y - createdFrame.Height / 2;
					(engine.GameInterface as ApplicationInterface).rootFrame.Add(createdFrame); 
				}
				palette._selectedFrameTemplate = null;
				this.Cursor = Cursors.Arrow;
			}
		}

		private void Window_KeyDown( object sender, KeyEventArgs e )
		{
			if (e.Key == Key.Delete && treeView.ElementHierarcyView.SelectedItem!=null)
			{
				var selected = (Fusion.Engine.Frames.Frame)treeView.ElementHierarcyView.SelectedItem;
				details.FrameDetailsControls.ItemsSource = null;
				if (selected.Parent!=null)
				{
					treeView.ElementHierarcyView.SetSelectedItem(selected.Parent);
					//TreeViewExtension.SetSelected(details.FrameDetailsControls, selected.Parent);
				}


				selected.Parent.Remove(selected);

				//treeView.ElementHierarcyView.Items.Remove(treeView.ElementHierarcyView.SelectedItem);

			}
		}

		private void DxElem_MouseDown( object sender, MouseButtonEventArgs e )
		{
			var hoveredframe = (engine.GameInterface as ApplicationInterface).rootFrame.ui.GetHoveredFrameExternally();
			if (hoveredframe!=null)
			{
				treeView.SetSelectedFrame(hoveredframe);
			}
		}

		//PropertyValueConverter
	}

	public class IntNumbers
	{
		public int number { get; set; }
	}

	public class Propsy : INotifyPropertyChanged
	{
		bool locked;
		public Propsy( PropertyInfo prop, Fusion.Engine.Frames.Frame obj )
		{
			locked = true;
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
				Prop = Activator.CreateInstance(prop.PropertyType);
				Prop = prop.GetValue(obj);
			}
			else
			{
				if (prop.GetValue(obj) != null)
				{
					Prop = Activator.CreateInstance(prop.PropertyType, (prop.GetValue(obj) as string).ToCharArray());
				}
				else
				{
					Prop = String.Empty;
				}
			}
			Obj.PropertyChanged+=(s,e)=> {
				if (e.PropertyName == PropName)
				{
					var val = PropInfo.GetValue(Obj);
					if (!Prop.Equals(val))
					{
						Prop = PropInfo.GetValue(Obj);
					}
				}
			};
			locked = false;
		}

		private object prop;
		public object Prop
		{
			get { return this.prop; }
			set
			{
				try
				{
					var convertedValue = Convert.ChangeType(value, propInfo.PropertyType);
					this.prop = convertedValue;
					if (!locked)
					{
						propInfo.SetValue(obj, convertedValue);
					}
					OnPropertyChanged();
				}
				catch (Exception)
				{

					throw;
				}
			}
		}
		private Fusion.Engine.Frames.Frame obj;
		public Fusion.Engine.Frames.Frame Obj
		{
			get { return this.obj; }
			set
			{
				this.obj = value;
			}
		}
		private PropertyInfo propInfo;
		public PropertyInfo PropInfo
		{
			get { return this.propInfo; }
			set
			{
				this.propInfo = value;
			}
		}

		private Type propType;
		public Type PropType
		{
			get { return this.propType; }
			set
			{
				this.propType = value;
			}
		}

		public IList<object> EnumValues
		{
			get; set;
		}

		public string PropName
		{
			get; set;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}
}
