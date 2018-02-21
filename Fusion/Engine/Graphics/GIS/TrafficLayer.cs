using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using Fusion.Core.Mathematics;
using SharpDX.Direct3D;
using System.Runtime.InteropServices;

namespace Fusion.Engine.Graphics.GIS
{
	public class TrafficLayer : Gis.GisLayer
	{
		StructuredBuffer Events;
		StructuredBuffer Particles;

		Ubershader		shader;
		StateFactory	factory;

		int GroupSize;

		Mesh fbxMesh;


		[Flags]
		public enum TrafficFlags : int
		{
			None			= 0,
			FillTrafficBuffer = 1 << 0,
			DrawTraffic = 1 << 1,
			XRAY = 1 << 2,
		}


		public struct VechicleEvent
		{
			public float StartTime;
			public float EndTime;
			public float VechId;
			public float Length;
			public Vector4 Direction;
			public Vector4 SHeightEHeightXX;
			public double StartPointLon;
			public double StartPointLat;
			public double EndPointLon;
			public double EndPointLat;
		}

		public struct Particle
		{
			public double Lon;
			public double Lat;
			public Vector4 Direction;
			public Color4 Color;
		}

		struct EachFrameData
		{
			public Vector4 TimeXXX;
		};

		struct ParticlesData
		{
			public Vector4 GroupdimMaxparticlesXX;
		};


		EachFrameData EachFrame;
		ParticlesData PartData;

		ConstantBuffer EachFrameCB;
		ConstantBuffer ParticlesCB;

		public int DrawCount = 0;

		public float Time { set { EachFrame.TimeXXX.X = value; } }
		public float Ind { set { EachFrame.TimeXXX.Y = value; } }
		public float Size { set { EachFrame.TimeXXX.Z = value; } }

		void EnumFunc(PipelineState ps, int flag)
		{
			var flags = (TrafficFlags)flag;

			ps.VertexInputElements	= flags.HasFlag(TrafficFlags.DrawTraffic) ? VertexInputElement.FromStructure<VertexColorTextureTBNRigid>() : null;
			ps.BlendState			= BlendState.Additive;
			ps.DepthStencilState	= DepthStencilState.Readonly;
			ps.RasterizerState		= RasterizerState.CullCCW;

			ps.Primitive = Primitive.TriangleList;
		}



		public TrafficLayer(Game game, StructuredBuffer evs, StructuredBuffer parts, string fbxFileName) : base(game)
		{
			Events		= evs;
			Particles	= parts;

			var count = evs.StructureCapacity;

			int numGroups = (count % 1024 != 0) ? ((count / 1024) + 1) : (count / 1024);
			double secondRoot = Math.Pow((double)numGroups, (double)(1.0 / 2.0));
			secondRoot = Math.Ceiling(secondRoot);
			GroupSize = (int)secondRoot;

			shader	= Game.Content.Load<Ubershader>("globe.TrafficEvents.hlsl");
			factory = shader.CreateFactory(typeof(TrafficFlags), EnumFunc);

			EachFrame = new EachFrameData {
				TimeXXX = new Vector4(25000, 0, 0, 0)
			};
			PartData = new ParticlesData {
				GroupdimMaxparticlesXX = new Vector4(GroupSize, count, 0, 0)
			};

			ParticlesCB = new ConstantBuffer(game.GraphicsDevice, Marshal.SizeOf(typeof(ParticlesData)));
			EachFrameCB = new ConstantBuffer(game.GraphicsDevice, Marshal.SizeOf(typeof(EachFrameData)));

			ParticlesCB.SetData(PartData);


			var scene = game.Content.Load<Scene>(fbxFileName);
			fbxMesh = scene.Meshes.First();
		}


		public void SetData(VechicleEvent[] events)
		{
			Events.ResetDataNewLength(events);

			var count = Events.StructureCapacity;

			int		numGroups	= (count % 1024 != 0) ? ((count / 1024) + 1) : (count / 1024);
			double	secondRoot	= Math.Pow((double)numGroups, (double)(1.0 / 2.0));
			secondRoot	= Math.Ceiling(secondRoot);
			GroupSize	= (int)secondRoot;

			PartData = new ParticlesData {
				GroupdimMaxparticlesXX = new Vector4(GroupSize, count, 0, 0)
			};

			ParticlesCB.SetData(PartData);
		}


		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			EachFrameCB.SetData(EachFrame);

			Game.GraphicsDevice.PipelineState = factory[(int)TrafficFlags.FillTrafficBuffer];

			Game.GraphicsDevice.ComputeShaderResources[0] = Events;
			Game.GraphicsDevice.SetCSRWBuffer(0, Particles, 0);

			Game.GraphicsDevice.ComputeShaderConstants[1] = EachFrameCB;
			Game.GraphicsDevice.ComputeShaderConstants[2] = ParticlesCB;

			PixHelper.BeginEvent(new SharpDX.Mathematics.Interop.RawColorBGRA(255, 0, 0, 255), "Update Traffic");
			Game.GraphicsDevice.Dispatch(GroupSize, GroupSize, 1);
			PixHelper.EndEvent();

			Game.GraphicsDevice.SetCSRWBuffer(0, null);

			DrawCount = Particles.GetStructureCount();


			Game.GraphicsDevice.PipelineState = factory[(int)(TrafficFlags.DrawTraffic)];
			Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;
			Game.GraphicsDevice.VertexShaderConstants[1] = EachFrameCB;
			//Game.GraphicsDevice.GeometryShaderConstants[0]	= constBuffer;

			Game.GraphicsDevice.SetupVertexInput(fbxMesh.VertexBuffer, fbxMesh.IndexBuffer);

			Game.GraphicsDevice.VertexShaderResources[0] = Particles;

			Game.GraphicsDevice.DrawInstancedIndexed(fbxMesh.IndexBuffer.Capacity, DrawCount, 0, 0, 0);
        }


		public override void Dispose()
		{
			base.Dispose();
		}


		public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
		{
			throw new NotImplementedException();
		}
	}
}
