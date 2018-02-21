using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;


namespace Fusion.Engine.Graphics.GIS.GlobeMath
{
    /// <summary>
    /// Represents a three dimensional line based on a point in space and a direction.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DRay : IEquatable<DRay>, IFormattable
    {
        /// <summary>
        /// The position in three dimensional space where the DRay starts.
        /// </summary>
        public DVector3 Position;

        /// <summary>
        /// The normalized direction in which the DRay points.
        /// </summary>
        public DVector3 Direction;

        /// <summary>
        /// Initializes a new instance of the <see cref="DRay"/> struct.
        /// </summary>
        /// <param name="position">The position in three dimensional space of the origin of the DRay.</param>
        /// <param name="direction">The normalized direction of the DRay.</param>
        public DRay(DVector3 position, DVector3 direction)
        {
            this.Position = position;
            this.Direction = direction;
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DVector3 point)
        {
            return DCollision.RayIntersectsPoint(ref this, ref point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DRay"/>.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DRay DRay)
        {
            DVector3 point;
            return DCollision.RayIntersectsRay(ref this, ref DRay, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DRay"/>.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="DVector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DRay DRay, out DVector3 point)
        {
            return DCollision.RayIntersectsRay(ref this, ref DRay, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="DPlane">The DPlane to test</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DPlane DPlane)
        {
            double distance;
            return DCollision.RayIntersectsPlane(ref this, ref DPlane, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="DPlane">The DPlane to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DPlane DPlane, out double distance)
        {
            return DCollision.RayIntersectsPlane(ref this, ref DPlane, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="DPlane">The DPlane to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="DVector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DPlane DPlane, out DVector3 point)
        {
            return DCollision.RayIntersectsPlane(ref this, ref DPlane, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3)
        {
            double distance;
            return DCollision.RayIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3, out double distance)
        {
            return DCollision.RayIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="DVector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3, out DVector3 point)
        {
            return DCollision.RayIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DBoundingBox box)
        {
            double distance;
            return DCollision.RayIntersectsBox(ref this, ref box, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(DBoundingBox box)
        {
            return Intersects(ref box);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DBoundingBox box, out double distance)
        {
            return DCollision.RayIntersectsBox(ref this, ref box, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="DVector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DBoundingBox box, out DVector3 point)
        {
            return DCollision.RayIntersectsBox(ref this, ref box, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DBoundingSphere sphere)
        {
            double distance;
            return DCollision.RayIntersectsSphere(ref this, ref sphere, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(DBoundingSphere sphere)
        {
            return Intersects(ref sphere);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DBoundingSphere sphere, out double distance)
        {
            return DCollision.RayIntersectsSphere(ref this, ref sphere, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="DVector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DBoundingSphere sphere, out DVector3 point)
        {
            return DCollision.RayIntersectsSphere(ref this, ref sphere, out point);
        }

        /// <summary>
        /// Calculates a world space <see cref="DRay"/> from 2d screen coordinates.
        /// </summary>
        /// <param name="x">X coordinate on 2d screen.</param>
        /// <param name="y">Y coordinate on 2d screen.</param>
        /// <param name="viewport"><see cref="ViewportD"/>.</param>
        /// <param name="worldViewProjection">Transformation <see cref="DMatrix"/>.</param>
        /// <returns>Resulting <see cref="DRay"/>.</returns>
        public static DRay GetPickDRay(int x, int y, ViewportD viewport, DMatrix worldViewProjection)
        {
            var nearPoint = new DVector3(x, y, 0);
            var farPoint = new DVector3(x, y, 1);

            nearPoint = DVector3.Unproject(nearPoint, viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth,
                                        viewport.MaxDepth, worldViewProjection);
            farPoint = DVector3.Unproject(farPoint, viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth,
                                        viewport.MaxDepth, worldViewProjection);

            DVector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new DRay(nearPoint, direction);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(DRay left, DRay right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(DRay left, DRay right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Position:{0} Direction:{1}", Position.ToString(), Direction.ToString());
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            return string.Format(CultureInfo.CurrentCulture, "Position:{0} Direction:{1}", Position.ToString(format, CultureInfo.CurrentCulture),
                Direction.ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "Position:{0} Direction:{1}", Position.ToString(), Direction.ToString());
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "Position:{0} Direction:{1}", Position.ToString(format, formatProvider),
                Direction.ToString(format, formatProvider));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ Direction.GetHashCode();
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="DVector4"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="DVector4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="DVector4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(DRay value)
        {
            return Position == value.Position && Direction == value.Direction;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (!ReferenceEquals(value.GetType(), typeof(DRay)))
                return false;

            return Equals((DRay)value);
        }
    }
}
