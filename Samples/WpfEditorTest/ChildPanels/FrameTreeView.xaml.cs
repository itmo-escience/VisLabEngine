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
using WpfEditorTest.UndoRedo;
using CommandManager = WpfEditorTest.UndoRedo.CommandManager;

namespace WpfEditorTest.ChildPanels
{
    /// <summary>
    /// Interaction logic for FrameTreeView.xaml
    /// </summary>
    public partial class FrameTreeView : Window
    {
		private UIContainer _scene { get; set; }

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

		private TextBlock initTreeViewItemHolder;
		private Brush treeViewItemHolderInitColor;

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

		public void AttachScene(UIContainer scene )
		{
			_scene = scene;
		}

		private void TextBlock_MouseDown( object sender, MouseButtonEventArgs e )
		{
			this.initTreeViewItemHolder = sender as TextBlock;
			var component = (UIComponent)(sender as TextBlock).Tag;

			SelectedFrameChangedInUI?.Invoke(this, component);

			if (component != null)
			{
				DataObject dataObject = new DataObject();
				dataObject.SetData(DataFormats.FileDrop, component);
				DragDrop.DoDragDrop(initTreeViewItemHolder,
									 dataObject,
									 DragDropEffects.Move);
			}
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

			while (currentFrame.Parent != null && currentFrame.Parent != _scene)
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

		private void TextBlock_PreviewDragOver( object sender, DragEventArgs e )
		{
			var treeViewItemHolder = sender as TextBlock;
			//e.Handled = false;

			e.Effects = DragDropEffects.None;

			UIComponent dataComponent = (UIComponent)e.Data.GetData(DataFormats.FileDrop);

			// If the string can be converted into a Brush, allow copying.
			if (dataComponent != null && treeViewItemHolder != this.initTreeViewItemHolder)
			{
				if (treeViewItemHolder.Tag is UIContainer)
				{
					e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
					treeViewItemHolder.Background = ApplicationConfig.TreeViewItemHolderAllowedHoveredColor;
				}
				else
				{
					e.Effects = DragDropEffects.None;
					treeViewItemHolder.Background = ApplicationConfig.TreeViewItemHolderRestrictedHoveredColor;
				}
			}
		}

		private void TextBlock_PreviewDrop( object sender, DragEventArgs e )
		{
			var treeViewItemHolder = sender as TextBlock;
			// If the DataObject contains string data, extract it.
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				UIComponent dataComponent = (UIComponent)e.Data.GetData(DataFormats.FileDrop);
				if (dataComponent !=null && treeViewItemHolder != this.initTreeViewItemHolder && treeViewItemHolder.Tag is UIContainer)
				{
					var command = new FrameParentChangeCommand(dataComponent, treeViewItemHolder.Tag as UIContainer, dataComponent.Parent);
					CommandManager.Instance.Execute(command);
				}
			}
			treeViewItemHolder.Background = treeViewItemHolderInitColor;
		}

		private void TextBlock_PreviewDragLeave( object sender, DragEventArgs e )
		{
			var treeViewItemHolder = sender as TextBlock;
			treeViewItemHolder.Background = treeViewItemHolderInitColor;
		}

		private void TextBlock_PreviewDragEnter( object sender, DragEventArgs e )
		{
			var treeViewItemHolder = sender as TextBlock;
			treeViewItemHolderInitColor = treeViewItemHolder.Background;
		}

		private void ElementHierarchyView_Drop( object sender, DragEventArgs e )
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				UIComponent dataComponent = (UIComponent)e.Data.GetData(DataFormats.FileDrop);
				if (dataComponent != null)
				{
					this.initTreeViewItemHolder = null;
				}
			}
		}
	}
}
