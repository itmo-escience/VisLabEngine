using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;

namespace Fusion.Engine.Frames.Abstract
{
	public class TestNewframe : IFrame, IFrameInternal
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
		public Texture Image { get; set; }
		public string ImageName { get; set; }
		public List<IFrame> Children { get; set; }

		public Frame Parent => throw new NotImplementedException();

		public Rectangle GlobalRectangle => throw new NotImplementedException();

		Rectangle IFrameInternal.GlobalRectangle { set => throw new NotImplementedException(); }
		public int ZOrder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public List<ITransition> Transitions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		bool IFrameInternal.CanAcceptControl => throw new NotImplementedException();

		bool IFrameInternal.IsDrawable => throw new NotImplementedException();

		public event EventHandler Tick
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler LayoutChanged
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler Activated
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler Deactivated
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<MouseEventArgs> MouseIn
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<MouseEventArgs> MouseMove
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<MouseEventArgs> MouseDrag
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<MouseEventArgs> MouseOut
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<MouseEventArgs> MouseWheel
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<MouseEventArgs> Click
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<MouseEventArgs> DoubleClick
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<MouseEventArgs> MouseDown
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<MouseEventArgs> MouseUp
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<StatusEventArgs> StatusChanged
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<MoveEventArgs> Move
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public event EventHandler<ResizeEventArgs> Resize
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public void Add( IFrame frame )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.Adjust()
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.AutoResize( bool fromUpdate )
		{
			throw new NotImplementedException();
		}

		public void Clear( IFrame frame )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.DrawFrame( GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.DrawFrameBorders( SpriteLayer spriteLayer, int clipRectIndex )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.DrawFrameImage( SpriteLayer spriteLayer, int clipRectIndex )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.DrawFrameText( SpriteLayer spriteLayer, int clipRectIndex )
		{
			throw new NotImplementedException();
		}

		public void ForEachAncestor( Action<IFrame> action )
		{
			throw new NotImplementedException();
		}

		public void ForEachChildren( Action<IFrame> action )
		{
			throw new NotImplementedException();
		}

		public List<IFrame> GetAncestorList()
		{
			throw new NotImplementedException();
		}

		public Rectangle GetBorderedRectangle()
		{
			throw new NotImplementedException();
		}

		public Rectangle GetPaddedRectangle( bool global )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.Init()
		{
			throw new NotImplementedException();
		}

		public void Insert( int index, IFrame frame )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.OnActivate()
		{
			throw new NotImplementedException();
		}

		public void OnClick( Point location, Keys key, bool doubleClick )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.OnDeactivate()
		{
			throw new NotImplementedException();
		}

		public void OnDrag( int dx, int dy )
		{
			throw new NotImplementedException();
		}

		public void OnMouseDown( Keys key )
		{
			throw new NotImplementedException();
		}

		public void OnMouseIn()
		{
			throw new NotImplementedException();
		}

		public void OnMouseMove( int dx, int dy )
		{
			throw new NotImplementedException();
		}

		public void OnMouseOut()
		{
			throw new NotImplementedException();
		}

		public void OnMouseUp( Keys key )
		{
			throw new NotImplementedException();
		}

		public void OnMouseWheel( int wheel )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.OnStatusChanged( FrameStatus status )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.OnTick()
		{
			throw new NotImplementedException();
		}

		public void Remove( IFrame frame )
		{
			throw new NotImplementedException();
		}

		public void ReorderChildren()
		{
			throw new NotImplementedException();
		}

		public void RunTransition<T, I>( string property, T targetValue, int delay, int period, Action callback ) where I : IInterpolator<T>, new()
		{
			throw new NotImplementedException();
		}

		public void RunTransition( string property, Color targetValue, int delay, int period, Action callback )
		{
			throw new NotImplementedException();
		}

		public void RunTransition( string property, int targetValue, int delay, int period, Action callback )
		{
			throw new NotImplementedException();
		}

		public void RunTransition( string property, float targetValue, int delay, int period, Action callback )
		{
			throw new NotImplementedException();
		}

		int IFrameInternal.SafeHalfOffset( int oldV, int newV, int x )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.Update( GameTime gameTime )
		{
			throw new NotImplementedException();
		}

		public void UpdateAnchors( int oldW, int oldH, int newW, int newH )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.UpdateChildrenUI( FrameProcessor ui )
		{
			throw new NotImplementedException();
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

		public void UpdateResize( bool UpdateChildren )
		{
			throw new NotImplementedException();
		}

		void IFrameInternal.UpdateTransitions( GameTime gameTime )
		{
			throw new NotImplementedException();
		}
	}
}
