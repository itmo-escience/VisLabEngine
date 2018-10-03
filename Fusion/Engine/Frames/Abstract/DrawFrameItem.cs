using Fusion.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Frames.Abstract
{
	class DrawFrameItem
	{
		public DrawFrameItem( IFrame frame, Color color, Rectangle outerClip, Rectangle innerClip, string text )
		{
			this.Frame = frame;
			this.OuterClip = outerClip;
			this.InnerClip = innerClip;
			this.Color = color;
			this.Text = text;
		}
		public IFrame Frame;
		public Color Color;
		public Rectangle OuterClip;
		public Rectangle InnerClip;
		public string Text;
	}
}
