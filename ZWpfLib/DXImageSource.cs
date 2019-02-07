using System;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Graphics.Display;
using SharpDX.Direct3D11;
using Texture2D = SharpDX.Direct3D11.Texture2D;

namespace ZWpfLib
{
	public class DXImageSource : D3DImage, IDisposable
	{
		public bool IsDisposed { get; protected set; }

		private static int _activeClients;
		private static D3D9 _D3D9;

		public DXImageSource()
		{
			StartD3D9();
		}
		~DXImageSource() { Dispose(false); }

		public void Dispose() { Dispose(true); }

		protected void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;

			if (disposing) {
				GC.SuppressFinalize(this);
			}
			EndD3D9();
			IsDisposed = true;
		}


        public void Invalidate()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);

            Lock();
            AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
            Unlock();
        }

	    private DeviceContext _deferredContext;
	    internal RenderTarget2D Buffer { get; private set; }
	    internal void SetBufferWithContext(RenderTarget2D target, DeviceContext deferredContext)
        {
            Buffer = target;
            _deferredContext = deferredContext;
            _texture = Buffer.Surface.Resource.QueryInterface<Texture2D>();
            var shared = _D3D9.Device.GetSharedD3D9(_texture);
            using (var surface = shared.GetSurfaceLevel(0))
            {
                if (TryLock(new Duration(default(TimeSpan))))
                {
                    SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
                    AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
                }

                Unlock();
            }
        }

	    private Texture2D _texture;
		public RenderTarget2D CopyBackBuffer(WpfDisplay display)
		{
		    Lock();
		    var newBuffer = display.ExtractBuffer();
		    var newTexture = newBuffer.Surface.Resource.QueryInterface<Texture2D>();

            _deferredContext.CopyResource(newTexture, _texture);
            display.RequestRender();
            while(!display.RenderRequestComplete) Thread.Sleep(5);

		    AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
		    Unlock();

            return newBuffer;
		}

		private static void StartD3D9()
		{
			if (_activeClients == 0)
				_D3D9 = new D3D9();
			_activeClients++;
		}

		private static void EndD3D9()
		{
			_activeClients--;
			if (_activeClients == 0)
				_D3D9.Dispose();
		}
	}
}
