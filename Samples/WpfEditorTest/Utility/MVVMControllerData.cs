using Fusion.Engine.Frames2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WpfEditorTest.ChildPanels;

namespace WpfEditorTest.Utility
{
    public class MVVMControllerData : INotifyPropertyChanged
	{
		public List<ControllerSlotData> Slots { get; set; } = new List<ControllerSlotData>();
		public List<UIController.Slot> SlotValues = new List<UIController.Slot>();
		public Dictionary<ControllerSlotData, UIController.Slot> SlotAssotiativeDictionary = new Dictionary<ControllerSlotData, UIController.Slot>();

		public MVVMControllerData( UIController controller )
		{
			foreach (var slot in controller.Slots)
			{
				var slotData = new ControllerSlotData(slot, controller);
				Slots.Add(slotData);
				SlotValues.Add(slot);
				SlotAssotiativeDictionary.Add(slotData, slot);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}

	public class ControllerStateData : INotifyPropertyChanged
	{
		private ObservableCollection<string> _availableProperties = new ObservableCollection<string>();

		public string Name { get; set; }
		public UIController.Slot Slot { get; set; }
		public ObservableCollection<string> AvailableProperties { get; set; }
		public ObservableCollection<ControllerSlotProperty> Properties { get; set; } = new ObservableCollection<ControllerSlotProperty>();

		public ControllerStateData( UIController.State state, UIController.Slot slot, UIController controller )
		{
			Name = state.Name;
			Slot = slot;
			foreach (var property in Slot.Properties)
			{
				var propertyData = new ControllerSlotProperty(Slot, property, controller);
				Properties.Add(propertyData);
			}
			RecalculatePublicProperties();

			Slot.Properties.CollectionChanged += (s, e) =>{
				Properties.Clear();
				foreach (var property in Slot.Properties)
				{
					var propertyData = new ControllerSlotProperty(Slot, property, controller);
					Properties.Add(propertyData);
				}

				RecalculatePublicProperties();
				OnPropertyChanged(nameof(AvailableProperties));
			};

			void RecalculatePublicProperties()
			{
				var publicProperties = Slot.Component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var propsies = (
					from prop in publicProperties
					where prop.GetMethod != null && prop.SetMethod != null && !prop.CustomAttributes.Any(ca => ca.AttributeType.Name == "XmlIgnoreAttribute")
						&& !Slot.Properties.Any(p => p.Name == prop.Name)
					select prop.Name
				).ToList();
				AvailableProperties = new ObservableCollection<string>(propsies);
			}
		}

		public override string ToString() => Name;

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}

	public class ControllerSlotProperty : INotifyPropertyChanged
	{
		public string Name { get; set; }
		public UIController.Slot Slot { get; set; }
		public UIController.PropertyValue PropertyValue { get; set; }

		public ControllerSlotProperty( UIController.Slot slot, UIController.PropertyValue propertyValue, UIController controller )
		{
			Name = propertyValue.Name;
			Slot = slot;
			PropertyValue = propertyValue;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}

	public class ControllerSlotData : INotifyPropertyChanged
	{
		public UIController.Slot Slot { get; set; }
		public List<ControllerStateData> States { get; set; } = new List<ControllerStateData>();
		public List<UIController.State> StateValues = new List<UIController.State>();
		public Dictionary<ControllerStateData, UIController.State> StateAssotiativeDictionary = new Dictionary<ControllerStateData, UIController.State>();

		public string Name { get; set; }

		public ControllerSlotData( UIController.Slot slot, UIController controller )
		{
			Name = slot.Name;
			Slot = slot;

			foreach (var state in controller.States)
			{
				var stateData = new ControllerStateData(state, slot, controller);
				States.Add(stateData);
				StateValues.Add(state);
				StateAssotiativeDictionary.Add(stateData, state);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}
}
