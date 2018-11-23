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
	/// Interaction logic for FramePalette.xaml
	/// </summary>
	public partial class FramePalette : UserControl, IDraggablePanel
	{
		public string _selectedFrameTemplate;

		public Point _previousMouseLocation { get; set; }
		public Transform _previousTransform { get; set; }
		public bool _mousePressed { get; set; }
		public InterfaceEditor _window { get; set; }

		public FramePalette( InterfaceEditor interfaceEditor )
		{
			InitializeComponent();
			Transform _previousTransform = this.RenderTransform;
			_window = interfaceEditor;
			Height = StaticData.OptionsWindowSize; Width = StaticData.OptionsWindowSize;

			//this.Margin = new Thickness(_window.Width - this.Width, 0, 0, _window.Height - this.Height);
			this.HorizontalAlignment = HorizontalAlignment.Right;
			this.VerticalAlignment = VerticalAlignment.Top;
		}

		public void Border_MouseDown( object sender, MouseButtonEventArgs e )
		{
			this._mousePressed = true;
			_previousMouseLocation = e.MouseDevice.GetPosition(_window);
			_previousTransform = this.RenderTransform;
		}

		private void Border_MouseDown_1( object sender, MouseButtonEventArgs e )
		{
			this._selectedFrameTemplate = (string)(sender as Border).Tag;
			_window.Cursor = Cursors.Hand;
		}

		private void UserControl_MouseUp( object sender, MouseButtonEventArgs e )
		{
			this._selectedFrameTemplate = null;
			_window.Cursor = Cursors.Arrow;
		}
	}
}
