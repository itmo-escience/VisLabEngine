using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for FrameDetails.xaml
	/// </summary>
	public partial class FrameDetails : UserControl, IDraggablePanel
	{
		public Point PreviousMouseLocation { get; set; }
		public Transform PreviousTransform { get; set; }
		public bool MousePressed { get; set; }
		public InterfaceEditor Window { get; set; }

		public FrameDetails( InterfaceEditor interfaceEditor )
		{
			InitializeComponent();
			PreviousTransform = RenderTransform;
			Window = interfaceEditor;
			Height = StaticData.OptionsWindowSize; Width = StaticData.OptionsWindowSize;

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Center;
		}

		public void Border_MouseDown( object sender, MouseButtonEventArgs e )
		{
			MousePressed = true;
			PreviousMouseLocation = e.MouseDevice.GetPosition(Window);
			PreviousTransform = RenderTransform;
		}

		private void Save_Click( object sender, RoutedEventArgs e )
		{
			Window.TrySaveFrameAsTemplate();
		}
	}
}
