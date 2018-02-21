using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics.Graph
{
	public class GraphConfig : GameModule {
		//link

        public float Max_mass { get; set; }
        public float Min_mass { get; set; }

        public float Rotation			{ get; set; } 
        public float MaxLinkWidth		{ get; set; } 
        public float LinkSize			{ get; set; } 
        
        public float MinParticleRadius	{ get; set; }
		public float NodeScale			{ get; set; } 

		public string NodePath			{ get; set; } 
		public string SavePath			{ get; set; } 
		public string LinkPath			{ get; set; }

		//this parameters could be changed from console
		[Config] public bool	LinksBehindAllNodes	{ get; set; }
		[Config] public bool	WhiteMode			{ get; set; }
		[Config] public int		StartRewind			{ get; set; }
		[Config] public int		EndRewind			{ get; set; }
		[Config] public float	processVelocity		{ get; set; }
		[Config] public float EdgeMaxOpacity { get; set; }


		public string GraphLayout { get; set; }
		public string nodeSign { get; set; }
		
	
		

		private readonly string[] layouts = new[] {"Graph/Compute", "", "Graph/Visheratin"};
		public override void Initialize()
		{
			
		}

		public GraphConfig(Game game) : base(game)
		{
			Min_mass = 0.5f;
			Max_mass = 0.5f;
			Rotation = 2.6f;

			MaxLinkWidth	= 1;//10
			LinkSize		= 20.0f;
			EdgeMaxOpacity	= 0.5f;

			MinParticleRadius	= 30;//250
			NodeScale			= 1;

			string s = @".\Data\testedgeList.txt"; //@".\Data\Banks\edges"
	        NodePath = s;
	        LinkPath = s;
	        SavePath = @".\Data\States\";
		    Directory.CreateDirectory(SavePath);

			
	        GraphLayout = @"Graph/Visheratin";
	        WhiteMode = false;
			StartRewind = 15;
			EndRewind = 1;
			LinksBehindAllNodes = true;
			processVelocity = 0.001f;
			nodeSign = @"Fonts\segoeSemiLight24";
		}
	}
}
