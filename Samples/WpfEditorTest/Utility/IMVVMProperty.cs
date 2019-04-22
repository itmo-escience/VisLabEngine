using Fusion.Engine.Frames2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WpfEditorTest.Utility
{
	public interface IMVVMProperty : INotifyPropertyChanged
	{
		object Prop { get; set; }

		INotifyPropertyChanged Obj { get; set; }

		PropertyInfo PropInfo { get; set; }
		Type PropType { get; set; }
		string PropName { get; set; }

		IList<object> EnumValues { get; set; }

		List<MVVMComponentProperty> PropertyProps { get; set; }
	}
}
