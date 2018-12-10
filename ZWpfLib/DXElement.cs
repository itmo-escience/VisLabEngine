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
			base.SnapsToDevicePixels = true;

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
				new PropertyMetadata((d, e) => ((DXElement)d).OnRendererChanged((Game)e.OldValue, (Game)e.NewValue)));


        private void OnRendererChanged(Game oldValue, Game newValue)
		{
			UpdateSize();
			UpdateReallyLoopRendering();

		    if (oldValue != null)
		    {
		        var d = (WpfDisplay) oldValue.GraphicsDevice.Display;
                d.UpdateReady -= SetReadyToRenderAgain;
		    }

		    if (newValue != null)
		    {
		        var d = (WpfDisplay) newValue.GraphicsDevice.Display;
		        d.UpdateReady += SetReadyToRenderAgain;
            }
		}


        private object _readyToRender = new object();
        private void SetReadyToRenderAgain(object sender, EventArgs e)
        {
            /*
            lock (_readyToRender)
            {
                var display = (WpfDisplay) sender;

                if(_buf != null)
                    display.ReturnBuffer();

                _buf = display.ExtractBuffer();
            }
            */
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
					_renderTimer.Start();
					CompositionTarget.Rendering += OnLoopRendering;
				} else {
					CompositionTarget.Rendering -= OnLoopRendering;
					_renderTimer.Stop();
				}
			}
		}


		void SetBackBuffer(RenderTarget2D target, DXImageSource sur)
		{
            sur.SetD3D11BackBuffer(target.Surface.Resource.QueryInterface<Texture2D>());
		}


		void OnLoopRendering(object sender, EventArgs e)
		{
		    var renderingTime = ((RenderingEventArgs) e).RenderingTime;

            if (!IsReallyLoopRendering)
				return;
			Render(renderingTime);
		}



		void UpdateSize()
		{
			if (Renderer == null || !Renderer.IsInitialized)
				return;
			Renderer.GraphicsDevice.Resize((int)DesiredSize.Width, (int)DesiredSize.Height);

            //Render();

			Console.WriteLine(DesiredSize);
		}

        private RenderTarget2D _buf = null;
		/// <summary>
		/// Will redraw the underlying surface once.
		/// </summary>
		public void Render(TimeSpan renderingTime)
		{
			if (Renderer == null || !Renderer.IsInitialized || IsInDesignMode || renderingTime == TimeSpan.Zero)
				return;


		    var display = (WpfDisplay) Renderer.GraphicsDevice.Display;

		    if (_buf != null)
		        display.ReturnBuffer();

		    _buf = display.ExtractBuffer();

            SetBackBuffer(_buf, Surface);
        }


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
