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
using Fusion.Engine.Frames2.Controllers;
using WpfEditorTest.Commands;
using WpfEditorTest.Utility;
using CommandManager = WpfEditorTest.Commands.CommandManager;

namespace WpfEditorTest.ChildPanels
{
    /// <summary>
    /// Interaction logic for FrameTreeView.xaml
    /// </summary>
    public partial class FrameTreeView : Window
    {
		private IUIModifiableContainer<ISlot> _scene { get; set; }

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
		public EventHandler<UIController<IControllerSlot>> ControllerSlotSelected;
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

		    System.Diagnostics.PresentationTraceSources.SetTraceLevel(ElementHierarchyView.ItemContainerGenerator, System.Diagnostics.PresentationTraceLevel.High);

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

		public void AttachScene( IUIModifiableContainer<ISlot> scene )
		{
			_scene = scene;
		}

		private void TextBlock_MouseDown( object sender, MouseButtonEventArgs e )
		{
			this.initTreeViewItemHolder = sender as TextBlock;
			var objectChecking = (sender as TextBlock).Tag;
			if (objectChecking is IControllerSlot)
			{
				//ControllerSlotSelected?.Invoke(this, objectChecking as UIController.Slot);
				return;
			}
			else if (objectChecking is UIController<IControllerSlot>)
			{
				ControllerSlotSelected?.Invoke(this, objectChecking as UIController<IControllerSlot>);
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

			while (currentFrame.Parent() != null && currentFrame.Parent() != _scene)
			{
				if (currentFrame.Parent() is UIController<IControllerSlot>)
				{
					components.Add((currentFrame.Parent() as UIController<IControllerSlot>).Slots.Where(s => s.Component == currentFrame).FirstOrDefault());
					currentFrame = currentFrame.Parent();
					components.Add(currentFrame);
				}
				else
				{
					currentFrame = currentFrame.Parent();
					components.Add(currentFrame);
				}


			}

			components.Reverse();
			TreeViewItem childNode;
			foreach (var frame in components)
			{
				childNode = parent.ItemContainerGenerator.ContainerFromItem(frame) as TreeViewItem;
				if (childNode == null)
				{
					return;
				}
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

			e.Effects = DragDropEffects.None;

			UIComponent dataComponent = (UIComponent)e.Data.GetData(DataFormats.FileDrop);

			if (dataComponent != null && treeViewItemHolder != this.initTreeViewItemHolder)
			{
				customAdorner.Visibility = Visibility.Visible;
				var toAncestor = (treeViewItemHolder.TransformToAncestor(ElementHierarchyView) as Transform);

				if (treeViewItemHolder.Tag is IUIModifiableContainer<ISlot> && !(treeViewItemHolder.Tag is UIController<IControllerSlot>))
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
					int index = (treeViewItemHolder.Tag as UIComponent).Parent().IndexOf((treeViewItemHolder.Tag as UIComponent));
					switch (IsOnEdges(treeViewItemHolder, e.GetPosition(treeViewItemHolder)))
					{
						case -1:
							{
								command = new UIComponentParentChangeCommand(dataComponent, (treeViewItemHolder.Tag as UIComponent).Parent() as IUIModifiableContainer<ISlot>, dataComponent.Parent() as IUIModifiableContainer<ISlot>, index);
								break;
							}
						case 1:
							{
								command = new UIComponentParentChangeCommand(dataComponent, (treeViewItemHolder.Tag as UIComponent).Parent() as IUIModifiableContainer<ISlot>, dataComponent.Parent() as IUIModifiableContainer<ISlot>, index+1);
								break;
							}
						case 0:
							{
								if (treeViewItemHolder.Tag is IUIModifiableContainer<ISlot> && !(treeViewItemHolder.Tag is UIController<IControllerSlot>))
									command = new UIComponentParentChangeCommand(dataComponent, treeViewItemHolder.Tag as IUIModifiableContainer<ISlot>, dataComponent.Parent() as IUIModifiableContainer<ISlot>);
								break;
							}
					}
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
					var command = new UIComponentParentChangeCommand(dataComponent, _scene, dataComponent.Parent() as IUIModifiableContainer<ISlot>);
					CommandManager.Instance.Execute(command);
				}
			}
			customAdorner.Visibility = Visibility.Hidden;
		}

		private void ScrollViewer_PreviewMouseWheel( object sender, MouseWheelEventArgs e )
		{
			var scroll = sender as ScrollViewer;
			if (e.Delta>0)
			{
				scroll.LineUp();
			}
			else
			{
				scroll.LineDown();
			}
		}
	}
}
