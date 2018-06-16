using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using System;

namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Cartocdn
{
	public class CartoCdnMap : BaseMapSource
	{
		public CartoCdnMap(Game game) : base(game)
		{
			MaxZoom = 19;
		}
			
		public readonly string ServerLetters = "abc";

		public override MapProjection Projection { get { return MercatorProjection.Instance; } }

		string UrlFormat = "http://{0}.basemaps.cartocdn.com/dark_nolabels/{1}/{2}/{3}.png";


		public override string Name {
			get { return "CartoCdnMap"; }
		}

		public override string ShortName {
			get { return "CDN"; }
		}

		protected override string RefererUrl {
			get { return "http://www.openstreetmap.org/"; }
		}


		public override string GenerateUrl(int x, int y, int zoom)
		{
			return String.Format(UrlFormat, ServerLetters[(x + y) % ServerLetters.Length], zoom, x, y);
		}
	}
}
