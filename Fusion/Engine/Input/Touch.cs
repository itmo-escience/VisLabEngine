using Fusion.Drivers.Input;
using Fusion.Engine.Common;

namespace Fusion.Engine.Input
{
    public abstract class Touch : GameModule
    {
        protected Touch(Game game) : base(game) { }

        public bool IsTouchSupported => true;

        public event TouchTapEventHandler Tap;
        public event TouchTapEventHandler DoubleTap;
        public event TouchTapEventHandler SecondaryTap;
        public event TouchTapEventHandler Manipulate;
        public event TouchTapEventHandler Hold;

        protected virtual void OnTap(TouchEventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var handler = Tap;
            handler?.Invoke(args);
        }

        protected virtual void OnDoubleTap(TouchEventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var handler = DoubleTap;
            handler?.Invoke(args);
        }

        protected virtual void OnSecondaryTap(TouchEventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var handler = SecondaryTap;
            handler?.Invoke(args);
        }

        protected virtual void OnManipulate(TouchEventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var handler = Manipulate;
            handler?.Invoke(args);
        }

        protected virtual void OnHold(TouchEventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var handler = Hold;
            handler?.Invoke(args);
        }
    }

	public class ConcreteTouch : Touch
	{
		private readonly InputDevice _device;

        /// <summary>
        ///
        /// </summary>
        /// <param name="game"></param>
        internal ConcreteTouch (Game game) : base(game)
		{
			_device = Game.InputDevice;

			_device.TouchGestureTap	         += OnTap;
			_device.TouchGestureDoubleTap    += OnDoubleTap;
			_device.TouchGestureSecondaryTap += OnSecondaryTap;
			_device.TouchGestureManipulate   += OnManipulate;
            _device.TouchHold                += OnHold;
		}

        /// <summary>
        ///
        /// </summary>
        public override void Initialize () { }

		/// <summary>
		///
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				_device.TouchGestureTap			-= OnTap;
				_device.TouchGestureDoubleTap	-= OnDoubleTap;
				_device.TouchGestureSecondaryTap -= OnSecondaryTap;
				_device.TouchGestureManipulate	-= OnManipulate;
				_device.TouchHold	            -= OnHold;
			}

			base.Dispose( disposing );
		}
	}

    public class DummyTouch : Touch
    {
        public DummyTouch(Game game) : base(game) { }

        public override void Initialize() { }
    }
}
