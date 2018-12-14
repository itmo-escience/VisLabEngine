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

		public Frame SelectedFrame { get; private set; }

		public event EventHandler<Frame> FrameSelected;

		public event EventHandler<Frame> FrameDeselected;

		public void SelectFrame( Frame frame )
		{
			SelectedFrame = frame;
			FrameSelected?.Invoke(this, SelectedFrame);
		}

		public void DeselectFrame()
		{
			FrameDeselected?.Invoke(this, SelectedFrame);
			SelectedFrame = null;
		}
	}
}
