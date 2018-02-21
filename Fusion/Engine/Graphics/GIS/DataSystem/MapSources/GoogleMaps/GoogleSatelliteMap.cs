using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources.GoogleMaps
{
	public class GoogleSatelliteMap : BaseGoogleMapSource
	{
		public override string Name
		{
			get { return "GoogleSatelliteMap"; }
		}

		public override string ShortName
		{
			get { return "GSM"; }
		}


		public GoogleSatelliteMap(Game game) : base(game)
		{
			UrlFormatServer		= "khms";
			UrlFormatRequest	= "kh";
			Server				= "googleapis.com";
            UrlFormat			= "https://{0}{1}.{8}/{2}?v={3}&hl={4}&x={5}&y={6}&z={7}";
			MapVersion = "716";

			MaxZoom = 19;
		}


		//https://khms1.googleapis.com/kh?v=716&hl=ru-RU&&x=186239&y=113710&z=18

		public override string GenerateUrl(int x, int y, int zoom)
		{
			//string sec1 = string.Empty; // after &x=...
			//string sec2 = string.Empty; // after &zoom=...
			//GetSecureWords(x, y, out sec1, out sec2);

			string res = String.Format(UrlFormat, UrlFormatServer, GetServerNum(x, y, 2), UrlFormatRequest, MapVersion, Language, x, y, zoom, Server);

			return res;
		}
	}
}
