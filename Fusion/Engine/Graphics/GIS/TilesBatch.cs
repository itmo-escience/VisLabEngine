using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.OpenStreetMaps;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using SharpDX.Direct3D;

namespace Fusion.Engine.Graphics.GIS
{
	public partial class TilesGisLayer : Gis.GisLayer
	{
		Ubershader		shader;
		StateFactory	factory;
		StateFactory	factoryWire;
	    //private DBoundingFrustum localFrustum;
	    //private DVector3 localCameraPos;
		Texture2D	frame;
		GlobeCamera camera;
	    private int maxTilesToRender;

		private double[] lodDistances;


		public class SelectedItem : Gis.SelectedItem
		{
			public long TileX;
			public long TileT;
		}


		[Flags]
		public enum TileFlags : int
		{
			SHOW_FRAMES		 = 0x0001,
            FIX_WATER        = 0x0002,
        }


		public TilesGisLayer(Game engine, GlobeCamera camera) : base(engine)
		{
			RegisterMapSources();
		    maxTilesToRender = 0;
			this.camera = camera;
      //      localFrustum = camera.Frustum;
		    //localCameraPos = camera.CameraPosition;

            lodDistances = new double[25];
			for (int i = 1; i < 25; i++) {
				var dist = GetOptimalDistanceForLevel(i-1);
				lodDistances[i] = dist;
			}

			CurrentMapSource = MapSources[9];

			frame	= _game.Content.Load<Texture2D>("redframe.tga");
			shader	= _game.Content.Load<Ubershader>("globe.Tile.hlsl");
			factory = shader.CreateFactory( typeof(TileFlags), Primitive.TriangleList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.CullCW, DepthStencilState.Default);
			factoryWire = shader.CreateFactory(typeof(TileFlags), Primitive.TriangleList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.Wireframe, DepthStencilState.Default);
		}


	    public void UpdateLodDistances()
	    {
	        lodDistances = new double[25];
	        for (int i = 1; i < 25; i++)
	        {
	            var dist = GetOptimalDistanceForLevel(i - 1);
	            lodDistances[i] = dist;
	        }
        }


		private int t = 0;
		public override void Update(GameTime gameTime)
		{
		    //localFrustum = camera.Frustum;
			//Console.Clear();
			//Console.WriteLine("Current Zoom Level: " + CurrentLevel);
			//
			//for(int i = 0; i < 15; i++)
			//	Console.WriteLine("Zoom: " + i + "\tError: " + GetLevelScreenSpaceError(i, camera.CameraDistance - camera.EarthRadius));
			//
			//Console.WriteLine(camera.FinalCamPosition);

			CurrentMapSource.Update(gameTime);

			if (_game.Keyboard.IsKeyDown(Keys.OemPlus)) {
				viewerHeight += viewerHeight*0.005;
				Console.WriteLine("Height: " + viewerHeight);
			}

			if (_game.Keyboard.IsKeyDown(Keys.OemMinus)) {
				viewerHeight -= viewerHeight*0.005;
				Console.WriteLine("Height: " + viewerHeight);
			}

		    //if (Game.Keyboard.IsKeyDown(Keys.U))
		    //{
		    //    localFrustum = camera.Frustum;
		    //    localCameraPos = camera.CameraPosition;
		    //    Console.WriteLine("Local frustum updated");
		    //}

            //DetermineTiles();
            DetermineTiles(3);
            //DetermineTilesDebug();
			//Console.WriteLine();
			//if (t != gameTime.Total.Seconds) {
			//	t = gameTime.Total.Seconds;
			//	Console.WriteLine("Tiles to render: " + tilesToRender.Count);
			//	Console.WriteLine("Viewer height: " + viewerHeight + "\n");
			//}
			//Console.WriteLine("Free tiles:		" + tilesPool.Count);
		}

	    public bool FixWater = false;

		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
            //Gis.Debug.Clear();
            //Gis.Debug.DrawDBoundingFrustum(camera.Frustum);
      //      var camPos = new DBoundingBox(localCameraPos, localCameraPos + DVector3.One);
		    //Gis.Debug.DrawSphere(100, Color.Red, DMatrix.Translation(localCameraPos));

            var dev = _game.GraphicsDevice;

			dev.VertexShaderConstants[0]	= constBuffer;
		    dev.PixelShaderConstants[0]     = constBuffer;
			dev.PixelShaderSamplers[0]		= SamplerState.AnisotropicClamp;
			dev.PixelShaderResources[1]		= frame;


		    TileFlags flags = _game.Keyboard.IsKeyDown(Keys.M) && _game.Keyboard.IsKeyDown(Keys.LeftAlt) ? TileFlags.SHOW_FRAMES : 0;

            flags |= FixWater ? TileFlags.FIX_WATER : 0;

            dev.PipelineState = _game.Keyboard.IsKeyDown(Keys.M) && _game.Keyboard.IsKeyDown(Keys.LeftAlt) ? factoryWire[(int)flags] : factory[(int)flags];
			//dev.PipelineState = factory[0];

		    if (tilesToRender.Count > maxTilesToRender)
		    {
		        maxTilesToRender = tilesToRender.Count;
                Console.WriteLine("maximal number of tiles to render reached: " + maxTilesToRender);
		    }


			//PixHelper.BeginEvent(new SharpDX.Mathematics.Interop.RawColorBGRA(255, 0, 0, 255), "Draw Tiles");
			foreach (var globeTile in tilesToRender) {
				var tex = CurrentMapSource.GetTile(globeTile.Value.X, globeTile.Value.Y, globeTile.Value.Z).Tile;
				dev.PixelShaderResources[0] = tex;

				dev.SetupVertexInput(globeTile.Value.VertexBuf, globeTile.Value.IndexBuf);
				dev.DrawIndexed(globeTile.Value.IndexBuf.Capacity, 0, 0);
			}
			//PixHelper.EndEvent();
		}


		public override void Dispose()
		{
			foreach (var mapSource in MapSources) {
				mapSource.Dispose();
			}

			//if (BaseMapSource.EmptyTile != null) {
			//	BaseMapSource.EmptyTile.Dispose();
			//	BaseMapSource.EmptyTile = null;
			//}

			foreach (var tile in tilesToRender) {
				tile.Value.Dispose();
			}
			foreach (var tile in tilesPool) {
				tile.Value.Dispose();
			}
		}


		public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
		{
			throw new NotImplementedException();
		}
	}
}
