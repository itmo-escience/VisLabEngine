using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Fusion.Engine.Frames2;

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for FrameDetails.xaml
	/// </summary>
	public partial class FrameDetails : Window
	{
		public FrameDetails()
		{
			InitializeComponent();
			Height = int.Parse(ConfigurationManager.AppSettings.Get("DetailsPanelHeight"));
			Width = ApplicationConfig.OptionsWindowSize;

			Left = int.Parse(ConfigurationManager.AppSettings.Get("DetailsPanelX"));
			Top = int.Parse(ConfigurationManager.AppSettings.Get("DetailsPanelY"));

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Center;

            Closing += (s, e) => {this.Hide(); e.Cancel = true; };
		}

	    public void SetSelectFrame(UIComponent frame)
	    {
	        var publicProperties = frame.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

	        var propsies = (
	            from property in publicProperties
	            where property.GetMethod != null && property.SetMethod != null && !property.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute")
	            select new MVVMFrameProperty(property, frame)
	        ).ToList();

	        FrameDetailsControls.ItemsSource = propsies.OrderBy(p => p.PropName).ToList();
        }
	}
}
