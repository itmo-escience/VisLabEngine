using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Containers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WpfEditorTest.Utility;

namespace WpfEditorTest.WPFConverters
{
	public class FixatorsValueConverter : IValueConverter
	{
		object elementToUpdate;
		Fixators propertyToChange;

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

			propertyToChange = (Fixators)value;

			switch (parameter)
			{
				case "Top":
					{
						return propertyToChange.Top;
					}
				case "Bottom":
					{
						return propertyToChange.Bottom;
					}
				case "Left":
					{
						return propertyToChange.Left;
					}
				case "Right":
					{
						return propertyToChange.Right;
					}
			}

			return value.ToString();
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			switch (parameter)
			{
				case "Top":
					{
						return propertyToChange.Top = (float)value;
					}
				case "Bottom":
					{
						return propertyToChange.Bottom = (float)value;
					}
				case "Left":
					{
						return propertyToChange.Left = (float)value;
					}
				case "Right":
					{
						return propertyToChange.Right = (float)value;
					}
			}
			//TODO
			//Getting Transform?
			//elementToUpdate.Placement.Transform() = new Matrix3x2(propertyToChange.ToArray());
			return propertyToChange;
		}
	}
}
