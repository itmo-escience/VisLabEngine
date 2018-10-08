using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Input;
using Fusion.Engine.Graphics;
using Fusion.Engine.Common;
using Forms = System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Fusion.Engine.Frames.Abstract;

namespace Fusion.Engine.Frames
{
	public interface IFrame/* : IFrameInternal*/
	{

		[XmlIgnore]
		Game Game { set; get; }
		//
		[XmlIgnore]
		FrameProcessor ui { get; }

		/// <summary>
		/// 
		/// </summary>

		string Name { get; set; }

		/// <summary>
		/// Is frame visible. Default true.
		/// </summary>

		bool Visible { get; set; }



		/// <summary>
		/// Frame visible but does not receive input 
		/// </summary>

		bool Ghost { get; set; }

		/// <summary>
		/// Is frame receive input. Default true.
		/// </summary>

		bool Enabled { get; set; }

		/// <summary>
		/// Should frame fit its size to content. Default false.
		/// </summary>
		//
		bool AutoSize { get; set; }

		bool AutoWidth { get; set; }

		bool AutoHeight { get; set; }

		/// <summary>
		/// Text font
		/// </summary>
		[XmlIgnore]
		SpriteFont Font { get; set; }

		///// <summary>
		///// Tag object
		///// </summary>

		//      	object		Tag;

		/// <summary>
		/// 
		/// </summary>

		ClippingMode ClippingMode { get; set; }

		/// <summary>
		/// Overall color that used as multiplier 
		/// for all children elements
		/// </summary>

		Color OverallColor { get; set; }

		/// <summary>
		/// Background color
		/// </summary>

		Color BackColor { get; set; }

		/// <summary>
		/// Background color
		/// </summary>

		Color BorderColor { get; set; }

		/// <summary>
		/// Foreground (e.g. text) color
		/// </summary>

		Color ForeColor { get; set; }

		/// <summary>
		/// Text shadow color
		/// </summary>

		Color ShadowColor { get; set; }

		/// <summary>
		/// Shadow offset
		/// </summary>

		Vector2 ShadowOffset { get; set; }

		/// <summary>
		/// Local X position of the frame
		/// </summary>

		int X { get; set; }

		/// <summary>
		/// Local Y position of the frame
		/// </summary>

		int Y { get; set; }

		/// <summary>
		///	Width of the frame
		/// </summary>

		int Width { get; set; }

		/// <summary>
		///	Height of the frame
		/// </summary>

		int Height { get; set; }

		/// <summary>
		/// Left gap between frame and its content
		/// </summary>

		int PaddingLeft { get; set; }

		/// <summary>
		/// Right gap between frame and its content
		/// </summary>

		int PaddingRight { get; set; }

		/// <summary>
		/// Top gap  between frame and its content
		/// </summary>

		int PaddingTop { get; set; }

		/// <summary>
		/// Bottom gap  between frame and its content
		/// </summary>

		int PaddingBottom { get; set; }

		/// <summary>
		/// Top and bottom padding
		/// </summary>
		int VPadding { set; }

		/// <summary>
		///	Left and right padding
		/// </summary>
		int HPadding { set; }

		/// <summary>
		/// Top, bottom, left and right padding
		/// </summary>
		int Padding { set; }

		/// <summary>
		/// 
		/// </summary>

		int BorderTop { get; set; }

		/// <summary>
		/// 
		/// </summary>

		int BorderBottom { get; set; }

		/// <summary>
		/// 
		/// </summary>

		int BorderLeft { get; set; }

		/// <summary>
		/// 
		/// </summary>

		int BorderRight { get; set; }

		/// <summary>
		/// 
		/// </summary>
		int Border { set; }

		/// <summary>
		/// 
		/// </summary>

		string Text { get; set; }
		/// <summary>
		/// 
		/// </summary>

		Alignment TextAlignment { get; set; }

		/// <summary>
		/// 
		/// </summary>

		int TextOffsetX { get; set; }

		/// <summary>
		/// 
		/// </summary>

		int TextOffsetY { get; set; }

		/// <summary>
		/// 
		/// </summary>

		TextEffect TextEffect { get; set; }

		/// <summary>
		/// 
		/// </summary>

		FrameAnchor Anchor { get; set; }



		int ImageOffsetX { get; set; }

		int ImageOffsetY { get; set; }

		FrameImageMode ImageMode { get; set; }

		Color ImageColor { get; set; }

		//Texture image;
		[XmlIgnore]
		Texture Image
		{
			get;
			set;
		}

		string ImageName
		{
			get;
			set;
		}

		#region	Events

		event EventHandler Tick;

		event EventHandler LayoutChanged;

		event EventHandler Activated;

		event EventHandler Deactivated;

		event EventHandler<MouseEventArgs> MouseIn;

		event EventHandler<MouseEventArgs> MouseMove;

		event EventHandler<MouseEventArgs> MouseDrag;

		event EventHandler<MouseEventArgs> MouseOut;

		event EventHandler<MouseEventArgs> MouseWheel;

		event EventHandler<MouseEventArgs> Click;

		event EventHandler<MouseEventArgs> DoubleClick;

		event EventHandler<MouseEventArgs> MouseDown;

		event EventHandler<MouseEventArgs> MouseUp;

		event EventHandler<StatusEventArgs> StatusChanged;

		event EventHandler<MoveEventArgs> Move;

		event EventHandler<ResizeEventArgs> Resize;
		#endregion


		/// <summary>
		/// Gets list of frame children
		/// </summary>


		//ILIst

		List<IFrame> Children
		{
			get;
			set;
		}


		/// <summary>
		/// Gets frame
		/// </summary>
		[XmlIgnore]
		IFrame Parent { get; }

		/// <summary>
		/// Global frame rectangle made 
		/// after all layouting and transitioning operation
		/// </summary>
		[XmlIgnore]
		Rectangle GlobalRectangle { get; }






		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Hierarchy stuff
		 * 
		-----------------------------------------------------------------------------------------*/

		int ZOrder
		{
			get;
			set;
		}

		/// <summary>
		/// Adds frame
		/// </summary>
		/// <param name="frame"></param>
		void Add( IFrame frame );



		/// <summary>
		/// Reordering children
		/// </summary>
		/// 
		void ReorderChildren();
		/// <summary>
		/// 
		/// </summary>
		/// <param name="frame"></param>
		void Clear( IFrame frame );


		/// <summary>
		/// Inserts frame at specified index
		/// </summary>
		/// <param name="index"></param>
		/// <param name="frame"></param>
		void Insert( int index, IFrame frame );



		/// <summary>
		/// 
		/// </summary>
		/// <param name="frame"></param>
		void Remove( IFrame frame );



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		List<IFrame> GetAncestorList();


		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		void ForEachAncestor( Action<IFrame> action );


		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		void ForEachChildren( Action<IFrame> action );


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Input stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		////FrameStatus oldStatus = FrameStatus.None;




		void OnClick( Point location, Keys key, bool doubleClick );


		void OnMouseIn();

		////bool IsMouseMove = false;

		void OnMouseMove( int dx, int dy );

		void OnDrag( int dx, int dy );


		void OnMouseOut();


		void OnMouseDown( Keys key );


		void OnMouseUp( Keys key );


		void OnMouseWheel( int wheel );




		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Update and draw stuff :
		 * 
		-----------------------------------------------------------------------------------------*/


		void UpdateGlobalRect( int px, int py );






















		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Utils :
		 * 
		-----------------------------------------------------------------------------------------*/

		////int oldX = int.MinValue;
		////int oldY = int.MinValue;
		////protected int oldW = int.MinValue;
		////protected int oldH = int.MinValue;
		////protected bool firstResize = true;






		/// <summary>
		/// 
		/// </summary>
		void UpdateResize( bool UpdateChildren = true );



		///// <summary>
		///// 
		///// </summary>
		///// <param name="forceTransitions"></param>
		//       void RunLayout (bool forceTransitions)
		//{
		//	if (layout!=null && !ui.SuppressLayout) {
		//		layout.RunLayout( this, forceTransitions );
		//	}
		//}



		/// <summary>
		/// Get global rectangle bound by borders
		/// </summary>
		/// <returns></returns>
		Rectangle GetBorderedRectangle();



		/// <summary>
		/// Get global rectangle padded and bound by borders
		/// </summary>
		/// <returns></returns>
		Rectangle GetPaddedRectangle( bool global = true );




		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Animation stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		List<ITransition> Transitions { get; set; }


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
		void RunTransition<T, I>( string property, T targetValue, int delay, int period, Action callback = null ) where I : IInterpolator<T>, new();



		/// <summary>
		/// 
		/// </summary>
		/// <param name="property"></param>
		/// <param name="targetValue"></param>
		/// <param name="delay"></param>
		/// <param name="period"></param>
		void RunTransition( string property, Color targetValue, int delay, int period, Action callback = null );



		/// <summary>
		/// 
		/// </summary>
		/// <param name="property"></param>
		/// <param name="targetValue"></param>
		/// <param name="delay"></param>
		/// <param name="period"></param>
		void RunTransition( string property, int targetValue, int delay, int period, Action callback = null );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="property"></param>
		/// <param name="targetValue"></param>
		/// <param name="delay"></param>
		/// <param name="period"></param>
		void RunTransition( string property, float targetValue, int delay, int period, Action callback = null );





		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Anchors :
		 * 
		-----------------------------------------------------------------------------------------*/


		/// <summary>
		/// 
		/// </summary>
		/// <param name="oldW"></param>
		/// <param name="oldH"></param>
		/// <param name="newW"></param>
		/// <param name="newH"></param>
		void UpdateAnchors( int oldW, int oldH, int newW, int newH );


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Layouting :
		 * 
		-----------------------------------------------------------------------------------------*/

	}

	internal interface IFrameInternal
	{
		[XmlIgnore]
		FrameProcessor ui { set; }

		bool CanAcceptControl { get; }

		bool IsDrawable { get; }

		[XmlIgnore]
		Rectangle GlobalRectangle { set; }

		/// <summary>
		/// Common init 
		/// </summary>
		/// <param name="game"></param>
		void Init();

		void UpdateChildrenUI( FrameProcessor ui );

		void OnStatusChanged( FrameStatus status );

		void OnTick();

		void OnActivate();

		void OnDeactivate();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="parentX"></param>
		/// <param name="parentY"></param>
		/// <param name="frame"></param>
		void UpdateInternal( GameTime gameTime );

		/// <summary>
		/// Updates frame stuff.
		/// </summary>
		/// <param name="gameTime"></param>
		void Update( GameTime gameTime );

		/// <summary>
		/// Draws frame stuff
		/// </summary>
		void DrawFrameBorders( SpriteLayer spriteLayer, int clipRectIndex );

		/// <summary>
		/// Draws frame stuff.
		/// </summary>
		/// <param name="gameTime"></param>
		void DrawFrame( GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex );



		/// <summary>
		/// Adjusts frame size to content, text, image etc.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="?"></param>
		void Adjust();

		/// <summary>
		/// Checks move and resize and calls appropriate events
		/// </summary>
		void UpdateMove();

		void AutoResize( bool fromUpdate = false );

		/// <summary>
		/// 
		/// </summary>
		void DrawFrameImage( SpriteLayer spriteLayer, int clipRectIndex );

		/// <summary>
		/// Draws string
		/// </summary>
		/// <param name="text"></param>
		void DrawFrameText( SpriteLayer spriteLayer, int clipRectIndex );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		void UpdateTransitions( GameTime gameTime );

		/// <summary>
		/// Incrementally preserving half offset
		/// </summary>
		/// <param name="oldV"></param>
		/// <param name="newV"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		int SafeHalfOffset( int oldV, int newV, int x );
	}


	static class IFrameExtentions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <param name="text"></param>
		/// <param name="backColor"></param>
		/// <returns></returns>
		public static IFrame Create( this IFrame source, FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor )
		{
			var constructor = source.GetType().GetConstructor(new Type[] { typeof(FrameProcessor) });

			if (constructor!=null)
			{
				IFrame newObject = constructor.Invoke(new object[] { ui }) as IFrame;

				newObject.X = x;
				newObject.Y = y;
				newObject.Width = w;
				newObject.Height = h;
				newObject.Text = text;
				newObject.BackColor = backColor;

				return newObject;
			}
			else
			{
				return null;
			}
		}

		public static List<IFrame> BFSList( IFrame v )
		{
			Queue<IFrame> Q = new Queue<IFrame>();
			List<IFrame> list = new List<IFrame>();

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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="sb"></param>
		static internal void DrawNonRecursive<T>( T rootFrame, GameTime gameTime, SpriteLayer spriteLayer ) where T:IFrame, IFrameInternal
		{
			if (rootFrame == null)
			{
				return;
			}

			var stack = new Stack<DrawFrameItem>();
			var list = new List<DrawFrameItem>();

			stack.Push(new DrawFrameItem(rootFrame, Color.White, ((IFrame)rootFrame).GlobalRectangle, rootFrame.GetBorderedRectangle(), rootFrame.Text));


			while (stack.Any())
			{

				var currentDrawFrame = stack.Pop();

				if (!((IFrameInternal)currentDrawFrame.Frame).IsDrawable)
				{
					continue;
				}

				list.Add(currentDrawFrame);
				currentDrawFrame.Frame.Children.Reverse();
				var reversedChildren = currentDrawFrame.Frame.Children;
				currentDrawFrame.Frame.Children.Reverse();
				foreach (var child in reversedChildren)
				{

					var color = currentDrawFrame.Color * child.OverallColor;
					var inner = Clip(child.GetBorderedRectangle(), currentDrawFrame.InnerClip);
					var outer = Clip(child.GlobalRectangle, currentDrawFrame.InnerClip);

					if (MathUtil.IsRectInsideRect(child.GlobalRectangle, currentDrawFrame.InnerClip))
					{
						stack.Push(new DrawFrameItem(child, color, outer, inner, currentDrawFrame.Text + "-" + child.Text));
					}
				}
			}



			for (int i = 0; i < list.Count; i++)
			{
				var drawFrame = list[i];

				spriteLayer.SetClipRectangle(i * 2 + 0, drawFrame.OuterClip, drawFrame.Color);
				spriteLayer.SetClipRectangle(i * 2 + 1, drawFrame.InnerClip, drawFrame.Color);

				((IFrameInternal)drawFrame.Frame).DrawFrameBorders(spriteLayer, i * 2 + 0);
				((IFrameInternal)drawFrame.Frame).DrawFrame(gameTime, spriteLayer, i * 2 + 1);
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
	}
}