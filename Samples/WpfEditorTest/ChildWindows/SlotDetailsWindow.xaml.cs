using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Controllers;
using WpfEditorTest.Utility;

namespace WpfEditorTest.ChildWindows
{
	/// <summary>
	/// Interaction logic for FrameDetails.xaml
	/// </summary>
	public partial class SlotDetailsWindow : Window
	{
		private UIController<IControllerSlot> selectedController;

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

		public void SetSelectFrame( UIController<IControllerSlot> controller )
		{
			if (controller==null)
			{
				selectedController = null;
				ControllerDetailsControls.ItemsSource = null;
				return;
			}
			selectedController = controller;
			//var data = new MVVMControllerData(controller);


			//ControllerDetailsControls.ItemsSource = data.Slots;

		}

		private void ButtonRemove_Click( object sender, RoutedEventArgs e )
		{
			//ControllerSlotProperty MVVMSlot = (sender as Button).Tag as ControllerSlotProperty;

			//MVVMSlot.Slot.Properties.Remove(MVVMSlot.Slot.Properties.Where(p => p.Name == MVVMSlot.PropertyValue.Name).FirstOrDefault());
		}

		private void ButtonAdd_Click( object sender, RoutedEventArgs e )
		{
			Button button = (sender as Button);
			var panel = VisualTreeHelper.GetParent(button);
			var box = VisualTreeHelper.GetChild(panel,1) as ComboBox;
			IControllerSlot slot = (sender as Button).Tag as IControllerSlot;

			var prop = slot.Component.GetType().GetProperty(box.Text);

			if (prop!=null)
			{
				var value = prop.GetValue(slot.Component);
				//slot.Properties.Add(new PropertyValueStates(box.Text, value));
			}
		}
	}
}
