using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;


namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources.MapBox
{
	public class LightMapV9 : BaseMapBoxMap
	{
		public override string Name {
			get { return "LightMapV9"; }
		}

		public override string ShortName {
			get { return "LMV9"; }
		}

		protected override string RefererUrl {
			get { return "https://www.mapbox.com/"; }
		}

		public LightMapV9(Game game) : base(game)
		{
			TileSize = 256;
			AcessToken = "pk.eyJ1Ijoia2FwYzNkIiwiYSI6ImNpbGpodG82czAwMmlubmtxamdsOHF0a3AifQ.xCbMUsy_a_0A9cd4GvjXKQ";
			UrlFormat = "https://api.mapbox.com/styles/v1/mapbox/light-v9/tiles/256/{0}/{1}/{2}?access_token={3}";
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			return String.Format(UrlFormat, zoom, x, y, AcessToken);
		}
	}
}
