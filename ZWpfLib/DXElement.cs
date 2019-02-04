using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Graphics.Display;
using Fusion.Engine.Common;
using SharpDX.Direct3D11;
using Texture2D = SharpDX.Direct3D11.Texture2D;

namespace ZWpfLib
{
	/// <summary>
	/// A <see cref="UIElement"/> displaying DirectX scene.
	/// Takes care of resizing and refreshing a <see cref="DXImageSource"/>.
	/// It does no Direct3D work, which is delegated to
	/// the <see cref="IDirect3D"/> <see cref="Renderer"/> object.
	/// </summary>
	public class DXElement : FrameworkElement
    {
		private readonly Stopwatch _renderTimer;

        /// <summary>
        /// The image source where the DirectX scene (from the <see cref="Renderer"/>) will be rendered.
        /// </summary>
        private DXImageSource Surface { get; }

        public DXElement()
        {
			SnapsToDevicePixels = true;

            _renderTimer = new Stopwatch();

			Surface = new DXImageSource();

            Surface.IsFrontBufferAvailableChanged += delegate {
				UpdateReallyLoopRendering();
				//if (!IsReallyLoopRendering && Surface.IsFrontBufferAvailable)
					//Render();
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
				typeof(DXElement),
				new PropertyMetadata((d, e) => ((DXElement)d).OnRendererChanged()));


        private void OnRendererChanged()
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

        protected override int VisualChildrenCount => 0;

        protected override void OnRender(DrawingContext dc)
		{
            base.OnRender(dc);
            dc.DrawImage(Surface, new Rect(RenderSize));
		}

        private bool IsReallyLoopRendering { get; set; }

        private void UpdateReallyLoopRendering()
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
					_renderTimer.Start();
					CompositionTarget.Rendering += OnLoopRendering;
				} else {
					CompositionTarget.Rendering -= OnLoopRendering;
					_renderTimer.Stop();
				}
			}
		}


        private TimeSpan _lastRenderTime;
        private void OnLoopRendering(object sender, EventArgs e)
		{
		    var renderingTime = ((RenderingEventArgs) e).RenderingTime;

            if (!IsReallyLoopRendering)
				return;

            if (renderingTime == _lastRenderTime)
                return;

		    _lastRenderTime = renderingTime;
            Render();
		}


        private void UpdateSize()
		{
			if (Renderer == null || !Renderer.IsInitialized)
				return;
			Renderer.GraphicsDevice.Resize((int)DesiredSize.Width, (int)DesiredSize.Height);

			Console.WriteLine(DesiredSize);
		}

        private void SetBackBuffer(RenderTarget2D target, DXImageSource sur, DeviceContext ctx)
        {
            sur.SetD3D11BackBuffer(target.Surface.Resource.QueryInterface<Texture2D>(), ctx);
        }

        private int frameCounter = 1;
        private RenderTarget2D _buf = null;
        /// <summary>
		/// Will redraw the underlying surface once.
		/// </summary>
		public void Render()
		{
			if (Renderer == null || !Renderer.IsInitialized || IsInDesignMode)
				return;

		    var display = (WpfDisplay) Renderer.GraphicsDevice.Display;

		    if (_buf != null)
		    {
		        display.ReturnBuffer(_buf);
		        _buf = null;
		        frameCounter = 0;
		    }

		    if (_buf == null)
		    {
		        _buf = display.ExtractBuffer();
                SetBackBuffer(_buf, Surface, display.DeferredContext);
                display.RequestRender();
            }

		    frameCounter++;
		}


		/// <summary>
		/// Gets a value indicating whether the control is in design mode
		/// (running in Blend or Visual Studio).
		/// </summary>
		public bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(this);
	}
}
