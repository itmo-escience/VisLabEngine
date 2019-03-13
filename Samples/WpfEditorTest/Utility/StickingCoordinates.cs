using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfEditorTest.Utility
{
	public class StickCoordinateX
	{
		public float X;
		public float TopY;
		public float BottomY;

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

		public StickCoordinateX( float x, float topY, float bottomY )
		{
			X = x;
			TopY = topY;
			BottomY = bottomY;
		}
	}

	public class StickCoordinateY
	{
		public float Y;
		public float LeftX;
		public float RightX;

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

		public StickCoordinateY( float y, float leftX, float rightX )
		{
			Y = y;
			LeftX = leftX;
			RightX = rightX;
		}
	}
}
