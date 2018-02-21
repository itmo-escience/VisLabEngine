using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Graphics.Graph {
	/// <summary>
	/// base graph class
	/// contains list of edges and nodes
	/// </summary>
	public class Graph {

		// Node in 3d space:
        [StructLayout(LayoutKind.Explicit)]
        public struct Vertice
        {
			[FieldOffset(0)]	public Vector3	Position;
            [FieldOffset(12)]	public Vector3	Velocity;
	        [FieldOffset(24)]	public Vector3	Acceleration;
	        [FieldOffset(36)]	public Vector3	Force;
	        [FieldOffset(48)]	public float	Mass;

			[FieldOffset(52)]	public float	Size;
			[FieldOffset(56)]	public Vector4	Color;

			[FieldOffset(72)]	public int	LinksPtr;
            [FieldOffset(76)]	public int	LinksCount;
	        [FieldOffset(80)]	public int	Id;
            [FieldOffset(84)]   public Vector3 Dummy;
        }

		
        // Edge between 2 particles:
        [StructLayout(LayoutKind.Explicit)]
        public struct Link
        {
            [FieldOffset(0)] public int Par1;
            [FieldOffset(4)] public int Par2;
            [FieldOffset(8)] public float Length;
            [FieldOffset(12)] public float Strength;
			[FieldOffset(16)] public Vector4 Color;
			[FieldOffset(32)] public float Width;
        }

		public List<Vertice>	Nodes;
		public List<Link>		Links;
		public int				NodesCount;
		public Dictionary<int, List<int>> neighboors;

		public Graph()
		{
			Nodes = new List<Vertice>();
			Links = new List<Link>();
			NodesCount = 0;
		}
	}
}
