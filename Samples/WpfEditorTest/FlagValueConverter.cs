using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfEditorTest
{
	public class FlagValueConverter : IMultiValueConverter
	{
		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
		{
			return null/*values*/;
		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
		{
			return null/*new object[] { value }*/;
		}
	}

	public class FlagEnumConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return Enum.GetValues(value.GetType());
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return value;
		}
	}
}
