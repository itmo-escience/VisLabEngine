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
using System.Windows.Shapes;

namespace WpfEditorTest.DialogWindows
{
	/// <summary>
	/// Interaction logic for SceneConfigWindow.xaml
	/// </summary>
	public partial class SceneConfigWindow : Window
	{
		public double SceneWidth { get; set; }
		public double SceneHeight { get; set; }

		public SceneConfigWindow(double sceneWidth , double sceneHeight)
		{
			InitializeComponent();

			SceneWidth = sceneWidth;
			SceneHeight = sceneHeight;

			txbxSceneWidth.Value = SceneWidth;
			txbxSceneHeight.Value = SceneHeight;
		}

		private void btnOK_Click( object sender, RoutedEventArgs e )
		{
			SceneWidth = txbxSceneWidth.Value.Value;
			SceneHeight = txbxSceneHeight.Value.Value;

			this.DialogResult = true;
		}

		private void btnCancel_Click( object sender, RoutedEventArgs e )
		{
			this.DialogResult = false;
		}
	}
}
