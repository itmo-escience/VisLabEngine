using Fusion.Engine.Common;
using Fusion.Engine.Frames2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfEditorTest.UndoRedo
{
	internal class FramePropertyChangeCommand : IEditorCommand
	{
		private AutoResetEvent _asyncThreadChangesDoneEvent = new AutoResetEvent(false);

		public bool IsDirty => true;

		UIComponent _frame;
		PropertyInfo _propertyToChange;
		object _valueToSet;
		object _previousValue;

		public FramePropertyChangeCommand( UIComponent frame, string propertyName, object valueToSet)
		{
			_frame = frame;
			_propertyToChange = _frame.GetType().GetProperty(propertyName);
			_valueToSet = valueToSet;
			_previousValue = _propertyToChange.GetValue(_frame);
		}

		public FramePropertyChangeCommand( UIComponent frame, string propertyName, object valueToSet, object forcedPreviousValue ) : this(frame, propertyName, valueToSet)
		{
			_previousValue = forcedPreviousValue;
		}

		public void Do()
		{
			_asyncThreadChangesDoneEvent.Reset();

			Game.ResourceWorker.Post(r => {
                r.ProcessQueue.Post(t => {
                    _propertyToChange.SetValue(_frame, _valueToSet);
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
                    _propertyToChange.SetValue(_frame, _previousValue);
					_asyncThreadChangesDoneEvent.Set();
				}, null, int.MaxValue);
            }, null, int.MaxValue);

			_asyncThreadChangesDoneEvent.WaitOne();
		}
	}
}
