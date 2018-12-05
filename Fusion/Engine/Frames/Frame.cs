using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Input;
using Fusion.Engine.Graphics;
using Fusion.Engine.Common;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Fusion.Engine.Frames
{
	public class Frame : INotifyPropertyChanged
	{
		public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

		/// <summary>
		/// Is frame visible. Default true.
		/// </summary>

		public bool Visible { get => _visible; set { _visible = value; OnPropertyChanged(); } }

		/// <summary>
		///
		/// </summary>
		internal bool CanAcceptControl { get { return Visible && OverallColor.A != 0 && !Ghost; } }

		/// <summary>
		///
		/// </summary>
		internal bool IsDrawable { get { return Visible && OverallColor.A != 0; } }

		/// <summary>
		/// Frame visible but does not receive input
		/// </summary>

		public bool Ghost { get => _ghost; set { _ghost = value; OnPropertyChanged(); } }

		/// <summary>
		/// Is frame receive input. Default true.
		/// </summary>

		public bool Enabled { get => _enabled; set { _enabled = value; OnPropertyChanged(); } }

		/// <summary>
		/// Should frame fit its size to content. Default false.
		/// </summary>
		//
		public bool AutoSize { get => _autoSize; set { _autoSize = value; OnPropertyChanged(); } }

		public bool AutoWidth { get => _autoWidth; set { _autoWidth = value; OnPropertyChanged(); } }

		public bool AutoHeight { get => _autoHeight; set { _autoHeight = value; OnPropertyChanged(); } }

		/// <summary>
		/// Text font
		/// </summary>
		[XmlIgnore]
		public virtual SpriteFont Font { get; set; }

		///// <summary>
		///// Tag object
		///// </summary>

		//      public	object		Tag;

		/// <summary>
		///
		/// </summary>

		public ClippingMode ClippingMode { get => _clippingMode; set { _clippingMode = value; OnPropertyChanged(); } }
		/// <summary>
		/// Overall color that used as multiplier
		/// for all children elements
		/// </summary>

		public virtual Color OverallColor { get => _overallColor; set { _overallColor = value; OnPropertyChanged(); } }

		/// <summary>
		/// Background color
		/// </summary>

		public virtual Color BackColor { get => _backColor; set { _backColor = value; OnPropertyChanged(); } }

		/// <summary>
		/// Background color
		/// </summary>

		public virtual Color BorderColor { get => _borderColor; set { _borderColor = value; OnPropertyChanged(); } }

		/// <summary>
		/// Foreground (e.g. text) color
		/// </summary>

		public virtual Color ForeColor { get => _foreColor; set { _foreColor = value; OnPropertyChanged(); } }

		/// <summary>
		/// Text shadow color
		/// </summary>

		public virtual Color ShadowColor { get => _shadowColor; set { _shadowColor = value; OnPropertyChanged(); } }

		/// <summary>
		/// Shadow offset
		/// </summary>

		public virtual Vector2 ShadowOffset { get => _shadowOffset; set { _shadowOffset = value; OnPropertyChanged(); } }

		/// <summary>
		/// Local X position of the frame
		/// </summary>

		public virtual int X { get => _x; set { _x = value; OnPropertyChanged(); } }

		/// <summary>
		/// Local Y position of the frame
		/// </summary>

		public virtual int Y { get => _y; set { _y = value; OnPropertyChanged(); } }

		/// <summary>
		///	Width of the frame
		/// </summary>

		public virtual int Width { get => _width; set { _width = value; OnPropertyChanged(); } }

		/// <summary>
		///	Height of the frame
		/// </summary>

		public virtual int Height { get => _height; set { _height = value; OnPropertyChanged(); } }

		/// <summary>
		/// Left gap between frame and its content
		/// </summary>

		public virtual int PaddingLeft { get => _paddingLeft; set { _paddingLeft = value; OnPropertyChanged(); } }

		/// <summary>
		/// Right gap between frame and its content
		/// </summary>

		public virtual int PaddingRight { get => _paddingRight; set { _paddingRight = value; OnPropertyChanged(); } }

		/// <summary>
		/// Top gap  between frame and its content
		/// </summary>

		public virtual int PaddingTop { get => _paddingTop; set { _paddingTop = value; OnPropertyChanged(); } }

		/// <summary>
		/// Bottom gap  between frame and its content
		/// </summary>

		public virtual int PaddingBottom { get => _paddingBottom; set { _paddingBottom = value; OnPropertyChanged(); } }

		/// <summary>
		/// Top and bottom padding
		/// </summary>
		public virtual int VPadding { set { PaddingBottom = PaddingTop = value; } }

		/// <summary>
		///	Left and right padding
		/// </summary>
		public virtual int HPadding { set { PaddingLeft = PaddingRight = value; } }

		/// <summary>
		/// Top, bottom, left and right padding
		/// </summary>
		public virtual int Padding { set { VPadding = HPadding = value; } }

		/// <summary>
		///
		/// </summary>

		public virtual int BorderTop { get => _borderTop; set { _borderTop = value; OnPropertyChanged(); } }

		/// <summary>
		///
		/// </summary>

		public virtual int BorderBottom { get => _borderBottom; set { _borderBottom = value; OnPropertyChanged(); } }

		/// <summary>
		///
		/// </summary>

		public virtual int BorderLeft { get => _borderLeft; set { _borderLeft = value; OnPropertyChanged(); } }

		/// <summary>
		///
		/// </summary>

		public virtual int BorderRight { get => _borderRight; set { _borderRight = value; OnPropertyChanged(); } }

		/// <summary>
		///
		/// </summary>
		public virtual int Border { set { BorderTop = BorderBottom = BorderLeft = BorderRight = value; } }

		/// <summary>
		///
		/// </summary>

		public virtual string Text { get => _text; set { _text = value; OnPropertyChanged(); } }
		/// <summary>
		///
		/// </summary>

		public Alignment TextAlignment { get => _textAlignment; set { _textAlignment = value; OnPropertyChanged(); } }

		/// <summary>
		///
		/// </summary>

		public virtual int TextOffsetX { get => _textOffsetX; set { _textOffsetX = value; OnPropertyChanged(); } }

		/// <summary>
		///
		/// </summary>

		public virtual int TextOffsetY { get => _textOffsetY; set { _textOffsetY = value; OnPropertyChanged(); } }

		/// <summary>
		///
		/// </summary>

		public TextEffect TextEffect { get => _textEffect; set { _textEffect = value; OnPropertyChanged(); } }

		/// <summary>
		///
		/// </summary>

		public FrameAnchor Anchor { get => _anchor; set { _anchor = value; OnPropertyChanged(); } }



		public virtual int ImageOffsetX { get => _imageOffsetX; set { _imageOffsetX = value; OnPropertyChanged(); } }

		public virtual int ImageOffsetY { get => _imageOffsetY; set { _imageOffsetY = value; OnPropertyChanged(); } }

		public FrameImageMode ImageMode { get => _imageMode; set { _imageMode = value; OnPropertyChanged(); } }

		public Color ImageColor { get => _imageColor; set { _imageColor = value; OnPropertyChanged(); } }

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
					image = Game.Instance.Content.Load<DiscTexture>(imageName);
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
				OnPropertyChanged();
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
				OnPropertyChanged();
			}
		}


		///// <summary>
		/////
		///// </summary>

		//public LayoutEngine	Layout	{
		//	get { return layout; }
		//	set { layout = value; if (LayoutChanged!=null) LayoutChanged(this, EventArgs.Empty); }
		//}

		//LayoutEngine layout = null;


		#region	Events

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
		#endregion


		/// <summary>
		/// Gets list of frame children
		/// </summary>


		//ILIst
		public ObservableCollection<Frame> Children
		{
			get { return children; }
			set
			{
				foreach (Frame child in value)
				{
					this.Add(child);
				}
				OnPropertyChanged();
			}
		}


		/// <summary>
		/// Gets frame
		/// </summary>
		[XmlIgnore]
		public Frame Parent { get { return parent; } /*internal set { } */}

		/// <summary>
		/// Global frame rectangle made
		/// after all layouting and transitioning operation
		/// </summary>
		[XmlIgnore]
		public Rectangle GlobalRectangle { get; private set; }


        /// <summary>
        /// Constructs basic frame
        /// </summary>
        public Frame() : this(0, 0, 1, 1, "", Color.Zero) { }

	    /// <summary>
	    /// Constructs basic frame
	    /// </summary>
	    /// <param name="x"></param>
	    /// <param name="y"></param>
	    /// <param name="w"></param>
	    /// <param name="h"></param>
	    /// <param name="text"></param>
	    /// <param name="backColor"></param>
	    public Frame(int x, int y, int w, int h, string text, Color backColor)
	    {
	        Init();

	        X = x;
	        Y = y;
	        Width = w;
	        Height = h;
	        Text = text;
	        BackColor = backColor;
	    }

	    #region Obsolete Constructors
        /// <inheritdoc />
        /// <param name="ui">Not used anymore</param>
        [Obsolete("Please use parameter-less constructor instead")]
	    public Frame(FrameProcessor ui) : this() { }

        /// <inheritdoc />
        /// <param name="ui">Not used anymore</param>
        [Obsolete("Please use version without FrameProcessor")]
	    public Frame(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : this(x, y, w, h, text, backColor) { }
	    #endregion

        /// <summary>
        /// Common init
        /// </summary>
        private void Init()
		{
			Padding = 0;
			Visible = true;
			Enabled = true;
			AutoWidth = false;
			AutoHeight = false;
			Font = Game.Instance.Content.Load<SpriteFont>(@"Fonts\textFont");
            ForeColor = Color.White;
			Border = 0;
			BorderColor = Color.White;
			ShadowColor = Color.Zero;
			OverallColor = Color.White;
		    Name = GenerateName(GetType());

			TextAlignment = Alignment.TopLeft;

			Anchor = FrameAnchor.Left | FrameAnchor.Top;

			ImageColor = Color.White;
		}


		/*-----------------------------------------------------------------------------------------
		 *
		 *	Hierarchy stuff
		 *
		-----------------------------------------------------------------------------------------*/

		private ObservableCollection<Frame> children = new ObservableCollection<Frame>();
		protected Frame parent = null;
		private int zOrder = 0;


		public int ZOrder
		{
			get { return zOrder; }
			set
			{
				zOrder = value;
				parent?.ReorderChildren();
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Adds frame
		/// </summary>
		/// <param name="frame"></param>
		public virtual void Add( Frame frame )
		{
			if (!children.Contains(frame))
			{
				children.Add(frame);
				// frame.UpdateChildrenUI(this.ui);
				if (frame.ZOrder == 0)
				{
					frame.ZOrder = children.Count;
				}
				ReorderChildren();
				frame.parent = this;

				frame.OnStatusChanged(FrameStatus.None);
				OnPropertyChanged("Children");
			}
		}

		public void UpdateChildrenUI( FrameProcessor ui )
		{
		    // this.Font = ui.DefaultFont;

			foreach (var child in this.Children)
			{
				child.UpdateChildrenUI(ui);
			}
		}

		internal void RestoreParents()
		{
			foreach (var child in Children)
			{
				child.parent = this;
				child.RestoreParents();
			}
		}

		/// <summary>
		/// Reordering children
		/// </summary>
		///
		public void ReorderChildren()
		{
			children = new ObservableCollection<Frame>(children.OrderBy(f => f.ZOrder));
			int j = 0;
			for (int i = 0; i < children.Count; i++)
			{
				children[i].zOrder = j++;
			}
		}
		/// <summary>
		///
		/// </summary>
		/// <param name="frame"></param>
		public virtual void Clear( Frame frame )
		{
			foreach (var child in children)
			{
				child.parent = null;
			}
			children.Clear();
			OnPropertyChanged("Children");
		}


		/// <summary>
		/// Inserts frame at specified index
		/// </summary>
		/// <param name="index"></param>
		/// <param name="frame"></param>
		public void Insert( int index, Frame frame )
		{
			if (!children.Contains(frame))
			{
				children.Insert(index, frame);
				frame.parent = this;
				frame.OnStatusChanged(FrameStatus.None);
				OnPropertyChanged("Children");
			}
		}



		/// <summary>
		///
		/// </summary>
		/// <param name="frame"></param>
		public void Remove( Frame frame )
		{
			if (this.children.Contains(frame))
			{
				this.children.Remove(frame);
				frame.parent = null;
				OnPropertyChanged("Children");
			}
		}



		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public List<Frame> GetAncestorList()
		{
			var list = new List<Frame>();

			var frame = this;

			while (frame != null)
			{
				list.Add(frame);
				frame = frame.parent;
			}

			return list;
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="action"></param>
		public void ForEachAncestor( Action<Frame> action )
		{
			GetAncestorList().ForEach(f => action(f));
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="action"></param>
		public void ForEachChildren( Action<Frame> action )
		{
			children.ToList().ForEach(f => action(f));
		}


		/*-----------------------------------------------------------------------------------------
		 *
		 *	Input stuff :
		 *
		-----------------------------------------------------------------------------------------*/

		FrameStatus oldStatus = FrameStatus.None;

		internal void OnStatusChanged( FrameStatus status )
		{
			if (StatusChanged != null)
			{
				oldStatus = status;
				StatusChanged(this, new StatusEventArgs() { Status = status });
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


		public virtual void OnMouseIn()
		{
			int x = Game.Instance.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.Instance.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseIn != null)
			{
				MouseIn(this, new MouseEventArgs() { Key = Keys.None, X = x, Y = y });
			}
		}

		private bool IsMouseMove = false;

		public virtual void OnMouseMove( int dx, int dy )
		{
			int x = Game.Instance.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.Instance.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseMove != null)
			{
				MouseMove(this, new MouseEventArgs() { Key = Keys.None, X = x, Y = y, DX = dx, DY = dy });
			}
		}

		public virtual void OnDrag( int dx, int dy )
		{
			int x = Game.Instance.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.Instance.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseDrag != null)
			{
				MouseDrag(this, new MouseEventArgs() { Key = Keys.None, X = x, Y = y, DX = dx, DY = dy });
			}
		}


		public virtual void OnMouseOut()
		{
			int x = Game.Instance.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.Instance.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseOut != null)
			{
				MouseOut(this, new MouseEventArgs() { Key = Keys.None, X = x, Y = y });
			}
		}


		public virtual void OnMouseDown( Keys key )
		{
			int x = Game.Instance.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.Instance.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseDown != null)
			{
				MouseDown(this, new MouseEventArgs() { Key = key, X = x, Y = y });
			}
		}


		public virtual void OnMouseUp( Keys key )
		{
			int x = Game.Instance.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.Instance.InputDevice.MousePosition.Y - GlobalRectangle.Y;
			if (MouseUp != null)
			{
				MouseUp(this, new MouseEventArgs() { Key = key, X = x, Y = y });
			}
		}


		public virtual void OnMouseWheel( int wheel )
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


		internal void OnTick()
		{
			if (Tick != null)
			{
				Tick(this, EventArgs.Empty);
			}
		}

		internal void OnActivate()
		{
			if (Activated != null)
			{
				Activated(this, EventArgs.Empty);
			}
		}

		internal void OnDeactivate()
		{
			if (Deactivated != null)
			{
				Deactivated(this, EventArgs.Empty);
			}
		}

		/*-----------------------------------------------------------------------------------------
		 *
		 *	Update and draw stuff :
		 *
		-----------------------------------------------------------------------------------------*/

		public static List<Frame> BFSList( Frame v )
		{
			Queue<Frame> Q = new Queue<Frame>();
			List<Frame> list = new List<Frame>();

			Q.Enqueue(v);

			while (Q.Any())
			{

				var t = Q.Dequeue();
				list.Add(t);

				foreach (var u in t.Children)
				{
					Q.Enqueue(u);
				}
			}

			return list;
		}


		public void UpdateGlobalRect( int px, int py )
		{
			GlobalRectangle = new Rectangle(X + px, Y + py, Width, Height);
			ForEachChildren(ch => ch.UpdateGlobalRect(px + X, py + Y));
			OnPropertyChanged("GlobalRectangle");
		}



		/// <summary>
		///
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="parentX"></param>
		/// <param name="parentY"></param>
		/// <param name="frame"></param>
		internal void UpdateInternal( GameTime gameTime )
		{
			var bfsList = BFSList(this);
			var bfsListR = bfsList.ToList();
			bfsListR.Reverse();
			//bfsList.Reverse();

			UpdateGlobalRect(0, 0);

			bfsList.ForEach(f => f.UpdateTransitions(gameTime));

			UpdateGlobalRect(0, 0);

			bfsList.ForEach(f => f.UpdateMove());
			bfsList.ForEach(f => f.UpdateResize());

			UpdateGlobalRect(0, 0);

			bfsList.ForEach(f => f.OnTick());
			bfsList.ForEach(f => f.Update(gameTime));
		}

		private class DrawFrameItem
		{
			public DrawFrameItem( Frame frame, Color color, Rectangle outerClip, Rectangle innerClip, string text )
			{
				Frame = frame;
				OuterClip = outerClip;
				InnerClip = innerClip;
				Color = color;
				Text = text;
			}

			public readonly Frame Frame;
			public readonly Color Color;
			public readonly Rectangle OuterClip;
			public readonly Rectangle InnerClip;
			public readonly string Text;
		}

        /// <summary>
        /// Draws frame and all it's descendants in non-recursive manner.
        /// </summary>
        /// <param name="rootFrame">Frame to start with</param>
        /// <param name="gameTime">World time</param>
        /// <param name="spriteLayer">Target sprite layer to draw into</param>
        internal static void DrawNonRecursive( Frame rootFrame, GameTime gameTime, SpriteLayer spriteLayer )
		{
			if (rootFrame == null)
			{
				return;
			}

			var stack = new Stack<DrawFrameItem>();
			var list = new List<DrawFrameItem>();

			stack.Push(new DrawFrameItem(rootFrame, Color.White, rootFrame.GlobalRectangle, rootFrame.GetBorderedRectangle(), rootFrame.Text));


			while (stack.Any())
			{

				var currentDrawFrame = stack.Pop();

				if (!currentDrawFrame.Frame.IsDrawable)
				{
					continue;
				}

				list.Add(currentDrawFrame);
				//currentDrawFrame.Frame.Children.Reverse();
				//var reversedChildren = currentDrawFrame.Frame.Children;
				foreach (var child in currentDrawFrame.Frame.Children.Reverse())
				{

					var color = currentDrawFrame.Color * child.OverallColor;
					var inner = Clip(child.GetBorderedRectangle(), currentDrawFrame.InnerClip);
					var outer = Clip(child.GlobalRectangle, currentDrawFrame.InnerClip);

					if (MathUtil.IsRectInsideRect(child.GlobalRectangle, currentDrawFrame.InnerClip))
					{
						stack.Push(new DrawFrameItem(child, color, outer, inner, currentDrawFrame.Text + "-" + child.Text));
					}
				}
				//currentDrawFrame.Frame.Children.Reverse();
			}

			for (var i = 0; i < list.Count; i++)
			{
				var drawFrame = list[i];

				spriteLayer.SetClipRectangle(i * 2 + 0, drawFrame.OuterClip, drawFrame.Color);
				spriteLayer.SetClipRectangle(i * 2 + 1, drawFrame.InnerClip, drawFrame.Color);

				drawFrame.Frame.DrawFrameBorders(spriteLayer, i * 2 + 0);
				drawFrame.Frame.DrawFrame(gameTime, spriteLayer, i * 2 + 1);
			}
		}



		/// <summary>
		/// Clips one rectangle by another.
		/// </summary>
		/// <param name="child"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		static Rectangle Clip( Rectangle child, Rectangle parent )
		{
			var r = new Rectangle();

			r.Left = Math.Max(child.Left, parent.Left);
			r.Right = Math.Min(child.Right, parent.Right);
			r.Top = Math.Max(child.Top, parent.Top);
			r.Bottom = Math.Min(child.Bottom, parent.Bottom);

			return r;
		}



		/// <summary>
		/// Updates frame stuff.
		/// </summary>
		/// <param name="gameTime"></param>
		protected virtual void Update( GameTime gameTime )
		{
		}



		/// <summary>
		/// Draws frame stuff
		/// </summary>
		void DrawFrameBorders( SpriteLayer spriteLayer, int clipRectIndex )
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

			var whiteTex = Game.Instance.RenderSystem.WhiteTexture;

			var clr = BorderColor;

			spriteLayer.Draw(whiteTex, gx, gy, w, bt, clr, clipRectIndex);
			spriteLayer.Draw(whiteTex, gx, gy + h - bb, w, bb, clr, clipRectIndex);
			spriteLayer.Draw(whiteTex, gx, gy + bt, bl, h - bt - bb, clr, clipRectIndex);
			spriteLayer.Draw(whiteTex, gx + w - br, gy + bt, br, h - bt - bb, clr, clipRectIndex);

		    var rectangle = GetBorderedRectangle();
		    spriteLayer.Draw(whiteTex, rectangle, BackColor, clipRectIndex);
		}


	    /// <summary>
	    /// Draws frame stuff.
	    /// </summary>
	    /// <param name="gameTime"></param>
	    /// <param name="spriteLayer">Target layer to draw</param>
	    /// <param name="clipRectIndex"></param>
	    protected virtual void DrawFrame( GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex )
		{
			DrawFrameImage(spriteLayer, clipRectIndex);
			DrawFrameText(spriteLayer, clipRectIndex);
		}



		/// <summary>
		/// Adjusts frame size to content, text, image etc.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="?"></param>
		protected virtual void Adjust()
		{
			throw new NotImplementedException();
		}



		/*-----------------------------------------------------------------------------------------
		 *
		 *	Utils :
		 *
		-----------------------------------------------------------------------------------------*/

		int oldX = int.MinValue;
		int oldY = int.MinValue;
		protected int oldW = int.MinValue;
		protected int oldH = int.MinValue;
		protected bool firstResize = true;


		/// <summary>
		/// Checks move and resize and calls appropriate events
		/// </summary>
		protected void UpdateMove()
		{
			if (oldX != X || oldY != Y)
			{
				if (Move != null)
				{
					Move(this, new MoveEventArgs() { X = X, Y = Y });
				}
				oldX = X;
				oldY = Y;
			}
		}



		/// <summary>
		///
		/// </summary>
		public virtual void UpdateResize( bool UpdateChildren = true )
		{
			AutoResize(true);
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
				if (parent != null)
				{
					parent.AutoResize();
				}
			}

		}

		protected void AutoResize( bool fromUpdate = false )
		{
			if (AutoWidth || AutoHeight && children.Count != 0)
			{
				Rectangle rect = new Rectangle(0, 0, 0, 0);

				foreach (var frame in children.ToList())
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



		///// <summary>
		/////
		///// </summary>
		///// <param name="forceTransitions"></param>
		//      public void RunLayout (bool forceTransitions)
		//{
		//	if (layout!=null && !ui.SuppressLayout) {
		//		layout.RunLayout( this, forceTransitions );
		//	}
		//}



		/// <summary>
		/// Get global rectangle bound by borders
		/// </summary>
		/// <returns></returns>
		public Rectangle GetBorderedRectangle()
		{
			return new Rectangle(
				GlobalRectangle.X + BorderLeft,
				GlobalRectangle.Y + BorderTop,
				Width - BorderLeft - BorderRight,
				Height - BorderTop - BorderBottom);
		}



		/// <summary>
		/// Get global rectangle padded and bound by borders
		/// </summary>
		/// <returns></returns>
		public Rectangle GetPaddedRectangle( bool global = true )
		{
			int x = global ? GlobalRectangle.X : 0;
			int y = global ? GlobalRectangle.Y : 0;

			return new Rectangle(
				x + BorderLeft + PaddingLeft,
				y + BorderTop + PaddingTop,
				Width - BorderLeft - BorderRight - PaddingLeft - PaddingRight,
				Height - BorderTop - BorderBottom - PaddingTop - PaddingBottom);
		}



		/// <summary>
		///
		/// </summary>
		protected virtual void DrawFrameImage( SpriteLayer spriteLayer, int clipRectIndex )
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



		/// <summary>
		/// Draws string
		/// </summary>
		/// <param name="text"></param>
		protected virtual void DrawFrameText( SpriteLayer spriteLayer, int clipRectIndex )
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


		/*-----------------------------------------------------------------------------------------
		 *
		 *	Animation stuff :
		 *
		-----------------------------------------------------------------------------------------*/

		List<ITransition> transitions = new List<ITransition>();


		private string _name;
		private bool _visible;
		private bool _ghost;
		private bool _enabled;
		private bool _autoHeight;
		private bool _autoWidth;
		private bool _autoSize;
		private ClippingMode _clippingMode = ClippingMode.ClipByFrame;
		private Color _overallColor;
		private Color _backColor;
		private Color _borderColor;
		private Color _foreColor;
		private Color _shadowColor;
		private Vector2 _shadowOffset;
		private int _x;
		private int _y;
		private int _width;
		private int _height;
		private int _paddingLeft;
		private int _paddingRight;
		private int _paddingTop;
		private int _paddingBottom;
		private int _borderTop;
		private int _borderBottom;
		private int _borderLeft;
		private int _borderRight;
		private string _text;
		private Alignment _textAlignment;
		private int _textOffsetX;
		private int _textOffsetY;
		private TextEffect _textEffect;
		private FrameAnchor _anchor;
		private int _imageOffsetX;
		private int _imageOffsetY;
		private FrameImageMode _imageMode;
		private Color _imageColor;


		/// <summary>
		/// Pushes new transition to the chain of animation transitions.
		/// Origin value will be retrived when transition starts.
		/// When one of the newest transitions starts, previous transitions on same property will be terminated.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="I"></typeparam>
		/// <param name="property"></param>
		/// <param name="termValue"></param>
		/// <param name="delay"></param>
		/// <param name="period"></param>
		public void RunTransition<T, I>( string property, T targetValue, int delay, int period, Action callback = null ) where I : IInterpolator<T>, new()
		{
			var pi = GetType().GetProperty(property);

			if (pi.PropertyType != typeof(T))
			{
				throw new ArgumentException(string.Format("Bad property and types: {0} is {1}, but values are {2}", property, pi.PropertyType, typeof(T)));
			}

			//	call ToList() to terminate LINQ evaluation :
			var toCancel = transitions.Where(t => t.TagName == property).ToList();

			transitions.Add(new Transition<T, I>(this, pi, targetValue, delay, period, toCancel, callback) { TagName = property });
		}



		/// <summary>
		///
		/// </summary>
		/// <param name="property"></param>
		/// <param name="targetValue"></param>
		/// <param name="delay"></param>
		/// <param name="period"></param>
		public void RunTransition( string property, Color targetValue, int delay, int period, Action callback = null )
		{
			RunTransition<Color, ColorInterpolator>(property, targetValue, delay, period, callback);
		}



		/// <summary>
		///
		/// </summary>
		/// <param name="property"></param>
		/// <param name="targetValue"></param>
		/// <param name="delay"></param>
		/// <param name="period"></param>
		public void RunTransition( string property, int targetValue, int delay, int period, Action callback = null )
		{
			RunTransition<int, IntInterpolator>(property, targetValue, delay, period, callback);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="property"></param>
		/// <param name="targetValue"></param>
		/// <param name="delay"></param>
		/// <param name="period"></param>
		public void RunTransition( string property, float targetValue, int delay, int period, Action callback = null )
		{
			RunTransition<float, FloatInterpolator>(property, targetValue, delay, period, callback);
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="gameTime"></param>
		void UpdateTransitions( GameTime gameTime )
		{
			foreach (var t in transitions)
			{
				t.Update(gameTime);
			}
			var done = transitions.Where(t => t.IsDone);
			foreach (var t in done)
			{
				t.Callback?.Invoke();
			}
			transitions.RemoveAll(t => t.IsDone);
		}



		/*-----------------------------------------------------------------------------------------
		 *
		 *	Anchors :
		 *
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Incrementally preserving half offset
		/// </summary>
		/// <param name="oldV"></param>
		/// <param name="newV"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		int SafeHalfOffset( int oldV, int newV, int x )
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


		/// <summary>
		///
		/// </summary>
		/// <param name="oldW"></param>
		/// <param name="oldH"></param>
		/// <param name="newW"></param>
		/// <param name="newH"></param>
		public virtual void UpdateAnchors( int oldW, int oldH, int newW, int newH )
		{
			int dw = newW - oldW;
			int dh = newH - oldH;

			if (!Anchor.HasFlag(FrameAnchor.Left) && !Anchor.HasFlag(FrameAnchor.Right))
			{
				X = SafeHalfOffset(oldW, newW, X);
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
				Y = SafeHalfOffset(oldH, newH, Y);
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


		/*-----------------------------------------------------------------------------------------
		 *
		 *	Layouting :
		 *
		-----------------------------------------------------------------------------------------*/

		#region Serialization
		/*-----------------------------------------------------------------------------------------
         *
         *	Serialization :
         *
        -----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Serializes the frame.
		/// </summary>
		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write(this.children.Count);
			foreach (var child in this.Children)
			{
				writer.Write(Assembly.GetAssembly(child.GetType()).FullName);
				writer.Write(child.GetType().FullName);
				child.Serialize(writer);
			}

			writer.Write(this.Name ?? "");
			writer.Write(this.Visible);
			writer.Write(this.Ghost);
			writer.Write(this.Enabled);
			writer.Write(this.AutoSize);
			writer.Write(this.AutoWidth);
			writer.Write(this.AutoHeight);
			writer.Write((int)this.ClippingMode);
			writer.Write(this.OverallColor);
			writer.Write(this.BackColor);
			writer.Write(this.BorderColor);
			writer.Write(this.ForeColor);
			writer.Write(this.ShadowColor);
			writer.Write(this.ShadowOffset);
			writer.Write(this.X);
			writer.Write(this.Y);
			writer.Write(this.Width);
			writer.Write(this.Height);
			writer.Write(this.PaddingLeft);
			writer.Write(this.PaddingRight);
			writer.Write(this.PaddingTop);
			writer.Write(this.PaddingBottom);
			writer.Write(this.BorderTop);
			writer.Write(this.BorderBottom);
			writer.Write(this.BorderLeft);
			writer.Write(this.BorderRight);
			writer.Write(this.Text ?? "");
			writer.Write((int)this.TextAlignment);
			writer.Write(this.TextOffsetX);
			writer.Write(this.TextOffsetY);
			writer.Write((int)this.TextEffect);
			writer.Write((int)this.Anchor);
			writer.Write(this.ImageOffsetX);
			writer.Write(this.ImageOffsetY);
			writer.Write((int)this.ImageMode);
			writer.Write(this.ImageColor);
			writer.Write(this.Image?.Name ?? "");
			writer.Write(this.ZOrder);
			writer.Write(this.GlobalRectangle);
		}

		/// <summary>
		/// Deerializes the frame.
		/// </summary>
		public virtual void Deserialize( BinaryReader reader )
		{
			//this.children = new List<Frame>();
			//this.transitions = new List<ITransition>();

			var childrenCount = reader.ReadInt32();
			var childTypeName = "";
			var assemblyName = "";
			object child;

			for (int i = 0; i < childrenCount; i++)
			{
				assemblyName = reader.ReadString();
				childTypeName = reader.ReadString();
				var assembly = Assembly.Load(assemblyName);
				var childType = assembly.GetType(childTypeName);
				//child = FormatterServices.GetUninitializedObject(childType);
				child = Activator.CreateInstance(childType, null);

				childType.GetMethod("Deserialize").Invoke(child, new object[] { reader });
				this.Children.Add(child as Frame);
			}

			this.Name = reader.ReadString();
			this.Visible = reader.ReadBoolean();
			this.Ghost = reader.ReadBoolean();
			this.Enabled = reader.ReadBoolean();
			this.AutoSize = reader.ReadBoolean();
			this.AutoWidth = reader.ReadBoolean();
			this.AutoHeight = reader.ReadBoolean();
			this.ClippingMode = (ClippingMode)reader.ReadInt32();
			this.OverallColor = reader.Read<Color>();
			this.BackColor = reader.Read<Color>();
			this.BorderColor = reader.Read<Color>();
			this.ForeColor = reader.Read<Color>();
			this.ShadowColor = reader.Read<Color>();
			this.ShadowOffset = reader.Read<Vector2>();
			this.X = reader.ReadInt32();
			this.Y = reader.ReadInt32();
			this.Width = reader.ReadInt32();
			this.Height = reader.ReadInt32();
			this.PaddingLeft = reader.ReadInt32();
			this.PaddingRight = reader.ReadInt32();
			this.PaddingTop = reader.ReadInt32();
			this.PaddingBottom = reader.ReadInt32();
			this.BorderTop = reader.ReadInt32();
			this.BorderBottom = reader.ReadInt32();
			this.BorderLeft = reader.ReadInt32();
			this.BorderRight = reader.ReadInt32();
			this.Text = reader.ReadString();
			this.TextAlignment = (Alignment)reader.ReadInt32();
			this.TextOffsetX = reader.ReadInt32();
			this.TextOffsetY = reader.ReadInt32();
			this.TextEffect = (TextEffect)reader.ReadInt32();
			this.Anchor = (FrameAnchor)reader.ReadInt32();
			this.ImageOffsetX = reader.ReadInt32();
			this.ImageOffsetY = reader.ReadInt32();
			this.ImageMode = (FrameImageMode)reader.ReadInt32();
			this.ImageColor = reader.Read<Color>();
			this.ImageName = reader.ReadString();
			if (!String.IsNullOrEmpty(ImageName))
			{
				Image = Game.Instance.Content.Load<DiscTexture>(ImageName);
			}
			this.ZOrder = reader.ReadInt32();
			this.GlobalRectangle = reader.Read<Rectangle>();
		}
		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string changedProperty = "" )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(changedProperty));
			if (changedProperty == "Children")
			{
				parent?.OnPropertyChanged("Children");
			}
		}

		public override string ToString()
		{
			return Name;
		}

	    #region Naming

        private static readonly Dictionary<Type, int> GeneratedCountOfType = new Dictionary<Type, int>();
	    private static string GenerateName(Type type)
	    {
	        if (GeneratedCountOfType.TryGetValue(type, out var value))
	        {
	            GeneratedCountOfType[type] = value + 1;
	        }
	        else
	        {
	            GeneratedCountOfType[type] = 1;
	        }

	        return $"{type.Name}_{value}";
        }

	    #endregion
    }
}