using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Fusion.Engine.Frames2;

namespace WpfEditorTest.Utility
{
	/// <summary>
	/// Interaction logic for FrameDetails.xaml
	/// </summary>
	public partial class FrameDetails : Window
	{
		public FrameDetails()
		{
			InitializeComponent();
			Height = double.Parse(ConfigurationManager.AppSettings.Get("DetailsPanelHeight"));
			Width = ApplicationConfig.OptionsWindowSize;

			Left = double.Parse(ConfigurationManager.AppSettings.Get("DetailsPanelX"));
			Top = double.Parse(ConfigurationManager.AppSettings.Get("DetailsPanelY"));

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Center;

            Closing += (s, e) => {this.Hide(); e.Cancel = true; };
		}

	    public void SetSelectFrame(UIComponent component)
	    {
			//      var publicProperties = frame.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			//var propsies = (
			//	from property in publicProperties
			//	where property.GetMethod != null && property.SetMethod != null && property.SetMethod.IsPublic && !property.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute")
			//	select new MVVMComponentProperty(property, frame)
			//).ToList();

			//publicProperties = frame.Placement.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			//propsies.AddRange((
			//	from property in publicProperties
			//	where property.GetMethod != null && property.SetMethod != null && property.SetMethod.IsPublic && !property.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute")
			//	select new MVVMComponentProperty(property, frame.Placement)
			//).ToList());


			DetailsScroll.DataContext = new MVVMComponentDetails(component);
			DetailsScroll.Visibility = Visibility.Visible;
			Title = component.GetType().Name + " details";

		}
	}
}
