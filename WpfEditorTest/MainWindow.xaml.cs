using Fusion.Engine.Common;
using GISTest;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace WpfEditorTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			var engine = new Game("TestGame");

			engine.GameServer		= new CustomGameServer(engine);
			engine.GameClient		= new CustomGameClient(engine);
			engine.GameInterface	= new CustomGameInterface(engine);

			engine.RenderSystem.StereoMode = Fusion.Engine.Graphics.StereoMode.WpfEditor;
			engine.RenderSystem.Width	= 1920;
			engine.RenderSystem.Height	= 1080;

			engine.LoadConfiguration("Config.ini");

			DxElem.Renderer = engine;

			Directory.SetCurrentDirectory(@"E:\GitHub\VisLabEngine\Samples\GISTest\bin\x64\Debug");
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			DxElem.HandleInput(this);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DxElem.Renderer.Invoker.Push("GlobeToSpb");
		}
	}
}
