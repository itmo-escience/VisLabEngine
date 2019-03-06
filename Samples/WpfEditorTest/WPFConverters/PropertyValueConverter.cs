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
	public class PropertyValueConverter : IValueConverter
	{
		private FrameAnchor _target;

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if (value==null)
			{
				return targetType.ToString();
			}

			if (value.GetType() == typeof(Fusion.Core.Mathematics.Color4))
			{
				Fusion.Core.Mathematics.Color4 fuCol = (Fusion.Core.Mathematics.Color4)value;
				System.Windows.Media.Color col = new System.Windows.Media.Color();
				col.A = (byte)(fuCol.Alpha * 255.0f);
				col.R = (byte)(fuCol.Red * 255.0f);
				col.G = (byte)(fuCol.Green * 255.0f);
				col.B = (byte)(fuCol.Blue * 255.0f);

				return col;
			}
			if (value.GetType() == typeof(Fusion.Core.Mathematics.Vector2))
			{
				switch (parameter)
				{
					case "X":
						{
							return ((Fusion.Core.Mathematics.Vector2)value).X;
						}
					case "Y":
						{
							return ((Fusion.Core.Mathematics.Vector2)value).Y;
						}
				}
			}

			if (value.GetType() == typeof(FrameAnchor))
			{
				FrameAnchor mask = (FrameAnchor)parameter;
				this._target = (FrameAnchor)value;
				return (mask & this._target) != 0; 
			}

			return value.ToString();
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if (value == null)
			{
				return value;
			}

			if (value.GetType() == typeof(System.Windows.Media.Color))
			{
				System.Windows.Media.Color col = (System.Windows.Media.Color)value;
				Fusion.Core.Mathematics.Color4 fuCol = new Fusion.Core.Mathematics.Color4(col.R / 255.0f, col.G / 255.0f, col.B / 255.0f, col.A / 255.0f);
				return fuCol;
			}

			switch (parameter)
			{
				case "X":
					{
						return new Fusion.Core.Mathematics.Vector2((float)value, 0);
					}
				case "Y":
					{
						return new Fusion.Core.Mathematics.Vector2(0, (float)value);
					}
			}

			if (parameter!=null && parameter.GetType() == typeof(FrameAnchor))
			{
				this._target ^= (FrameAnchor)parameter;
				return this._target; 
			}

			return value;
		}
	}
}
