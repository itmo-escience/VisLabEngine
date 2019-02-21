using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
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
using System.Windows.Shapes;

namespace WpfEditorTest.ChildWindows
{
	/// <summary>
	/// Interaction logic for ConsoleWindow.xaml
	/// </summary>
	public partial class ConsoleWindow : Window
	{
		private string _inputString;

        public ConsoleWindow()
		{
			InitializeComponent();

			Height = double.Parse(ConfigurationManager.AppSettings.Get("ConsoleWindowHeight"));
			Width = double.Parse(ConfigurationManager.AppSettings.Get("ConsoleWindowWidth"));

			Left = double.Parse(ConfigurationManager.AppSettings.Get("ConsoleWindowX"));
			Top = double.Parse(ConfigurationManager.AppSettings.Get("ConsoleWindowY"));

			Closing += ( s, e ) => { this.Hide(); e.Cancel = true; };

            Log.AddListener(new LogRecorder());
            LogRecorder.TraceRecorded += (s, e) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    string messages = "";
                    foreach (LogMessage message in LogRecorder.GetLines())
                    {
                        messages += message.MessageText + Environment.NewLine;
                    }
                    OutputField.Text = messages;
                });
            };
        }

		private void TextBox_KeyDown( object sender, KeyEventArgs e )
		{
			if (e.Key == Key.Enter)
			{
				_inputString = InputField.Text;
				InputField.Text = string.Empty;

                Log.Message("Test: " + _inputString);
            }
		}
	}
}
