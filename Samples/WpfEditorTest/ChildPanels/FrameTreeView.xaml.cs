using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fusion.Engine.Frames;

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for FrameTreeView.xaml
	/// </summary>
	public partial class FrameTreeView : UserControl, IDraggablePanel
	{
		ItemsControl FrameDetailsControls;

		public Point _previousMouseLocation { get; set; }
		public Transform _previousTransform { get; set; }
		public bool _mousePressed { get; set; }
		public InterfaceEditor _window { get; set; }
		public Fusion.Engine.Frames.Frame Selectedframe {
			get => selectedframe;
			set { selectedframe = value;
				this.selectedFrameChanged?.Invoke(this, null); }
		}

		Fusion.Engine.Frames.Frame selectedframe;
		Fusion.Engine.Frames.Frame lastSelectedframe;
		int lastSelectedframeBorder;
		Fusion.Core.Mathematics.Color lastSelectedframeBorderColor;
		public EventHandler selectedFrameChanged;

		public FrameTreeView( InterfaceEditor interfaceEditor, ItemsControl frameDetailsControls)
		{
			InitializeComponent();
			FrameDetailsControls = frameDetailsControls;

			Transform _previousTransform = this.RenderTransform;
			_window = interfaceEditor;
			Height = StaticData.OptionsWindowSize; Width = StaticData.OptionsWindowSize;

			//this.Margin = new Thickness(_window.Width - this.Width, this.Height*2, 0, _window.Height - this.Height * 3);
			this.HorizontalAlignment = HorizontalAlignment.Right;
			this.VerticalAlignment = VerticalAlignment.Bottom;

			selectedFrameChanged += ( s, e ) => {

			};
		}

		private void TextBlock_MouseDown( object sender, MouseButtonEventArgs e )
		{
			SetSelectedFrame((Fusion.Engine.Frames.Frame)((sender as TextBlock).Tag));
		}

		public void SetSelectedFrame( Fusion.Engine.Frames.Frame frame )
		{
			//if (lastSelectedframe != null)
			//{
			//	lastSelectedframe.Border = lastSelectedframeBorder;
			//	lastSelectedframe.BorderColor = lastSelectedframeBorderColor;
			//}

			Selectedframe = frame; 

			lastSelectedframe = Selectedframe;
			lastSelectedframeBorder = Selectedframe.BorderTop;
			lastSelectedframeBorderColor = Selectedframe.BorderColor;

			//Selectedframe.Border = 2;
			//Selectedframe.BorderColor = Fusion.Core.Mathematics.Color.Yellow;



			var publicProperties = Selectedframe.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			List<Propsy> propsies = new List<Propsy>();
			foreach (var property in publicProperties)
			{
				if (property.GetMethod != null && property.SetMethod != null && !property.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute"))
				{
					propsies.Add(new Propsy(property, Selectedframe));
				}
			}

			FrameDetailsControls.ItemsSource = propsies.OrderBy(p => p.PropName).ToList();
		}

		public void Border_MouseDown( object sender, MouseButtonEventArgs e )
		{
			this._mousePressed = true;
			_previousMouseLocation = e.MouseDevice.GetPosition(_window);
			_previousTransform = this.RenderTransform;
		}

		private void SaveScene_Click(object sender, RoutedEventArgs e )
		{
			_window.TrySaveSceneAsTemplate();
		}

		private void LoadScene_Click( object sender, RoutedEventArgs e )
		{
			_window.TryLoadSceneAsTemplate();
		}
	}
}
