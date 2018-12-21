using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Fusion.Engine.Frames;

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
			Height = ApplicationConfig.OptionsWindowSize; Width = ApplicationConfig.OptionsWindowSize;

			Left = int.Parse(ConfigurationManager.AppSettings.Get("DetailsPanelX"));
			Top = int.Parse(ConfigurationManager.AppSettings.Get("DetailsPanelY"));
			Visibility = (Visibility)Enum.Parse(typeof(Visibility), ConfigurationManager.AppSettings.Get("DetailsPanelVisibility"));

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Center;

            Closing += (s, e) => {Visibility= Visibility.Collapsed; e.Cancel = true; };
		}

	    public void SetSelectFrame(Frame frame)
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
