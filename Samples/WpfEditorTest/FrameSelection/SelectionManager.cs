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
	internal class SelectionManager
	{
		public static SelectionManager Instance { get; } = new SelectionManager();

		private SelectionManager() { }

		public List<UIComponent> SelectedFrames { get; private set; } = new List<UIComponent>();

		public event EventHandler<List<UIComponent>> FrameSelected;

		public event EventHandler<UIComponent> FrameUpdated;

		public void SelectFrame( List<UIComponent> frame )
		{
			foreach (UIComponent selectedFrame in SelectedFrames)
			{
				selectedFrame.PropertyChanged -= OnFrameUpdated;
			}
			SelectedFrames = frame;
			foreach (UIComponent selectedFrame in SelectedFrames)
			{
				selectedFrame.PropertyChanged += OnFrameUpdated;
			}
			FrameSelected?.Invoke(this, SelectedFrames);
		}

		private void OnFrameUpdated( object frame, PropertyChangedEventArgs args)
		{
		    Application.Current.Dispatcher.Invoke(() => FrameUpdated?.Invoke(this, (UIComponent)frame));
		}
	}
}