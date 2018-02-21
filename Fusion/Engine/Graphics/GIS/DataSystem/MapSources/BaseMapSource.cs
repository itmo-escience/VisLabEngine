using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.Concurrent;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using ContainmentType = Fusion.Engine.Graphics.GIS.GlobeMath.ContainmentType;

#pragma warning disable 0414

namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources
{
	public abstract class BaseMapSource
	{
		public Game Game;
		/// <summary>
		/// minimum level of zoom
		/// </summary>
		public int		MinZoom;
		public float	TimeUntilRemove = 600;

		const int MaxDownloadTries = 3;

		/// <summary>
		/// maximum level of zoom
		/// </summary>
		public int		MaxZoom		= 18;
		public int		TileSize	= 256;
		public static	Texture2D	EmptyTile;

		List<string> ToRemove = new List<string>();
		
		public Dictionary<string, MapTile>	RamCache	= new Dictionary<string, MapTile>();

		Random r = new Random();

		string UserAgent;

		int		TimeoutMs		= 5000;
		string	requestAccept	= "*/*";

		public abstract MapProjection Projection { get; }

		bool isDisposed = false;


		protected BaseMapSource(Game game)
		{
			Game = game;

			if (EmptyTile == null) {
				EmptyTile = Game.Content.Load<Texture2D>(@"empty.png");
			}

			UserAgent = string.Format("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:{0}.0) Gecko/{2}{3:00}{4:00} Firefox/{0}.0.{1}", r.Next(3, 14), r.Next(1, 10), r.Next(DateTime.Today.Year - 4, DateTime.Today.Year), r.Next(12), r.Next(30));
		}

		public abstract string Name {
			get;
		}

		public abstract string ShortName {
			get;
		}

		protected abstract string RefererUrl { get; }


		public virtual void Update(GameTime gameTime)
		{
		    lock (RamCache)
		    {
		        foreach (var cachedTile in RamCache)
		        {
		            cachedTile.Value.Time += gameTime.ElapsedSec;

		            if (cachedTile.Value.Time > TimeUntilRemove)
		            {
		                try
		                {
		                    if (cachedTile.Value.IsLoaded)
		                    {
		                        //cachedTile.Value.Tile.Dispose();
		                        ToRemove.Add(cachedTile.Key);
		                    }
		                }
		                catch (Exception e)
		                {
		                    Log.Warning(e.Message);
		                }
		            }
		        }

		        foreach (var e in ToRemove)
		        {
		            RamCache[e].Tile.Dispose();
		            RamCache.Remove(e);
		        }
		    }
            //Log.Message($"Load requests in queue: {requests}");
            ToRemove.Clear();		    
		}


		public abstract string GenerateUrl(int x, int y, int zoom);

		//public MapTile GetTile(Vector2 latLon, int zoom);
		//public MapTile GetTile(float lat, float lon, int zoom);
		public MapTile GetTile(int x, int y, int zoom)
		{
			return CheckTileInMemory(x, y, zoom);
		}

		

		public byte[] DownloadMapTile(string url)
		{
			try {

				var client = new WebClient();
				client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
				//client.Headers["User-Agent"] = UserAgent;
				//client.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                return client.DownloadData(url);

				//var request = (HttpWebRequest) WebRequest.Create(url);
				//
				////WebClient wc = new WebClient();
				//request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
				//request.Timeout				= TimeoutMs;
				//request.UserAgent			= UserAgent;
				//request.ReadWriteTimeout	= TimeoutMs * 6;
				//request.Accept				= requestAccept;
				//request.Referer				= RefererUrl;
				//
				//HttpWebResponse response = (HttpWebResponse) request.GetResponse();
				//
				//using (var s = new MemoryStream()) {
				//	var responseStream = response.GetResponseStream();
				//	if (responseStream != null) responseStream.CopyTo(s);
				//	return s.ToArray();
				//}

			} catch (Exception e) {
				Log.Warning($"{e.Message}Url: {url}");
				return null;
			}
		}


		string GetKey(int m, int n, int level)
		{
			//return string.Format(ShortName + level + "_" + m + "_" + n);
		    return $"{ShortName}{level}_{m}_{n}";
		}

	    private int requests = 0;

		MapTile CheckTileInMemory(int m, int n, int level)
		{
			string key = GetKey(m,n,level);
			//string path = @"cache\" + Name + @"\" + key + ".jpg";
		    string path = $"cache\\{Name}\\{key}.jpg";
		    bool flag = false;
		    MapTile ct = null;
		    lock (RamCache)
		    {
		        if (!RamCache.ContainsKey(key))
		        {
		            ct = new MapTile
		            {
		                Path = path,
		                Url = GenerateUrl(m, n, level),
		                LruIndex = 0,
		                Tile = EmptyTile,
		                X = m,
		                Y = n,
		                Zoom = level
		            };

		            RamCache.Add(key, ct);
		            flag = true;
		        }
		    }
		    if (flag)
		    {
		        requests++;
		        Gis.ResourceWorker.Post(r =>
		        {
		            var tile = r.Data as MapTile;

		            if (!File.Exists(tile.Path))
		            {

		                r.ProcessQueue.Post(t =>
		                {
		                    requests--;
		                    var data = DownloadMapTile(tile.Url);

		                    // TODO: responde to tile loading error
		                    if (data == null || data.Length == 0)
		                    {
		                        tile.LoadingTries++;
		                        return;
		                    }

		                    tile.Tile = new Texture2D(Game.Instance.GraphicsDevice, data);

		                    var fileName = tile.Path;
		                    r.DiskWRQueue.Post(q =>
		                    {
		                        var file = new FileInfo(fileName);
		                        file.Directory.Create();

		                        using (var f = File.OpenWrite(fileName))
		                        {
		                            var bytes = q.Data as byte[];
		                            f.Write(bytes, 0, bytes.Length);
		                        }
		                    }, data);

		                    tile.IsLoaded = true;
		                }, r.Data);
		            }
		            else
		            {
		                r.DiskWRQueue.Post(q =>
		                {
		                    requests--;
		                    using (var stream = File.OpenRead(tile.Path))
		                    {
		                        tile.Tile = new Texture2D(Game.Instance.GraphicsDevice, stream);
		                        tile.IsLoaded = true;
		                    }
		                }, null);
		            }

		        }, ct);
		    }
		    lock (RamCache)
		    {
		        RamCache[key].LruIndex = level;
		        RamCache[key].Time = 0.0f;		    

		        return RamCache[key];
		    }
        }

        private bool IsTileInFrustum(int x, int y, int z, DBoundingFrustum frustum)
        {
            //return true;
            long numTiles = 1 << z;
            double x0 = ((double)(x + 0) / (double)numTiles);
            double y0 = ((double)(y + 0) / (double)numTiles);
            double x1 = ((double)(x + 1) / (double)numTiles);
            double y1 = ((double)(y + 1) / (double)numTiles);

            List<DVector3> tilePoints = new List<DVector3>();
            int density = 15;

            float xStep = Math.Abs((float)(x0 - x1)) / density;
            float yStep = Math.Abs((float)(y0 - y1)) / density;

            for (double i = Math.Min(x0, x1); i <= Math.Max(x0, x1); i += xStep)
            {
                for (double j = Math.Min(y0, y1); j <= Math.Max(y0, y1); j += yStep)
                    tilePoints.Add(GetCartesianCoord(i, j, z));
            }

            foreach (var tilePoint in tilePoints)
            {
                if (frustum.Contains(tilePoint) != ContainmentType.Disjoint)
                {
                    return true;
                }
            }

            return false;
        }

        DVector3 GetCartesianCoord(double x, double y, double z)
        {
            var lonLat = this.Projection.TileToWorldPos(x, y);
            DVector3 result = GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(lonLat), GeoHelper.EarthRadius);
            return result;
        }

        DVector3 GetTileCenterPosition(int x, int y, int z)
        {
            long numTiles = 1 << z;

            double x0 = ((double)(x + 0) / (double)numTiles);
            double y0 = ((double)(y + 0) / (double)numTiles);
            double x1 = ((double)(x + 1) / (double)numTiles);
            double y1 = ((double)(y + 1) / (double)numTiles);

            var xHalf = (x0 + x1) / 2.0;
            var yHalf = (y0 + y1) / 2.0;

            var lonLat = this.Projection.TileToWorldPos(xHalf, yHalf);
            var ret = GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(lonLat), GeoHelper.EarthRadius);
            return ret;
        }

        public void Dispose()
		{
			isDisposed = true;
		    lock (RamCache)
		    {
		        foreach (var tile in RamCache)
		        {
		            tile.Value.Tile.Dispose();
		            tile.Value.Tile = null;
		        }
		        RamCache.Clear();
		    }
		}

	}
}
