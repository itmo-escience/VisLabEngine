﻿using System;
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
			[FieldOffset(4)] Vector3 Dummy;
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

		public bool IsDoubleBuffer { get; protected set; }

		public Texture2D Texture;

	    private VertexBuffer _currentBuffer;

		public Gis.GeoPoint[] PointsCpu { get; protected set; }

		public class SelectedItem : Gis.SelectedItem {}

		public override void Dispose()
		{
		    _currentBuffer?.Dispose();
		}

		public LinesGisLayer(Game engine, int linesPointsCount, bool isDynamic = false) : base(engine)
		{
		    TransparencyMultiplier = 1.0f;
		    OverallColor = Color4.White;

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

	    public void AddLine(List<Gis.GeoPoint> lonLatPoints)
	    {            
	        var newPoints = new List<Gis.GeoPoint>(PointsCpu);

	        newPoints.Add(new Gis.GeoPoint { Lon = lonLatPoints[0].Lon, Lat = lonLatPoints[0].Lat, Color = OverallColor });	        
            for (var i = 1; i < lonLatPoints.Count - 1; i++)
	        {
	            newPoints.Add(new Gis.GeoPoint { Lon = lonLatPoints[i].Lon, Lat = lonLatPoints[i].Lat, Color = OverallColor });
	            newPoints.Add(new Gis.GeoPoint { Lon = lonLatPoints[i].Lon, Lat = lonLatPoints[i].Lat, Color = OverallColor });
            }
	        newPoints.Add(new Gis.GeoPoint { Lon = lonLatPoints[lonLatPoints.Count - 1].Lon, Lat = lonLatPoints[lonLatPoints.Count - 1].Lat, Color = OverallColor });

            PointsCpu = newPoints.ToArray();

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

			if (_isDirty) {
				_linesConstantBuffer.SetData(_linesConstData);
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