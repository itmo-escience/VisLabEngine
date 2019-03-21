using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using SharpDX.Direct3D11;
using BlendState = Fusion.Drivers.Graphics.BlendState;
using DepthStencilState = Fusion.Drivers.Graphics.DepthStencilState;
using Point = Fusion.Core.Mathematics.Point;
using RasterizerState = Fusion.Drivers.Graphics.RasterizerState;
using SamplerState = Fusion.Drivers.Graphics.SamplerState;
using Texture2D = Fusion.Drivers.Graphics.Texture2D;

namespace Fusion.Engine.Graphics.GIS
{
	public class LinesGisLayer : Gis.GisLayer
	{
	    private readonly Ubershader _shader;
	    private readonly StateFactory _factory;
	    private readonly StateFactory _thinFactory;

		[Flags]
		public enum LineFlags : int
		{
			DRAW_LINES				= 1 << 0,
			DRAW_SEGMENTED_LINES	= 1 << 1,
			ARC_LINE				= 1 << 2,
			ADD_CAPS				= 1 << 3,
			FADING_LINE				= 1 << 4,
			THIN_LINE				= 1 << 5,
			OVERALL_COLOR = 1 << 6,
			TEXTURED_LINE = 1 << 7,
			PALETTE_COLOR = 1 << 8,
		    GEO_LINES = 1 << 9,
        }

		public int Flags;

		[StructLayout(LayoutKind.Explicit)]
		private struct LinesConstDataStruct {
			[FieldOffset(0)] public float TransparencyMultiplayer;
			[FieldOffset(4)] public Vector3 Dummy;
			[FieldOffset(16)] public Color4 OverallColor;
		}

		private LinesConstDataStruct _linesConstData = new LinesConstDataStruct();
		private readonly ConstantBuffer _linesConstantBuffer;

	    private bool _isDynamic;
	    private bool _isDirty = false;
		public float TransparencyMultiplier {
		    get => _linesConstData.TransparencyMultiplayer;
            set {
				_linesConstData.TransparencyMultiplayer = value;
				_isDirty = true;
			}
		}

		public Color4 OverallColor {
		    get => _linesConstData.OverallColor;
            set {
				_linesConstData.OverallColor = value;
				_isDirty = true;
			}
		}

	    private TimeSpan _animationTimer = TimeSpan.Zero;
        private TimeSpan _animationDuration = TimeSpan.FromSeconds(1);
        private float _animationSpeedMultiplier = 1;
	    public float AnimationSpeed
	    {
	        get => _animationSpeedMultiplier;
	        set
	        {
                if(value <= 0) throw new ArgumentException("Multiplier should be greater then zero");
	            _animationSpeedMultiplier = value;
                _animationDuration = TimeSpan.FromSeconds(1 / _animationSpeedMultiplier);
	        }
	    }

		public bool IsDoubleBuffer { get; protected set; }

		public Texture2D Texture;

	    private VertexBuffer _currentBuffer;
	    private int _initialPointsCount;

	    public Gis.GeoPoint[] PointsCpu { get; set; }

		public class SelectedItem : Gis.SelectedItem {}

		public override void Dispose()
		{
		    _currentBuffer?.Dispose();
		}

		public LinesGisLayer(Game engine, int linesPointsCount, bool isDynamic = false) : base(engine)
		{
		    TransparencyMultiplier = 1.0f;
		    OverallColor = Color4.White;

		    _initialPointsCount = linesPointsCount;
            _isDynamic = isDynamic;
            _shader = Game.Content.Load<Ubershader>("globe.Line.hlsl");
			_factory = _shader.CreateFactory( typeof(LineFlags), Primitive.LineList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.CullNone, DepthStencilState.None);
			_thinFactory = _shader.CreateFactory( typeof(LineFlags), Primitive.LineList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.CullNone, DepthStencilState.None);
			_linesConstantBuffer = new ConstantBuffer(engine.GraphicsDevice, typeof(LinesConstDataStruct));

            PointsCpu	= new Gis.GeoPoint[linesPointsCount];
			Flags		= (int)(LineFlags.THIN_LINE);

		    var vbOptions = _isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;
		    _currentBuffer = new VertexBuffer(Game.Instance.GraphicsDevice, typeof(Gis.GeoPoint), PointsCpu.Length, vbOptions);
        }

		public void UpdatePointsBuffer()
		{
		    if (_currentBuffer != null && _currentBuffer.Capacity != PointsCpu.Length)
		    {
		        _currentBuffer.Dispose();

		        var vbOptions = _isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;
		        _currentBuffer = new VertexBuffer(Game.Instance.GraphicsDevice, typeof(Gis.GeoPoint), PointsCpu.Length, vbOptions);
		    }

		    _currentBuffer?.SetData(PointsCpu);
		}

	    public void AddLine(List<Gis.GeoPoint> lonLatPoints, bool updateBuffer = true)
	    {
	        var textureRatio = Texture == null ? 1 : ((float)Texture.Width) / (Texture.Height);
            Gis.GeoPoint Clone(Gis.GeoPoint p, float textureDistance)
	        {
                var r = new Gis.GeoPoint { Lon = p.Lon, Lat = p.Lat, Color = p.Color, Tex0 = p.Tex0, Tex1 = p.Tex1 };
	            var w = 2 * (Math.Abs(r.Tex0.X) < float.Epsilon ? .5f : r.Tex0.X);
	            r.Tex1.Y = textureDistance / (textureRatio * w);
	            return r;
	        }
	        var newPoints = new List<Gis.GeoPoint>(PointsCpu);

	        float totalDistance = 0.0f;
	        newPoints.Add(Clone(lonLatPoints[0], totalDistance));
            for (var i = 1; i < lonLatPoints.Count - 1; i++)
            {
                totalDistance += (float) GeoHelper.DistanceBetweenTwoPoints(
                    new DVector2(lonLatPoints[i - 1].Lon, lonLatPoints[i - 1].Lat),
                    new DVector2(lonLatPoints[i].Lon, lonLatPoints[i - 1].Lat)
                );

                newPoints.Add(Clone(lonLatPoints[i], totalDistance));
                newPoints.Add(Clone(lonLatPoints[i], totalDistance));
            }
	        totalDistance += (float)GeoHelper.DistanceBetweenTwoPoints(
	            new DVector2(lonLatPoints[lonLatPoints.Count - 2].Lon, lonLatPoints[lonLatPoints.Count - 2].Lat),
	            new DVector2(lonLatPoints[lonLatPoints.Count - 1].Lon, lonLatPoints[lonLatPoints.Count - 1].Lat)
	        );
            newPoints.Add(Clone(lonLatPoints[lonLatPoints.Count - 1], totalDistance));

            PointsCpu = newPoints.ToArray();

           if (updateBuffer) UpdatePointsBuffer();
	    }

	    public static LinesGisLayer CreateFromLines(Game game, List<List<Gis.GeoPoint>> lonLatLines)
	    {
	        LinesGisLayer layer = new LinesGisLayer(game, lonLatLines.Sum(a => a.Count), false);

            var textureRatio = layer.Texture == null ? 1 : ((float)layer.Texture.Width) / (layer.Texture.Height);

	        Gis.GeoPoint Clone(Gis.GeoPoint p, float textureDistance)
	        {
	            var r = new Gis.GeoPoint { Lon = p.Lon, Lat = p.Lat, Color = p.Color, Tex0 = p.Tex0, Tex1 = p.Tex1 };
	            var w = 2 * (Math.Abs(r.Tex0.X) < float.Epsilon ? .5f : r.Tex0.X);
	            r.Tex1.Y = textureDistance / (textureRatio * w);
	            return r;
	        }


	        List<Gis.GeoPoint> newPoints = new List<Gis.GeoPoint>();
	        foreach (var lonLatPoints in lonLatLines)
	        {
	            float totalDistance = 0.0f;
	            newPoints.Add(Clone(lonLatPoints[0], totalDistance));
	            for (var i = 1; i < lonLatPoints.Count() - 1; i++)
	            {
	                totalDistance += (float)GeoHelper.DistanceBetweenTwoPoints(
	                    new DVector2(lonLatPoints[i - 1].Lon, lonLatPoints[i - 1].Lat),
	                    new DVector2(lonLatPoints[i].Lon, lonLatPoints[i - 1].Lat)
	                );

	                newPoints.Add(Clone(lonLatPoints[i], totalDistance));
	                newPoints.Add(Clone(lonLatPoints[i], totalDistance));
	            }
	            totalDistance += (float)GeoHelper.DistanceBetweenTwoPoints(
	                new DVector2(lonLatPoints[lonLatPoints.Count() - 2].Lon, lonLatPoints[lonLatPoints.Count() - 2].Lat),
	                new DVector2(lonLatPoints[lonLatPoints.Count() - 1].Lon, lonLatPoints[lonLatPoints.Count() - 1].Lat)
	            );
	            newPoints.Add(Clone(lonLatPoints[lonLatPoints.Count() - 1], totalDistance));
            }


	        layer.PointsCpu = newPoints.ToArray();
	        layer.UpdatePointsBuffer();
            return layer;
	    }


        public void AddLine(List<DVector3> cartPoints, Color4 lineColor, bool updateBuffer = true)
	    {
	        var geoPoints = cartPoints
	            .Select(p =>
	            {
	                var height = (float) (p.Length() - GeoHelper.EarthRadius);
                    var g = GeoHelper.CartesianToSpherical(p.Normalized() * GeoHelper.EarthRadius);

	                return new Gis.GeoPoint
	                {
	                    Color = lineColor,
                        Lon = g.X,
                        Lat = g.Y,
                        Tex0 = new Vector4(1, 0, 0, 0),
                        Tex1 = new Vector4(height, 0, 0, 0)
	                };
	            }).ToList();

            AddLine(geoPoints, updateBuffer);
	    }

		public void AddLine(List<DVector3> cartPoints, float halfWidth, Color4 lineColor, bool updateBuffer = true)
		{
			var geoPoints = cartPoints
				.Select(p =>
				{
					var g = GeoHelper.CartesianToSpherical(p.Normalized() * GeoHelper.EarthRadius);

					return new Gis.GeoPoint
					{
						Color = lineColor,
						Lon = g.X,
						Lat = g.Y,
						Tex0 = new Vector4(halfWidth, 0, 0, 0),
						Tex1 = new Vector4(0, 0, 0, 0)
					};
				}).ToList();

			AddLine(geoPoints, updateBuffer);
		}

        public void AddLine(List<DVector4> cartPoints, Color4 lineColor, bool updateBuffer = true)
		{
			var geoPoints = cartPoints
				.Select(p =>
				{
					var pp = new DVector3(p.X, p.Y, p.Z);
					var g = GeoHelper.CartesianToSpherical(pp.Normalized() * GeoHelper.EarthRadius);

					return new Gis.GeoPoint
					{
						Color = lineColor,
						Lon = g.X,
						Lat = g.Y,
						Tex0 = new Vector4((float)p.W, 0, 0, 0),
						Tex1 = new Vector4(0, 0, 0, 0)
					};
				}).ToList();

			AddLine(geoPoints, updateBuffer);
		}

		public void AddLine(List<DVector2> lonLatPoints, float halfWidth, Color4 lineColor, bool updateBuffer = true)
		{
			var geoPoints = lonLatPoints
				.Select(p =>
				{
					return new Gis.GeoPoint
					{
						Color = lineColor,
						Lon = p.X,
						Lat = p.Y,
						Tex0 = new Vector4(halfWidth, 0, 0, 0),
						Tex1 = new Vector4(0, 0, 0, 0)
					};
				}).ToList();

			AddLine(geoPoints, updateBuffer);
		}

		public void Clear()
	    {
	        PointsCpu = new Gis.GeoPoint[_initialPointsCount];

            UpdatePointsBuffer();
	    }

        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			var dev = Game.GraphicsDevice;

			if (((LineFlags) Flags).HasFlag(LineFlags.THIN_LINE)) {
				dev.PipelineState = _thinFactory[Flags];
			}
			else {
				dev.PipelineState = _factory[Flags];
			}

		    _linesConstantBuffer.SetData(_linesConstData);

            if (_isDirty) {
				_isDirty = false;
			}

			dev.GeometryShaderConstants[0]	= constBuffer;
			dev.VertexShaderConstants[0]	= constBuffer;
			dev.PixelShaderConstants[0]		= constBuffer;
			dev.PixelShaderConstants[1]		= _linesConstantBuffer;

			dev.PixelShaderResources[0] = Texture;
			dev.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap;

			dev.SetupVertexInput(_currentBuffer, null);
			dev.Draw(_currentBuffer.Capacity, 0);
		}

	    public override void Update(GameTime time)
	    {
	        _animationTimer += time.Elapsed;
	        _linesConstData.Dummy = new Vector3((float) _animationTimer.Ticks / _animationDuration.Ticks, 0, 0);

	        while (_animationTimer > _animationDuration)
	        {
	            _animationTimer -= _animationDuration;
	        }

            base.Update(time);
        }

		public static LinesGisLayer GenerateGrid(Game Game, DVector2 leftTop, DVector2 rightBottom, int dimX, int dimY, Color color, MapProjection projection, bool keepQuad = false)
		{
			var lt = projection.WorldToTilePos(leftTop.X,		leftTop.Y, 0);
			var rb = projection.WorldToTilePos(rightBottom.X,	rightBottom.Y, 0);

			if (keepQuad) {
				rb.Y = lt.Y + (rb.X - lt.X);
			}

			double stepX = Math.Abs(rb.X - lt.X) / (dimX - 1);
			double stepY = Math.Abs(rb.Y - lt.Y) / (dimY - 1);


			List<Gis.GeoPoint> points = new List<Gis.GeoPoint>();

			// Too lazy
			for (int row = 1; row < dimY-1; row++) {
				for (int col = 0; col < dimX-1; col++) {
					var coords0 = projection.TileToWorldPos(lt.X + stepX * col,		lt.Y + stepY * row, 0);
					var coords1 = projection.TileToWorldPos(lt.X + stepX * (col+1), lt.Y + stepY * row, 0);

					points.Add(new Gis.GeoPoint {
						Lon		= DMathUtil.DegreesToRadians(coords0.X),
						Lat		= DMathUtil.DegreesToRadians(coords0.Y),
						Color	= color
					});
					points.Add(new Gis.GeoPoint {
						Lon		= DMathUtil.DegreesToRadians(coords1.X),
						Lat		= DMathUtil.DegreesToRadians(coords1.Y),
						Color	= color
					});
				}
			}
			for (int col = 1; col < dimX-1; col++) {
				for (int row = 0; row < dimY-1; row++) {
					var coords0 = projection.TileToWorldPos(lt.X + stepX * col,	lt.Y + stepY * row, 0);
					var coords1 = projection.TileToWorldPos(lt.X + stepX * col, lt.Y + stepY * (row+1), 0);

					points.Add(new Gis.GeoPoint {
						Lon		= DMathUtil.DegreesToRadians(coords0.X),
						Lat		= DMathUtil.DegreesToRadians(coords0.Y),
						Color	= color
					});
					points.Add(new Gis.GeoPoint {
						Lon		= DMathUtil.DegreesToRadians(coords1.X),
						Lat		= DMathUtil.DegreesToRadians(coords1.Y),
						Color	= color
					});
				}
			}

			var linesLayer = new LinesGisLayer(Game, points.Count);
			Array.Copy(points.ToArray(), linesLayer.PointsCpu, points.Count);
			linesLayer.UpdatePointsBuffer();
			linesLayer.Flags = (int)(LineFlags.THIN_LINE);

			return linesLayer;
		}

		public static LinesGisLayer GenerateDistanceGrid(Game Game, DVector2 lonLatLeftBottomCorner, double step, int xStepsCount, int yStepsCount, Color color)
		{

			List<Gis.GeoPoint> points = new List<Gis.GeoPoint>();

			// Too lazy
			//var yPoint = lonLatLeftBottomCorner;

			//for (int row = 0; row < yStepsCount; row++) {
			//
			//	yPoint = GeoHelper.RhumbDestinationPoint(yPoint, 0, step);
			//
			//	for (int col = 0; col < xStepsCount; col++)
			//	{
			//		var coords0 = GeoHelper.RhumbDestinationPoint(yPoint, 90, step * col);
			//		//var coords1 = GeoHelper.RhumbDestinationPoint(coords0, 90, step);
			//
			//		points.Add(new Gis.GeoPoint {
			//			Lon = DMathUtil.DegreesToRadians(coords0.X),
			//			Lat = DMathUtil.DegreesToRadians(coords0.Y),
			//			Color = color
			//		});
			//		//points.Add(new Gis.GeoPoint {
			//		//	Lon = DMathUtil.DegreesToRadians(coords1.X),
			//		//	Lat = DMathUtil.DegreesToRadians(coords1.Y),
			//		//	Color = color
			//		//});
			//	}
			//}

			for (int col = 0; col < xStepsCount; col++) {
				var xPoint = GeoHelper.RhumbDestinationPoint(lonLatLeftBottomCorner, 90, step * col);

				for (int row = 0; row < yStepsCount; row++) {
					var coords0 = GeoHelper.RhumbDestinationPoint(xPoint, 0, step * row);
					//var coords1 = GeoHelper.RhumbDestinationPoint(xPoint, 0, step * (row + 1));

					points.Add(new Gis.GeoPoint {
						Lon = DMathUtil.DegreesToRadians(coords0.X),
						Lat = DMathUtil.DegreesToRadians(coords0.Y),
						Color = color
					});
					//points.Add(new Gis.GeoPoint
					//{
					//	Lon = DMathUtil.DegreesToRadians(coords1.X),
					//	Lat = DMathUtil.DegreesToRadians(coords1.Y),
					//	Color = color
					//});
				}
			}


			var indeces = new List<int>();

			for (int col = 0; col < xStepsCount-1; col++) {
				for (int row = 0; row < yStepsCount; row++) {
					indeces.Add(row + (col+1) * yStepsCount);
					indeces.Add(row + col*yStepsCount);
				}
			}

			for (int row = 0; row < yStepsCount-1; row++)
			{
				for (int col = 0; col < xStepsCount; col++)
				{
					indeces.Add(col * yStepsCount + row);
					indeces.Add((col) * yStepsCount + row + 1);
				}
			}

			var newPoints = new List<Gis.GeoPoint>();
			foreach (var ind in indeces) {
				newPoints.Add(points[ind]);
			}

			var linesLayer = new LinesGisLayer(Game, newPoints.Count);
			Array.Copy(newPoints.ToArray(), linesLayer.PointsCpu, newPoints.Count);
			linesLayer.UpdatePointsBuffer();
			linesLayer.Flags = (int)(LineFlags.THIN_LINE);

			return linesLayer;
		}

		public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
		{
			return null;
		}
	}
}
