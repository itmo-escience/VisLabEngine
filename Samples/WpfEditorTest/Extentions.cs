using System.Collections.Generic;
using System.Windows.Controls;

namespace WpfEditorTest
{
	public static class TreeViewExtension
	{
		/// <summary>
		/// Expand a TreeView to a specific node
		/// </summary>
		/// <param name="tv">The treeview</param>
		/// <param name="node">The string of the node in the Item.Tag property to expand to</param>
		public static void JumpToFolder(this TreeView tv, Fusion.Engine.Frames.Frame node )
		{
			bool done = false;
			ItemCollection ic = tv.Items;

			while (!done)
			{
				bool found = false;

				foreach (TreeViewItem tvi in ic)
				{
					if (tvi.Equals(node))
					{
						found = true;
						tvi.IsExpanded = true;
						ic = tvi.Items;
						if (tvi.Equals(node)) done = true;
						break;
					}
				}

				done = (found == false && done == false);
			}
		}

		public static void SetSelectedItem( this TreeView treeView, Fusion.Engine.Frames.Frame item )
		{
			SetSelected(treeView, item);
		}

		public static void SetSelected( ItemsControl parent, Fusion.Engine.Frames.Frame child )
		{
			var currentFrame = child;
			List<Fusion.Engine.Frames.Frame> frames = new List<Fusion.Engine.Frames.Frame>();

			while (currentFrame.Parent!=null && currentFrame.Parent.Text!="Scene")
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
