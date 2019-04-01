using Fusion.Engine.Frames2;
using System.Collections.Generic;
using System.Linq;
using WpfEditorTest.FrameSelection;

namespace WpfEditorTest.Commands
{
	public class SelectFrameCommand : IEditorCommand
	{
		public bool IsDirty => false;

		private readonly List<UIComponent> _frames;
		private readonly List<UIComponent> _oldFrames;

	    public SelectFrameCommand( params UIComponent[] frame ) : this(frame.ToList()) { }

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
