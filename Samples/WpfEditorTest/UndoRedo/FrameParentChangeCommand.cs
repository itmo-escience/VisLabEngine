using Fusion.Engine.Frames2;

namespace WpfEditorTest.UndoRedo
{
	internal class FrameParentChangeCommand: IEditorCommand
	{
		private UIComponent _frame;
		private UIContainer _newParent;
		private UIContainer _oldParent;

		public FrameParentChangeCommand( UIComponent frame, UIContainer newParent )
		{
			this._frame = frame;
			this._newParent = newParent;
			this._oldParent = _frame.Parent;
		}

		public FrameParentChangeCommand( UIComponent frame, UIContainer newParent, UIContainer oldParent ) : this(frame, newParent)
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