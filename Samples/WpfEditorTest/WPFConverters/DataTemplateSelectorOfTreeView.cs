using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfEditorTest.ChildPanels;

namespace WpfEditorTest.WPFConverters
{
	public class DataTemplateSelectorOfTreeView : DataTemplateSelector
	{
		public DataTemplate tv1Template { get; set; }
		public DataTemplate tv2Template { get; set; }
		public DataTemplate tv3Template { get; set; }

		public override DataTemplate SelectTemplate( object item, DependencyObject container )
		{
			if (item.GetType().IsSubclassOf(typeof(UIController)))
				return tv2Template;
			else if (item is UIController.Placement)
				return tv3Template;
			else return tv1Template;
		}
	}
}
