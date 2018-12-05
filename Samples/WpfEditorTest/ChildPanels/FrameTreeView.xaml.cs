using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Frame = Fusion.Engine.Frames.Frame;

namespace WpfEditorTest.ChildPanels
{
    /// <summary>
    /// Interaction logic for FrameTreeView.xaml
    /// </summary>
    public partial class FrameTreeView : Window
    {
        private Frame _selectedFrame;
        public Frame SelectedFrame
        {
            get => _selectedFrame;
            set
            {
                _selectedFrame = value;
                ElementHierarchyView.SetSelectedItem(value);
            }
        }

        public EventHandler<Frame> SelectedFrameChangedInUI;
		public EventHandler RequestFrameDeletionInUI;

		public FrameTreeView()
		{
			InitializeComponent();

			Height = StaticData.OptionsWindowSize;
		    Width = StaticData.OptionsWindowSize;

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Bottom;

            Closing += (s, e) => { e.Cancel = true; };
		}

		private void TextBlock_MouseDown( object sender, MouseButtonEventArgs e )
		{
		    SelectedFrameChangedInUI?.Invoke(this, (Frame)(sender as TextBlock).Tag);
        }

		private void Window_KeyDown( object sender, KeyEventArgs e )
		{
			if (e.Key == Key.Delete)
			{
				RequestFrameDeletionInUI?.Invoke(this,null);
			}
		}
	}
}
