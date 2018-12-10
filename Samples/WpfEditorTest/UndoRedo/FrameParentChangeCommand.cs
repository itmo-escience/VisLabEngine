using Fusion.Engine.Frames;

namespace WpfEditorTest.UndoRedo
{
	internal class FrameParentChangeCommand: IEditorCommand
	{
		private Frame _frame;
		private Frame _newParent;
		private Frame _oldParent;

		public FrameParentChangeCommand( Frame frame, Frame newParent )
		{
			this._frame = frame;
			this._newParent = newParent;
			this._oldParent = _frame.Parent;
		}

		public FrameParentChangeCommand( Frame frame, Frame newParent, Frame oldParent ) : this(frame, newParent)
		{
			this._oldParent = oldParent;
		}

		public void Do()
		{
			_oldParent?.Remove(_frame);
			_newParent?.Add(_frame);
		}

		public void Undo()
		{
			_newParent?.Remove(_frame);
			_oldParent?.Add(_frame);
		}
	}
}