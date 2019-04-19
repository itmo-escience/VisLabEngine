using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Utils;

namespace WpfEditorTest.FrameSelection
{
	internal class SelectionManager : INotifyPropertyChanged
	{

		public static SelectionManager Instance { get; } = new SelectionManager();

		private SelectionManager() { }

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsSingleElementSelected { get { return SelectedFrames.Count == 1; } }

		public List<IUIComponent> SelectedFrames { get; private set; } = new List<IUIComponent>();

		public event EventHandler<List<IUIComponent>> FrameSelected;

		public event EventHandler<IUIComponent> UIComponentUpdated;
		public event EventHandler<IUIComponent> PlacementRecreated;
		public event EventHandler<ISlot> SlotUpdated;

		public void SelectFrame( List<IUIComponent> frame )
		{
			foreach (IUIComponent selectedFrame in SelectedFrames)
			{
				selectedFrame.PropertyChanged -= OnUIComponentUpdated;
				selectedFrame.Placement.PropertyChanged -= OnSlotUpdated;
			}
			SelectedFrames = frame;
			foreach (IUIComponent selectedFrame in SelectedFrames)
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
			if ((frame as IUIComponent).Placement != null)
			{
				(frame as IUIComponent).Placement.PropertyChanged += OnSlotUpdated;

				if (args.PropertyName == "Placement")
				{
					Application.Current.Dispatcher.InvokeAsync(() => PlacementRecreated?.Invoke(this, (IUIComponent)frame)); 
				}
			}

		    Application.Current.Dispatcher.InvokeAsync(() => UIComponentUpdated?.Invoke(this, (IUIComponent)frame));
		}

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
		}
	}
}