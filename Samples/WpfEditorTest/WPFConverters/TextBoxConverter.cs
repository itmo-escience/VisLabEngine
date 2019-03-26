using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Fusion.Engine.Frames;

namespace WpfEditorTest.WPFConverters
{
	public class TextBoxConverter : IMultiValueConverter
	{
		private FrameAnchor _target;



		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
		{
			if (values == null)
			{
				return targetType.ToString();
			}

			return values[0].ToString();
		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
		{
			if (value == null)
			{
				return new object[] { value };
			}

			return new object[] { value };
		}
	}
}
