using Fusion.Engine.Common;
using Fusion.Engine.Frames2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WpfEditorTest.UndoRedo
{
	internal class FramePropertyChangeCommand : IEditorCommand
	{
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
            Game.ResourceWorker.Post(r => {
                _propertyToChange.SetValue(_frame, _valueToSet);
            }, null, int.MaxValue);
        }

		public void Undo()
		{
            Game.ResourceWorker.Post(r => {
                _propertyToChange.SetValue(_frame, _previousValue);
            }, null, int.MaxValue);
        }
	}
}
