using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.Vehicle;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using SharpDX.Direct3D;
using System.Runtime.InteropServices;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	public class ScalarVectorField : Gis.GisLayer
	{
		Ubershader		shader;
		StateFactory	factory;

		public class SelectedItem : Gis.SelectedItem
		{
			public int PointIndex;
		}


		[Flags]
		public enum FieldFlags : int
		{
			None = 0,
			DrawScalarData	= 1 << 0,
			DrawVectorData	= 1 << 1,
			DrawIsolines	= 1 << 2, 
			DrawArrows		= 1 << 3, 
			ClipValues		= 1 << 4, 
            ClipMin         = 1 << 5,
            ClipMax         = 1 << 6,
			Normalize		= 1 << 7,
		}
		
		public FieldFlags Flags { set; get; }

		VertexBuffer	vB;
		IndexBuffer		iB;

		SamplerState Sampler = SamplerState.LinearClamp;

		internal struct ConstData {
			public Vector4 FactorMinMaxDeltatime;
			public Vector4 VectorLeftRightTopBottomMargins;
			public Vector4 ArrowsscaleMaxspeedIsolineVelocitymult;
		    public Vector4 OpacityInitialMinMax;
		}

		protected ConstantBuffer	cB;


		public StructuredBuffer ScalarDataFirstFrameGpu;
		public StructuredBuffer VectorDataFirstFrameGpu;

		public StructuredBuffer ScalarDataSecondFrameGpu;
		public StructuredBuffer VectorDataSecondFrameGpu;

		
		public Texture2D Palette;
		public Texture2D ArrowTexture;
		public RenderTarget2D VelocityMap;

		public Gis.GeoPoint[]	PointsCpu { get; protected set; }

		public struct FieldParameters
		{
			internal ConstData constData;

			public float Factor				{ get { return constData.FactorMinMaxDeltatime.X; } set { constData.FactorMinMaxDeltatime.X = value; } }
			public float MinValue			{ get { return constData.FactorMinMaxDeltatime.Y; } set { constData.FactorMinMaxDeltatime.Y = value; } }
			public float MaxValue			{ get { return constData.FactorMinMaxDeltatime.Z; } set { constData.FactorMinMaxDeltatime.Z = value; } }
            public float MinValueNI         { get { return constData.OpacityInitialMinMax.Y; } set { constData.OpacityInitialMinMax.Y = value; } }
            public float MaxValueNI         { get { return constData.OpacityInitialMinMax.Z; } set { constData.OpacityInitialMinMax.Z = value; } }
		    public float MinValueMult       { get; set; }
            public float MaxValueMult       { get; set; }

			public float Opacity { get { return constData.OpacityInitialMinMax.X; } set { constData.OpacityInitialMinMax.X = value; } }

			public float VectorLeftMargin	{ get { return constData.VectorLeftRightTopBottomMargins.X; } set { constData.VectorLeftRightTopBottomMargins.X = value; } }
			public float VectorRightMargin	{ get { return constData.VectorLeftRightTopBottomMargins.Y; } set { constData.VectorLeftRightTopBottomMargins.Y = value; } }
			public float VectorTopMargin	{ get { return constData.VectorLeftRightTopBottomMargins.Z; } set { constData.VectorLeftRightTopBottomMargins.Z = value; } }
			public float VectorBottomMargin { get { return constData.VectorLeftRightTopBottomMargins.W; } set { constData.VectorLeftRightTopBottomMargins.W = value; } }
			public float ArrowsScale		{ get { return constData.ArrowsscaleMaxspeedIsolineVelocitymult.X; } set { constData.ArrowsscaleMaxspeedIsolineVelocitymult.X = value; } }
			public float MaxSpeed			{ get { return constData.ArrowsscaleMaxspeedIsolineVelocitymult.Y; } set { constData.ArrowsscaleMaxspeedIsolineVelocitymult.Y = value; } }
			public float IsolineDensity		{ get { return constData.ArrowsscaleMaxspeedIsolineVelocitymult.Z; } set { constData.ArrowsscaleMaxspeedIsolineVelocitymult.Z = value; } }

			public float VelocityMultiplication { get { return constData.ArrowsscaleMaxspeedIsolineVelocitymult.W; } set { constData.ArrowsscaleMaxspeedIsolineVelocitymult.W = value; } }

			public float LinesOpacity		{ set; get; }

			public bool DrawFlowLines { get; set; }
		}

		public FieldParameters Parameters;


		/////////////////////////////////////
		/////////////////////////////////////	Flow lines part
		/////////////////////////////////////
		[Flags]
		public enum SopliFlags : int
		{
			DrawSopli	= 1 << 0,
			UpdateSopli = 1 << 1,
		}

		struct Particle
		{
			public Vector4 LonLatDefaultLonLat;
			public Vector4 LifetimeTotallifetime;   // Texture Coordinates
		};

		struct ParticlesData
		{
			public Vector4 LineLengthWidthOpacity;
			public Vector4 GroupdimMaxparticles;
		};

		int particlesCount;
		StructuredBuffer	flowLines;
		IndexBuffer			flowIndeces;

		Ubershader		flowShader;
		StateFactory	flowFactory;

		ParticlesData	partData;
		ConstantBuffer	particlesCBuffer;

		int groupSize;


		void EnumFunc(PipelineState ps, int flag)
		{
			var flags = (FieldFlags)flag;

			ps.VertexInputElements = VertexInputElement.FromStructure<Gis.GeoPoint>();
			//ps.BlendState = flags.HasFlag(FieldFlags.XRAY) ? BlendState.Additive : BlendState.AlphaBlend;
			//ps.DepthStencilState = flags.HasFlag(FieldFlags.NO_DEPTH) ? DepthStencilState.None : DepthStencilState.Default;
			//ps.RasterizerState = flags.HasFlag(FieldFlags.CULL_NONE) ? RasterizerState.CullNone : RasterizerState.CullCW;

			ps.BlendState			= BlendState.AlphaBlend;
			ps.DepthStencilState	= DepthStencilState.None;
			ps.RasterizerState		= RasterizerState.CullCCW;

			ps.Primitive = Primitive.TriangleList;
		}



        //public ScalarVectorField(Game game) : base(game)
        //{

        //}

        public bool IsVector = false;

		public ScalarVectorField(Game game, Gis.GeoPoint[] points, int[] indeces, bool isVector = false, bool isDynamic = false) : base(game)
		{
			shader		= _game.Content.Load<Ubershader>("globe.SVField.hlsl");
			factory		= shader.CreateFactory(typeof(FieldFlags), EnumFunc);
			//factoryXray = shader.CreateFactory(typeof(FieldFlags), Primitive.TriangleList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.Additive, RasterizerState.CullCW, DepthStencilState.None);

			var vbOptions = isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;

			vB = new VertexBuffer(_game.GraphicsDevice, typeof(Gis.GeoPoint), points.Length, vbOptions);
			vB.SetData(points);

			iB = new IndexBuffer(Game.Instance.GraphicsDevice, indeces.Length);
			iB.SetData(indeces);

			PointsCpu = points;

			cB = new ConstantBuffer(_game.GraphicsDevice, typeof(ConstData));
			Parameters.constData = new ConstData();
			Parameters.constData.FactorMinMaxDeltatime				= Vector4.One;
			Parameters.constData.VectorLeftRightTopBottomMargins	= new Vector4(MathUtil.Rad(-180.0f), MathUtil.Rad(-90.0f), MathUtil.Rad(360.0f), MathUtil.Rad(180.0f));
			Parameters.constData.ArrowsscaleMaxspeedIsolineVelocitymult = new Vector4(200.0f, 1.0f, 700.0f, 1.0f);
		    Parameters.MinValueMult = 0;
		    Parameters.MaxValueMult = 1;
			Parameters.LinesOpacity = 1.0f;
			Parameters.Opacity		= 0.33f;

			//if (scalarDataSize != 0) {
			//	ScalarDataFirstFrameGpu = new StructuredBuffer(Game.GraphicsDevice, sizeof(float), scalarDataSize, StructuredBufferFlags.None);
			//}

			if (isVector) {
				VelocityMap = new RenderTarget2D(_game.GraphicsDevice, ColorFormat.Rg32F, 2048, 2048);
				ArrowTexture = game.Content.Load<Texture2D>("arrow");
            }
            IsVector = isVector;			
			Palette = _game.Content.Load <Texture2D>("pallete");
		}



		public void SetupFlowLines(Vector2[] initialPositions, float[] totalLifeTimes, int flowLineSize = 32)
		{
			if (initialPositions == null || totalLifeTimes == null || initialPositions.Length != totalLifeTimes.Length) {
				Log.Error("Setup flow failed");
			}


			flowShader = _game.Content.Load<Ubershader>("globe.FlowLines.hlsl");
			flowFactory = new StateFactory(flowShader, typeof(SopliFlags), (state, i) => {
				state.VertexInputElements	= null;
				state.BlendState			= BlendState.AlphaBlend;
				state.DepthStencilState		= DepthStencilState.None;
				state.RasterizerState		= RasterizerState.CullNone;

				state.Primitive = Primitive.LineList;
			});
			
			particlesCount	= flowLineSize*initialPositions.Length;
            flowLines		= new StructuredBuffer(_game.GraphicsDevice, typeof(Particle), particlesCount, StructuredBufferFlags.None);

			var particles = new Particle[particlesCount];

			for (int i = 0; i < particlesCount; i++) {
				var initPos = initialPositions[i/flowLineSize];
				var time	= totalLifeTimes[i / flowLineSize];

				particles[i] = new Particle {
                    LonLatDefaultLonLat		= new Vector4(initPos.X, initPos.Y, initPos.X, initPos.Y),
					LifetimeTotallifetime	= new Vector4(0, time, 0, 0)
				};
			}

			var indeces = new int[2*(flowLineSize - 1)*initialPositions.Length];
			flowIndeces = new IndexBuffer(_game.GraphicsDevice, indeces.Length);

			int indInd = 0;
			for(int lineInd = 0; lineInd < initialPositions.Length; lineInd++) {
				for(int ind = 0; ind < flowLineSize-1; ind++) {
					int i = lineInd * flowLineSize + ind;
					indeces[indInd++] = i;
					indeces[indInd++] = i + 1;
                }
			}

			flowLines.SetData(particles);
			flowIndeces.SetData(indeces);

			int		numGroups	= (particlesCount % 1024 != 0) ? ((particlesCount / 1024) + 1) : (particlesCount / 1024);
			double	secondRoot	= Math.Pow((double)numGroups, (double)(1.0 / 2.0));
			secondRoot = Math.Ceiling(secondRoot);
			groupSize = (int)secondRoot;

			partData = new ParticlesData();
			partData.LineLengthWidthOpacity		= new Vector4(flowLineSize, 0.1f, 0.0f, 0.0f);
			partData.GroupdimMaxparticles	= new Vector4(groupSize, particlesCount, 0, 0);
			
			particlesCBuffer = new ConstantBuffer(_game.GraphicsDevice, typeof(ParticlesData));
			particlesCBuffer.SetData(partData);
        }


		public override void Dispose()
		{
			//shader.Dispose();	
			factory.Dispose();
			//factoryXray.Dispose();

			vB.Dispose();
			iB.Dispose();

			cB.Dispose();

			base.Dispose();
		}


		public void UpdatePointsBuffer()
		{
			if (vB == null) return;

			vB.SetData(PointsCpu);
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			if (vB == null || iB == null) {
				Log.Warning("Poly layer null reference");
				return;
			}

			Parameters.constData.FactorMinMaxDeltatime.W = gameTime.ElapsedSec;
		    Parameters.MaxValue = MathUtil.Lerp(Parameters.MinValueNI, Parameters.MaxValueNI, Parameters.MaxValueMult);
		    Parameters.MinValue = MathUtil.Lerp(Parameters.MinValueNI, Parameters.MaxValueNI, Parameters.MinValueMult);            
            cB.SetData(Parameters.constData);            

			_game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			_game.GraphicsDevice.VertexShaderConstants[1]	= cB;
			_game.GraphicsDevice.PixelShaderConstants[1]		= cB;

			_game.GraphicsDevice.SetupVertexInput(vB, iB);

			// Vector part
			if (VelocityMap != null) {
				DepthStencilSurface depth;
				RenderTargetSurface[] surfaces;
				_game.GraphicsDevice.GetTargets(out depth, out surfaces);

				_game.GraphicsDevice.SetTargets(null, VelocityMap);

				_game.GraphicsDevice.PipelineState = factory[(int)FieldFlags.DrawVectorData];

				_game.GraphicsDevice.VertexShaderResources[5] = VectorDataFirstFrameGpu;
				_game.GraphicsDevice.VertexShaderResources[6] = VectorDataSecondFrameGpu;

				PixHelper.BeginEvent(new SharpDX.Mathematics.Interop.RawColorBGRA(255, 0, 0, 255), "Draw vector data");
				_game.GraphicsDevice.DrawIndexed(iB.Capacity, 0, 0);
				PixHelper.EndEvent();

				// Restore previous targets
				_game.GraphicsDevice.SetTargets(depth, surfaces);
			}

			// Scalar part
			_game.GraphicsDevice.PipelineState = factory[(int)FieldFlags.DrawScalarData | (int)Flags];

			if (Palette != null)
				_game.GraphicsDevice.PixelShaderResources[0] = Palette;

			if(VelocityMap != null)
				_game.GraphicsDevice.PixelShaderResources[1] = VelocityMap;

			_game.GraphicsDevice.VertexShaderResources[3] = ScalarDataFirstFrameGpu;
			_game.GraphicsDevice.VertexShaderResources[4] = ScalarDataSecondFrameGpu;

			_game.GraphicsDevice.PixelShaderResources[7] = ArrowTexture;

			_game.GraphicsDevice.PixelShaderSamplers[0] = Sampler;
			_game.GraphicsDevice.PixelShaderSamplers[1] = SamplerState.AnisotropicClamp;
			
			_game.GraphicsDevice.DrawIndexed(iB.Capacity, 0, 0);


			// Sopli part
			if (Parameters.DrawFlowLines && VelocityMap != null && flowLines != null) {


				// Update sopli
				PixHelper.BeginEvent(new SharpDX.Mathematics.Interop.RawColorBGRA(255, 0, 0, 255), "Sopli updating");

				_game.GraphicsDevice.PipelineState = flowFactory[(int)SopliFlags.UpdateSopli];

				partData.LineLengthWidthOpacity.Z = Parameters.LinesOpacity;
                particlesCBuffer.SetData(partData);

				_game.GraphicsDevice.ComputeShaderConstants[1] = cB;
				_game.GraphicsDevice.ComputeShaderConstants[2] = particlesCBuffer;

				_game.GraphicsDevice.ComputeShaderResources[1] = VelocityMap;
				_game.GraphicsDevice.ComputeShaderSamplers[0] = Sampler;

				_game.GraphicsDevice.SetCSRWBuffer(0, flowLines);

				_game.GraphicsDevice.Dispatch(groupSize, groupSize, 1);

				_game.GraphicsDevice.SetCSRWBuffer(0, null);

				PixHelper.EndEvent();

				// Draw sopli
				PixHelper.BeginEvent(new SharpDX.Mathematics.Interop.RawColorBGRA(255, 0, 0, 255), "Sopli drawing");

				_game.GraphicsDevice.VertexShaderResources[5] = flowLines;
                _game.GraphicsDevice.VertexShaderConstants[2] = particlesCBuffer;
				
				_game.GraphicsDevice.PipelineState = flowFactory[(int)SopliFlags.DrawSopli];
				
				_game.GraphicsDevice.SetupVertexInput(null, flowIndeces);
				_game.GraphicsDevice.DrawIndexed(flowIndeces.Capacity, 0, 0);

				PixHelper.EndEvent();
			}
		}

		public double selectDistance = 3.0;

		public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
		{
			DVector3[] rayHitPoints;
			var ret = new List<Gis.SelectedItem>();

			if (!GeoHelper.LineIntersection(nearPoint, farPoint, GeoHelper.EarthRadius, out rayHitPoints)) return ret;

			var rayLonLatRad = GeoHelper.CartesianToSpherical(rayHitPoints[0]);

			double minDistance = 0;
			int		minIndex = -1;

			for (int i = 0; i < PointsCpu.Length; i++) {
				var point		= PointsCpu[i];
				var pointLonLat = new DVector2(point.Lon, point.Lat);
				var dist = GeoHelper.DistanceBetweenTwoPoints(pointLonLat, rayLonLatRad);

				if (dist < minDistance || minIndex == -1 ) {
					minDistance = dist;
					minIndex = i;
				}
			}

			if (minIndex >= 0 && minDistance < selectDistance)
				ret.Add(new SelectedItem {
					Distance	= minDistance,
					PointIndex	= minIndex
				});

			return ret;
		}

	    public List<SelectedItem> SelectAll(DVector3 nearPoint, DVector3 farPoint, double radius)
	    {
            DVector3[] rayHitPoints;
            var ret = new List<SelectedItem>();

            if (!GeoHelper.LineIntersection(nearPoint, farPoint, GeoHelper.EarthRadius, out rayHitPoints)) return ret;

            var rayLonLatRad = GeoHelper.CartesianToSpherical(rayHitPoints[0]);

            for (int i = 0; i < PointsCpu.Length; i++)
            {
                var point = PointsCpu[i];
                var pointLonLat = new DVector2(point.Lon, point.Lat);
                var dist = GeoHelper.DistanceBetweenTwoPoints(pointLonLat, rayLonLatRad);

                if (dist < radius)
                {
                    ret.Add(new SelectedItem()
                    {
                        Distance = dist,
                        PointIndex = i,
                    });
                }
            }
            return ret;
        }	    
	}
}
