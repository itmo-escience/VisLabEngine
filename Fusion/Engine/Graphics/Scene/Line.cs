using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Fusion.Drivers.Graphics;
using System.Reflection;
using System.ComponentModel.Design;
using Fusion.Core;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Graphics
{
    public sealed partial class Line : IEquatable<Line>
    {
        public List<LinePoint> Points { get; private set; }

        public int PointCount { get { return Points.Count; } }

        public Line()
        {
            Points = new List<LinePoint>();
        }

        public bool Equals(Line other)
        {
            if (other == null) return false;

            if (this.PointCount != other.PointCount) return false;

            if (!this.Points.SequenceEqual(other.Points)) return false;

            return true;
        }

        /// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		public void Deserialize(BinaryReader reader)
        {
            int pointCount = reader.ReadInt32();
            Points = reader.Read<LinePoint>(pointCount).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PointCount);
            writer.Write(Points.ToArray());
        }
    }
}
