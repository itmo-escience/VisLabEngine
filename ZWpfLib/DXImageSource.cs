using System;
using System.Windows;
using System.Windows.Interop;
using SharpDX.Direct3D9;

namespace ZWpfLib
{
	public class DXImageSource : D3DImage, IDisposable
	{
		public bool IsDisposed { get; protected set; }
		Texture backBuffer;
		SharpDX.Direct3D11.Texture2D d3d11BackBuf;

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

			var t = d3d11BackBuf; d3d11BackBuf = null;
			if(t != null && !t.IsDisposed) {
				SetBackBuffer(d3d9.Device.GetSharedD3D9(t));
			}

			if (backBuffer != null) {
				Lock();
				AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
				Unlock();
			}
		}


		public void SetD3D11BackBuffer(SharpDX.Direct3D11.Texture2D texture)
		{
			d3d11BackBuf = texture;
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
						Lock();
						SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
						AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
						Unlock();
					}
				}
				else {
					Lock();
					SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
					AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
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
