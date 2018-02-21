using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources.MapBox
{
	class MoiseevMap : BaseMapBoxMap
	{
		public		override string Name		=> "MoiseevMap";
		public		override string ShortName	=> "MMap";
		protected	override string RefererUrl	=> "https://www.mapbox.com/";

		public MoiseevMap(Game game) : base(game)
		{
			TileSize = 256;
			UrlFormat = "https://api.mapbox.com/styles/v1/alexmoiseev69/cj206ahat003h2smf8dl5lks1/tiles/256/{0}/{1}/{2}@2x?access_token=pk.eyJ1IjoiYWxleG1vaXNlZXY2OSIsImEiOiJjajBxdGo2aXUwMDB6MnZyaHpvemp5NWd5In0.agaeE7gZokwGeQCsvuke6w";
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			return string.Format(UrlFormat, zoom, x, y);
		}
	}
}
