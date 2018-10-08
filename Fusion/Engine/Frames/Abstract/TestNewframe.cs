using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;

namespace Fusion.Engine.Frames.Abstract
{
	public class TestNewframe : /*IFrame,*/ IFrameInternal
	{
		public Game Game { get; set; }

		public FrameProcessor ui { get; private set; }
		FrameProcessor IFrameInternal.ui { set { ui = value; } }

		public string Name { get; set; }
		public bool Visible { get; set; }
		public bool Ghost { get; set; }
		public bool Enabled { get; set; }
		public bool AutoSize { get; set; }
		public bool AutoWidth { get; set; }
		public bool AutoHeight { get; set; }
		public SpriteFont Font { get; set; }
		public ClippingMode ClippingMode { get; set; } = ClippingMode.ClipByFrame;
		public Color OverallColor { get; set; }
		public Color BackColor { get; set; }
		public Color BorderColor { get; set; }
		public Color ForeColor { get; set; }
		public Color ShadowColor { get; set; }
		public Vector2 ShadowOffset { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int PaddingLeft { get; set; }
		public int PaddingRight { get; set; }
		public int PaddingTop { get; set; }
		public int PaddingBottom { get; set; }
		public int VPadding { set { PaddingBottom = PaddingTop = value; } }
		public int HPadding { set { PaddingLeft = PaddingRight = value; } }
		public int Padding { set { VPadding = HPadding = value; } }
		public int BorderTop { get; set; }
		public int BorderBottom { get; set; }
		public int BorderLeft { get; set; }
		public int BorderRight { get; set; }
		public int Border { set { BorderTop = BorderBottom = BorderLeft = BorderRight = value; } }
		public string Text { get; set; }
		public Alignment TextAlignment { get; set; }
		public int TextOffsetX { get; set; }
		public int TextOffsetY { get; set; }
		public TextEffect TextEffect { get; set; }
		public FrameAnchor Anchor { get; set; }
		public int ImageOffsetX { get; set; }
		public int ImageOffsetY { get; set; }
		public FrameImageMode ImageMode { get; set; }
		public Color ImageColor { get; set; }
		private Texture image;
		[XmlIgnore]
		public Texture Image
		{
			get
			{
				if (this.image != null)
				{
					return this.image;
				}
				else if (!String.IsNullOrEmpty(imageName))
				{
					image = ui.Game.Content.Load<DiscTexture>(imageName);
					return image;
				}
				else
				{
					return null;
				}
			}
			set
			{
				image = value;
			}
		}
		private string imageName;
		public string ImageName
		{
			get
			{
				if (this.Image != null)
				{
					return imageName = this.Image.Name;
				}
				else
				{
					return imageName;
				}
			}
			set
			{
				imageName = value;
			}
		}
		private List<TestNewframe> _children;
		public List<TestNewframe> Children
		{
			get { return _children; }
			set
			{
				foreach (TestNewframe child in value)
				{
					this.Add(child);
				}
			}
		}
		private TestNewframe _parent;
		[XmlIgnore]
		public TestNewframe Parent { get { return _parent; } }
		[XmlIgnore]
		public Rectangle GlobalRectangle { get; private set; }
		Rectangle IFrameInternal.GlobalRectangle { set { GlobalRectangle = value; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="id"></param>
		public TestNewframe( FrameProcessor ui )
		{
			Game = ui.Game;
			this.ui = ui;
			((IFrameInternal)this).Init();
		}

		/// <summary>
		/// Parameterless constructor
		/// </summary>
		protected TestNewframe()
		{
			//Init();
		}

		private int _zOrder = 0;
		public int ZOrder
		{
			get { return _zOrder; }
			set
			{
				_zOrder = value;
				_parent?.ReorderChildren();
			}
		}
		public List<ITransition> Transitions { get; set; }

		bool IFrameInternal.CanAcceptControl { get { return Visible && OverallColor.A != 0 && !Ghost; } }

		bool IFrameInternal.IsDrawable { get { return Visible && OverallColor.A != 0; } }

		public event EventHandler Tick;

		public event EventHandler LayoutChanged;

		public event EventHandler Activated;

		public event EventHandler Deactivated;

		public event EventHandler<MouseEventArgs> MouseIn;

		public event EventHandler<MouseEventArgs> MouseMove;

		public event EventHandler<MouseEventArgs> MouseDrag;

		public event EventHandler<MouseEventArgs> MouseOut;

		public event EventHandler<MouseEventArgs> MouseWheel;

		public event EventHandler<MouseEventArgs> Click;

		public event EventHandler<MouseEventArgs> DoubleClick;

		public event EventHandler<MouseEventArgs> MouseDown;

		public event EventHandler<MouseEventArgs> MouseUp;

		public event EventHandler<StatusEventArgs> StatusChanged;

		public event EventHandler<MoveEventArgs> Move;

		public event EventHandler<ResizeEventArgs> Resize;

		public void Add( TestNewframe frame )
		{
			if (!_children.Contains(frame))
			{
				_children.Add(frame);
				((IFrameInternal)frame).UpdateChildrenUI(this.ui);
				if (frame.ZOrder == 0)
				{
					frame.ZOrder = _children.Count;
				}
				ReorderChildren();
				frame._parent = this;

				((IFrameInternal)frame).OnStatusChanged(FrameStatus.None);
			}
		}

		void IFrameInternal.Adjust()
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.AutoResize( bool fromUpdate )
		{
			if (AutoWidth || AutoHeight && _children.Count != 0)
			{
				Rectangle rect = new Rectangle(0, 0, 0, 0);

				foreach (var frame in _children.ToList())
				{
					if (frame == this || frame.Visible == false) continue;
					rect.Right = Math.Max(frame.X + frame.Width + PaddingRight, rect.Right);
					rect.Bottom = Math.Max(frame.Y + frame.Height + PaddingBottom, rect.Bottom);
				}

				if (AutoWidth)
				{
					//this.X += rect.X;
					this.Width = rect.Width;
				}
				if (AutoHeight)
				{
					//this.Y += rect.Y;
					this.Height = rect.Height;
				}
				if (!fromUpdate) UpdateResize();
			}
		}

		public void Clear( TestNewframe frame )
		{
			foreach (var child in _children)
			{
				child._parent = null;
			}
			_children.Clear();
		}

		void IFrameInternal.DrawFrame( GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex )
		{
			((IFrameInternal)this).DrawFrameImage(spriteLayer, clipRectIndex);
			((IFrameInternal)this).DrawFrameText(spriteLayer, clipRectIndex);
		}

		void IFrameInternal.DrawFrameBorders( SpriteLayer spriteLayer, int clipRectIndex )
		{
			//clipRectIndex = ClippingMode == ClippingMode.None ? 0 : clipRectIndex;
			int gx = GlobalRectangle.X;
			int gy = GlobalRectangle.Y;
			int w = Width;
			int h = Height;
			int bt = BorderTop;
			int bb = BorderBottom;
			int br = BorderRight;
			int bl = BorderLeft;

			var whiteTex = Game.RenderSystem.WhiteTexture;

			var clr = BorderColor;

			spriteLayer.Draw(whiteTex, gx, gy, w, bt, clr, clipRectIndex);
			spriteLayer.Draw(whiteTex, gx, gy + h - bb, w, bb, clr, clipRectIndex);
			spriteLayer.Draw(whiteTex, gx, gy + bt, bl, h - bt - bb, clr, clipRectIndex);
			spriteLayer.Draw(whiteTex, gx + w - br, gy + bt, br, h - bt - bb, clr, clipRectIndex);

			spriteLayer.Draw(whiteTex, GetBorderedRectangle(), BackColor, clipRectIndex);
		}

		void IFrameInternal.DrawFrameImage( SpriteLayer spriteLayer, int clipRectIndex )
		{
			if (Image == null)
			{
				return;
			}

			var gp = GetPaddedRectangle();
			var bp = GetBorderedRectangle();
			clipRectIndex = ClippingMode == ClippingMode.None ? 0 : clipRectIndex;

			if (ImageMode == FrameImageMode.Stretched)
			{
				spriteLayer.Draw(Image, bp, ImageColor, clipRectIndex);
				return;
			}

			if (ImageMode == FrameImageMode.Centered)
			{
				int x = bp.X + gp.Width / 2 - Image.Width / 2 + ImageOffsetX;
				int y = bp.Y + gp.Height / 2 - Image.Height / 2 + ImageOffsetY;
				spriteLayer.Draw(Image, x, y, Image.Width, Image.Height, ImageColor, clipRectIndex);
				return;
			}

			if (ImageMode == FrameImageMode.Tiled)
			{
				spriteLayer.Draw(Image, bp, new Rectangle(0, 0, bp.Width, bp.Height), ImageColor, clipRectIndex);
				return;
			}

			if (ImageMode == FrameImageMode.DirectMapped)
			{
				spriteLayer.Draw(Image, bp, bp, ImageColor, clipRectIndex);
				return;
			}
			if (ImageMode == FrameImageMode.Fitted)
			{
				float wm = (float)gp.Width / Image.Width, hm = (float)gp.Height / Image.Height;

				if (wm < hm)
				{
					float x = gp.X + (float)gp.Width / 2 - Image.Width * wm / 2 + ImageOffsetX;
					float y = gp.Y + (float)gp.Height / 2 - Image.Height * wm / 2 + ImageOffsetY;
					spriteLayer.Draw(Image, x, y, Image.Width * wm, Image.Height * wm, ImageColor, clipRectIndex);
				}
				else
				{
					float x = gp.X + (float)gp.Width / 2 - Image.Width * hm / 2 + ImageOffsetX;
					float y = gp.Y + (float)gp.Height / 2 - Image.Height * hm / 2 + ImageOffsetY;
					spriteLayer.Draw(Image, x, y, Image.Width * hm, Image.Height * hm, ImageColor, clipRectIndex);
				}
				return;
			}
			if (ImageMode == FrameImageMode.Cropped)
			{
				float wm = (float)gp.Width / Image.Width, hm = (float)gp.Height / Image.Height;
				if (wm > hm)
				{
					float x = gp.X + gp.Width / 2 - Image.Width * wm / 2 + ImageOffsetX;
					float y = gp.Y + gp.Height / 2 - Image.Height * wm / 2 + ImageOffsetY;
					spriteLayer.Draw(Image, x, y, Image.Width * wm, Image.Height * wm, ImageColor, clipRectIndex);
				}
				else
				{
					float x = gp.X + gp.Width / 2 - Image.Width * hm / 2 + ImageOffsetX;
					float y = gp.Y + gp.Height / 2 - Image.Height * hm / 2 + ImageOffsetY;
					spriteLayer.Draw(Image, x, y, Image.Width * hm, Image.Height * hm, ImageColor, clipRectIndex);
				}
				return;
			}
		}

		void IFrameInternal.DrawFrameText( SpriteLayer spriteLayer, int clipRectIndex )
		{
			if (string.IsNullOrEmpty(Text))
			{
				return;
			}

			//clipRectIndex = ClippingMode == ClippingMode.None ? 0 : clipRectIndex;

			var r = Font.MeasureStringF(Text);
			int x = 0;
			int y = 0;
			var gp = GetPaddedRectangle();

			int hAlign = 0;
			int vAlign = 0;

			switch (TextAlignment)
			{
				case Alignment.TopLeft: hAlign = -1; vAlign = -1; break;
				case Alignment.TopCenter: hAlign = 0; vAlign = -1; break;
				case Alignment.TopRight: hAlign = 1; vAlign = -1; break;
				case Alignment.MiddleLeft: hAlign = -1; vAlign = 0; break;
				case Alignment.MiddleCenter: hAlign = 0; vAlign = 0; break;
				case Alignment.MiddleRight: hAlign = 1; vAlign = 0; break;
				case Alignment.BottomLeft: hAlign = -1; vAlign = 1; break;
				case Alignment.BottomCenter: hAlign = 0; vAlign = 1; break;
				case Alignment.BottomRight: hAlign = 1; vAlign = 1; break;

				case Alignment.BaselineLeft: hAlign = -1; vAlign = 2; break;
				case Alignment.BaselineCenter: hAlign = 0; vAlign = 2; break;
				case Alignment.BaselineRight: hAlign = 1; vAlign = 2; break;
			}

			if (hAlign < 0) x = gp.X;
			if (hAlign == 0) x = gp.X + (int)(gp.Width / 2 - r.Width / 2);
			if (hAlign > 0) x = gp.X + (int)(gp.Width - r.Width);

			if (vAlign < 0) y = gp.Y + (int)(0);
			if (vAlign == 0) y = gp.Y + (int)(Font.CapHeight / 2 - Font.BaseLine + gp.Height / 2);
			if (vAlign > 0) y = gp.Y + (int)(gp.Height - Font.LineHeight);
			if (vAlign == 2) y = gp.Y - Font.BaseLine;

			/*if (TextAlignment==Alignment.BaselineLeft) {
				x	=	gp.X;
				y	=	gp.Y - Font.BaseLine;
			}

			if (TextAlignment==Alignment.BaselineLeft) {
				x	=	gp.X;
				y	=	gp.Y - Font.BaseLine;
			}

			if (TextAlignment==Alignment.BaselineLeft) {
				x	=	gp.X;
				y	=	gp.Y - Font.BaseLine;
			} */

			/*if (TextEffect==TextEffect.Shadow) {
				Font.DrawString( sb, Text, x + TextOffsetX+1, y + TextOffsetY+1, ShadowColor, 0, false );
			} */

			if (ShadowColor.A != 0)
			{
				Font.DrawString(spriteLayer, Text, x + TextOffsetX + ShadowOffset.X, y + TextOffsetY + ShadowOffset.Y, ShadowColor, clipRectIndex, 0, false);
			}

			Font.DrawString(spriteLayer, Text, x + TextOffsetX, y + TextOffsetY, ForeColor, clipRectIndex, 0, false);
		}

		public void ForEachAncestor( Action<TestNewframe> action )
		{
			GetAncestorList().ForEach(f => action(f));
		}

		public void ForEachChildren( Action<TestNewframe> action )
		{
			_children.ToList().ForEach(f => action(f));
		}

		public List<TestNewframe> GetAncestorList()
		{
			var list = new List<TestNewframe>();

			var frame = this;

			while (frame != null)
			{
				list.Add(frame);
				frame = frame._parent;
			}

			return list;
		}

		public Rectangle GetBorderedRectangle()
		{
			return new Rectangle(
				GlobalRectangle.X + BorderLeft,
				GlobalRectangle.Y + BorderTop,
				Width - BorderLeft - BorderRight,
				Height - BorderTop - BorderBottom);
		}

		public Rectangle GetPaddedRectangle( bool global = true)
		{
			int x = global ? GlobalRectangle.X : 0;
			int y = global ? GlobalRectangle.Y : 0;

			return new Rectangle(
				x + BorderLeft + PaddingLeft,
				y + BorderTop + PaddingTop,
				Width - BorderLeft - BorderRight - PaddingLeft - PaddingRight,
				Height - BorderTop - BorderBottom - PaddingTop - PaddingBottom);
		}

		void IFrameInternal.Init()
		{
			Padding = 0;
			Visible = true;
			Enabled = true;
			AutoWidth = false;
			AutoHeight = false;
			Font = ui?.DefaultFont;
			ForeColor = Color.White;
			Border = 0;
			BorderColor = Color.White;
			ShadowColor = Color.Zero;
			OverallColor = Color.White;

			TextAlignment = Alignment.TopLeft;

			Anchor = FrameAnchor.Left | FrameAnchor.Top;

			ImageColor = Color.White;

			//LayoutChanged	+= (s,e) => RunLayout(true);
			//Resize			+= (s,e) => RunLayout(true);
		}

		public void Insert( int index, TestNewframe frame )
		{
			if (!_children.Contains(frame))
			{
				_children.Insert(index, frame);
				frame._parent = this;
				((IFrameInternal)frame).OnStatusChanged(FrameStatus.None);
			}
		}

		void IFrameInternal.OnActivate()
		{
			if (Activated != null)
			{
				Activated(this, EventArgs.Empty);
			}
		}

		public void OnClick( Point location, Keys key, bool doubleClick )
		{
			int x = location.X - GlobalRectangle.X;
			int y = location.Y - GlobalRectangle.Y;
			if (doubleClick)
			{
				if (DoubleClick != null)
				{
					DoubleClick(this, new MouseEventArgs() { Key = key, X = x, Y = y });
				}
			}
			else
			{
				if (Click != null)
				{
					Click(this, new MouseEventArgs() { Key = key, X = x, Y = y });
				}
			}
		}

		void IFrameInternal.OnDeactivate()
		{
			if (Deactivated != null)
			{
				Deactivated(this, EventArgs.Empty);
			}
		}

		public void OnDrag( int dx, int dy )
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseDrag != null)
			{
				MouseDrag(this, new MouseEventArgs() { Key = Keys.None, X = x, Y = y, DX = dx, DY = dy });
			}
		}

		public void OnMouseDown( Keys key )
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseDown != null)
			{
				MouseDown(this, new MouseEventArgs() { Key = key, X = x, Y = y });
			}
		}

		public void OnMouseIn()
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseIn != null)
			{
				MouseIn(this, new MouseEventArgs() { Key = Keys.None, X = x, Y = y });
			}
		}

		public void OnMouseMove( int dx, int dy )
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseMove != null)
			{
				MouseMove(this, new MouseEventArgs() { Key = Keys.None, X = x, Y = y, DX = dx, DY = dy });
			}
		}

		public void OnMouseOut()
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseOut != null)
			{
				MouseOut(this, new MouseEventArgs() { Key = Keys.None, X = x, Y = y });
			}
		}

		public void OnMouseUp( Keys key )
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;
			if (MouseUp != null)
			{
				MouseUp(this, new MouseEventArgs() { Key = key, X = x, Y = y });
			}
		}

		public void OnMouseWheel( int wheel )
		{
			if (MouseWheel != null)
			{
				MouseWheel(this, new MouseEventArgs() { Wheel = wheel });
			}
			else if (Parent != null)
			{
				Parent.OnMouseWheel(wheel);
			}
		}

		FrameStatus oldStatus = FrameStatus.None;

		void IFrameInternal.OnStatusChanged( FrameStatus status )
		{
			if (StatusChanged != null)
			{
				oldStatus = status;
				StatusChanged(this, new StatusEventArgs() { Status = status });
			}
		}

		void IFrameInternal.OnTick()
		{
			if (Tick != null)
			{
				Tick(this, EventArgs.Empty);
			}
		}

		public void Remove( TestNewframe frame )
		{
			if (this._children.Contains(frame))
			{
				this._children.Remove(frame);
				frame._parent = this;
			}
		}

		public void ReorderChildren()
		{
			_children = _children.OrderBy(f => f.ZOrder).ToList();
			int j = 0;
			for (int i = 0; i < _children.Count; i++)
			{
				_children[i]._zOrder = j++;
			}
		}

		List<ITransition> _transitions = new List<ITransition>();

		public void RunTransition<T, I>( string property, T targetValue, int delay, int period, Action callback ) where I : IInterpolator<T>, new()
		{
			var pi = GetType().GetProperty(property);

			if (pi.PropertyType != typeof(T))
			{
				throw new ArgumentException(string.Format("Bad property and types: {0} is {1}, but values are {2}", property, pi.PropertyType, typeof(T)));
			}

			//	call ToList() to terminate LINQ evaluation :
			var toCancel = _transitions.Where(t => t.TagName == property).ToList();

			_transitions.Add(new Transition<T, I>(this, pi, targetValue, delay, period, toCancel, callback) { TagName = property });
		}

		public void RunTransition( string property, Color targetValue, int delay, int period, Action callback )
		{
			RunTransition<Color, ColorInterpolator>(property, targetValue, delay, period, callback);
		}

		public void RunTransition( string property, int targetValue, int delay, int period, Action callback )
		{
			RunTransition<int, IntInterpolator>(property, targetValue, delay, period, callback);
		}

		public void RunTransition( string property, float targetValue, int delay, int period, Action callback )
		{
			RunTransition<float, FloatInterpolator>(property, targetValue, delay, period, callback);
		}

		int IFrameInternal.SafeHalfOffset( int oldV, int newV, int x )
		{
			int dw = newV - oldV;

			if ((dw & 1) == 1)
			{

				if (dw > 0)
				{

					if ((oldV & 1) == 1)
					{
						dw++;
					}

				}
				else
				{

					if ((oldV & 1) == 0)
					{
						dw--;
					}
				}

				return x + dw / 2;

			}
			else
			{
				return x + dw / 2;
			}
		}

		void IFrameInternal.Update( GameTime gameTime )
		{
		}

		public void UpdateAnchors( int oldW, int oldH, int newW, int newH )
		{
			int dw = newW - oldW;
			int dh = newH - oldH;

			if (!Anchor.HasFlag(FrameAnchor.Left) && !Anchor.HasFlag(FrameAnchor.Right))
			{
				X = ((IFrameInternal)this).SafeHalfOffset(oldW, newW, X);
			}

			if (!Anchor.HasFlag(FrameAnchor.Left) && Anchor.HasFlag(FrameAnchor.Right))
			{
				X = X + dw;
			}

			if (Anchor.HasFlag(FrameAnchor.Left) && !Anchor.HasFlag(FrameAnchor.Right))
			{
			}

			if (Anchor.HasFlag(FrameAnchor.Left) && Anchor.HasFlag(FrameAnchor.Right))
			{
				Width = Width + dw;
			}



			if (!Anchor.HasFlag(FrameAnchor.Top) && !Anchor.HasFlag(FrameAnchor.Bottom))
			{
				Y = ((IFrameInternal)this).SafeHalfOffset(oldH, newH, Y);
			}

			if (!Anchor.HasFlag(FrameAnchor.Top) && Anchor.HasFlag(FrameAnchor.Bottom))
			{
				Y = Y + dh;
			}

			if (Anchor.HasFlag(FrameAnchor.Top) && !Anchor.HasFlag(FrameAnchor.Bottom))
			{
			}

			if (Anchor.HasFlag(FrameAnchor.Top) && Anchor.HasFlag(FrameAnchor.Bottom))
			{
				Height = Height + dh;
			}
		}

		void IFrameInternal.UpdateChildrenUI( FrameProcessor ui )
		{
			if (this.ui != ui)
			{
				this.ui = ui;//
				this.Game = ui.Game;// 
				this.Font = ui.DefaultFont;//
			}
			foreach (var child in this.Children)
			{
				((IFrameInternal)child).UpdateChildrenUI(ui);
			}
		}

		public void UpdateGlobalRect( int px, int py )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.UpdateInternal( GameTime gameTime )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.UpdateMove()
		{
			throw new NotImplementedException();
		}

		int oldX = int.MinValue;
		int oldY = int.MinValue;
		protected int oldW = int.MinValue;
		protected int oldH = int.MinValue;
		protected bool firstResize = true;

		public void UpdateResize( bool UpdateChildren = true )
		{
			((IFrameInternal)this).AutoResize(true);
			if (oldW != Width || oldH != Height)
			{

				Resize?.Invoke(this, new ResizeEventArgs() { Width = Width, Height = Height });


				if (!firstResize && UpdateChildren)
				{
					ForEachChildren(f => f.UpdateAnchors(oldW, oldH, Width, Height));
				}

				firstResize = false;

				oldW = Width;
				oldH = Height;
				if (_parent != null)
				{
					((IFrameInternal)_parent).AutoResize();
				}
			}
		}

		void IFrameInternal.UpdateTransitions( GameTime gameTime )
		{
			throw new NotImplementedException();
		}
	}
}
