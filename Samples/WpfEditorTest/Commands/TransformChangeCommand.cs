using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Utils;
using System;
using System.Threading;

namespace WpfEditorTest.Commands
{
	internal class TransformChangeCommand : IEditorCommand
	{
		private readonly AutoResetEvent _asyncThreadChangesDoneEvent = new AutoResetEvent(false);
		private readonly ISlot _slot;
		private readonly Matrix3x2 _valueToSet;
		private readonly Matrix3x2 _previousValue;
		private readonly bool _hasSetter;

		public TransformChangeCommand( ISlot slot, Matrix3x2 valueToSet)
		{
			this._slot = slot;
			this._valueToSet = valueToSet;

			var property = _slot.GetType().GetProperty("Transform");
			if (property != null)
			{
				_previousValue = (Matrix3x2)property.GetValue(_slot);
			}
		}

		public TransformChangeCommand( ISlot placement, Matrix3x2 valueToSet, Matrix3x2 forcedPreviousValue ) : this(placement, valueToSet)
		{
			_previousValue = forcedPreviousValue;
		}

		public bool IsDirty => true;

		public void Do()
		{

		}

		public void Undo()
		{

		}
	}
}