using SharpDX.Direct3D11;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Fusion.Engine.Common;

namespace ZWpfLib
{
	/// <summary>
	/// A <see cref="UIElement"/> displaying DirectX scene. 
	/// Takes care of resizing and refreshing a <see cref="DXImageSource"/>.
	/// It does no Direct3D work, which is delegated to
	/// the <see cref="IDirect3D"/> <see cref="Renderer"/> object.
	/// </summary>
	public class DXElementAsync : FrameworkElement
    {
		Stopwatch renderTimer;

		Task gameTask;
		CancellationTokenSource cancSource;
		CancellationToken token;


		/// <summary>
		/// The image source where the DirectX scene (from the <see cref="Renderer"/>) will be rendered.
		/// </summary>
		public DXImageSourceAsync Surface { get; }


		public DXElementAsync()
        {
			base.SnapsToDevicePixels = true;

            renderTimer = new Stopwatch();
			Surface = new DXImageSourceAsync();
			Surface.IsFrontBufferAvailableChanged += delegate {
				UpdateReallyLoopRendering();
				if (!IsReallyLoopRendering && Surface.IsFrontBufferAvailable)
					Render();
			};
			IsVisibleChanged += delegate { UpdateReallyLoopRendering(); };
		}


		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if(Renderer == null) return IntPtr.Zero;

			Renderer.HandleMessage(lParam, hwnd);

			return IntPtr.Zero;
		}


		public void HandleInput(Window wnd)
		{
			HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(wnd).Handle);
			source.AddHook(new HwndSourceHook(WndProc));
		}



		/// <summary>
		/// The D3D device that will handle the drawing
		/// </summary>
		public Game Renderer
		{
			get { return (Game)GetValue(RendererProperty); }
			set { SetValue(RendererProperty, value); }
		}

		public static readonly DependencyProperty RendererProperty =
			DependencyProperty.Register(
				"Renderer",
				typeof(Game),
				typeof(DXElementAsync),
				new PropertyMetadata((d, e) => ((DXElementAsync)d).OnRendererChanged((Game)e.OldValue, (Game)e.NewValue)));


		private void OnRendererChanged(Game oldValue, Game newValue)
		{
			UpdateSize();
			UpdateReallyLoopRendering();
		}


		protected override void OnVisualParentChanged(DependencyObject oldParent)
		{
			if (IsInDesignMode)
				return;
			UpdateReallyLoopRendering();
		}

		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			base.ArrangeOverride(finalSize);
			UpdateSize();
			return finalSize;
		}

		protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
		{
			int w = (int)Math.Ceiling(availableSize.Width);
			int h = (int)Math.Ceiling(availableSize.Height);
			return new System.Windows.Size(w, h);
		}

		protected override Visual GetVisualChild(int index)
		{
			throw new ArgumentOutOfRangeException();
		}

		protected override int VisualChildrenCount { get { return 0; } }


		protected override void OnRender(DrawingContext dc)
		{
			dc.DrawImage(Surface, new Rect(RenderSize));
		}


		bool IsReallyLoopRendering { get; set; }

		void UpdateReallyLoopRendering()
		{
			var newValue =
				!IsInDesignMode
				&& Renderer != null
				&& Surface.IsFrontBufferAvailable
				&& VisualParent != null
				&& IsVisible;

			if (newValue != IsReallyLoopRendering)
			{
				IsReallyLoopRendering = newValue;
				if (IsReallyLoopRendering) {
					renderTimer.Start();
					CompositionTarget.Rendering += OnLoopRendering;

					cancSource = new CancellationTokenSource();
					token = cancSource.Token;

					var tok = token;
					var ren = Renderer;
					var sur = Surface;
					ren.OnInitialized +=() => { SetBackBuffer(ren, sur); } ;
					ren.RenderSystem.DisplayBoundsChanged += (o, rec) => {
						SetBackBuffer(ren, sur);
					};
					gameTask = new Task(() => { ren.RunExternal(tok); } );
					gameTask.Start();
				} else {
					CompositionTarget.Rendering -= OnLoopRendering;
					renderTimer.Stop();
					cancSource.Cancel();
					cancSource.Dispose();
					gameTask = null;
				}
			}
		}


		void SetBackBuffer(Game ren, DXImageSourceAsync sur)
		{
			if (ren == null) return;
			sur.SetD3D11BackBuffer(ren.GraphicsDevice.BackbufferColor.Surface.Resource.QueryInterface<Texture2D>());
		}


		void OnLoopRendering(object sender, EventArgs e) 
		{
			if (!IsReallyLoopRendering)
				return;
			Render(); 
		}



		void UpdateSize()
		{
			if (Renderer == null)
				return;
			Renderer.GraphicsDevice.Resize((int)DesiredSize.Width, (int)DesiredSize.Height);
			Console.WriteLine(DesiredSize);
		}

		
		/// <summary>
		/// Will redraw the underlying surface once.
		/// </summary>
		public void Render()
		{
			if (Renderer == null || IsInDesignMode)
				return;

			Surface.Invalidate();
		}



		#region override input: Key, Mouse
		/*
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			if (Renderer is IInteractiveDirect3D)
				((IInteractiveDirect3D)Renderer).OnMouseDown(this, e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (Renderer is IInteractiveDirect3D)
				((IInteractiveDirect3D)Renderer).OnMouseMove(this, e);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			if (Renderer is IInteractiveDirect3D)
				((IInteractiveDirect3D)Renderer).OnMouseUp(this, e);
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			base.OnMouseWheel(e);
			if (Renderer is IInteractiveDirect3D)
				((IInteractiveDirect3D)Renderer).OnMouseWheel(this, e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (Renderer is IInteractiveDirect3D)
				((IInteractiveDirect3D)Renderer).OnKeyDown(this, e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (Renderer is IInteractiveDirect3D)
				((IInteractiveDirect3D)Renderer).OnKeyUp(this, e);
		}
		*/
		#endregion



		/// <summary>
		/// Gets a value indicating whether the control is in design mode
		/// (running in Blend or Visual Studio).
		/// </summary>
		public bool IsInDesignMode
        {
            get { return DesignerProperties.GetIsInDesignMode(this); }
        }
	}
}
