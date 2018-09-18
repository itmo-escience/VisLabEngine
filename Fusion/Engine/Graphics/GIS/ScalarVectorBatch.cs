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
	public class ScalarVectorBatch : Gis.GisLayer
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
			NorthPoleRegion = 1 << 8,
			DrawVectorArrows = 1 << 9,
			ClipZero		= 1 << 10,
			CullCW			= 1 << 11,
			CullEarth		= 1 << 12,
		}

		VertexBuffer	vB;
		IndexBuffer		iB;

		SamplerState Sampler = SamplerState.LinearClamp;
		SamplerState FlowSampler = SamplerState.PointWrap;

		internal struct ConstData {
			public Vector4 FactorMinMaxDeltatime;
			public Vector4 VectorLeftRightTopBottomMargins;
			public Vector4 ArrowsscaleMaxspeedIsolineVelocitymult;
		    public Vector4 OpacityInitialMinMax;
		}

		protected ConstantBuffer cB;

		public StructuredBuffer ScalarDataFirstFrameGpu;
		public StructuredBuffer ScalarDataSecondFrameGpu;

		public StructuredBuffer VectorDataXComponentFirstFrameGpu;
		public StructuredBuffer VectorDataXComponentSecondFrameGpu;
		public StructuredBuffer VectorDataYComponentFirstFrameGpu;
		public StructuredBuffer VectorDataYComponentSecondFrameGpu;

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

			public float Opacity			{ get { return constData.OpacityInitialMinMax.X; } set { constData.OpacityInitialMinMax.X = value; } }

			public float VectorLeftMargin	{ get { return constData.VectorLeftRightTopBottomMargins.X; } set { constData.VectorLeftRightTopBottomMargins.X = value; } }
			public float VectorRightMargin	{ get { return constData.VectorLeftRightTopBottomMargins.Y; } set { constData.VectorLeftRightTopBottomMargins.Y = value; } }
			public float VectorTopMargin	{ get { return constData.VectorLeftRightTopBottomMargins.Z; } set { constData.VectorLeftRightTopBottomMargins.Z = value; } }
			public float VectorBottomMargin { get { return constData.VectorLeftRightTopBottomMargins.W; } set { constData.VectorLeftRightTopBottomMargins.W = value; } }

			public float ArrowsScale		{ get { return constData.ArrowsscaleMaxspeedIsolineVelocitymult.X; } set { constData.ArrowsscaleMaxspeedIsolineVelocitymult.X = value; } }
			public float MaxSpeed			{ get { return constData.ArrowsscaleMaxspeedIsolineVelocitymult.Y; } set { constData.ArrowsscaleMaxspeedIsolineVelocitymult.Y = value; } }
			public float IsolineDensity		{ get { return constData.ArrowsscaleMaxspeedIsolineVelocitymult.Z; } set { constData.ArrowsscaleMaxspeedIsolineVelocitymult.Z = value; } }
            public float ZOrder				{ get; set; }

			public float VelocityMultiplication { get { return constData.ArrowsscaleMaxspeedIsolineVelocitymult.W; } set { constData.ArrowsscaleMaxspeedIsolineVelocitymult.W = value; } }

			public float LinesOpacity	{ set; get; }

			public bool DrawFlowLines	{ get; set; }
			public bool DrawArrows		{ get; set; }
            public bool DrawIsolines	{ get { return (Flags & FieldFlags.DrawIsolines) != 0; } set { Flags = value ? Flags | FieldFlags.DrawIsolines : Flags & ~FieldFlags.DrawIsolines ; } }
            public bool ClipMin			{ get; set; }
			public bool ClipZero		{ get; set; }
			public bool CullCW			{ get; set; }
			public bool CullEarth		{ get; set; }

			public FieldFlags Flags { set; get; }

			public Color3 FlowLinesColor { set; get; }

			public string PalettePath;
			public bool IsNorthPole;

			public Texture2D Palette;		    
        }

		public FieldParameters Parameters;


		#region Flow lines part
		[Flags]
		public enum SopliFlags : int
		{
			DrawSopli		= 1 << 0,
			UpdateSopli		= 1 << 1,
			NorthPoleRegion = 1 << 2,
		}

		struct Particle
		{
			public Vector4 LonLatDefaultLonLat;
			public Vector4 LifetimeTotallifetime;   // Texture Coordinates
		};

		internal struct ParticlesData
		{
			public Vector4 LineLengthWidthOpacityRed;
			public Vector4 GroupdimMaxparticlesGreenBlue;
		};

		
		Ubershader		flowShader;
		StateFactory	flowFactory;

		public class FlowPack
		{
			internal int				ParticlesCount;
			internal StructuredBuffer	FlowLines;
			internal IndexBuffer		FlowIndeces;

			internal ParticlesData	PartData;
			internal ConstantBuffer	ParticlesCBuffer;

			internal int GroupSize;
		}
		
		#endregion


		public class FieldJob
		{
			public enum JobType
			{
				Scalar,
				Vector
			}

			public FieldParameters Parameters;
			public StructuredBuffer FirstBuffer;
			public StructuredBuffer SecondBuffer;
			public StructuredBuffer FirstYComponentBuffer;
			public StructuredBuffer SecondYComponentBuffer;

			public FlowPack FlowPack;

			public JobType Type;
		}

		List<FieldJob> Jobs = new List<FieldJob>();        


		public void AddJob(FieldJob job)
		{
			Jobs.Add(job);
		}


		void EnumFunc(PipelineState ps, int flag)
		{
			var flags = (FieldFlags)flag;

			ps.VertexInputElements = VertexInputElement.FromStructure<Gis.GeoPoint>();
			//ps.BlendState = flags.HasFlag(FieldFlags.XRAY) ? BlendState.Additive : BlendState.AlphaBlend;
			//ps.DepthStencilState = flags.HasFlag(FieldFlags.NO_DEPTH) ? DepthStencilState.None : DepthStencilState.Default;
			//ps.RasterizerState = flags.HasFlag(FieldFlags.CULL_NONE) ? RasterizerState.CullNone : RasterizerState.CullCW;

			ps.BlendState			= BlendState.AlphaBlend;
			ps.DepthStencilState	= DepthStencilState.None;
			ps.RasterizerState		= flags.HasFlag(FieldFlags.CullCW) ? RasterizerState.CullCW : RasterizerState.CullCCW;


			if(flags.HasFlag(FieldFlags.DrawVectorData)) ps.BlendState = BlendState.Opaque;

			ps.Primitive = Primitive.TriangleList;
		}


		
		public ScalarVectorBatch(Game game) : base(game)
		{

		}


		public ScalarVectorBatch(Game game, Gis.GeoPoint[] points, int[] indeces, bool isDynamic = false) : base(game)
		{
			shader		= Game.Content.Load<Ubershader>("globe.SVFieldBatch.hlsl");
			factory		= shader.CreateFactory(typeof(FieldFlags), EnumFunc);

			flowShader = Game.Content.Load<Ubershader>("globe.FlowLines.hlsl");
			flowFactory = new StateFactory(flowShader, typeof(SopliFlags), (state, i) => {
				state.VertexInputElements	= null;
				state.BlendState			= BlendState.AlphaBlend;
				state.DepthStencilState		= DepthStencilState.None;
				state.RasterizerState		= RasterizerState.CullNone;

				state.Primitive = Primitive.LineList;
			});

		    ZOrder = 100;

			var vbOptions = isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;

			vB = new VertexBuffer(Game.GraphicsDevice, typeof(Gis.GeoPoint), points.Length, vbOptions);
			vB.SetData(points);

			iB = new IndexBuffer(Game.Instance.GraphicsDevice, indeces.Length);
			iB.SetData(indeces);

			PointsCpu = points;

			cB = new ConstantBuffer(Game.GraphicsDevice, typeof(ConstData));
			//Parameters.constData = new ConstData();
			Parameters.constData.FactorMinMaxDeltatime				= Vector4.One;
			Parameters.constData.VectorLeftRightTopBottomMargins	= new Vector4(MathUtil.Rad(-180.0f), MathUtil.Rad(-90.0f), MathUtil.Rad(360.0f), MathUtil.Rad(180.0f));
			Parameters.constData.ArrowsscaleMaxspeedIsolineVelocitymult	= new Vector4(200.0f, 1.0f, 700.0f, 1.0f);
		    Parameters.MinValueMult = 0;
		    Parameters.MaxValueMult = 1;
			Parameters.LinesOpacity = 1.0f;

			Parameters.Opacity = 1.0f;
			
			Parameters.Palette = Game.Content.Load <Texture2D>("pallete");
		    Parameters.PalettePath = "pallete.tga";
            VelocityMap		= new RenderTarget2D(Game.GraphicsDevice, ColorFormat.Rgba32F, 2048, 2048);
			ArrowTexture	= Game.Content.Load<Texture2D>("arrowWhite");
		}



		public static FlowPack SetupFlowLines(Vector2[] initialPositions, float[] totalLifeTimes, int flowLineSize = 32)
		{
			var ret = new FlowPack();

			ret.ParticlesCount = flowLineSize*initialPositions.Length;
            ret.FlowLines		= new StructuredBuffer(Game.Instance.GraphicsDevice, typeof(Particle), ret.ParticlesCount, StructuredBufferFlags.None);

			var particles = new Particle[ret.ParticlesCount];

			for (int i = 0; i < ret.ParticlesCount; i++) {
				var initPos = initialPositions[i/flowLineSize];
				var time	= totalLifeTimes[i / flowLineSize];

				particles[i] = new Particle {
                    LonLatDefaultLonLat		= new Vector4(initPos.X, initPos.Y, initPos.X, initPos.Y),
					LifetimeTotallifetime	= new Vector4(0, time, 0, 0)
				};
			}

			var indeces = new int[2*(flowLineSize - 1)*initialPositions.Length];
			ret.FlowIndeces = new IndexBuffer(Game.Instance.GraphicsDevice, indeces.Length);

			int indInd = 0;
			for(int lineInd = 0; lineInd < initialPositions.Length; lineInd++) {
				for(int ind = 0; ind < flowLineSize-1; ind++) {
					int i = lineInd * flowLineSize + ind;
					indeces[indInd++] = i;
					indeces[indInd++] = i + 1;
                }
			}

			ret.FlowLines.SetData(particles);
			ret.FlowIndeces.SetData(indeces);

			int		numGroups	= (ret.ParticlesCount % 1024 != 0) ? ((ret.ParticlesCount / 1024) + 1) : (ret.ParticlesCount / 1024);
			double	secondRoot	= Math.Pow((double)numGroups, (double)(1.0 / 2.0));
			secondRoot = Math.Ceiling(secondRoot);
			ret.GroupSize = (int)secondRoot;

			ret.PartData = new ParticlesData();
			ret.PartData.LineLengthWidthOpacityRed		= new Vector4(flowLineSize, 0.1f, 0.0f, 0.0f);
			ret.PartData.GroupdimMaxparticlesGreenBlue	= new Vector4(ret.GroupSize, ret.ParticlesCount, 0, 0);
			
			ret.ParticlesCBuffer = new ConstantBuffer(Game.Instance.GraphicsDevice, typeof(ParticlesData));
			ret.ParticlesCBuffer.SetData(ret.PartData);


			return ret;
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
			vB?.SetData(PointsCpu);
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			if (vB == null || iB == null) {
				Log.Warning("Poly layer null reference");
				return;
			}

			if (!Jobs.Any()) return;     

            Jobs.Sort((a, b) => b.Parameters.ZOrder.CompareTo(a.Parameters.ZOrder));

			Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			Game.GraphicsDevice.VertexShaderConstants[1]	= cB;
			Game.GraphicsDevice.PixelShaderConstants[1]		= cB;
			

			foreach (var job in Jobs) {
				Game.GraphicsDevice.SetupVertexInput(vB, iB);

				if (job.Type == FieldJob.JobType.Scalar) {
					DoScalarJob(job, gameTime);
				}
				else if (job.Type == FieldJob.JobType.Vector) {
					DoVelocityJob(job, gameTime);
				}
				
			}
            
			Jobs.Clear();
        }


		void DoVelocityJob(FieldJob vJob, GameTime gameTime)
		{
			Parameters = vJob.Parameters;
			VectorDataXComponentFirstFrameGpu	= vJob.FirstBuffer;
			VectorDataXComponentSecondFrameGpu	= vJob.SecondBuffer;
			VectorDataYComponentFirstFrameGpu	= vJob.FirstYComponentBuffer;
			VectorDataYComponentSecondFrameGpu	= vJob.SecondYComponentBuffer;

			SetBuffer(gameTime);
			DrawVelocity();

			DrawArrows();
			DrawFlowLines(vJob.FlowPack);
		}


		void DoScalarJob(FieldJob job, GameTime gameTime)
		{
			using (new PixEvent("Drav Scalar Data")) {
				Parameters = job.Parameters;
				ScalarDataFirstFrameGpu		= job.FirstBuffer;
				ScalarDataSecondFrameGpu	= job.SecondBuffer;
				SetBuffer(gameTime);
				DrawScalarData();
			}
		}


		protected void SetBuffer(GameTime gameTime)
		{
			Parameters.constData.FactorMinMaxDeltatime.W = gameTime.ElapsedSec;            
			Parameters.MaxValue = MathUtil.Lerp(Parameters.MinValueNI, Parameters.MaxValueNI, Parameters.MaxValueMult);
			Parameters.MinValue = MathUtil.Lerp(Parameters.MinValueNI, Parameters.MaxValueNI, Parameters.MinValueMult);
			cB.SetData(Parameters.constData);
        }

		protected void DrawVelocity()
		{
			// Vector part
			if (VelocityMap != null) {
				DepthStencilSurface depth;
				RenderTargetSurface[] surfaces;
				Game.GraphicsDevice.GetTargets(out depth, out surfaces);
				var oldViewPort = Game.GraphicsDevice.GetViewport();

				Game.GraphicsDevice.Clear(VelocityMap.Surface, Color4.Zero);
				Game.GraphicsDevice.SetTargets(null, VelocityMap);

				var flags = FieldFlags.DrawVectorData;
				if(Parameters.IsNorthPole) flags |= FieldFlags.NorthPoleRegion;

				Game.GraphicsDevice.PipelineState = factory[(int)flags];

				Game.GraphicsDevice.VertexShaderResources[5] = VectorDataXComponentFirstFrameGpu;
				Game.GraphicsDevice.VertexShaderResources[6] = VectorDataXComponentSecondFrameGpu;
				Game.GraphicsDevice.VertexShaderResources[7] = VectorDataYComponentFirstFrameGpu;
				Game.GraphicsDevice.VertexShaderResources[8] = VectorDataYComponentSecondFrameGpu;

				PixHelper.BeginEvent(new SharpDX.Mathematics.Interop.RawColorBGRA(255, 0, 0, 255), "Draw vector data");
				Game.GraphicsDevice.DrawIndexed(iB.Capacity, 0, 0);
				PixHelper.EndEvent();

				// Restore previous targets
				Game.GraphicsDevice.SetTargets(depth, surfaces);
				Game.GraphicsDevice.SetViewport(oldViewPort);

				Game.GraphicsDevice.VertexShaderResources[5] = null;
				Game.GraphicsDevice.VertexShaderResources[6] = null;
				Game.GraphicsDevice.VertexShaderResources[7] = null;
				Game.GraphicsDevice.VertexShaderResources[8] = null;
			}
		}

		protected void DrawScalarData()
		{
			// Scalar part
		    var f = FieldFlags.DrawScalarData | FieldFlags.Normalize;
		    f |= Parameters.Flags;
		    if (Parameters.ClipMin)		f |= FieldFlags.ClipMin;
			if (Parameters.ClipZero)	f |= FieldFlags.ClipZero;
			if (Parameters.CullCW)		f |= FieldFlags.CullCW;
			if (Parameters.CullEarth)	f |= FieldFlags.CullEarth;

			Game.GraphicsDevice.PipelineState = factory[(int)f];

			if (Parameters.Palette != null)
				Game.GraphicsDevice.PixelShaderResources[0] = Parameters.Palette;

			if(VelocityMap != null)
				Game.GraphicsDevice.PixelShaderResources[1] = VelocityMap;

			Game.GraphicsDevice.VertexShaderResources[3] = ScalarDataFirstFrameGpu;
			Game.GraphicsDevice.VertexShaderResources[4] = ScalarDataSecondFrameGpu;

			Game.GraphicsDevice.PixelShaderResources[7] = ArrowTexture;

			Game.GraphicsDevice.PixelShaderSamplers[0] = Sampler;
			
			Game.GraphicsDevice.DrawIndexed(iB.Capacity, 0, 0);


			Game.GraphicsDevice.PixelShaderResources[0]		= null;
			Game.GraphicsDevice.PixelShaderResources[1]		= null;
			Game.GraphicsDevice.VertexShaderResources[3]	= null;
			Game.GraphicsDevice.VertexShaderResources[4]	= null;
			Game.GraphicsDevice.PixelShaderResources[7]		= null;
			Game.GraphicsDevice.PixelShaderSamplers[0]		= null;
		}


		protected void DrawArrows()
		{
			if (!Parameters.DrawArrows || VelocityMap == null) return;
			// Scalar part
			var flags = FieldFlags.DrawVectorArrows;

			if(Parameters.IsNorthPole) flags |= FieldFlags.NorthPoleRegion;

            Game.GraphicsDevice.PipelineState = factory[(int)(flags)];

			Game.GraphicsDevice.PixelShaderResources[1] = VelocityMap;
			Game.GraphicsDevice.PixelShaderResources[7] = ArrowTexture;

			Game.GraphicsDevice.PixelShaderSamplers[0] = Sampler;
			Game.GraphicsDevice.PixelShaderSamplers[1] = FlowSampler;

			Game.GraphicsDevice.DrawIndexed(iB.Capacity, 0, 0);

			Game.GraphicsDevice.PixelShaderResources[1] = null;
			Game.GraphicsDevice.PixelShaderResources[7] = null;
			Game.GraphicsDevice.PixelShaderSamplers[0]	= null;
			Game.GraphicsDevice.PixelShaderSamplers[1]	= null;
		}


		protected void DrawFlowLines(FlowPack pack)
		{
			// Sopli part
			if (Parameters.DrawFlowLines && VelocityMap != null && pack.FlowLines != null) {
				// Update sopli
				PixHelper.BeginEvent(new SharpDX.Mathematics.Interop.RawColorBGRA(255, 0, 0, 255), "Sopli updating");

				var flags = SopliFlags.UpdateSopli;
				if (Parameters.IsNorthPole) flags |= SopliFlags.NorthPoleRegion;
				
				Game.GraphicsDevice.PipelineState = flowFactory[(int)flags];

				pack.PartData.LineLengthWidthOpacityRed.Z		= Parameters.LinesOpacity;
				pack.PartData.LineLengthWidthOpacityRed.W		= Parameters.FlowLinesColor.Red;
				pack.PartData.GroupdimMaxparticlesGreenBlue.Z	= Parameters.FlowLinesColor.Green;
				pack.PartData.GroupdimMaxparticlesGreenBlue.W	= Parameters.FlowLinesColor.Blue;
				pack.ParticlesCBuffer.SetData(pack.PartData);

				Game.GraphicsDevice.ComputeShaderConstants[1] = cB;
				Game.GraphicsDevice.ComputeShaderConstants[2] = pack.ParticlesCBuffer;

				Game.GraphicsDevice.ComputeShaderResources[1]	= VelocityMap;
				Game.GraphicsDevice.ComputeShaderSamplers[0]	= FlowSampler;

				Game.GraphicsDevice.SetCSRWBuffer(0, pack.FlowLines);

				Game.GraphicsDevice.Dispatch(pack.GroupSize, pack.GroupSize, 1);

				Game.GraphicsDevice.SetCSRWBuffer(0, null);
				Game.GraphicsDevice.ComputeShaderSamplers[0] = null;
				Game.GraphicsDevice.ComputeShaderResources[1] = null;

				Game.GraphicsDevice.ComputeShaderConstants[1] = null;
				Game.GraphicsDevice.ComputeShaderConstants[2] = null;


				PixHelper.EndEvent();

				// Draw sopli
				PixHelper.BeginEvent(new SharpDX.Mathematics.Interop.RawColorBGRA(255, 0, 0, 255), "Sopli drawing");

				Game.GraphicsDevice.VertexShaderResources[5] = pack.FlowLines;
                Game.GraphicsDevice.VertexShaderConstants[2] = pack.ParticlesCBuffer;
				
				Game.GraphicsDevice.PipelineState = flowFactory[(int)SopliFlags.DrawSopli];
				
				Game.GraphicsDevice.SetupVertexInput(null, pack.FlowIndeces);
				Game.GraphicsDevice.DrawIndexed(pack.FlowIndeces.Capacity, 0, 0);

				Game.GraphicsDevice.SetupVertexInput(null, null);
				Game.GraphicsDevice.VertexShaderResources[5] = null;
				Game.GraphicsDevice.VertexShaderConstants[2] = null;

				PixHelper.EndEvent();
			}
		}


        public List<Gis.SelectedItem> SelectInPoint(DVector3 selectPoint)
        {
            var ret = new List<Gis.SelectedItem>();
            var rayLonLatRad = GeoHelper.CartesianToSpherical(selectPoint);

            double minDistance = 0;
            int minIndex = -1;

            for (int i = 0; i < PointsCpu.Length; i++)
            {
                var point = PointsCpu[i];
                var pointLonLat = new DVector2(point.Lon, point.Lat);
                var dist = GeoHelper.DistanceBetweenTwoPoints(pointLonLat, rayLonLatRad);

                if (dist < minDistance || minIndex == -1)
                {
                    minDistance = dist;
                    minIndex = i;
                }
            }

            if (minIndex >= 0 && minDistance < selectDistance)
                ret.Add(new SelectedItem
                {
                    Distance = minDistance,
                    PointIndex = minIndex
                });

            return ret;
        }

		double selectDistance = 8.0;
		public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
		{
			DVector3[] rayHitPoints;
			var ret = new List<Gis.SelectedItem>();

			if (!GeoHelper.LineIntersection(nearPoint, farPoint, GeoHelper.EarthRadius, out rayHitPoints)) return ret;

            return SelectInPoint(rayHitPoints[0]);
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
