﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfEditorTest.WPFConverters
{
	class ButtonEnableConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return ((bool)value == true) ? 0 : 1;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return ((int)value == 0) ? true : false;
		}
	}
}
