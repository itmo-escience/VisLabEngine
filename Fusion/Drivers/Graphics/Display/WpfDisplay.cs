using System;
using System.Collections.Concurrent;
using System.Threading;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using D3DDevice = SharpDX.Direct3D11.Device;

namespace Fusion.Drivers.Graphics.Display
{
	public class WpfDisplay : BaseDisplay
	{
        private ConcurrentQueue<RenderTarget2D> _oldBuffers = new ConcurrentQueue<RenderTarget2D>();
	    private RenderTarget2D _currentBuffer;
	    private volatile RenderTarget2D _readyBuffer;
	    private DepthStencil2D _backbufferDepth;

        private readonly object _lockHolder = new object();

	    private int _clientWidth;
	    private int _clientHeight;
	    private const int SyncTime = (int)(1000.0 / 60.0);

        public DeviceContext DeferredContext { get; set; }
        public override RenderTarget2D BackbufferColor => _currentBuffer;
	    public override DepthStencil2D BackbufferDepth => _backbufferDepth;
	    public override StereoEye[] StereoEyeList => new[] { StereoEye.Mono };
        public override StereoEye TargetEye { get; internal set; }
	    public override bool Fullscreen { get => false; internal set { } }
	    public override Rectangle Bounds => new Rectangle(0, 0, _clientWidth, _clientHeight);
	    public override Form Window => null;
	    public override bool Focused { get => true; }

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

	        DeferredContext = new DeviceContext(device.Device);
            RecreateBuffers();
	    }

	    private void RecreateBuffers() {

            _readyBuffer?.Dispose();
		    _currentBuffer?.Dispose();
		    while (_oldBuffers.TryDequeue(out var buffer))
		    {
		        buffer.Dispose();
		    }
		    _backbufferDepth?.Dispose();

		    //_renderRequested = false;

            _oldBuffers.Enqueue(CreateBuffer());
		    _oldBuffers.Enqueue(CreateBuffer());
		    _oldBuffers.Enqueue(CreateBuffer());
		    _oldBuffers.Enqueue(CreateBuffer());
		    _currentBuffer = CreateBuffer();
	        _readyBuffer = CreateBuffer();

            _backbufferDepth = new DepthStencil2D(device, DepthFormat.D24S8, _currentBuffer.Width, _currentBuffer.Height, _currentBuffer.SampleCount);
        }

	    private RenderTarget2D CreateBuffer()
	    {
	        return new RenderTarget2D(device, ColorFormat.Bgra8, _clientWidth, _clientHeight, false, false, true);
        }

		public override void Prepare() { }

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


		    RenderTarget2D old;
		    do
		    {
		        old = _readyBuffer;
		    } while (Interlocked.CompareExchange(ref _readyBuffer, _currentBuffer, old) != old);

		    if (old != null)
		        _oldBuffers.Enqueue(old);

            if (!_oldBuffers.TryDequeue(out _currentBuffer))
            {
                throw new InvalidOperationException("There is no backBuffer in the queue. What happened?");
            }

		    if (_currentBuffer.IsDisposed || _currentBuffer.Width != _clientWidth ||
		        _currentBuffer.Height != _clientHeight)
		    {
                _currentBuffer.Dispose();
		        _currentBuffer = CreateBuffer();
		    }
		}

        /// <summary>
        /// Gets one rendered buffer
        /// </summary>
        /// <returns></returns>
	    public RenderTarget2D ExtractBuffer()
        {
            RenderTarget2D extracted;

            do
            {
                extracted = _readyBuffer;
            } while (extracted == null || extracted.IsDisposed ||
                     Interlocked.CompareExchange(ref _readyBuffer, null, extracted) != extracted
            );

            return extracted;
	    }

	    /// <summary>
	    /// Returns extracted buffer to front buffer collection
	    /// </summary>
	    /// <returns></returns>
        public void ReturnBuffer(RenderTarget2D buffer)
	    {
            if (buffer == null)
	        {
                Log.Warning("Null buffer was returned.");
	            return;
	        }

	        _oldBuffers.Enqueue(buffer);
	    }

	    private volatile bool _renderRequested;
	    public volatile bool RenderRequestComplete = true;
	    public void RequestRender()
	    {
	        RenderRequestComplete = false;
            _renderRequested = true;
	    }

		public override void Update()
		{
		    if (_isResizeRequested)
		    {
		        _clientWidth = _reqWidth;
		        _clientHeight = _reqHeight;

		        RecreateBuffers();

		        device.NotifyViewportChanges();

		        _isResizeRequested = false;
		    }

            if (_renderRequested)
		    {
		        var lst = DeferredContext.FinishCommandList(false);
		        if (lst != null)
		        {
		            device.Device.ImmediateContext.ExecuteCommandList(lst, false);
                    lst.Dispose();
		        } else
		            Log.Warning("Empty command list");

		        _renderRequested = false;
		        RenderRequestComplete = true;
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


	    private volatile bool _isResizeRequested = false;
	    private int _reqWidth = 1;
		private int _reqHeight = 1;
	}
}
