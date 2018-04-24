using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;


namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources.OpenStreetMaps
{
	public abstract class BaseMapZenHeightmap : BaseMapSource
	{
		protected BaseMapZenHeightmap(Game game) : base(game)
		{
		}

		//protected List<> 
		public readonly string ServerLetters = "abc";

		public override MapProjection Projection { get { return MercatorProjection.Instance; } }
	}


	public class MapZenHeightmap : BaseMapZenHeightmap
    {
		static readonly string UrlFormat = "https://s3.amazonaws.com/elevation-tiles-prod/terrarium/{0}/{1}/{2}.png";


		public override string Name
		{
			get { return "MapZenHeightmap"; }
		}

		public override string ShortName
		{
			get { return "MapZen"; }
		}

		protected override string RefererUrl
		{
			get { return "https://mapzen.com/"; }
		}

		public MapZenHeightmap(Game game) : base(game)
		{
			MaxZoom = 15;
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			return String.Format(UrlFormat, zoom, x, y);
		}

	}
}
