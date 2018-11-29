using System.Windows;
using System.Windows.Controls;

namespace WpfEditorTest.ChildPanels
{
    /// <summary>
    /// Interaction logic for DraggablePanel.xaml
    /// </summary>
    public partial class DraggablePanel : Window
    {
        public ScrollViewer Holder => ContentHolder;
        public DraggablePanel()
        {
            InitializeComponent();
        }
    }
}
