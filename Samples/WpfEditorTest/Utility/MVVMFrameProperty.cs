using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using WpfEditorTest.Commands;
using WpfEditorTest.Utility;

namespace WpfEditorTest.Utility
{
	public class MVVMComponentProperty : IMVVMProperty
	{
		public MVVMComponentProperty( PropertyInfo prop, INotifyPropertyChanged obj )
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

			Obj.PropertyChanged += ( s, e ) => ChangeProperty(e);

			if (PropType.IsClass && PropType != typeof(string) && PropType != typeof(object))
			{
				var propNotified = _prop as INotifyPropertyChanged;

				var publicProperties = PropType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

				PropertyProps = (
					from property in publicProperties
					where property.GetMethod != null && property.SetMethod != null && property.SetMethod.IsPublic && !property.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute")
					select new MVVMComponentProperty(property, propNotified)
				).ToList();

				propNotified.PropertyChanged += ( s, e ) => {
					OnPropertyChanged(nameof(_prop));
				};
			}
		}

		public void ChangeProperty( PropertyChangedEventArgs e )
		{
			if (e.PropertyName == PropName)
			{
				var val = PropInfo.GetValue(Obj);
				if (Prop != null && !Prop.Equals(val))
				{
					_prop = PropInfo.GetValue(Obj);
					OnPropertyChanged(nameof(Prop));
				}
			}
		}

		private object _prop;
		public virtual object Prop
		{
			get => _prop;
			set
            {
                if (value == null) return;

				var convertedValue = Convert.ChangeType(value, PropInfo.PropertyType);

				IEditorCommand command=null;
				if (Obj.GetType().GetInterfaces().Contains(typeof(IUIComponent)))
				{
					command = new UIComponentPropertyChangeCommand(Obj as IUIComponent, PropName, value);
				}
				else if (Obj.GetType().GetInterfaces().Contains(typeof(ISlot)))
				{
					command = new SlotPropertyChangeCommand((Obj as ISlot).Component, PropName, value);
				}

				if (command!=null)
				{
					CommandManager.Instance.Execute(command); 
				}
				else
				{
					PropInfo.SetValue(Obj, convertedValue);
					OnPropertyChanged(nameof(Prop));
				}

				//OnPropertyChanged();
			}
		}

		public INotifyPropertyChanged Obj { get; set; }

		public PropertyInfo PropInfo { get; set; }
		public Type PropType { get; set; }
		public string PropName { get; set; }

		public IList<object> EnumValues { get; set; }

		public List<MVVMComponentProperty> PropertyProps { get; set; } = new List<MVVMComponentProperty>();

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}
}
