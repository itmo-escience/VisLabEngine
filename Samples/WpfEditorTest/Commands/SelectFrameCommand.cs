using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Utils;
using System.Collections.Generic;
using System.Linq;
using WpfEditorTest.FrameSelection;

namespace WpfEditorTest.Commands
{
	public class SelectFrameCommand : IEditorCommand
	{
		public bool IsDirty => false;

		private readonly List<IUIComponent> _frames;
		private readonly List<IUIComponent> _oldFrames;

	    public SelectFrameCommand( params IUIComponent[] frame ) : this(frame.ToList()) { }

		public SelectFrameCommand( List<IUIComponent> frame )
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
