using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Frame = Fusion.Engine.Frames.Frame;

namespace WpfEditorTest.ChildPanels
{
    /// <summary>
    /// Interaction logic for FrameTreeView.xaml
    /// </summary>
    public partial class FrameTreeView : UserControl, IDraggablePanel
    {
        private readonly ItemsControl _frameDetailsControls;

        public Point PreviousMouseLocation { get; set; }
        public Transform PreviousTransform { get; set; }
        public bool MousePressed { get; set; }
        public InterfaceEditor Window { get; set; }

        private Frame _selectedFrame;
        public Frame SelectedFrame
        {
            get => _selectedFrame;
            set => SetSelectedFrame(value);
        }

	    public EventHandler<Frame> SelectedFrameChangedInUI;

		public FrameTreeView( InterfaceEditor interfaceEditor, ItemsControl frameDetailsControls)
		{
			InitializeComponent();
			_frameDetailsControls = frameDetailsControls;

		    Window = interfaceEditor;
            PreviousTransform = RenderTransform;
			Height = StaticData.OptionsWindowSize;
		    Width = StaticData.OptionsWindowSize;

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Bottom;
		}

		private void TextBlock_MouseDown( object sender, MouseButtonEventArgs e )
		{
		    SelectedFrameChangedInUI?.Invoke(this, SelectedFrame);
        }

		private void SetSelectedFrame(Frame frame)
		{
			_selectedFrame = frame;

			var publicProperties = SelectedFrame.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			var propsies = (
			    from property in publicProperties
			    where property.GetMethod != null && property.SetMethod != null && !property.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute")
			    select new Propsy(property, SelectedFrame)
			).ToList();

		    _frameDetailsControls.ItemsSource = propsies.OrderBy(p => p.PropName).ToList();
		}

		public void Border_MouseDown( object sender, MouseButtonEventArgs e )
		{
			MousePressed = true;
			PreviousMouseLocation = e.MouseDevice.GetPosition(Window);
			PreviousTransform = RenderTransform;
		}

	    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
	    {
	        e.Handled = true;
	    }

		private void SaveScene_Click(object sender, RoutedEventArgs e )
		{
			Window.TrySaveSceneAsTemplate();
		}

		private void LoadScene_Click( object sender, RoutedEventArgs e )
		{
			Window.TryLoadSceneAsTemplate();
		}
	}
}
