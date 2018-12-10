using System;
using System.Configuration;
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

			Left = int.Parse(ConfigurationManager.AppSettings.Get("TreeViewPanelX"));
			Top = int.Parse(ConfigurationManager.AppSettings.Get("TreeViewPanelY"));
			Visibility = (Visibility)Enum.Parse(typeof(Visibility), ConfigurationManager.AppSettings.Get("TreeViewPanelVisibility"));

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Bottom;

            Closing += (s, e) => { Visibility = Visibility.Collapsed; e.Cancel = true; };
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
