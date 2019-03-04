using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Fusion.Engine.Frames2;
using WpfEditorTest.UndoRedo;
using WpfEditorTest.Utility;
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
		public EventHandler<UIController> ControllerSlotSelected;
		public EventHandler RequestFrameDeletionInUI;

		private TextBlock initTreeViewItemHolder;
		private Brush treeViewItemHolderInitColor;

		public Point LeftTextBlockPoint = new Point();
		public Point RightTextBlockPoint = new Point();
		AdornerLayer parentAdorner;
		CustomTreeViewAdorner customAdorner;

		public FrameTreeView()
		{
			InitializeComponent();

			Height = double.Parse(ConfigurationManager.AppSettings.Get("TreeViewPanelHeight"));
		    Width = ApplicationConfig.OptionsWindowSize;

			Left = double.Parse(ConfigurationManager.AppSettings.Get("TreeViewPanelX"));
			Top = double.Parse(ConfigurationManager.AppSettings.Get("TreeViewPanelY"));

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Bottom;

			Closing += (s, e) => { this.Hide(); e.Cancel = true; };

			parentAdorner = AdornerLayer.GetAdornerLayer(ElementHierarchyView);
			parentAdorner.Add(customAdorner = new CustomTreeViewAdorner(ElementHierarchyView, this));
		}

		public void AttachScene(UIContainer scene )
		{
			_scene = scene;
		}

		private void TextBlock_MouseDown( object sender, MouseButtonEventArgs e )
		{
			this.initTreeViewItemHolder = sender as TextBlock;
			var objectChecking = (sender as TextBlock).Tag;
			if (objectChecking is UIController.Slot)
			{
				//ControllerSlotSelected?.Invoke(this, objectChecking as UIController.Slot);
				return;
			}
			else if (objectChecking is UIController)
			{
				ControllerSlotSelected?.Invoke(this, objectChecking as UIController);
			}
			else
			{
				ControllerSlotSelected?.Invoke(this, null);
			}


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
			List<object> components = new List<object>();

			while (currentFrame.Parent != null && currentFrame.Parent != _scene)
			{
				if (currentFrame.Parent is UIController)
				{
					components.Add((currentFrame.Parent as UIController).Slots.Where(s => s.Component == currentFrame).FirstOrDefault());
					currentFrame = currentFrame.Parent;
					components.Add(currentFrame);
				}
				else
				{
					currentFrame = currentFrame.Parent;
					components.Add(currentFrame);
				}


			}

			components.Reverse();
			TreeViewItem childNode;

			foreach (var frame in components)
			{
				childNode = parent.ItemContainerGenerator.ContainerFromItem(frame) as TreeViewItem;
				childNode.IsExpanded = true;
				parent.UpdateLayout();
				parent = parent
					.ItemContainerGenerator
					.ContainerFromItem(frame)
					as ItemsControl;
			}
			parent.UpdateLayout();
			childNode = parent.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
			if (childNode != null)
			{
				childNode.IsSelected = true; 
			}
		}

		public static bool IsInFirstHalf( FrameworkElement container, Point mousePosition)
		{
			return mousePosition.Y < container.ActualHeight / 2;
		}

		public static int IsOnEdges( FrameworkElement container, Point mousePosition )
		{
			if (mousePosition.Y < container.ActualHeight / 5)
			{
				return -1; // mouse is on upper edge of the container
			}
			else if (mousePosition.Y > container.ActualHeight / 5 * 4)
			{
				return 1; // mouse is on lower edge of the container
			}
			else
			{
				return 0; // mouse is approximately in the center of the container
			}
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
				customAdorner.Visibility = Visibility.Visible;
				var toAncestor = (treeViewItemHolder.TransformToAncestor(ElementHierarchyView) as Transform);

				if (treeViewItemHolder.Tag is UIContainer && !(treeViewItemHolder.Tag is UIController))
				{
					e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
					treeViewItemHolder.Background = ApplicationConfig.TreeViewItemHolderAllowedHoveredColor;
				}
				else
				{
					e.Effects = DragDropEffects.None;
					treeViewItemHolder.Background = ApplicationConfig.TreeViewItemHolderRestrictedHoveredColor;
				}

				switch (IsOnEdges(treeViewItemHolder, e.GetPosition(treeViewItemHolder)))
				{
					case -1:
						{
							LeftTextBlockPoint = new Point(toAncestor.Value.OffsetX,
								toAncestor.Value.OffsetY);
							RightTextBlockPoint = new Point(treeViewItemHolder.RenderSize.Width,
								toAncestor.Value.OffsetY);
							break;
						}
					case 1:
						{
							LeftTextBlockPoint = new Point(toAncestor.Value.OffsetX,
								toAncestor.Value.OffsetY + treeViewItemHolder.RenderSize.Height);
							RightTextBlockPoint = new Point(treeViewItemHolder.RenderSize.Width,
								toAncestor.Value.OffsetY + treeViewItemHolder.RenderSize.Height);
							break;
						}
					case 0:
						{
							LeftTextBlockPoint = new Point(0,
								-10);
							RightTextBlockPoint = new Point(0,
								-10);
							break;
						}
				}

				parentAdorner.RenderTransform = new TranslateTransform(LeftTextBlockPoint.X, LeftTextBlockPoint.Y);
				parentAdorner.Update();
			}
		}

		private void TextBlock_PreviewDrop( object sender, DragEventArgs e )
		{
			e.Handled = true;
			var treeViewItemHolder = sender as TextBlock;
			// If the DataObject contains string data, extract it.
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				UIComponent dataComponent = (UIComponent)e.Data.GetData(DataFormats.FileDrop);
				if (dataComponent !=null && treeViewItemHolder != this.initTreeViewItemHolder && treeViewItemHolder.Tag is UIComponent)
				{
					IEditorCommand command = null;
					int index = (treeViewItemHolder.Tag as UIComponent).Parent.Children.IndexOf(treeViewItemHolder.Tag as UIComponent);
					switch (IsOnEdges(treeViewItemHolder, e.GetPosition(treeViewItemHolder)))
					{
						case -1:
							{
								command = new FrameParentChangeCommand(dataComponent, (treeViewItemHolder.Tag as UIComponent).Parent, dataComponent.Parent, index);
								break;
							}
						case 1:
							{
								command = new FrameParentChangeCommand(dataComponent, (treeViewItemHolder.Tag as UIComponent).Parent, dataComponent.Parent, index+1);
								break;
							}
						case 0:
							{
								if (treeViewItemHolder.Tag is UIContainer && !(treeViewItemHolder.Tag is UIController))
									command = new FrameParentChangeCommand(dataComponent, treeViewItemHolder.Tag as UIContainer, dataComponent.Parent);
								break;
							}
					}

					//command = new FrameParentChangeCommand(dataComponent, treeViewItemHolder.Tag as UIContainer, dataComponent.Parent);
					if (command!=null)
					{
						CommandManager.Instance.Execute(command); 
					}
				}
			}
			treeViewItemHolder.Background = treeViewItemHolderInitColor;
			customAdorner.Visibility = Visibility.Hidden;
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
					var command = new FrameParentChangeCommand(dataComponent, _scene, dataComponent.Parent);
					CommandManager.Instance.Execute(command);
				}
			}
			customAdorner.Visibility = Visibility.Hidden;
		}

		private void TextBlock_MouseMove( object sender, MouseEventArgs e )
		{
			//IsInFirstHalf(sender as TreeViewItem, e.GetPosition(sender as TreeViewItem));
		}
	}
}
