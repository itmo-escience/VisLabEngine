using System;
using System.Windows;
using System.Windows.Interop;
using Fusion.Drivers.Graphics;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;

namespace ZWpfLib
{
	public class DXImageSource : D3DImage, IDisposable
	{
		public bool IsDisposed { get; protected set; }
		Texture backBuffer;

		static int activeClients;
		static D3D9 d3d9;


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
				SetBackBuffer((Texture)null);
				GC.SuppressFinalize(this);
			}
			EndD3D9();
			IsDisposed = true;
		}


        public void Invalidate()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);

            if (backBuffer == null) return;
            {
                Lock();
                AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
                Unlock();
            }
        }

	    private SharpDX.Direct3D11.Texture2D buffer = null;
		public void SetD3D11BackBuffer(SharpDX.Direct3D11.Texture2D texture, DeviceContext ctx)
		{
		    bool Same(SharpDX.Direct3D11.Texture2D x, SharpDX.Direct3D11.Texture2D y)
		    {
		        var xd = x.Description;
		        var yd = y.Description;
		        return xd.Width == yd.Width && xd.Height == yd.Height && xd.Format == yd.Format;
		    };

		    var shared = d3d9.Device.GetSharedD3D9(texture);

		    if (buffer == null || buffer.IsDisposed || !Same(texture, buffer))
		    {
		        buffer = texture;
		        SetBackBuffer(shared);
            }
		    else
		    {
                Lock();
                ctx.CopyResource(texture, buffer);
		        AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
                Unlock();
            }
        }


		public void SetBackBuffer(Texture texture)
		{
			if (IsDisposed)
				throw new ObjectDisposedException(GetType().Name);

			Texture toDelete = null;
			try {
				if (texture != backBuffer) {
					// if it's from the private (SDX9ImageSource) D3D9 device, dispose of it
					if (backBuffer != null && backBuffer.Device.NativePointer == d3d9.Device.NativePointer)
						toDelete = backBuffer;
					backBuffer = texture;
				}

				if (texture != null) {
					using (Surface surface = texture.GetSurfaceLevel(0)) {
					    if (TryLock(new Duration(default(TimeSpan))))
					    {
					        SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
					        AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
					    }

					    Unlock();
					}
				}
				else {
				    if (TryLock(new Duration(default(TimeSpan))))
				    {
				        SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
				        AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
				    }

				    Unlock();
				}
			}
			finally {
				if (toDelete != null) {
					toDelete.Dispose();
				}
			}
		}

		private static void StartD3D9()
		{
			if (activeClients == 0)
				d3d9 = new D3D9();
			activeClients++;
		}

		private static void EndD3D9()
		{
			activeClients--;
			if (activeClients == 0)
				d3d9.Dispose();
		}
	}
}
