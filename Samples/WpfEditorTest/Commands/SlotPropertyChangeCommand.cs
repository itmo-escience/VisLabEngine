using Fusion.Engine.Common;
using Fusion.Engine.Frames2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfEditorTest.Commands
{
	class SlotPropertyChangeCommand : IEditorCommand
	{
		private readonly AutoResetEvent _asyncThreadChangesDoneEvent = new AutoResetEvent(false);
		private readonly UIComponent _component;
		private readonly string _propertyName;
		private readonly object _valueToSet;
		private object _previousValue;
		private bool _hasSetter;

		public bool IsDirty => true;

		public SlotPropertyChangeCommand( UIComponent component, string propertyName, object valueToSet )
		{
			_component = component;
			_propertyName = propertyName;
			_valueToSet = valueToSet;

		}

		public SlotPropertyChangeCommand( UIComponent component, string propertyName, object valueToSet, object forcedPreviousValue ) : this(component, propertyName, valueToSet)
		{
			_previousValue = forcedPreviousValue;
		}

		public void Do()
		{
			var _propertyToChange = _component.Placement.GetType().GetProperty(_propertyName);
			if (_propertyToChange != null)
			{
				if (_previousValue==null)
					_previousValue = _propertyToChange.GetValue(_component.Placement);
				_hasSetter = _propertyToChange.GetSetMethod() != null;
			}
			if (_hasSetter)
			{
				_asyncThreadChangesDoneEvent.Reset();

				Game.ResourceWorker.Post(r =>
				{
					r.ProcessQueue.Post(t =>
					{
						_propertyToChange.SetValue(_component.Placement, _valueToSet);
						_asyncThreadChangesDoneEvent.Set();
					}, null, int.MaxValue);
				}, null, int.MaxValue);

				_asyncThreadChangesDoneEvent.WaitOne();
			}
			else
			{
				throw new Exception($"Slot property {_propertyToChange.Name} does not have setter.");
			}
		}

		public void Undo()
		{
			var _propertyToChange = _component.Placement.GetType().GetProperty(_propertyName);
			if (_propertyToChange != null)
			{
				_hasSetter = _propertyToChange.GetSetMethod() != null;
			}
			if (_hasSetter)
			{
				_asyncThreadChangesDoneEvent.Reset();

				Game.ResourceWorker.Post(r =>
				{
					r.ProcessQueue.Post(t =>
					{
						_propertyToChange.SetValue(_component.Placement, _previousValue);
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
