using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Fusion.Engine.Frames2;

namespace WpfEditorTest.FrameSelection
{
	internal class SelectionManager : INotifyPropertyChanged
	{

		public static SelectionManager Instance { get; } = new SelectionManager();

		private SelectionManager() { }

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsSingleElementSelected { get { return SelectedFrames.Count == 1; } }

		public List<UIComponent> SelectedFrames { get; private set; } = new List<UIComponent>();

		public event EventHandler<List<UIComponent>> FrameSelected;

		public event EventHandler<UIComponent> UIComponentUpdated;
		public event EventHandler<UIComponent> PlacementRecreated;
		public event EventHandler<ISlot> SlotUpdated;

		public void SelectFrame( List<UIComponent> frame )
		{
			foreach (UIComponent selectedFrame in SelectedFrames)
			{
				selectedFrame.PropertyChanged -= OnUIComponentUpdated;
				selectedFrame.Placement.PropertyChanged -= OnSlotUpdated;
			}
			SelectedFrames = frame;
			foreach (UIComponent selectedFrame in SelectedFrames)
			{
				selectedFrame.PropertyChanged += OnUIComponentUpdated;
				selectedFrame.Placement.PropertyChanged += OnSlotUpdated;
			}
			FrameSelected?.Invoke(this, SelectedFrames);
			OnPropertyChanged(nameof(IsSingleElementSelected));
		}

		private void OnSlotUpdated( object slot, PropertyChangedEventArgs args )
		{
			Application.Current.Dispatcher.InvokeAsync(() => SlotUpdated?.Invoke(this, (ISlot)slot));
		}

		private void OnUIComponentUpdated( object frame, PropertyChangedEventArgs args)
		{
			if ((frame as UIComponent).Placement != null)
			{
				(frame as UIComponent).Placement.PropertyChanged += OnSlotUpdated;

				if (args.PropertyName == "Placement")
				{
					Application.Current.Dispatcher.InvokeAsync(() => PlacementRecreated?.Invoke(this, (UIComponent)frame)); 
				}
			}

		    Application.Current.Dispatcher.InvokeAsync(() => UIComponentUpdated?.Invoke(this, (UIComponent)frame));
		}

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}
}