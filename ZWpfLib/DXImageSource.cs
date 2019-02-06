using System;
using System.Windows;
using System.Windows.Interop;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;

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

	    private Texture2D _buffer;
		public void SetBackBuffer(Texture2D texture, DeviceContext deferredContext)
		{
		    bool Same(Texture2D x, Texture2D y)
		    {
		        var xd = x.Description;
		        var yd = y.Description;
		        return xd.Width == yd.Width && xd.Height == yd.Height && xd.Format == yd.Format;
		    };

		    if (_buffer == null || _buffer.IsDisposed || !Same(texture, _buffer))
		    {
                _buffer?.Dispose();
		        _buffer = texture;

		        var shared = _D3D9.Device.GetSharedD3D9(_buffer);
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
		    else
		    {
                Lock();
                deferredContext.CopyResource(texture, _buffer);
		        AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
                Unlock();
            }
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
