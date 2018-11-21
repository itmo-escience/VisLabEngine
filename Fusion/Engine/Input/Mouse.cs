using Fusion.Core.Mathematics;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;

namespace Fusion.Engine.Input
{
    public abstract class Mouse : GameModule
    {
        internal Mouse(Game game) : base(game) { }

        /// <summary>
        /// Difference between last and current mouse position.
        /// Use it for shooters.
        /// </summary>
        public abstract Vector2 PositionDelta { get; }

        /// <summary>
        /// Mouse position relative to top-left corner
        /// </summary>
        public abstract Point Position { get; }

        /// <summary>
        /// Force mouse to main window center on each frame.
        /// </summary>
        public abstract bool IsMouseCentered { get; set; }

        /// <summary>
        /// Clip mouse by main window's client area border.
        /// </summary>
        public abstract bool IsMouseClipped { get; set; }

        /// <summary>
        /// Set and get mouse visibility
        /// </summary>
        public abstract bool IsMouseHidden { get; set; }

        /// <summary>
        /// System value: MouseWheelScrollLines
        /// </summary>
        public abstract int MouseWheelScrollLines { get; }

        /// <summary>
        /// System value: MouseWheelScrollDelta
        /// </summary>
        public abstract int MouseWheelScrollDelta { get; }

        public event MouseMoveHandlerDelegate Move;
        public event MouseScrollEventHandler Scroll;

        protected virtual void OnMove(object sender, MouseMoveEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var handler = Move;
            handler?.Invoke(sender, e);
        }

        protected virtual void OnScroll(object sender, MouseScrollEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var handler = Scroll;
            handler?.Invoke(sender, e);
        }
    }

	public class ConcreteMouse : Mouse
	{
		InputDevice device;

		/// <summary>
		///
		/// </summary>
		/// <param name="Game"></param>
		internal ConcreteMouse ( Game Game ) : base(Game)
		{
			this.device	= Game.InputDevice;

			device.MouseScroll += DeviceMouseScroll;
			device.MouseMove += DeviceMouseMove;
		}

	    /// <inheritdoc cref="GameModule.Initialize"/>
        public override void Initialize() { }

		/// <inheritdoc cref="GameModule.Dispose"/>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				device.MouseScroll -= DeviceMouseScroll;
				device.MouseMove -= DeviceMouseMove;
			}

			base.Dispose(disposing);
		}

	    /// <inheritdoc cref="Mouse.PositionDelta"/>
        public override Vector2 PositionDelta => device.RelativeMouseOffset;

		/// <inheritdoc cref="Mouse.Position"/>
		public override Point Position => new Point( (int)device.GlobalMouseOffset.X, (int)device.GlobalMouseOffset.Y );

        /// <inheritdoc cref="Mouse.IsMouseCentered"/>
        public override	bool IsMouseCentered {
			get { return device.IsMouseCentered; }
			set { device.IsMouseCentered = value; }
		}

        /// <inheritdoc cref="Mouse.IsMouseClipped"/>
        public override bool IsMouseClipped {
			get { return device.IsMouseClipped; }
			set { device.IsMouseClipped = value; }
		}

        /// <inheritdoc cref="Mouse.IsMouseHidden"/>
        public override bool IsMouseHidden {
			get { return device.IsMouseHidden; }
			set { device.IsMouseHidden = value; }
		}

        /// <inheritdoc cref="Mouse.MouseWheelScrollLines"/>
        public override int MouseWheelScrollLines => device.MouseWheelScrollLines;

        /// <inheritdoc cref="Mouse.MouseWheelScrollDelta"/>
        public override int MouseWheelScrollDelta => device.MouseWheelScrollDelta;

		private void DeviceMouseMove(object sender, InputDevice.MouseMoveEventArgs e)
		{
            OnMove(sender, new MouseMoveEventArgs { Position = e.Position, Offset = e.Offset });
		}

		private void DeviceMouseScroll(object sender, InputDevice.MouseScrollEventArgs e)
		{
            OnScroll(sender, new MouseScrollEventArgs { WheelDelta = e.WheelDelta });
		}
	}

    public class DummyMouse : Mouse
    {
        public DummyMouse(Game game) : base(game) { }

        public override void Initialize() { }

        public override Vector2 PositionDelta => new Vector2();
        public override Point Position => new Point();
        public override bool IsMouseCentered
        {
            get => false;
            set { }
        }

        public override bool IsMouseClipped
        {
            get => false;
            set { }
        }

        public override bool IsMouseHidden
        {
            get => false;
            set { }
        }

        public override int MouseWheelScrollLines => 0;
        public override int MouseWheelScrollDelta => 0;
    }
}
