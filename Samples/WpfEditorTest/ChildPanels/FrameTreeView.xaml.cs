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
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Frames2.Controllers;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Frames2.Utils;
using WpfEditorTest.Commands;
using WpfEditorTest.Utility;
using CommandManager = WpfEditorTest.Commands.CommandManager;

namespace WpfEditorTest.Utility
{
    /// <summary>
    /// Interaction logic for FrameTreeView.xaml
    /// </summary>
    public partial class FrameTreeView : Window
    {
		private IUIModifiableContainer<ISlot> _scene { get; set; }

        private IUIComponent _selectedFrame;
        public IUIComponent SelectedFrame
        {
            get => _selectedFrame;
            set
            {
                _selectedFrame = value;
				SetSelected(ElementHierarchyView, _selectedFrame);
            }
        }

        public EventHandler<IUIComponent> SelectedFrameChangedInUI;
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
			else if (objectChecking is UIController)
			{
				ControllerSlotSelected?.Invoke(this, objectChecking as UIController);
			}
			else
			{
				ControllerSlotSelected?.Invoke(this, null);
			}


			var component = (IUIComponent)(sender as TextBlock).Tag;

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

		public void SetSelected( ItemsControl parent, IUIComponent child )
		{
			var currentFrame = child;
			List<object> components = new List<object>();

			while (currentFrame.Parent() != null && currentFrame.Parent() != _scene)
			{
				if (currentFrame.Parent() is UIController)
				{
					components.Add((currentFrame.Parent() as UIController).Slots.Where(s => s.Component == currentFrame).FirstOrDefault());
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

			IUIComponent component = CreateComponentFromDrop(e.Data);

			if (component != null && treeViewItemHolder != this.initTreeViewItemHolder)
			{
				customAdorner.Visibility = Visibility.Visible;
				var toAncestor = (treeViewItemHolder.TransformToAncestor(ElementHierarchyView) as Transform);

				if (treeViewItemHolder.Tag is IUIModifiableContainer<ISlot> && !(treeViewItemHolder.Tag is UIController))
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

			IUIComponent component = CreateComponentFromDrop(e.Data);

				if (component != null && treeViewItemHolder != this.initTreeViewItemHolder && treeViewItemHolder.Tag is IUIComponent)
				{
					IEditorCommand command = null;
					int index = (treeViewItemHolder.Tag as IUIComponent).Parent().IndexOf((treeViewItemHolder.Tag as IUIComponent));
					switch (IsOnEdges(treeViewItemHolder, e.GetPosition(treeViewItemHolder)))
					{
						case -1:
							{
								command = new UIComponentParentChangeCommand(component, (treeViewItemHolder.Tag as IUIComponent).Parent() as IUIModifiableContainer<ISlot>, component.Parent() as IUIModifiableContainer<ISlot>, index);
								break;
							}
						case 1:
							{
								command = new UIComponentParentChangeCommand(component, (treeViewItemHolder.Tag as IUIComponent).Parent() as IUIModifiableContainer<ISlot>, component.Parent() as IUIModifiableContainer<ISlot>, index+1);
								break;
							}
						case 0:
							{
								if (treeViewItemHolder.Tag is IUIModifiableContainer<ISlot> && !(treeViewItemHolder.Tag is UIController))
									command = new UIComponentParentChangeCommand(component, treeViewItemHolder.Tag as IUIModifiableContainer<ISlot>, component.Parent() as IUIModifiableContainer<ISlot>);
								break;
							}
					}
					if (command!=null)
					{
						CommandManager.Instance.Execute(command);

					foreach (IUIComponent child in UIHelper.BFSTraverse(component))
					{
						UIManager.MakeComponentNameValid(child, _scene, child);
					}
				}
				this.initTreeViewItemHolder = null;
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

			IEditorCommand command;
			IUIComponent component = CreateComponentFromDrop(e.Data);

			if (component != null)
			{
				command = new UIComponentParentChangeCommand(component, _scene, component.Parent() as IUIModifiableContainer<ISlot>);
				CommandManager.Instance.Execute(command);

				foreach (IUIComponent child in UIHelper.BFSTraverse(component))
				{
					UIManager.MakeComponentNameValid(child, _scene, child);
				}
			}

			customAdorner.Visibility = Visibility.Hidden;
		}

		private IUIComponent CreateComponentFromDrop( IDataObject data )
		{
			if (data.GetDataPresent(DataFormats.StringFormat))
			{
				string dataString = (string)data.GetData(DataFormats.StringFormat);
				if (!string.IsNullOrEmpty(dataString))
				{
					return CreateFrameFromFile(System.IO.Path.Combine(ApplicationConfig.TemplatesPath, dataString) + ".xml");
				}
				else
				{
					return null;
				}
			}
			else
			{
				if (data.GetData(DataFormats.FileDrop) is IUIComponent)
				{
					return data.GetData(DataFormats.FileDrop) as IUIComponent;
				}
				else
				{
					var component =
					 Activator.CreateInstance(data.GetData(DataFormats.FileDrop) as Type) as IUIComponent;
					component.DefaultInit();
					return component;
				}
			}


			//TODO
			//PaletteWindow.SelectedFrameTemplate = null;
			//_parentHighlightPanel.SelectedFrame = null;
		}

		public IUIComponent CreateFrameFromFile( string filePath )
		{
			IUIComponent createdFrame = null;
			try
			{
				Fusion.Core.Utils.UIComponentSerializer.Read(filePath, out createdFrame);
			}
			catch (Exception ex)
			{
				Fusion.Log.Error($"---------------ERROR---------------");
				Fusion.Log.Error($"Could not deserialize file \"{filePath}\".\n");
				Fusion.Log.Error($"Next exception is thrown:\n{ex.Message}\n");
				Fusion.Log.Error($"Exception stack trace:\n{ex.StackTrace}");
				Fusion.Log.Error($"---------------ERROR---------------\n");
			}

			return createdFrame;
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
