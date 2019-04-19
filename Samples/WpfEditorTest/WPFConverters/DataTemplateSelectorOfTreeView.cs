using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Frames2.Controllers;
using Fusion.Engine.Frames2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WpfEditorTest.Utility;

namespace WpfEditorTest.WPFConverters
{
	public class DataTemplateSelectorOfTreeView : DataTemplateSelector
	{
		public DataTemplate tvContainerTemplate { get; set; }
		public DataTemplate tvControllerTemplate { get; set; }
		public DataTemplate tvComponentTemplate { get; set; }

		public override DataTemplate SelectTemplate( object item, DependencyObject container )
		{
			var content = (item as ISlot).Component;
			if (content is UIController)
				return tvControllerTemplate;

			if (content is IUIModifiableContainer<ISlot> ctr)
			{
				//BindingOperations.EnableCollectionSynchronization(ctr.Slots, ctr.ChildrenAccessLock);
				return tvContainerTemplate;
			}

			//if (content is ISlot)
				return tvComponentTemplate;

		}

		public static class GenericHelper
		{
			internal static bool IsSubclassOfRawGeneric( Type generic, Type toCheck )
			{
				while (toCheck != null && toCheck != typeof(object))
				{
					var cur = toCheck.IsGenericType ? toCheck.GenericTypeArguments.FirstOrDefault() : toCheck;
					if (cur != null && cur.GetInterfaces().Contains(typeof(IControllerSlot)))
					{
						return true;
					}
					toCheck = toCheck.BaseType;
				}
				return false;
			}
		}
	}
}
