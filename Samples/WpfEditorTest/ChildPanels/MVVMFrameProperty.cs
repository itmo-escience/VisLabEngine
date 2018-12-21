using Fusion.Engine.Frames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WpfEditorTest.UndoRedo;

namespace WpfEditorTest.ChildPanels
{
	public class MVVMFrameProperty : INotifyPropertyChanged
	{
		public MVVMFrameProperty( PropertyInfo prop, Frame obj )
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
<<<<<<< HEAD:Samples/WpfEditorTest/Property.cs
					    _prop = PropInfo.GetValue(Obj);
					    OnPropertyChanged(nameof(Prop));
                    }
=======
						_prop = PropInfo.GetValue(Obj);
						OnPropertyChanged(nameof(Prop));
					}
>>>>>>> InterfaceEditor:Samples/WpfEditorTest/ChildPanels/MVVMFrameProperty.cs
				}
			};
		}

		private object _prop;
		public object Prop
		{
			get => _prop;
			set
			{
<<<<<<< HEAD:Samples/WpfEditorTest/Property.cs
                var convertedValue = Convert.ChangeType(value, PropInfo.PropertyType);
=======
				var convertedValue = Convert.ChangeType(value, PropInfo.PropertyType);
>>>>>>> InterfaceEditor:Samples/WpfEditorTest/ChildPanels/MVVMFrameProperty.cs
				//PropInfo.SetValue(Obj, convertedValue);
				var command = new FramePropertyChangeCommand(Obj, PropName, value);
				CommandManager.Instance.Execute(command);

<<<<<<< HEAD:Samples/WpfEditorTest/Property.cs
			    OnPropertyChanged();
            }
=======
				OnPropertyChanged();
			}
>>>>>>> InterfaceEditor:Samples/WpfEditorTest/ChildPanels/MVVMFrameProperty.cs
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
