using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Input;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using System.IO;

namespace Fusion.Engine.Graphics.Graph {
	public class GraphHelper
	{
		private GraphLayer graph;

		public GraphHelper(GraphLayer g)
		{
			graph = g;
		}

		/// <summary>
		/// Select node.
		/// Highlight desired node.
		/// </summary>
		/// <param name="reconstructGraphAfterSelection"></param>
		/// <returns></returns>
		public int NodeSelection(bool reconstructGraphAfterSelection = false)
		{
			Vector2 cursor = Game.Instance.Mouse.Position;
			Vector3 nodePosition;
			int trueIndex;
			int selNode = -1;
			
			bool selected = graph.SelectNode(cursor, StereoEye.Mono, 0.025f, out selNode, out nodePosition, out trueIndex);
		    //graph.CreateLinks(graph.currentIteration);

			return trueIndex;
		}



		/// <summary>
		/// Select node with deselection of previous ones
		/// </summary>
		/// <returns></returns>
		public int NodeSelectionWithDeselect()
		{
			graph.selectedVertice = -1;//.Clear();
			return NodeSelection();
		}

		static Random rand = new Random();

		/// <summary>
		/// Returns normalized random vector with X=0
		/// </summary>
		/// <returns></returns>
		public static Vector3 RadialRandomVector()
		{
			Vector2 r;
			do
			{
				r = rand.NextVector2(-Vector2.One, Vector2.One);
			} while (r.Length() > 1);

			r.Normalize();

			return new Vector3(0, r.X, r.Y); //
		}

		/// <summary>
		/// Returns normalized random 3D vector
		/// </summary>
		/// <returns></returns>
		public static Vector3 RadialRandomVector3D()
		{
			Vector3 r;
			do
			{
				r = rand.NextVector3(-Vector3.One, Vector3.One);
			} while (r.Length() > 1);
			r.Normalize();
			return r; //
		}



		Rectangle CalculateStringSize(string text, SpriteFont font, out string[] lines)
		{
			var rec = new Rectangle();
			lines = text.Split('\n');
			float width = 0;
			foreach (var line in lines)
			{
				var curRec = font.MeasureString(line);

				if (curRec.Width > width) width = curRec.Width;

			}

			float height = (lines.Length) * font.LineHeight + font.CapHeight;

			rec.Width = (int)width;
			rec.Height = (int)height;

			return rec;
		}



		public Vector2 PointPositionToScreenspace(DVector3 dPos, Frame frame, StereoEye eye)
		{
			var cam = graph.Camera;
			var viewMatrix = cam.GetViewMatrix(eye);
			var projMatrix = cam.GetProjectionMatrix(eye);

			
			var matrix = viewMatrix * projMatrix;
			DMatrix dMatrix = new DMatrix(matrix.M11, matrix.M12, matrix.M13, matrix.M14,
				matrix.M21, matrix.M22, matrix.M23, matrix.M24,
				matrix.M31, matrix.M32, matrix.M33, matrix.M34,
				matrix.M41, matrix.M42, matrix.M43, matrix.M44);
				
			
			var position = DVector3.Project(dPos, frame.X, frame.Y, 
				frame.Width, frame.Height, 
			cam.FreeCamZNear, cam.FreeCamZFar, dMatrix);

			return new Vector2((float)position.X, (float)position.Y);
		}

	

		/// <summary>
		/// Projects pixels
		/// </summary>
		/// <param name="point"></param>
		/// <param name="frame"></param>
		/// <returns></returns>
		public static Vector2 PixelsToProj(Vector2 point, float width, float height)
		{
			Vector2 proj = new Vector2(
				(float)point.X / (float)width,
				(float)point.Y / (float)height
			);
			proj.X = proj.X * 2 - 1;
			proj.Y = -proj.Y * 2 + 1;
			return proj;
		}

	    public static Vector2 ProjToPixels(Vector2 proj, float width, float height)
	    {
            return new Vector2((proj.X + 1) * 0.5f * width, (-proj.Y + 1) * 0.5f * height);
	    }
	}
}
