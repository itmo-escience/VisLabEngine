using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Shell;
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
        Invoker _invoker;
        LogMessageType _lowestMessageLevelToPrint;

        public ConsoleWindow(Invoker invoker)
		{
			InitializeComponent();

			Height = double.Parse(ConfigurationManager.AppSettings.Get("ConsoleWindowHeight"));
			Width = double.Parse(ConfigurationManager.AppSettings.Get("ConsoleWindowWidth"));

			Left = double.Parse(ConfigurationManager.AppSettings.Get("ConsoleWindowX"));
			Top = double.Parse(ConfigurationManager.AppSettings.Get("ConsoleWindowY"));

			LogTypeComboBox.SelectedIndex = int.Parse(ConfigurationManager.AppSettings.Get("ConsoleFilterItemIndex"));

			Closing += ( s, e ) => { this.Hide(); e.Cancel = true; };

            _invoker = invoker;

            Log.AddListener(new LogRecorder());
            LogRecorder.TraceRecorded += (s, e) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    UpdatOutputText();
                });
            };
        }

		private void TextBox_KeyDown( object sender, KeyEventArgs e )
		{
			if (e.Key == Key.Enter)
			{
                string cmd = InputField.Text;
                InputField.Text = string.Empty;
                try
                {
                    Log.Message(">{0}", cmd);
                    _invoker.Push(cmd);
                }
                catch (Exception exp)
                {
                    Log.Error(exp.Message);
                }
            }
		}

		private void LogTypeMenuItem_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			var comboBox = sender as ComboBox;
			_lowestMessageLevelToPrint = (LogMessageType)(comboBox.SelectedItem as FrameworkElement).Tag;
            UpdatOutputText();
        }

        private void UpdatOutputText()
        {
            string messages = "";
            foreach (LogMessage message in LogRecorder.GetLines())
            {
                if (message.MessageType >= _lowestMessageLevelToPrint)
                {
                    messages += message.MessageText + Environment.NewLine;
                }
            }
            OutputField.Text = messages;
            ScrollViewer.ScrollToEnd();
        }
	}
}
