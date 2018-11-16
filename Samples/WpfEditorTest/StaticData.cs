using Fusion.Engine.Frames;
using FusionUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfEditorTest
{
	public static class StaticData
	{
		public static int OptionsWindowSize = 335;

		public static ObservableCollection<Type> availableFrameElements
		{
			get
			{
				ObservableCollection<Type> collection = new ObservableCollection<Type>();
				List<Type> types = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
									from assemblyType in domainAssembly.GetTypes()
									where typeof(Frame).IsAssignableFrom(assemblyType)
									select assemblyType).ToList();
				foreach (var type in types)
				{
					collection.Add(type);
				}
				return collection;
			}
		}
	}
}
