using System;
using System.Runtime.InteropServices;
using SharpDX.Direct3D9;

namespace ZWpfLib
{
	public class D3D9 : IDisposable
	{
		protected Direct3DEx context;
		protected DeviceEx device;
		Texture renderTarget;



		protected D3D9(bool b) { /* do nothing constructor */ }

		public D3D9() : this(null)
		{
		}

		public D3D9(DeviceEx device) 
		{
			if (device != null) {
				throw new NotSupportedException("dunno how to get the context");
			}
			else {
				context = new Direct3DEx();

				PresentParameters presentparams = new PresentParameters {
					Windowed	= true,
					SwapEffect	= SwapEffect.Discard,
					DeviceWindowHandle		= GetDesktopWindow(),
					PresentationInterval	= PresentInterval.Default
				};
				this.device = new DeviceEx(context, 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve, presentparams);
			}
		}


		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				device?.Dispose();
				context?.Dispose();
			}
		}


		[DllImport("user32.dll", SetLastError = false)]
		static extern IntPtr GetDesktopWindow();


		public bool IsDisposed { get { return device == null; } }
		public DeviceEx Device { get { return device; } }
		public Texture RenderTarget { get { return Prepared(ref renderTarget); } }



		public void Reset(int w, int h)
		{
			if (w < 1)
				throw new ArgumentOutOfRangeException("w");
			if (h < 1)
				throw new ArgumentOutOfRangeException("h");

			renderTarget?.Dispose();
			renderTarget = new Texture(this.device, w, h, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);

			// TODO test that...
			using (var surface = renderTarget.GetSurfaceLevel(0))
				device.SetRenderTarget(0, surface);
		}

		protected T Prepared<T>(ref T property)
		{
			if (property == null)
				Reset(1, 1);
			return property;
		}


		public void SetBackBuffer(DXImageSource dximage) { dximage.SetBackBuffer(RenderTarget); }
	}
}
