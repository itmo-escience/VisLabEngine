using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.BingMaps;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.GoogleMaps;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.MapBox;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.OpenStreetMaps;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.YandexMaps;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Cartocdn;

namespace Fusion.Engine.Graphics.GIS
{
    partial class TilesAtlasLayer
    {
		public static List<BaseMapSource> MapSources = new List<BaseMapSource>();

		public enum MapSource : int
		{
			OpenStreetMap		= 0,
			GoogleMap			= 1,
			GoogleSatteliteMap	= 2,
			Yandex				= 3,
			YandexSatellite		= 4,
			PencilMap			= 5,
			SpaceStationMap		= 6,
			PirateMap			= 7,
			BingMap				= 8,
			BingMapSatellite	= 9,
			Dark				= 10,
			MoiseevMap			= 11,
			DarkV9				= 12,
			LightMap			= 13,
			CartoCdnMap			= 14,
		}

		public BaseMapSource CurrentMapSource { get; internal set; }
        public BaseMapSource CurrentHeightmapSource { get; internal set; }

        protected void RegisterMapSources()
		{
            MapSources.ForEach(a => a.Dispose());
            MapSources.Clear();

			MapSources.Add(new OpenStreetMap(_game)		);
			MapSources.Add(new GoogleMap(_game)			);
			MapSources.Add(new GoogleSatelliteMap(_game)	);
			MapSources.Add(new YandexMap(_game)			);
			MapSources.Add(new YandexSatelliteMap(_game)	);
			MapSources.Add(new PencilMap(_game)			);
			MapSources.Add(new SpaceStationMap(_game)	);
			MapSources.Add(new PirateMap(_game)			);
			MapSources.Add(new BaseBingMapsSource(_game)	);
			MapSources.Add(new BingMapSatellite(_game)	);
			MapSources.Add(new Dark(_game)				);
			MapSources.Add(new MoiseevMap(_game)			);
			MapSources.Add(new DarkV9(_game)				);
			MapSources.Add(new LightMap(_game)			);
			MapSources.Add(new CartoCdnMap(_game)		);
        }

	    public MapSource CurrentMapSourecEnum;

        private bool yandexMercator => CurrentMapSource.Projection is MercatorProjectionYandex;

	    public void SetMapSource(MapSource map)
	    {
			var oldProj = CurrentMapSource.Projection;

			CurrentMapSource = MapSources[(int)map];  
	        CurrentMapSourecEnum = map;            


            //         if (!oldProj.Equals(CurrentMapSource.Projection)) {
            //	updateTiles = true;
            //}

            tileContainer?.Clear();
	        lastNodes = new HashSet<NodeInfo>();
	        rootTile = null;

	    }

	}
}
