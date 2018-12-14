using Fusion.Engine.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfEditorTest.FrameSelection;

namespace WpfEditorTest.UndoRedo
{
	public class SelectFrameCommand : IEditorCommand
	{
		private Frame _frame;
		private Frame _oldFrame;

		public SelectFrameCommand( Frame frame)
		{
			this._frame = frame;
			this._oldFrame = SelectionManager.Instance.SelectedFrame;
		}

		public void Do()
		{
			SelectionManager.Instance.SelectFrame(_frame);
		}

		public void Undo()
		{
			SelectionManager.Instance.SelectFrame(_oldFrame);
		}
	}
}
