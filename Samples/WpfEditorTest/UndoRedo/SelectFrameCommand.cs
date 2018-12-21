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
		private List<Frame> _frames;
		private List<Frame> _oldFrames;

		public SelectFrameCommand( List<Frame> frame )
		{
			this._frames = frame;
			this._oldFrames = SelectionManager.Instance.SelectedFrames;
		}

		public void Do()
		{
			SelectionManager.Instance.SelectFrame(_frames);
		}

		public void Undo()
		{
			SelectionManager.Instance.SelectFrame(_oldFrames);
		}
	}
}
