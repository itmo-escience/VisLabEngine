using Fusion.Engine.Common;
using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Frames2.Utils;
using System.Threading;

namespace WpfEditorTest.Commands
{
	internal class UIComponentParentChangeCommand: IEditorCommand
	{
		private readonly AutoResetEvent _asyncThreadChangesDoneEvent = new AutoResetEvent(false);
		public bool IsDirty => true;

		private readonly IUIComponent _frame;
		private readonly IUIModifiableContainer<ISlot> _newParent;
		private readonly IUIModifiableContainer<ISlot> _oldParent;
		private readonly int _index;

		public UIComponentParentChangeCommand( IUIComponent frame, IUIModifiableContainer<ISlot> newParent )
		{
			this._frame = frame;
			this._newParent = newParent;
			this._oldParent = _frame.Parent() as IUIModifiableContainer<ISlot>;
			this._index = int.MaxValue;
		}

		public UIComponentParentChangeCommand( IUIComponent frame, IUIModifiableContainer<ISlot> newParent, int index ) : this(frame, newParent)
		{
			this._index = index;
		}

		public UIComponentParentChangeCommand( IUIComponent frame, IUIModifiableContainer<ISlot> newParent, IUIModifiableContainer<ISlot> oldParent ) : this(frame, newParent)
		{
			this._oldParent = oldParent;
		}

		public UIComponentParentChangeCommand( IUIComponent frame, IUIModifiableContainer<ISlot> newParent, IUIModifiableContainer<ISlot> oldParent, int index ) : this(frame, newParent, oldParent)
		{
			this._index = index;
		}

		public void Do()
		{
			_asyncThreadChangesDoneEvent.Reset();
			Game.ResourceWorker.Post(r => {
                r.ProcessQueue.Post(t => {
                    _oldParent?.Remove(_frame);
                    _newParent?.Insert(_frame, _index);
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
                    _oldParent?.Insert(_frame, _index);
					_asyncThreadChangesDoneEvent.Set();
				}, null, int.MaxValue);
            }, null, int.MaxValue);
			_asyncThreadChangesDoneEvent.WaitOne();
		}
	}
}