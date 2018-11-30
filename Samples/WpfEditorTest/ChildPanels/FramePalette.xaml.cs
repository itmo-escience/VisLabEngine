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
	public partial class FramePalette : Window
	{
		public string _selectedFrameTemplate;

		public Point PreviousMouseLocation { get; set; }
		public Transform PreviousTransform { get; set; }
		public bool MousePressed { get; set; }

		public FramePalette()
		{
			InitializeComponent();

			Height = StaticData.OptionsWindowSize;
			Width = StaticData.OptionsWindowSize;

			this.HorizontalAlignment = HorizontalAlignment.Right;
			this.VerticalAlignment = VerticalAlignment.Top;

			Closing += ( s, e ) => { e.Cancel = true; };
		}


		private void Border_MouseDown_1( object sender, MouseButtonEventArgs e )
		{
			this._selectedFrameTemplate = (string)(sender as Border).Tag;
		}
	}
}
