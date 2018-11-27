using System.Windows;
using System.Windows.Media;

namespace WpfEditorTest.ChildPanels
{
	internal interface IDraggablePanel
	{
		Point PreviousMouseLocation { get; set; }
		Transform PreviousTransform { get; set; }
		bool MousePressed { get; set; }
		InterfaceEditor Window { get; set; }
	}
}