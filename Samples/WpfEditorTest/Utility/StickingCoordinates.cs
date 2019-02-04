using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfEditorTest.Utility
{
	public class StickCoordinateX
	{
		public int X;
		public int TopY;
		public int BottomY;

		private bool isActive;

		public EventHandler ActiveChanged;

		public bool IsActive
		{
			get => isActive;
			set
			{
				isActive = value;
				this.ActiveChanged?.Invoke(this, null);
			}
		}

		public StickCoordinateX( int x, int topY, int bottomY )
		{
			X = x;
			TopY = topY;
			BottomY = bottomY;
		}
	}

	public class StickCoordinateY
	{
		public int Y;
		public int LeftX;
		public int RightX;

		private bool isActive;

		public EventHandler ActiveChanged;

		public bool IsActive
		{
			get => isActive;
			set
			{
				isActive = value;
				this.ActiveChanged?.Invoke(this, null);
			}
		}

		public StickCoordinateY( int y, int leftX, int rightX )
		{
			Y = y;
			LeftX = leftX;
			RightX = rightX;
		}
	}
}
