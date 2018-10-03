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

		Keys Key;
	}


	public class MouseEventArgs : EventArgs
	{

		Keys Key = Keys.None;

		int X = 0;

		int Y = 0;

		int DX = 0;

		int DY = 0;

		int Wheel = 0;
	}


	public class StatusEventArgs : EventArgs
	{

		FrameStatus Status;
	}


	public class MoveEventArgs : EventArgs
	{

		int X;

		int Y;
	}


	public class ResizeEventArgs : EventArgs
	{

		int Width;

		int Height;
	}
}
