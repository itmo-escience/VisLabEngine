using Fusion.Engine.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Frames.Abstract
{
	public class KeyEventArgs : EventArgs
	{

		public Keys Key;
	}


	public class MouseEventArgs : EventArgs
	{

		public Keys Key = Keys.None;

		public int X = 0;

		public int Y = 0;

		public int DX = 0;

		public int DY = 0;

		public int Wheel = 0;
	}


	public class StatusEventArgs : EventArgs
	{

		public FrameStatus Status;
	}


	public class MoveEventArgs : EventArgs
	{

		public int X;

		public int Y;
	}


	public class ResizeEventArgs : EventArgs
	{

		public int Width;

		public int Height;
	}
}
