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
			AcceptChanges();
		}

		private void btnCancel_Click( object sender, RoutedEventArgs e )
		{
			DeclineChanges();
		}

		private void txbxSceneWidth_KeyDown( object sender, KeyEventArgs e )
		{
			CheckKey(e);
		}

		private void txbxSceneHeight_KeyDown( object sender, KeyEventArgs e )
		{
			CheckKey(e);
		}

		private void CheckKey( KeyEventArgs e )
		{
			switch (e.Key)
			{
				case Key.Enter:
					{
						AcceptChanges();
						break;
					}
				case Key.Escape:
					{
						DeclineChanges();
						break;
					}
			}
		}

		private void AcceptChanges()
		{
			SceneWidth = txbxSceneWidth.Value.Value;
			SceneHeight = txbxSceneHeight.Value.Value;
			this.DialogResult = true;
		}

		private void DeclineChanges()
		{
			this.DialogResult = false;
		}




	}
}
