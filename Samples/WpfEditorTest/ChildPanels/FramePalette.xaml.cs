using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfEditorTest.Utility
{
	/// <summary>
	/// Interaction logic for FramePalette.xaml
	/// </summary>
	public partial class FramePalette : Window
	{
		private string selectedFrameTemplate;

		public Point PreviousMouseLocation { get; set; }
		public Transform PreviousTransform { get; set; }
		public bool MousePressed { get; set; }

		private Border templateHolder;
		private Brush templateHolderInitColor;
		private Brush templateHolderSelectedColor;

		public string SelectedFrameTemplate
		{
			get => selectedFrameTemplate;
			set {
				selectedFrameTemplate = value;
				this.templateHolder.Background = selectedFrameTemplate != null ? templateHolderSelectedColor : templateHolderInitColor;
			}
		}

		public FramePalette()
		{
			InitializeComponent();

			Height = double.Parse(ConfigurationManager.AppSettings.Get("PalettePanelHeight"));
			Width = ApplicationConfig.OptionsWindowSize;

			Left = double.Parse(ConfigurationManager.AppSettings.Get("PalettePanelX"));
			Top = double.Parse(ConfigurationManager.AppSettings.Get("PalettePanelY"));

			this.HorizontalAlignment = HorizontalAlignment.Right;
			this.VerticalAlignment = VerticalAlignment.Top;

			templateHolderSelectedColor = new LinearGradientBrush()
			{
				GradientStops = new GradientStopCollection {
					new GradientStop(Colors.Green,0.0d),
					new GradientStop(Colors.LightGreen,0.5d),
					new GradientStop(Colors.Green,1.0d),
				},
				StartPoint = new Point(0.5, 0),
				EndPoint = new Point(0.5,1),
			};

			Closing += ( s, e ) => { this.Hide(); e.Cancel = true; };
		}


		private void Border_MouseDown_1( object sender, MouseButtonEventArgs e )
		{
			if(this.templateHolder!=null)
				this.templateHolder.Background = templateHolderInitColor;

			this.templateHolder = sender as Border;
			this.templateHolderInitColor = templateHolder.Background;
			this.SelectedFrameTemplate = (string)this.templateHolder.Tag;

			if (SelectedFrameTemplate != null)
			{
				DataObject dataObject = new DataObject();
				dataObject.SetData(DataFormats.StringFormat, SelectedFrameTemplate);
				DragDrop.DoDragDrop(templateHolder,
									 dataObject,
									 DragDropEffects.Move);

				this.SelectedFrameTemplate = null;
			}
		}

		private void Border_MouseDown_2( object sender, MouseButtonEventArgs e )
		{
			if (this.templateHolder != null)
				this.templateHolder.Background = templateHolderInitColor;

			this.templateHolder = sender as Border;
			this.templateHolderInitColor = templateHolder.Background;

			if (this.templateHolder.Tag != null)
			{
				DataObject dataObject = new DataObject();
				dataObject.SetData(DataFormats.FileDrop, (Type)this.templateHolder.Tag);
				DragDrop.DoDragDrop(templateHolder,
									 dataObject,
									 DragDropEffects.Move);

				this.SelectedFrameTemplate = null;
			}
		}

		private void Window_Drop( object sender, DragEventArgs e )
		{
			if (e.Data.GetDataPresent(DataFormats.StringFormat))
			{
				string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
				if (!string.IsNullOrEmpty(dataString))
				{
					this.SelectedFrameTemplate = null;
				}
			}
		}
	}
}
