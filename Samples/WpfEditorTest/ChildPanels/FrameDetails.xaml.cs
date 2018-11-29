using System.Windows;
using System.Windows.Input;

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for FrameDetails.xaml
	/// </summary>
	public partial class FrameDetails : Window
	{
		public FrameDetails()
		{
			InitializeComponent();
			Height = StaticData.OptionsWindowSize; Width = StaticData.OptionsWindowSize;

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Center;

            Closing += (s, e) => e.Cancel = true;
		}

		private void UserControl_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
			e.Handled = true;
		}
	}
}
