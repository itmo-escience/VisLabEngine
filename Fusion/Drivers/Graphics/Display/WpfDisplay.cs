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

/*
namespace Fusion.Drivers.Graphics.Display
{
	class WpfDisplay : BaseDisplay
	{
		StereoEye[] eyeList = new[] { StereoEye.Mono };

		RenderTarget2D backbufferColor;
		DepthStencil2D backbufferDepth;
		int clientWidth;
		int clientHeight;


		public WpfDisplay(Game game, GraphicsDevice device, GraphicsParameters parameters) : base(game, device, parameters)
		{
			var factory = new Factory1();

			var adapter = factory.GetAdapter(0);
			//for(int i = 0; i < factory.GetAdapterCount(); i++)
			//	Console.WriteLine(factory.GetAdapter(i).Description.Description);

			d3dDevice = new D3DDevice(adapter, DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug, new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_11_1, SharpDX.Direct3D.FeatureLevel.Level_11_0 });

		}

		public override StereoEye TargetEye { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public override StereoEye[] StereoEyeList => throw new NotImplementedException();

		public override RenderTarget2D BackbufferColor => throw new NotImplementedException();

		public override DepthStencil2D BackbufferDepth => throw new NotImplementedException();

		public override bool Fullscreen { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public override Rectangle Bounds => throw new NotImplementedException();

		public override Form Window => throw new NotImplementedException();

		public override void Prepare()
		{
			throw new NotImplementedException();
		}

		public override void SwapBuffers(int syncInterval)
		{
			throw new NotImplementedException();
		}

		public override void Update()
		{
			throw new NotImplementedException();
		}
	}
}
*/