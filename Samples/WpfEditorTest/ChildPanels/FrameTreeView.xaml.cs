using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Fusion.Engine.Frames2;

namespace WpfEditorTest.ChildPanels
{
    /// <summary>
    /// Interaction logic for FrameTreeView.xaml
    /// </summary>
    public partial class FrameTreeView : Window
    {
        private UIComponent _selectedFrame;
        public UIComponent SelectedFrame
        {
            get => _selectedFrame;
            set
            {
                _selectedFrame = value;
				SetSelected(ElementHierarchyView, _selectedFrame);
            }
        }

        public EventHandler<UIComponent> SelectedFrameChangedInUI;
		public EventHandler RequestFrameDeletionInUI;

		public FrameTreeView()
		{
			InitializeComponent();

			Height = int.Parse(ConfigurationManager.AppSettings.Get("TreeViewPanelHeight"));
		    Width = ApplicationConfig.OptionsWindowSize;

			Left = int.Parse(ConfigurationManager.AppSettings.Get("TreeViewPanelX"));
			Top = int.Parse(ConfigurationManager.AppSettings.Get("TreeViewPanelY"));

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Bottom;

            Closing += (s, e) => { this.Hide(); e.Cancel = true; };
		}

		private void TextBlock_MouseDown( object sender, MouseButtonEventArgs e )
		{
		    SelectedFrameChangedInUI?.Invoke(this, (UIComponent)(sender as TextBlock).Tag);
        }

		private void Window_KeyDown( object sender, KeyEventArgs e )
		{
			if (e.Key == Key.Delete)
			{
				RequestFrameDeletionInUI?.Invoke(this,null);
			}
		}

		public void SetSelected( ItemsControl parent, UIComponent child )
		{
			var currentFrame = child;
			List<UIComponent> frames = new List<UIComponent>();

			while (currentFrame.Parent != null /*&& currentFrame.Parent.Text != "Scene"*/)
			{
				currentFrame = currentFrame.Parent;
				frames.Add(currentFrame);
			}

			frames.Reverse();
			TreeViewItem childNode;

			foreach (var frame in frames)
			{
				childNode = parent.ItemContainerGenerator.ContainerFromItem(frame) as TreeViewItem;
				childNode.IsExpanded = true;
				parent.UpdateLayout();
				parent = parent
					.ItemContainerGenerator
					.ContainerFromItem(frame)
					as ItemsControl;
			}
			childNode = parent.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
			childNode.IsSelected = true;
		}
	}
}
