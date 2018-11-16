using FEF = Fusion.Engine.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WpfEditorTest
{
	public class FramePropertyHolder : DependencyObject
	{
		public static readonly DependencyProperty PropertiesProperty;
		//public static readonly DependencyProperty PriceProperty;

		static FramePropertyHolder()
		{
			FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata();

			PropertiesProperty = DependencyProperty.Register("Properties", typeof(ObservableCollection<PropertyDependant>), typeof(FramePropertyHolder), metadata
				/*new ValidateValueCallback(ValidateValue)*/);
		}

		public FramePropertyHolder()
		{
			this.Properties = new ObservableCollection<PropertyDependant>();
		}

		private static bool ValidateValue( object value )
		{
			int currentValue = (int)value;
			if (currentValue >= 0) // если текущее значение от нуля и выше
				return true;
			return false;
		}

		public ObservableCollection<PropertyDependant> Properties
		{
			get { return (ObservableCollection<PropertyDependant>)GetValue(PropertiesProperty); }
			set { SetValue(PropertiesProperty, value); }
		}
	}

	public class PropertyDependant : DependencyObject, INotifyPropertyChanged
	{
		public static readonly DependencyProperty PropProperty;
		public static readonly DependencyProperty ObjProperty;

		static PropertyDependant()
		{
			PropProperty = DependencyProperty.Register("Prop", typeof(object), typeof(PropertyDependant));
			ObjProperty = DependencyProperty.Register("Obj", typeof(FEF.Frame), typeof(PropertyDependant));
		}

		public PropertyDependant( PropertyInfo prop, FEF.Frame obj)
		{
			if (prop.PropertyType != typeof(string))
			{
				Prop = Activator.CreateInstance(prop.PropertyType);
				Prop = prop.GetValue(obj);
			}
			else
			{
				if (prop.GetValue(obj) != null)
				{
					Prop = Activator.CreateInstance(prop.PropertyType, (prop.GetValue(obj) as string).ToCharArray());
				}
				else
				{
					Prop = String.Empty;
				}
			}
			Obj = obj;
			PropName = prop.Name;
		}

		public object Prop
		{
			get { return (object)GetValue(PropProperty); }
			set { SetValue(PropProperty, value);
				this.OnPropertyChanged("Prop");
			}
		}
		public FEF.Frame Obj
		{
			get { return (FEF.Frame)GetValue(ObjProperty); }
			set { SetValue(ObjProperty, value);
				this.OnPropertyChanged("Obj");
			}
		}

		public string PropName
		{
			get;set;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged( string changedProperty )
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}

	public class Property : INotifyPropertyChanged
	{

		public Property( PropertyInfo prop, FEF.Frame obj )
		{
			if (prop.PropertyType!=typeof(string))
			{
				Prop = Activator.CreateInstance(prop.PropertyType);
				Prop = prop.GetValue(obj);
			}
			else
			{
				if (prop.GetValue(obj) !=null)
				{
					Prop = Activator.CreateInstance(prop.PropertyType, (prop.GetValue(obj) as string).ToCharArray()); 
				}
				else
				{
					Prop = String.Empty;
				}
			}
			Obj = obj;
		}

		private object prop;
		public object Prop
		{
			get { return this.prop; }
			set
			{
				this.prop = value;
				this.OnPropertyChanged("Prop");
			}
		}
		private FEF.Frame obj;
		public FEF.Frame Obj
		{
			get { return this.obj; }
			set
			{
				this.obj = value;
				this.OnPropertyChanged("Obj");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged( string changedProperty )
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}
}
