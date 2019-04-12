using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfEditorTest.WPFConverters
{
	class DictionaryConverter : IValueConverter
	{
		private Dictionary<string, IUIStyle> _styles = new Dictionary<string, IUIStyle>();
		private UIComponent _component;

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			 _component = value as UIComponent;
			if (_component.GetType().GetProperty("Style") != null)
			{
				_styles = UIStyleManager.Instance.GetStyles(_component.GetType());
			}

			return _styles.Keys.ToList();
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if (_component.GetType().GetProperty("Style") != null)
			{
				_component.GetType().GetProperty("Style").SetValue(_component, _styles[value as string]);
			}


			return _styles;
		}
	}
}
