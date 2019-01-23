﻿using System;
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

			if (value.GetType() == typeof(Fusion.Core.Mathematics.Color))
			{
				Fusion.Core.Mathematics.Color fuCol = (Fusion.Core.Mathematics.Color)value;
				System.Windows.Media.Color col = new System.Windows.Media.Color();
				col.A = fuCol.A;
				col.R = fuCol.R;
				col.G = fuCol.G;
				col.B = fuCol.B;

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
			if (value.GetType() == typeof(System.Windows.Media.Color))
			{
				System.Windows.Media.Color col = (System.Windows.Media.Color)value;
				Fusion.Core.Mathematics.Color fuCol = new Fusion.Core.Mathematics.Color(col.R,col.G,col.B,col.A);
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
