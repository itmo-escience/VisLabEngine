using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;
using Fusion.Core.Mathematics;
using Fusion.Engine.Input;
using SharpDX.Direct3D;

namespace Fusion.Engine.Graphics.GIS
{
	public class PointsFbxLayer2 : Gis.GisLayer
	{
		Ubershader shader;
		StateFactory factory;
		ConstantBuffer fbxBuf;

		[StructLayout(LayoutKind.Explicit)]
		struct ConstDataStruct
		{
			[FieldOffset(0)]
			public Vector4 SunDirectionTransparency;
			[FieldOffset(16)]
			public Color4 OverallColor;
		}

		ConstDataStruct constData;

		public Color4 OverallColor { set { constData.OverallColor = value; } get { return constData.OverallColor; } }
		public float Transparency { set { constData.SunDirectionTransparency.W = value; } get { return constData.SunDirectionTransparency.W; } }
		public int DrawCount { set; get; }

		[Flags]
		public enum FbxFlags : int
		{
			NONE = 0,
			USE_OVERALL_COLOR = 1 << 0,

		}


		public struct FbxPoint2
		{
			public double Lon;
			public double Lat;
			public Color4 Color;
			public Vector4 XVectorSize;
			//public Vector4 YVectorHeight;
		}

		StructuredBuffer points;

		public FbxPoint2[] PointsCpu { get; protected set; }
		public Mesh FbxMesh { get; protected set; }


		void EnumFunc(PipelineState ps, int flag)
		{
			var flags = (FbxFlags)flag;

			ps.VertexInputElements = VertexInputElement.FromStructure<VertexColorTextureTBNRigid>();
			ps.BlendState = BlendState.AlphaBlend;
			ps.DepthStencilState = DepthStencilState.Default;
			ps.RasterizerState = RasterizerState.CullCW;

			ps.Primitive = Primitive.TriangleList;
		}


		/// <summary>
		/// Warning: only first mesh is used
		/// </summary>
		/// <param name="engine"></param>
		/// <param name="fbxFileName"></param>
		public PointsFbxLayer2(Game engine, string fbxFileName, int maxPointsCount) : base(engine)
		{
			var scene = engine.Content.Load<Scene>(fbxFileName);
			FbxMesh = scene.Meshes.First();

			PointsCpu = new FbxPoint2[maxPointsCount];

			shader = engine.Content.Load<Ubershader>("globe.Fbx2.hlsl");
			factory = shader.CreateFactory(typeof(FbxFlags), EnumFunc);

			constData = new ConstDataStruct();
			constData.SunDirectionTransparency = new Vector4(1, 1, 0, 1);

			fbxBuf = new ConstantBuffer(engine.GraphicsDevice, Marshal.SizeOf(typeof(ConstDataStruct)));
			fbxBuf.SetData(constData);


			points = new StructuredBuffer(engine.GraphicsDevice, typeof(FbxPoint2), maxPointsCount, StructuredBufferFlags.None);
		}


		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}


		private float time = 0;

		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			//if(Game.Keyboard.IsKeyDown(Keys.Space))
			//	time += gameTime.ElapsedSec*0.1f;
			//
			//float amount = time - (int)time;
			//constData.ViewDirectionTransparency.X = MathUtil.Lerp(0, MathUtil.TwoPi, amount);
			//
			//
			//Console.WriteLine("Angle:" + MathUtil.RadiansToDegrees(MathUtil.Lerp(0, MathUtil.TwoPi, amount)));


			fbxBuf.SetData(constData);

			_game.GraphicsDevice.PipelineState = factory[(int)FbxFlags.NONE];
			_game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;
			_game.GraphicsDevice.VertexShaderConstants[1] = fbxBuf;
			_game.GraphicsDevice.PixelShaderConstants[1] = fbxBuf;

			_game.GraphicsDevice.VertexShaderResources[0] = points;

			_game.GraphicsDevice.SetupVertexInput(FbxMesh.VertexBuffer, FbxMesh.IndexBuffer);

			PixHelper.BeginEvent(new SharpDX.Mathematics.Interop.RawColorBGRA(255, 0, 0, 255), "Draw fbx layer");
			_game.GraphicsDevice.DrawInstancedIndexed(FbxMesh.IndexBuffer.Capacity, DrawCount, 0, 0, 0);
			PixHelper.EndEvent();
		}


		public void UpdatePointsBuffer()
		{
			if (points == null) return;

			points.SetData(PointsCpu);
		}

	}
}
