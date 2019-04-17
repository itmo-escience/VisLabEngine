using Fusion.Engine.Common;
using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Utils;
using System;
using System.Reflection;
using System.Threading;

namespace WpfEditorTest.Commands
{
	internal class UIComponentPropertyChangeCommand : IEditorCommand
	{
		private readonly AutoResetEvent _asyncThreadChangesDoneEvent = new AutoResetEvent(false);
	    private readonly IUIComponent _component;
	    private readonly PropertyInfo _propertyToChange;
	    private readonly object _valueToSet;
	    private readonly object _previousValue;
		private readonly bool _hasSetter;

		public bool IsDirty => true;

		public UIComponentPropertyChangeCommand( IUIComponent frame, string propertyName, object valueToSet)
		{
			_component = frame;
			_propertyToChange = _component.GetType().GetProperty(propertyName);
			_valueToSet = valueToSet;
			if (_propertyToChange!=null)
			{
				_previousValue = _propertyToChange.GetValue(_component);
				_hasSetter = _propertyToChange.GetSetMethod()!=null;
			}
		}

		public UIComponentPropertyChangeCommand( IUIComponent frame, string propertyName, object valueToSet, object forcedPreviousValue ) : this(frame, propertyName, valueToSet)
		{
			_previousValue = forcedPreviousValue;
		}

		public void Do()
		{
			if (_hasSetter)
			{
				_asyncThreadChangesDoneEvent.Reset();

				Game.ResourceWorker.Post(r =>
				{
					r.ProcessQueue.Post(t =>
					{
						_propertyToChange.SetValue(_component, _valueToSet);
						_asyncThreadChangesDoneEvent.Set();
					}, null, int.MaxValue);
				}, null, int.MaxValue);

				_asyncThreadChangesDoneEvent.WaitOne();
			}
			else
			{
				throw new Exception($"Property {_propertyToChange.Name} does not have setter.");
			}
		}

		public void Undo()
		{
			if (_hasSetter)
			{
				_asyncThreadChangesDoneEvent.Reset();

				Game.ResourceWorker.Post(r =>
				{
					r.ProcessQueue.Post(t =>
					{
						_propertyToChange.SetValue(_component, _previousValue);
						_asyncThreadChangesDoneEvent.Set();
					}, null, int.MaxValue);
				}, null, int.MaxValue);

				_asyncThreadChangesDoneEvent.WaitOne(); 
			}
			else
			{
				throw new Exception($"Property {_propertyToChange.Name} does not have setter.");
			}
		}
	}
}
