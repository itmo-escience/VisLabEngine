using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Fusion.Core;
using Fusion.Core.Mathematics;


namespace Fusion.Engine.Graphics
{
    public struct LinePoint : IEquatable<LinePoint>
    {
        /// <summary>
		/// XYZ postiion
		/// </summary>
        public Vector4 Position;



        public bool Equals(LinePoint other)
        {
            return (this.Position == other.Position);
        }
        
        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LinePoint && Equals((LinePoint)obj);
        }

        public override int GetHashCode()
        {
            return Misc.FastHash(Position);
        }

        public static bool operator ==(LinePoint point1, LinePoint point2)
        {
            return point1.Equals(point2);
        }


        public static bool operator !=(LinePoint point1, LinePoint point2)
        {
            return !(point1.Equals(point2));
        }
    }
}
