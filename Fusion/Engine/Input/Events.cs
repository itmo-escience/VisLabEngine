﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Input {

	public delegate void MouseMoveHandlerDelegate	( object sender, MouseMoveEventArgs e );
	public delegate void MouseScrollEventHandler	( object sender, MouseScrollEventArgs e );
    public delegate void MousePressEventHandler     (object sender, MousePressEventArgs e);
    public delegate void KeyDownEventHandler	( object sender, KeyEventArgs e );
	public delegate void KeyUpEventHandler		( object sender, KeyEventArgs e );
	public delegate void KeyPressEventHandler	( object sender, KeyPressArgs e );

	public delegate void TouchTapEventHandler(TouchEventArgs args);


	public class KeyEventArgs : EventArgs 
	{
		public Keys	Key;
	}

	public class KeyPressArgs : EventArgs 
	{
		public char	KeyChar;
	}

	public class MouseScrollEventArgs : EventArgs 
	{
		/// <summary>
		/// See: InputDevice.MouseWheelScrollDelta.
		/// </summary>
		public int WheelDelta;
	}

	public class MouseMoveEventArgs : EventArgs 
	{
		public Vector2 Position;
		public Vector2 Offset;
	}

    public class MousePressEventArgs : EventArgs
    {
        public Vector2 Position;
        public Keys Key;
    }

    public class TouchEventArgs
	{
		public Point Position;
		public float ScaleDelta;
		public float RotationDelta;
		public bool	IsEventBegin;
		public bool	IsEventEnd;
		public int	FingersCount;
	}
}
