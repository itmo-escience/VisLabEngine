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
        private Invoker _invoker;
        private LogMessageType _lowestMessageLevelToPrint;
        private LogMessage _lastPrintedMessage;

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
                Application.Current.Dispatcher.InvokeAsync(AddLastLogLines);
            };

            UpdateOutputText();
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
			var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configFile.AppSettings.Settings["ConsoleFilterItemIndex"].Value = LogTypeComboBox.SelectedIndex.ToString();
			configFile.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);


			UpdateOutputText();
        }

        private void AddLastLogLines()
        {
            if (!LogRecorder.GetLines().Contains(_lastPrintedMessage))
            {
                UpdateOutputText();
                return;
            }

            var notPrintedMessages = LogRecorder.GetLines().SkipWhile(mes => mes != _lastPrintedMessage).ToList();
            Paragraph paragraph = new Paragraph();
            foreach (LogMessage message in notPrintedMessages)
            {
                if (message.MessageType >= _lowestMessageLevelToPrint)
                {
                    var text = new Run(message.MessageText + Environment.NewLine);
                    switch (message.MessageType)
                    {
                        case LogMessageType.Debug:
                            text.Foreground = Brushes.DarkGray;
                            text.Background = Brushes.Black;
                            break;
                        case LogMessageType.Verbose:
                            text.Foreground = Brushes.Gray;
                            text.Background = Brushes.Black;
                            break;
                        case LogMessageType.Information:
                            text.Foreground = Brushes.White;
                            text.Background = Brushes.Black;
                            break;
                        case LogMessageType.Warning:
                            text.Foreground = Brushes.Yellow;
                            text.Background = Brushes.Black;
                            break;
                        case LogMessageType.Error:
                            text.Foreground = Brushes.Red;
                            text.Background = Brushes.Black;
                            break;
                        case LogMessageType.Fatal:
                            text.Foreground = Brushes.Black;
                            text.Background = Brushes.Red;
                            break;
                    }
                    paragraph.Inlines.Add(text);
                }
            }

            _lastPrintedMessage = notPrintedMessages.LastOrDefault();
            if (paragraph.Inlines.Count > 0)
            {
                OutputField.Document.Blocks.Add(paragraph);
                ScrollViewer.ScrollToEnd(); 
            }     
        }

        private void UpdateOutputText()
        {
			Paragraph paragraph = new Paragraph();
            foreach (LogMessage message in LogRecorder.GetLines())
            {
                if (message.MessageType >= _lowestMessageLevelToPrint)
                {
					var text = new Run(message.MessageText + Environment.NewLine);
					switch (message.MessageType)
					{
						case LogMessageType.Debug:
							text.Foreground = Brushes.DarkGray;
							text.Background = Brushes.Black;
							break;
						case LogMessageType.Verbose:
							text.Foreground = Brushes.Gray;
							text.Background = Brushes.Black;
							break;
						case LogMessageType.Information:
							text.Foreground = Brushes.White;
							text.Background = Brushes.Black;
							break;
						case LogMessageType.Warning:
							text.Foreground = Brushes.Yellow;
							text.Background = Brushes.Black;
							break;
						case LogMessageType.Error:
							text.Foreground = Brushes.Red;
							text.Background = Brushes.Black;
							break;
						case LogMessageType.Fatal:
							text.Foreground = Brushes.Black;
							text.Background = Brushes.Red;
							break;
					}
					paragraph.Inlines.Add(text);
				}
            }

			OutputField.Document.Blocks.Clear();
			OutputField.Document.Blocks.Add(paragraph);
            _lastPrintedMessage = LogRecorder.GetLines().LastOrDefault();
            ScrollViewer.ScrollToEnd();
        }
	}
}
