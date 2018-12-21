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
	public class DataTemplateSelectorOfProperty : DataTemplateSelector
	{
		public override DataTemplate
			SelectTemplate( object item, DependencyObject container )
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null && item != null && item is MVVMFrameProperty)
			{
				MVVMFrameProperty Prop = item as MVVMFrameProperty;



				if (Prop.PropType == typeof(Fusion.Core.Mathematics.Color))
					return
						element.FindResource("ColorProp") as DataTemplate;
				if (Prop.PropType == typeof(Boolean))
					return
						element.FindResource("BoolProp") as DataTemplate;
				if (Prop.PropType == typeof(int))
					return
						element.FindResource("IntegerProp") as DataTemplate;
				if (Prop.PropType == typeof(Single))
					return
						element.FindResource("FloatProp") as DataTemplate;
				if (Prop.PropType.IsEnum)
				{
					if (Prop.PropType.CustomAttributes.Any(ca => ca.AttributeType.Name == "FlagsAttribute"))
					{
						return
							element.FindResource("MultiEnumProp") as DataTemplate; 
					}
					else
					{
						return
							element.FindResource("EnumProp") as DataTemplate;
					}
				}
				//if (Prop.PropType == typeof(Fusion.Core.Mathematics.Vector2))
				//	return
				//		element.FindResource("VectorProp") as DataTemplate;
				return
					element.FindResource("StringProp") as DataTemplate;
			}

			return null;
		}
	}
}
