using Fusion.Engine.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WpfEditorTest.UndoRedo
{
	class FramePropertyChangeCommand : IEditorCommand
	{
		Frame _frame;
		PropertyInfo _propertyToChange;
		object _valueToSet;
		object _previousValue;

		public FramePropertyChangeCommand(Frame frame, string propertyName, object valueToSet)
		{
			_frame = frame;
			_propertyToChange = _frame.GetType().GetProperty(propertyName);
			_valueToSet = valueToSet;
			_previousValue = _propertyToChange.GetValue(_frame);
		}

		public FramePropertyChangeCommand( Frame frame, string propertyName, object valueToSet, object forcedPreviousValue ) : this(frame, propertyName, valueToSet)
		{
			_previousValue = forcedPreviousValue;
		}

		public void Do()
		{
			_propertyToChange.SetValue(_frame, _valueToSet);
		}

		public void Undo()
		{
			_propertyToChange.SetValue(_frame, _previousValue);
		}
	}
}
