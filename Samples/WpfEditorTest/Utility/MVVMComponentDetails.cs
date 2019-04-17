using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Controllers;
using Fusion.Engine.Frames2.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfEditorTest.Utility
{
	public class MVVMComponentDetails : INotifyPropertyChanged
	{
		public string SlotName { get; set; }
		public Visibility NeedStyle { get; set; } = Visibility.Collapsed;
		public string StartStyle { get; set; } = null;
		public List<MVVMComponentProperty> SlotProps { get; set; } = new List<MVVMComponentProperty>();
		public IUIComponent Component { get; set; }
		public List<MVVMComponentProperty> ComponentProps { get; set; } = new List<MVVMComponentProperty>();

		public event PropertyChangedEventHandler PropertyChanged;

		public MVVMComponentDetails(IUIComponent component)
		{
			Component = component;
			SlotName = component.Placement.GetType().Name;
			NeedStyle = component.GetType().GetProperty("Style") != null? Visibility.Visible: Visibility.Collapsed;

			if (NeedStyle == Visibility.Visible)
			{
				StartStyle = (component.GetType().GetProperty("Style").GetValue(component) as IUIStyle).Name;
			}

			var publicProperties = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			ComponentProps = (
				from property in publicProperties
				where property.GetMethod != null && property.SetMethod != null && property.SetMethod.IsPublic && !property.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute")
				select new MVVMComponentProperty(property, component)
			).ToList();

			publicProperties = component.Placement.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			SlotProps = (
				from property in publicProperties
				where property.GetMethod != null && property.SetMethod != null && property.SetMethod.IsPublic && !property.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute")
				select new MVVMComponentProperty(property, component.Placement)
			).ToList();

				
		}
	}
}
