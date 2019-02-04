using Fusion.Engine.Frames2;
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
		private List<UIComponent> _frames;
		private List<UIComponent> _oldFrames;

		public SelectFrameCommand( List<UIComponent> frame )
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
