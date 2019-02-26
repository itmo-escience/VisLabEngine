using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WpfEditorTest.ChildPanels;

namespace WpfEditorTest.WPFConverters
{
	public class TransformValueConverter : IValueConverter
	{
		UIComponent elementToUpdate;
		Matrix3x2 propertyToChange;

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if (value == null)
			{
				return targetType.ToString();
			}

			if (value.GetType().GetInterfaces().Contains(typeof(IMVVMProperty)))
			{
				elementToUpdate = ((IMVVMProperty)value).Obj;
				return null;
			}

			propertyToChange = (Matrix3x2)value;

			switch (parameter)
			{
				case "M11":
					{
						return propertyToChange.M11;
					}
				case "M12":
					{
						return propertyToChange.M12;
					}
				case "M21":
					{
						return propertyToChange.M21;
					}
				case "M22":
					{
						return propertyToChange.M22;
					}
				case "M31":
					{
						return propertyToChange.M31;
					}
				case "M32":
					{
						return propertyToChange.M32;
					}
			}

			return value.ToString();
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			switch (parameter)
			{
				case "M11":
					{
						propertyToChange.M11 = (float)value;
						break;
					}
				case "M12":
					{
						propertyToChange.M12 = (float)value;
						break;
					}
				case "M21":
					{
						propertyToChange.M21 = (float)value;
						break;
					}
				case "M22":
					{
						propertyToChange.M22 = (float)value;
						break;
					}
				case "M31":
					{
						propertyToChange.M31 = (float)value;
						break;
					}
				case "M32":
					{
						propertyToChange.M32 = (float)value;
						break;
					}
			}
			elementToUpdate.Transform = new Matrix3x2(propertyToChange.ToArray());
			return propertyToChange;
		}
	}
}
