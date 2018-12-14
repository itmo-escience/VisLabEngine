using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Frames;

namespace WpfEditorTest.FrameSelection
{
	internal class SelectionManager
	{
		public static SelectionManager Instance { get; } = new SelectionManager();

		private SelectionManager()
		{
		}

		public List<Frame> SelectedFrames { get; private set; } = new List<Frame>();

		public event EventHandler<List<Frame>> FrameSelected;

		public event EventHandler<List<Frame>> FrameDeselected;

		public void SelectFrame( List<Frame> frame )
		{
				SelectedFrames = frame;
				FrameSelected?.Invoke(this, SelectedFrames);
		}

		public void DeselectFrame()
		{
			FrameDeselected?.Invoke(this, SelectedFrames);
			SelectedFrames = null;
		}
	}
}
