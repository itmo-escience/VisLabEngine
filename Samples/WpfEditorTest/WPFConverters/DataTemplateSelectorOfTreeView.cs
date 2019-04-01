using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WpfEditorTest.ChildPanels;

namespace WpfEditorTest.WPFConverters
{
	public class DataTemplateSelectorOfTreeView : DataTemplateSelector
	{
		public DataTemplate tvContainerTemplate { get; set; }
		public DataTemplate tvControllerTemplate { get; set; }
		public DataTemplate tvComponentTemplate { get; set; }

		public override DataTemplate SelectTemplate( object item, DependencyObject container )
		{
			if (item.GetType().IsSubclassOf(typeof(UIController)))
				return tvControllerTemplate;

		    if (item is UIController.Slot)
				return tvComponentTemplate;

		    if (item is UIContainer ctr)
		    {
                BindingOperations.EnableCollectionSynchronization(ctr.Children, ctr.ChildrenAccessLock);
		    }

			return tvContainerTemplate;
		}
	}
}
