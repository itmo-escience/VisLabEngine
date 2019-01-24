using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Fusion.Engine.Frames;

namespace WpfEditorTest.FrameSelection
{
	internal class SelectionManager
	{
		public static SelectionManager Instance { get; } = new SelectionManager();

		private SelectionManager() { }

		public List<Frame> SelectedFrames { get; private set; } = new List<Frame>();

		public event EventHandler<List<Frame>> FrameSelected;

		public event EventHandler<Frame> FrameUpdated;

		public void SelectFrame( List<Frame> frame )
		{
			foreach (Frame selectedFrame in SelectedFrames)
			{
				selectedFrame.PropertyChanged -= OnFrameUpdated;
			}
			SelectedFrames = frame;
			foreach (Frame selectedFrame in SelectedFrames)
			{
				selectedFrame.PropertyChanged += OnFrameUpdated;
			}
			FrameSelected?.Invoke(this, SelectedFrames);
		}

		private void OnFrameUpdated( object frame, PropertyChangedEventArgs args)
		{
		    Application.Current.Dispatcher.Invoke(() => FrameUpdated?.Invoke(this, (Frame)frame));
		}
	}
}