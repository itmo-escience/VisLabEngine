using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Fusion.Engine.Frames2;

namespace WpfEditorTest.ChildWindows
{
	/// <summary>
	/// Interaction logic for FrameDetails.xaml
	/// </summary>
	public partial class SlotDetailsWindow : Window
	{
		public SlotDetailsWindow()
		{
			InitializeComponent();
			Height = double.Parse(ConfigurationManager.AppSettings.Get("SlotDetailsWindowHeight"));
			Width = ApplicationConfig.OptionsWindowSize;

			Left = double.Parse(ConfigurationManager.AppSettings.Get("SlotDetailsWindowX"));
			Top = double.Parse(ConfigurationManager.AppSettings.Get("SlotDetailsWindowY"));

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Center;

            Closing += (s, e) => {this.Hide(); e.Cancel = true; };
		}

		public void SetSelectFrame(UIController.Slot slot)
		{
			var properties = slot?.Properties;

			//var propsies = (
			//	from property in publicProperties
			//	where property.GetMethod != null && property.SetMethod != null && !property.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute")
			//	select new MVVMFrameProperty(property, slot)
			//).ToList();

			FrameDetailsControls.ItemsSource = properties?.OrderBy(p => p.Name).ToList();
		}
	}
}
