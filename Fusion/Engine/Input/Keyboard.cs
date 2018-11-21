using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;
using Fusion.Core.IniParser.Model;


namespace Fusion.Engine.Input
{
    public abstract class Keyboard : GameModule
    {
        internal Keyboard(Game game) : base(game) { }

        /// <summary>
        /// Indicates whether keyboard should be scanned.
        /// If ScanKeyboard equals false methods IsKeyDown and IsKeyUp indicate that all keys are unpressed.
        /// All events like FormKeyPress, FormKeyDown, FormKeyUp will work.
        /// </summary>
        public abstract bool ScanKeyboard { get; set; }

        /// <summary>
        /// Gets keyboard bindings.
        /// </summary>
        public abstract IEnumerable<KeyBind> Bindings { get; }

        /// <summary>
        /// Binds command to key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyDownCommand"></param>
        /// <param name="keyUpCommand"></param>
        public abstract void Bind(Keys key, string keyDownCommand, string keyUpCommand);

        /// <summary>
        /// Unbind commands from key.
        /// </summary>
        /// <param name="key"></param>
        public abstract void Unbind(Keys key);


        /// <summary>
        /// Indicates that given key is already bound.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract bool IsBound(Keys key);


        /// <summary>
        /// Returns whether a specified key is currently being pressed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract bool IsKeyDown(Keys key);

        /// <summary>
        /// Returns whether a specified key is currently not pressed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract bool IsKeyUp(Keys key);

        public event KeyDownEventHandler KeyDown;
        public event KeyUpEventHandler KeyUp;
        public event KeyDownEventHandler FormKeyDown;
        public event KeyUpEventHandler FormKeyUp;
        public event KeyPressEventHandler FormKeyPress;

        protected void OnKeyDown(object sender, Fusion.Engine.Input.KeyEventArgs args)
        {
            var handler = KeyDown;
            handler?.Invoke(sender, args);
        }

        protected void OnKeyUp(object sender, Fusion.Engine.Input.KeyEventArgs args)
        {
            var handler = KeyUp;
            handler?.Invoke(sender, args);
        }

        protected void OnFormKeyDown(object sender, Fusion.Engine.Input.KeyEventArgs args)
        {
            var handler = FormKeyDown;
            handler?.Invoke(sender, args);
        }

        protected void OnFormKeyUp(object sender, Fusion.Engine.Input.KeyEventArgs args)
        {
            var handler = FormKeyUp;
            handler?.Invoke(sender, args);
        }

        protected void OnFormKeyPress(object sender, Fusion.Engine.Input.KeyPressArgs args)
        {
            var handler = FormKeyPress;
            handler?.Invoke(sender, args);
        }
    }

	public class ConcreteKeyboard : Keyboard
	{
		private readonly InputDevice _device;
	    private readonly Dictionary<Keys, KeyBind> _bindings = new Dictionary<Keys, KeyBind>();

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="Game"></param>
		internal ConcreteKeyboard ( Game Game ) : base(Game)
		{
			_device	=	Game.InputDevice;

			_device.KeyDown += DeviceKeyDown;
			_device.KeyUp += DeviceKeyUp;

			_device.FormKeyDown += DeviceFormKeyDown;
			_device.FormKeyUp += DeviceFormKeyUp;
			_device.FormKeyPress += DeviceFormKeyPress;
		}

		/// <summary>
		///
		/// </summary>
		public override void Initialize () { }

	    /// <inheritdoc />
		public override bool ScanKeyboard {
			get {
				return _scanKeyboard;
			}
			set {
				if (value!=_scanKeyboard) {
					if (value) {
						_scanKeyboard = true;
					} else {
						_scanKeyboard = false;
						_device.RemoveAllPressedKeys();
					}
				}
			}
		}
	    private bool _scanKeyboard = true;

	    /// <inheritdoc />
        public override IEnumerable<KeyBind> Bindings
	    {
	        get
	        {
	            return _bindings
	                .Select(keyvalue => keyvalue.Value)
	                .ToArray();
	        }
	    }

	    /// <inheritdoc />
        public override void Bind ( Keys key, string keyDownCommand, string keyUpCommand )
		{
			_bindings.Add( key, new KeyBind(key, keyDownCommand, keyUpCommand ) );
		}

	    /// <inheritdoc />
        public override void Unbind ( Keys key )
		{
			_bindings.Remove(key);
		}

	    /// <inheritdoc />
        public override bool IsBound ( Keys key )
		{
			return _bindings.ContainsKey( key );
		}

		/// <summary>
		/// Gets keyboard configuration.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<KeyData> GetConfiguration ()
		{
			return Bindings.Select( bind => new KeyData( bind.Key.ToString(), bind.KeyDownCommand + " | " + bind.KeyUpCommand ) );
		}

		/// <summary>
		/// Sets keyboard configuration
		/// </summary>
		/// <param name="configuration"></param>
		public override void SetConfiguration ( IEnumerable<KeyData> configuration )
		{
			_bindings.Clear();

			foreach ( var keyData in configuration ) {

				var key = (Keys)Enum.Parse(typeof(Keys), keyData.KeyName, true );

				var cmds	=	keyData.Value.Split('|').Select( s => s.Trim() ).ToArray();

				string cmdDown	=	null;
				string cmdUp	=	null;

				if (!string.IsNullOrWhiteSpace(cmds[0])) {
					cmdDown = cmds[0];
				}

				if (cmds.Length>1 && !string.IsNullOrWhiteSpace(cmds[1])) {
					cmdUp = cmds[1];
				}

				Bind( key, cmdDown, cmdUp );
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				_device.KeyDown -= DeviceKeyDown;
				_device.KeyUp -= DeviceKeyUp;

				_device.FormKeyDown -= DeviceFormKeyDown;
				_device.FormKeyUp -= DeviceFormKeyUp;
				_device.FormKeyPress -= DeviceFormKeyPress;
			}

			base.Dispose( disposing );
		}

	    /// <inheritdoc />
        public override bool IsKeyDown ( Keys key )
		{
			return ( _scanKeyboard && _device.IsKeyDown( (Fusion.Drivers.Input.Keys)key ) );
		}


		/// <inheritdoc />
		public override bool IsKeyUp ( Keys key )
		{
			return ( !_scanKeyboard || _device.IsKeyUp( (Fusion.Drivers.Input.Keys)key ) );
		}

		private void DeviceKeyDown ( object sender, InputDevice.KeyEventArgs e )
		{
			if (!_scanKeyboard) {
				return;
			}

		    if (_bindings.TryGetValue( (Keys)e.Key, out var bind )) {
				try {
					if (!string.IsNullOrWhiteSpace(bind.KeyDownCommand)) {
						Game.Invoker.Push( bind.KeyDownCommand );
					}
				} catch ( Exception cmdLineEx ) {
					Log.Error("{0}", cmdLineEx.Message );
				}
			}

            OnKeyDown( sender, new KeyEventArgs(){ Key = (Keys)e.Key } );
		}

		private void DeviceKeyUp ( object sender, InputDevice.KeyEventArgs e )
		{
		    if (_bindings.TryGetValue( (Keys)e.Key, out var bind )) {
				try {
					if (!string.IsNullOrWhiteSpace(bind.KeyUpCommand)) {
						Game.Invoker.Push( bind.KeyUpCommand );
					}
				} catch ( Exception cmdLineEx ) {
					Log.Error("{0}", cmdLineEx.Message );
				}
			}

            OnKeyUp(sender, new KeyEventArgs(){ Key = (Keys)e.Key });
		}

		private void DeviceFormKeyDown(object sender, InputDevice.KeyEventArgs e)
		{
            OnFormKeyDown(sender, new KeyEventArgs(){ Key = (Keys)e.Key } );
		}

		private void DeviceFormKeyUp(object sender, InputDevice.KeyEventArgs e)
		{
            OnFormKeyUp(sender, new KeyEventArgs(){ Key = (Keys)e.Key } );
		}

		private void DeviceFormKeyPress(object sender, InputDevice.KeyPressArgs e)
		{
            OnFormKeyPress(sender, new KeyPressArgs(){ KeyChar = e.KeyChar });
		}
	}

    public class DummyKeyboard : Keyboard
    {
        public DummyKeyboard(Game game) : base(game) { }

        public override void Initialize() { }

        public override bool ScanKeyboard {
            get => false;
            set { }
        }
        public override IEnumerable<KeyBind> Bindings => new List<KeyBind>();

        public override void Bind(Keys key, string keyDownCommand, string keyUpCommand) { }
        public override void Unbind(Keys key) { }
        public override bool IsBound(Keys key) => false;

        public override bool IsKeyDown(Keys key) => false;
        public override bool IsKeyUp(Keys key) => true;
    }
}
