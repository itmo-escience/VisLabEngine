using System.Linq;
using System.Reflection;
using System.Windows;
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
			Height = StaticData.OptionsWindowSize; Width = StaticData.OptionsWindowSize;

			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Center;

            Closing += (s, e) => e.Cancel = true;
		}

	    public void SetSelectFrame(Frame frame)
	    {
	        var publicProperties = frame.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

	        var propsies = (
	            from property in publicProperties
	            where property.GetMethod != null && property.SetMethod != null && !property.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute")
	            select new Propsy(property, frame)
	        ).ToList();

	        FrameDetailsControls.ItemsSource = propsies.OrderBy(p => p.PropName).ToList();
        }
	}
}
