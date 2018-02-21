using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Input;

namespace Fusion.Engine.Graphics.Graph
{
	
	public abstract class LayoutSystem : IDisposable
	{
		public Game Game;
		public GraphLayer graph;

		// Constructor: ----------------------------------------------------------------------------------------
		public LayoutSystem(Game game, GraphLayer layer)
		{
			Game	= game;
			graph	= layer;
		}
		// ----------------------------------------------------------------------------------------------------
		


		public virtual void Update(GameTime time) { }
		
		public virtual void Dispose() { }
	}
}
