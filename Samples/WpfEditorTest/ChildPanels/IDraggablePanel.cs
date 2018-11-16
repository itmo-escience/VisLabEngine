using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfEditorTest.ChildPanels
{
	internal interface IDraggablePanel
	{

		Point _previousMouseLocation { get; set; }
		Transform _previousTransform { get; set; }
		bool _mousePressed { get; set; }
		Window _window { get; set; }

		void Border_MouseDown( object sender, MouseButtonEventArgs e );
	}
}