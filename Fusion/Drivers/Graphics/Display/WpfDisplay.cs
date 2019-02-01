using System;
using System.Collections.Concurrent;
using System.Threading;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Buffer = SharpDX.Direct3D11.Buffer;
using D3DDevice = SharpDX.Direct3D11.Device;

namespace Fusion.Drivers.Graphics.Display
{
	public class WpfDisplay : BaseDisplay
	{
	    public event EventHandler UpdateReady;

	    public override StereoEye[] StereoEyeList => new[] { StereoEye.Mono };

        private ConcurrentQueue<RenderTarget2D> _frontBuffers = new ConcurrentQueue<RenderTarget2D>();
	    private RenderTarget2D _currentBuffer;
	    private readonly object _extractedBufferLockHolder = new object();
        public override RenderTarget2D BackbufferColor => _currentBuffer;

        private DepthStencil2D _backbufferDepth;
	    public override DepthStencil2D BackbufferDepth => _backbufferDepth;

	    public override StereoEye TargetEye { get; internal set; }
	    public override bool Fullscreen { get => false; internal set { } }
	    public override Rectangle Bounds => new Rectangle(0, 0, _clientWidth, _clientHeight);
	    public override Form Window => null;
	    public override bool Focused { get => true; }


        private int _clientWidth;
	    private int _clientHeight;

	    private const int SyncTime = (int)(1000.0 / 60.0);

		public WpfDisplay(Game game, GraphicsDevice device, GraphicsParameters parameters) : base(game, device, parameters)
		{
			_clientWidth = parameters.Width;
			_clientHeight = parameters.Height;

			var adapter = new Factory1().GetAdapter(0);

		    var creationFlags = DeviceCreationFlags.BgraSupport;
#if Debug
            creationFlags |= DeviceCreationFlags.Debug;
#endif

            d3dDevice = new D3DDevice(adapter, creationFlags, FeatureLevel.Level_11_1, FeatureLevel.Level_11_0);
		}

	    internal override void CreateDisplayResources()
		{
			base.CreateDisplayResources();

            DeferredContext?.Dispose();

            _currentBuffer?.Dispose();
		    while (_frontBuffers.TryDequeue(out var buffer))
                buffer.Dispose();

			_backbufferDepth?.Dispose();

            DeferredContext = new DeviceContext(device.Device);

		    _frontBuffers.Enqueue(CreateBuffer());
		    _frontBuffers.Enqueue(CreateBuffer());
		    _frontBuffers.Enqueue(CreateBuffer());
		    _currentBuffer = CreateBuffer();

            _backbufferDepth = new DepthStencil2D(device, DepthFormat.D24S8, _currentBuffer.Width, _currentBuffer.Height, _currentBuffer.SampleCount);
        }

	    private RenderTarget2D CreateBuffer()
	    {
	        return new RenderTarget2D(device, ColorFormat.Bgra8, _clientWidth, _clientHeight, false, false, true);
        }

		public override void Prepare() { }

	    public DeviceContext DeferredContext { get; private set; }

		public override void SwapBuffers(int syncInterval)
		{
		    var q = new Query(d3dDevice, new QueryDescription { Type = QueryType.Event });

            d3dDevice.ImmediateContext.Flush();
		    d3dDevice.ImmediateContext.End(q);

            while (true)
            {
                if (d3dDevice.ImmediateContext.GetData(q, out int queryResult) && queryResult != 0)
                {
                    break;
                }
            }

            q.Dispose();

            _frontBuffers.Enqueue(_currentBuffer);
            if (!_frontBuffers.TryDequeue(out _currentBuffer))
            {
                throw new InvalidOperationException("There is no backBuffer in the queue. What happened?");
            }
		}


        /// <summary>
        /// Gets one buffer out of front buffer collection
        /// </summary>
        /// <returns></returns>
	    public RenderTarget2D ExtractBuffer()
	    {
	            //if (_extractedBuffer != null)
	            //    throw new InvalidOperationException("Return extracted buffer before extracting again.");

	        if (!_frontBuffers.TryDequeue(out var extractedBuffer))
	        {
                throw new InvalidOperationException("There is no backBuffer in the queue. What happened?");
	        }

	        return extractedBuffer;
	    }

	    /// <summary>
	    /// Returns extracted buffer to front buffer collection
	    /// </summary>
	    /// <returns></returns>
        public void ReturnBuffer(RenderTarget2D buffer)
	    {
	        if (buffer == null) throw new InvalidOperationException("There is nothing to return");

	        if (buffer.IsDisposed || !(buffer.Width == _clientWidth && buffer.Height == _clientHeight))
	        {
	            _frontBuffers.Enqueue(CreateBuffer());

                //buffer.Dispose();
            }
	        else
	        {
                _frontBuffers.Enqueue(buffer);
            }
	    }

	    private bool _renderRequested = false;
	    public void RequestRender()
	    {
	        _renderRequested = true;
	    }

		public override void Update()
		{
            if (_isResizeRequested) {
				_clientWidth = _reqWidth;
				_clientHeight = _reqHeight;

				CreateDisplayResources();

				device.NotifyViewportChanges();

				_isResizeRequested = false;
			}

            UpdateReady?.Invoke(this, null);

		    if (_renderRequested)
		    {
		        var lst = DeferredContext.FinishCommandList(false);
                device.Device.ImmediateContext.ExecuteCommandList(lst, false);
		        _renderRequested = false;
		    }
		}


	    internal override void Resize(int width, int height)
		{
			if (width == 0 || height == 0) return;
			if (_clientWidth == width && _clientHeight == height) return;

			_reqWidth = width;
			_reqHeight = height;
			_isResizeRequested = true;
		}


	    private bool _isResizeRequested = false;
	    private int _reqWidth = 1;
		private int _reqHeight = 1;
	}
}
