using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using SharpDX.Direct3D;
using Keys = Fusion.Engine.Input.Keys;

namespace Fusion.Engine.Graphics.GIS
{
    public partial class TilesAtlasLayer : Gis.GisLayer
    {        
        Ubershader shader;
        StateFactory factory;
        StateFactory factoryWire;
        //private DBoundingFrustum localFrustum;
        //private DVector3 localCameraPos;
        Texture2D frame;
        GlobeCamera camera;

        private int maxTilesToRender;

        private double[] lodDistances;

        public bool FixWater = false;

        public class SelectedItem : Gis.SelectedItem
        {
            public long TileX;
            public long TileT;
        }


        [Flags]
        public enum TileFlags : int
        {
            SHOW_FRAMES = 0x0001,
            FIX_WATER = 0x0002,
            YANDEX = 0x0004,
            TESSELLATE = 0x0008,
        }

        StructuredBuffer instDataGpu;

        private int tilesLimit => TileAtlasContainer.MaxTiles;

        public TilesAtlasLayer(Game engine, GlobeCamera camera) : base(engine)
        {
            RegisterMapSources();
            maxTilesToRender = 0;
            this.camera = camera;
            //      localFrustum = camera.Frustum;
            //localCameraPos = camera.CameraPosition;
            
            lodDistances = new double[25];
            for (int i = 1; i < 25; i++)
            {
                var dist = GetOptimalDistanceForLevel(i - 1);
                lodDistances[i] = dist;
            }

            CurrentMapSource = MapSources[9];

            frame = Game.Content.Load<Texture2D>("redframe.tga");
            shader = Game.Content.Load<Ubershader>("globe.TileBatch.hlsl");
            factory = shader.CreateFactory(typeof(TileFlags), Primitive.PatchList4CP, VertexInputElement.FromStructure<InstPoint>(), BlendState.AlphaBlend, RasterizerState.CullCW, DepthStencilState.Default);
            factoryWire = shader.CreateFactory(typeof(TileFlags), Primitive.PatchList4CP, VertexInputElement.FromStructure<InstPoint>(), BlendState.AlphaBlend, RasterizerState.Wireframe, DepthStencilState.Default);

            instDataGpu = new StructuredBuffer(engine.GraphicsDevice, typeof(InstStruct), tilesLimit, StructuredBufferFlags.None);

            CreateStaticBuffers(tileDensity, ref tileVertexBuffer, out tileIndexBuffer);

        }

        public override void Update(GameTime gameTime)
        {

            CurrentMapSource.Update(gameTime);
            //CurrentHeightmapSource.Update(gameTime);

            DetermineTilesNew();           
        }
      
        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
            //Gis.Debug.Clear();
            //Gis.Debug.DrawDBoundingFrustum(camera.Frustum);
            //      var camPos = new DBoundingBox(localCameraPos, localCameraPos + DVector3.One);
            //Gis.Debug.DrawSphere(100, Color.Red, DMatrix.Translation(localCameraPos));

            //dev.PipelineState = factory[0];
            lock (tilesLock)
            {
                if (tilesToRender.Count > maxTilesToRender)
                {
                    maxTilesToRender = tilesToRender.Count;
                    Console.WriteLine("maximal number of tiles to render reached: " + maxTilesToRender);
                }

                
                var instData = new List<InstStruct>();
                int i = 0;
                foreach (var globeTile in tilesToRender)
                {
                    var tile = TileContainer.AddTile(
                        CurrentMapSource.GetTile(globeTile.Value.X, globeTile.Value.Y, globeTile.Value.Z),
                        globeTile.Key);
                    var heightTile = CurrentHeightmapSource != null ? HeightmapContainer.AddTile(
                        CurrentHeightmapSource.GetTile(globeTile.Value.X, globeTile.Value.Y, globeTile.Value.Z),
                        globeTile.Key) : -1;
                    if (tile < 0) tile = 0;
                    if (heightTile < 0) heightTile = 0;

                    instData.Add(new InstStruct()
                    {
                        x = (uint) globeTile.Value.X,
                        y = (uint) globeTile.Value.Y,
                        level = (uint) globeTile.Value.Z,
                        density = (uint) tileDensity,
                        texIndex = (uint) tile,
                        heightmapIndex = (uint) heightTile,
                    });
                    TileContainer.RemoveTile(globeTile.Key);
                    HeightmapContainer.RemoveTile(globeTile.Key);
                    i++;
                    //dev.PixelShaderResources[0] = tex;
                }
                instData.Sort((s1, s2) => (s1.x == s2.x) ? s1.y.CompareTo(s2.y) : s1.x.CompareTo(s2.x));
                var id = instData;

                PixHelper.BeginEvent(new SharpDX.Mathematics.Interop.RawColorBGRA(255, 0, 0, 255), "Draw Tiles");
                var dev = Game.GraphicsDevice;

                dev.VertexShaderConstants[0] = constBuffer;
                dev.HullShaderConstants[0] = constBuffer;
                dev.DomainShaderConstants[0] = constBuffer;
                dev.PixelShaderConstants[0] = constBuffer;
                dev.DomainShaderSamplers[0] = SamplerState.AnisotropicClamp;
                dev.PixelShaderSamplers[0] = SamplerState.AnisotropicClamp;
                dev.PixelShaderResources[0] = TileContainer.Atlas;
                dev.DomainShaderResources[0] = HeightmapContainer.Atlas;
                dev.PixelShaderResources[1] = frame;


                TileFlags flags = Game.Keyboard.IsKeyDown(Keys.M) /*&& Game.Keyboard.IsKeyDown(Keys.LeftAlt)*/ ? TileFlags.SHOW_FRAMES : 0;

                flags |= FixWater ? TileFlags.FIX_WATER : 0;
                flags |= yandexMercator ? TileFlags.YANDEX : 0;
                if (!Game.Keyboard.IsKeyDown(Keys.T))
                {
                    flags |= TileFlags.TESSELLATE;
                }

                dev.PipelineState = Game.Keyboard.IsKeyDown(Keys.M)/* && Game.Keyboard.IsKeyDown(Keys.LeftAlt)*/ ? factoryWire[(int)flags] : factory[(int)flags];
                if (instData.Count > 0)
                {
                    instDataGpu.SetData(id.ToArray(), 0, Math.Min(id.Count, tilesLimit));
                    dev.VertexShaderResources[2] = instDataGpu;
                    dev.HullShaderResources[2] = instDataGpu;
                    dev.DomainShaderResources[2] = instDataGpu;
                    dev.SetupVertexInput(tileVertexBuffer, tileIndexBuffer);
                    dev.DrawInstancedIndexed(tileIndexBuffer.Capacity, Math.Min(instData.Count, tilesLimit), 0, 0, 0);
                }
                PixHelper.EndEvent();
            }
        }

        private VertexBuffer tileVertexBuffer;
        private IndexBuffer tileIndexBuffer;

        struct InstStruct
        {
            public uint x;
            public uint y;
            public uint level;
            public uint density;
            public uint texIndex;
            public uint heightmapIndex;
            public Vector3 dummy;
        }

        public struct InstPoint
        {
            [Vertex("TEXCOORD", 0)]
            public Vector4 XYDD;
            [Vertex("TEXCOORD", 2)]
            public Vector4 Tex0;
            [Vertex("TEXCOORD", 3)]
            public Vector4 Tex1;
            [Vertex("Color")]
            public Color4 Color;
        }

        private static void CreateStaticBuffers(int density, ref VertexBuffer vb, out IndexBuffer ib)

        {
            int[] indexes;
            InstPoint[] vertices;

            DisposableBase.SafeDispose(ref vb);

            CalculateVertices(out vertices, out indexes, density);

            vb = new VertexBuffer(Game.Instance.GraphicsDevice, typeof(InstPoint), vertices.Length);
            ib = new IndexBuffer(Game.Instance.GraphicsDevice, indexes.Length);
            ib.SetData(indexes);
            vb.SetData(vertices, 0, vertices.Length);
        }



        static void CalculateVertices(out InstPoint[] vertices, out int[] indeces, int density)

        {
            int RowsCount = density + 1;
            int ColumnsCount = RowsCount;

            //var el = Game.GetService<LayerService>().ElevationLayer;

            var verts = new List<InstPoint>();
            float step = 1.0f / (RowsCount - 1);
            double dStep = 1.0 / (double)(RowsCount - 1);

            for (int row = 0; row < RowsCount; row++)
            {
                for (int col = 0; col < ColumnsCount; col++)
                {

                    //float elev = 0.0f;
                    //if (zoom > 8) elev = el.GetElevation(sc.X, sc.Y) / 1000.0f;	               


                    verts.Add(new InstPoint()
                    {
                        Tex0 = new Vector4(step * col, step * row, 0, 0),
                        XYDD = new Vector4(col, row, 0, 0),
                    });
                }

            }

            var tindexes = new List<int>();

            for (int row = 0; row < RowsCount - 1; row++)
            {
                for (int col = 0; col < ColumnsCount - 1; col++)
                {
                    tindexes.Add(col + row * ColumnsCount);
                    tindexes.Add(col + 1 + row * ColumnsCount);                    
                    tindexes.Add(col + 1 + (row + 1) * ColumnsCount);
                    tindexes.Add(col + (row + 1) * ColumnsCount);

                    //tindexes.Add(col + 1 + row * ColumnsCount);
                    //tindexes.Add(col + (row + 1) * ColumnsCount);
                    //tindexes.Add(col + 1 + (row + 1) * ColumnsCount);
                }
            }

            vertices = verts.ToArray();
            indeces = tindexes.ToArray();
        }


        public override void Dispose()
        {
            foreach (var mapSource in MapSources)
            {
                mapSource.Dispose();
            }

            if (BaseMapSource.EmptyTile != null)
            {
                BaseMapSource.EmptyTile.Dispose();
                BaseMapSource.EmptyTile = null;
            }

            foreach (var tile in tilesToRender)
            {
                tile.Value.Dispose();
            }
            //foreach (var tile in tilesPool)
            //{
            //    tile.Value.Dispose();
            //}
        }


        public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
        {
            throw new NotImplementedException();
        }

    }
}
