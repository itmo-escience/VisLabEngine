﻿using Fusion.Engine.Frames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WpfEditorTest.UndoRedo;

namespace WpfEditorTest
{
	public class Propsy : INotifyPropertyChanged
	{
		public Propsy( PropertyInfo prop, Frame obj )
		{
			Obj = obj;
			PropName = prop.Name;
			PropInfo = prop;
			PropType = prop.PropertyType;
			if (PropType.IsEnum)
			{
				EnumValues = Enum.GetValues(PropType).Cast<object>().ToList();
			}

			if (prop.PropertyType != typeof(string))
			{
				//_prop = Activator.CreateInstance(prop.PropertyType);
				_prop = prop.GetValue(obj);
			}
			else
			{
				if (prop.GetValue(obj) != null)
				{
					_prop = Activator.CreateInstance(prop.PropertyType, (prop.GetValue(obj) as string).ToCharArray());
				}
				else
				{
					_prop = string.Empty;
				}
			}

			Obj.PropertyChanged += ( s, e ) => {
				if (e.PropertyName == PropName)
				{
					var val = PropInfo.GetValue(Obj);
					if (!Prop.Equals(val))
					{
						_prop = PropInfo.GetValue(Obj);
						OnPropertyChanged(nameof(Prop));
					}
				}
			};
		}

		private object _prop;
		public object Prop
		{
			get => _prop;
			set
			{
				var convertedValue = Convert.ChangeType(value, PropInfo.PropertyType);
				//PropInfo.SetValue(Obj, convertedValue);
				var command = new FramePropertyChangeCommand(Obj, PropName, value);
				CommandManager.Instance.Execute(command);

				OnPropertyChanged();
			}
		}

		public Frame Obj { get; set; }

		public PropertyInfo PropInfo { get; set; }
		public Type PropType { get; set; }
		public string PropName { get; set; }

		public IList<object> EnumValues { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}
}
