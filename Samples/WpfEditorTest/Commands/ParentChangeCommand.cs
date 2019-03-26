using Fusion.Engine.Common;
using Fusion.Engine.Frames2;
using System.Threading;
using System.Windows;

namespace WpfEditorTest.UndoRedo
{
	internal class FrameParentChangeCommand: IEditorCommand
	{
		private AutoResetEvent _asyncThreadChangesDoneEvent = new AutoResetEvent(false);

		public bool IsDirty => true;

		private UIComponent _frame;
		private UIContainer _newParent;
		private UIContainer _oldParent;
		private int _index;

		public FrameParentChangeCommand( UIComponent frame, UIContainer newParent )
		{
			this._frame = frame;
			this._newParent = newParent;
			this._oldParent = _frame.Parent;
			this._index = int.MaxValue;
		}

		public FrameParentChangeCommand( UIComponent frame, UIContainer newParent, int index ) : this(frame, newParent)
		{
			this._index = index;
		}

		public FrameParentChangeCommand( UIComponent frame, UIContainer newParent, UIContainer oldParent ) : this(frame, newParent)
		{
			this._oldParent = oldParent;
		}

		public FrameParentChangeCommand( UIComponent frame, UIContainer newParent, UIContainer oldParent, int index ) : this(frame, newParent, oldParent)
		{
			this._index = index;
		}

		public void Do()
		{
			_asyncThreadChangesDoneEvent.Reset();

			Game.ResourceWorker.Post(r => {
                r.ProcessQueue.Post(t => {
                    _oldParent?.Remove(_frame);
                    _newParent?.AddAt(_frame, _index);
					_asyncThreadChangesDoneEvent.Set();
				}, null, int.MaxValue);
            }, null, int.MaxValue);

			_asyncThreadChangesDoneEvent.WaitOne();
		}

		public void Undo()
		{
			_asyncThreadChangesDoneEvent.Reset();

			Game.ResourceWorker.Post(r => {
                r.ProcessQueue.Post(t => {
                    _newParent?.Remove(_frame);
                    _oldParent?.AddAt(_frame, _index);
					_asyncThreadChangesDoneEvent.Set();
				}, null, int.MaxValue);
            }, null, int.MaxValue);

			_asyncThreadChangesDoneEvent.WaitOne();
		}
	}
}