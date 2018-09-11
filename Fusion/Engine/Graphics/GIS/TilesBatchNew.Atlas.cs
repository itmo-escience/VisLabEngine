using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;

namespace Fusion.Engine.Graphics.GIS
{
    partial class TilesAtlasLayer
    {

        #region TileAtlas

        public class TileAtlasContainer : IDisposable
        {
            public static int MaxTiles = 2048;
            public Texture2DArray Atlas;
            Dictionary<string, int> cachedTiles = new Dictionary<string, int>();
            Dictionary<int, string> tileKeys = new Dictionary<int,string>();
            Queue<int> freeIndicies = new Queue<int>();
            HashSet<int> usedIndicies = new HashSet<int>();

            StateFactory tileMergerFactory;
            Ubershader shader;
            private int TileSize;
            public bool LoadNeighbourPixels = false;
            public bool UseMipMaps = true;
            public TileAtlasContainer(int tileSize)
            {

                for (int i = 1; i < MaxTiles; i++)
                {
                    freeIndicies.Enqueue(i);
                }

                TileSize = tileSize;
                var ts = LoadNeighbourPixels ? tileSize + 1 : tileSize;
                Atlas = new Texture2DArray(Game.Instance.GraphicsDevice, ts, ts, MaxTiles, ColorFormat.Rgba8, UseMipMaps);
                init();


                shader = Game.Instance.Content.Load<Ubershader>("globe.TileWarper.hlsl");
                tileMergerFactory = shader.CreateFactory(typeof(EmptyFlags), Primitive.TriangleStrip, VertexInputElement.FromStructure<InstPoint>(), BlendState.AlphaBlend, RasterizerState.CullCW, DepthStencilState.Default);                
            }

            private void init()
            {
                Atlas.SetFromTexture(BaseMapSource.EmptyTile, 0);
            }

            public bool ContainsTile(string key)
            {
                return cachedTiles.ContainsKey(key);
            }

            private int nextIndex()
            {
                int i = freeIndicies.Dequeue();
                int i0 = i;
                freeIndicies.Enqueue(i);
                while (usedIndicies.Contains(i))
                {
                    i = freeIndicies.Dequeue();
                    freeIndicies.Enqueue(i);
                    if (i == i0)
                    {
                        Log.Error("Max possible number of tiles exceeded");
                        return -1;
                    }
                }
                
                return i;
            }
            public enum EmptyFlags : int
            {
                Empty = 0,
            }
            private struct TileInfo
            {
                [Vertex("TEXCOORD", 1)]
                public uint Right;
                [Vertex("TEXCOORD", 2)]
                public uint Bottom;
                [Vertex("TEXCOORD", 2)]
                public uint BottomRight;
                [Vertex("TEXCOORD", 2)]
                public uint Resolution;
            }

            private HashSet<string> finishedTiles= new HashSet<string>();
            private HashSet<string> rightTiles = new HashSet<string>();
            private HashSet<string> bottomTiles = new HashSet<string>();
            private HashSet<string> bottomRightTiles = new HashSet<string>();

            public int AddTile(MapTile tile, string key)
            {
                if (tile.Tile == BaseMapSource.EmptyTile) return 0;
                int ind = 0;
                if (cachedTiles.ContainsKey(key) )
                {
                    ind = cachedTiles[key];
                    usedIndicies.Add(ind);
                    if (!LoadNeighbourPixels || finishedTiles.Contains(key))
                    {                        
                        return ind;
                    }
                }
                else
                {
                    ind = nextIndex();
                    if (ind < 0) return 0;
                    usedIndicies.Add(ind);
                }
                
                if (LoadNeighbourPixels && cachedTiles.ContainsKey(key))
                {
                    int x, y, zoom;
                    ParseKey(key, out x, out y, out zoom);
                    int nx = (x + 1) % (1 << zoom),
                        ny = (y + 1) % (1 << zoom);

                    int right = cachedTiles.ContainsKey(GenerateKey(nx, y, zoom)) ? cachedTiles[GenerateKey(nx, y, zoom)] : 0,
                        bottom = cachedTiles.ContainsKey(GenerateKey(x, ny, zoom)) ? cachedTiles[GenerateKey(x, ny, zoom)] : 0,
                        bottomRight = cachedTiles.ContainsKey(GenerateKey(nx, ny, zoom)) ? cachedTiles[GenerateKey(nx, ny, zoom)] : 0;

                    if ((right != 0 && !rightTiles.Contains(key)) || (bottom != 0 && !bottomTiles.Contains(key)) || (bottomRight != 0 && !bottomRightTiles.Contains(key)))
                    {
                        if (right != 0) rightTiles.Add(key);
                        if (bottom != 0) bottomTiles.Add(key);
                        if (bottomRight != 0) bottomRightTiles.Add(key);
                        DepthStencilSurface depth;
                        RenderTargetSurface[] surfaces;
                        var Game = Common.Game.Instance;
                        var dev = Game.GraphicsDevice;
                        dev.GetTargets(out depth, out surfaces);

                        var newTex = new RenderTarget2D(Game.GraphicsDevice, ColorFormat.Rgba8, TileSize + 1,
                            TileSize + 1, false, false);

                        TileInfo info = new TileInfo()
                        {
                            Right = (uint) right,
                            Bottom = (uint) bottom,
                            BottomRight = (uint) bottomRight,
                            Resolution = (uint) (TileSize + 1),
                        };

                        var cb = ConstantBuffer.Create(dev, info);

                        dev.SetTargets(null, newTex);
                        dev.PipelineState = tileMergerFactory[0];
                        dev.PixelShaderSamplers[0] = SamplerState.PointClamp;
                        dev.PixelShaderResources[0] = tile.Tile;
                        dev.PixelShaderResources[1] = Atlas;
                        dev.PixelShaderConstants[0] = cb;
                        dev.Draw(4, 0);

                        cb.Dispose();                        

                        Atlas.SetFromResource(newTex, ind);
                        dev.SetTargets(depth, surfaces);
                        newTex.Dispose();
                        if (right * bottom * bottomRight != 0)
                            finishedTiles.Add(key);
                    }                    
                }
                else
                {
                    Atlas.SetFromTexture(tile.Tile, ind);
                }

                cachedTiles[key] = ind;
                if (tileKeys.ContainsKey(ind) && tileKeys[ind] != key)
                {
                    cachedTiles.Remove(tileKeys[ind]);
                    finishedTiles.Remove(tileKeys[ind]);
                    rightTiles.Remove(tileKeys[ind]);
                    bottomTiles.Remove(tileKeys[ind]);
                    bottomRightTiles.Remove(tileKeys[ind]);
                }
                tileKeys[ind] = key;
                return ind;
            }
            

            static void ParseKey(string key, out int x, out int y, out int zoom)
            {
                var ss = key.Split('_');
                x = int.Parse(ss[0]);
                y = int.Parse(ss[1]);
                zoom = int.Parse(ss[2]);
            }

            public void Clear()
            {
                for (int i = 1; i < MaxTiles; i++)
                {
                    freeIndicies.Enqueue(i);
                }                
                cachedTiles.Clear();                
                init();
            }

            public void RemoveTile(string key)
            {
                if (cachedTiles.ContainsKey(key))
                {
                    var ind = cachedTiles[key];
                    if (ind == 0) return;
                    usedIndicies.Remove(ind);
                }
            }
            

            public void Dispose()
            {
                Atlas?.Dispose();
                cachedTiles.Clear();                
                freeIndicies.Clear();
            }
        }

        private TileAtlasContainer tileContainer, heightMapContainer;

        public TileAtlasContainer TileContainer
        {
            get
            {
                if (tileContainer == null) tileContainer = new TileAtlasContainer(CurrentMapSource.TileSize);
                return tileContainer;
            }
        }

        public TileAtlasContainer HeightmapContainer
        {
            get
            {
                if (heightMapContainer == null) heightMapContainer = new TileAtlasContainer(CurrentMapSource.TileSize)
                {
                    LoadNeighbourPixels = true,
                    UseMipMaps = false,
                };
                return heightMapContainer;
            }
        }

        #endregion

    }
}
