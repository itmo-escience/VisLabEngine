using Fusion.Engine.Frames;
using FusionUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfEditorTest
{
	public static class ApplicationConfig
	{
		public static int OptionsWindowSize = 335;
		public static string TemplatesPath = Path.GetFullPath(Path.Combine(Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName,
			"..\\..\\..\\FramesXML"));
		public static string BaseTitle = "InterfaceEditor";
		public static string BaseSceneName = "NewScene";

		public static int DefaultVisualGridSizeMultiplier = 1;
		public static double DefaultGridLinesThickness = 1;
		public static SolidColorBrush DefaultGridLinesBrush = new SolidColorBrush(Colors.LightSteelBlue) { Opacity=0.5d };


	}
}
