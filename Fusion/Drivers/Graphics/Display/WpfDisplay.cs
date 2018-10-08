using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Native.NvApi;
using Fusion.Engine.Common;
using D3DDevice = SharpDX.Direct3D11.Device;
using System.Threading;

namespace Fusion.Drivers.Graphics.Display
{
	class WpfDisplay : BaseDisplay
	{
		StereoEye[] eyeList = new[] { StereoEye.Mono };

		RenderTarget2D backbufferColor;
		DepthStencil2D backbufferDepth;
		int clientWidth;
		int clientHeight;

		int syncTime = (int)(1000.0 / 60.0);

		public WpfDisplay(Game game, GraphicsDevice device, GraphicsParameters parameters) : base(game, device, parameters)
		{
			clientWidth		= parameters.Width;
			clientHeight	= parameters.Height;

			var factory = new Factory1();
			var adapter = factory.GetAdapter(0);
			//for(int i = 0; i < factory.GetAdapterCount(); i++)
			//	Console.WriteLine(factory.GetAdapter(i).Description.Description);

			d3dDevice = new D3DDevice(adapter, DeviceCreationFlags.BgraSupport
#if Debug
				| DeviceCreationFlags.Debug
#endif               
				, new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_11_1, SharpDX.Direct3D.FeatureLevel.Level_11_0 });



		}

		public override StereoEye TargetEye { get; set; }

		public override StereoEye[] StereoEyeList => eyeList;

		public override RenderTarget2D BackbufferColor => backbufferColor;

		public override DepthStencil2D BackbufferDepth => backbufferDepth;

		public override bool Fullscreen { get => false; set { } }

		public override Rectangle Bounds => new Rectangle(0,0, clientWidth, clientHeight);

		public override Form Window => null;

		public override bool Focused { get => true; }


		public override void CreateDisplayResources()
		{
			base.CreateDisplayResources();

			backbufferColor?.Dispose();
			backbufferDepth?.Dispose();

			backbufferColor = new RenderTarget2D(device, ColorFormat.Bgra8, clientWidth, clientHeight, false, false, true);
			backbufferDepth = new DepthStencil2D(device, DepthFormat.D24S8, backbufferColor.Width, backbufferColor.Height, backbufferColor.SampleCount);
		}


		public override void Prepare()
		{
			
		}

		public override void SwapBuffers(int syncInterval)
		{
			d3dDevice.ImmediateContext.Flush();

			//if (syncInterval < 0) return;
			//
			//int miliseconds = (int)(Game.Time.ElapsedSec * 1000);
			//
			//if(miliseconds < syncTime) {
			//	Thread.Sleep(syncTime - miliseconds);
			//}
		}

		public override void Update()
		{
			if(isResizeRequested) {
				clientWidth = reqWidth;
				clientHeight = reqHeight;

				CreateDisplayResources();

				device.NotifyViewportChanges();

				isResizeRequested = false;
			}
		}


		public override void Resize(int width, int height)
		{
			if (width == 0 || height == 0) return;
			if (clientWidth == width && clientHeight == height) return;

			reqWidth = width;
			reqHeight = height;
			isResizeRequested = true;
		}


		bool isResizeRequested = false;
		int reqWidth = 1;
		int reqHeight = 1;

	}
}
