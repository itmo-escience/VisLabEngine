using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;

namespace Fusion.Engine.Graphics.GIS
{
    partial class TilesBatchNew
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
            public TileAtlasContainer(int tileSize)
            {

                for (int i = 1; i < MaxTiles; i++)
                {
                    freeIndicies.Enqueue(i);
                }
                Atlas = new Texture2DArray(Game.Instance.GraphicsDevice, tileSize, tileSize, MaxTiles, ColorFormat.Rgba8, true);
                init();
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

            public int AddTile(MapTile tile, string key)
            {
                if (tile.Tile == BaseMapSource.EmptyTile) return 0;
                if (cachedTiles.ContainsKey(key))
                {
                    int index = cachedTiles[key];
                    usedIndicies.Add(index);
                    return index;
                }

                int ind = nextIndex();
                if (ind < 0) return 0;
                usedIndicies.Add(ind);
                Atlas.SetFromTexture(tile.Tile, ind);
                cachedTiles[key] = ind;
                if (tileKeys.ContainsKey(ind)) cachedTiles.Remove(tileKeys[ind]);
                tileKeys[ind] = key;
                return ind;
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

        private TileAtlasContainer tileContainer;

        public TileAtlasContainer TileContainer
        {
            get
            {
                if (tileContainer == null) tileContainer = new TileAtlasContainer(CurrentMapSource.TileSize);
                return tileContainer;
            }
        }

        #endregion

    }
}
