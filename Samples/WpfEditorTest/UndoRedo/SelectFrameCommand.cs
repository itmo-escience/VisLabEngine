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

		public SelectFrameCommand( Frame frame)
		{
			this._frame = frame;
		}

		public void Do()
		{
			SelectionManager.Instance.SelectFrame(_frame);
		}

		public void Undo()
		{
			SelectionManager.Instance.DeselectFrame();
		}
	}
}
