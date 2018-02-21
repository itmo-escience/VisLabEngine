using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;


namespace Fusion.Engine.Graphics.GIS.GlobeMath
{
    /// <summary>
    /// Represents a bounding sphere in three dimensional space.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DBoundingSphere : IEquatable<DBoundingSphere>, IFormattable
    {
        /// <summary>
        /// The center of the sphere in three dimensional space.
        /// </summary>
        public DVector3 Center;

        /// <summary>
        /// The radius of the sphere.
        /// </summary>
        public double Radius;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBoundingBox"/> struct.
        /// </summary>
        /// <param name="center">The center of the sphere in three dimensional space.</param>
        /// <param name="radius">The radius of the sphere.</param>
        public DBoundingSphere(DVector3 center, double radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DRay"/>.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DRay DRay)
        {
            double distance;
            return DCollision.RayIntersectsSphere(ref DRay, ref this, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DRay"/>.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DRay DRay, out double distance)
        {
            return DCollision.RayIntersectsSphere(ref DRay, ref this, out distance);
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
            return DCollision.RayIntersectsSphere(ref DRay, ref this, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="DPlane">The DPlane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref DPlane DPlane)
        {
            return DCollision.PlaneIntersectsSphere(ref DPlane, ref this);
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
            return DCollision.SphereIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DBoundingBox box)
        {
            return DCollision.BoxIntersectsSphere(ref box, ref this);
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
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DBoundingSphere sphere)
        {
            return DCollision.SphereIntersectsSphere(ref this, ref sphere);
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
        /// Determines whether the current objects contains a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref DVector3 point)
        {
            return DCollision.SphereContainsPoint(ref this, ref point);
        }

        /// <summary>
        /// Determines whether the current objects contains a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3)
        {
            return DCollision.SphereContainsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
        }

        /// <summary>
        /// Determines whether the current objects contains a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref DBoundingBox box)
        {
            return DCollision.SphereContainsBox(ref this, ref box);
        }

        /// <summary>
        /// Determines whether the current objects contains a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref DBoundingSphere sphere)
        {
            return DCollision.SphereContainsSphere(ref this, ref sphere);
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingSphere" /> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the sphere.</param>
        /// <param name="start">The start index from points arDRay to start compute the bounding sphere.</param>
        /// <param name="count">The count of points to process to compute the bounding sphere.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding sphere.</param>
        /// <exception cref="System.ArgumentNullException">points</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// start
        /// or
        /// count
        /// </exception>
        public static void FromPoints(DVector3[] points, int start, int count, out DBoundingSphere result)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            // Check that start is in the correct range
            if (start < 0 || start >= points.Length)
            {
                throw new ArgumentOutOfRangeException("start", start, string.Format("Must be in the range [0, {0}]", points.Length - 1));
            }

            // Check that count is in the correct range
            if (count < 0 || (start + count) > points.Length)
            {
                throw new ArgumentOutOfRangeException("count", count, string.Format("Must be in the range <= {0}", points.Length));
            }

            var upperEnd = start + count;

            //Find the center of all points.
            DVector3 center = DVector3.Zero;
            for (int i = start; i < upperEnd; ++i)
            {
                DVector3.Add(ref points[i], ref center, out center);
            }

            //This is the center of our sphere.
            center /= (double)count;

            //Find the radius of the sphere
            double radius = 0f;
            for (int i = start; i < upperEnd; ++i)
            {
                //We are doing a relative distance comparison to find the maximum distance
                //from the center of our sphere.
                double distance;
                DVector3.DistanceSquared(ref center, ref points[i], out distance);

                if (distance > radius)
                    radius = distance;
            }

            //Find the real distance from the DistanceSquared.
            radius = (double)Math.Sqrt(radius);

            //Construct the sphere.
            result.Center = center;
            result.Radius = radius;
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingSphere"/> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the sphere.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding sphere.</param>
        public static void FromPoints(DVector3[] points, out DBoundingSphere result)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            FromPoints(points, 0, points.Length, out result);
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingSphere"/> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the sphere.</param>
        /// <returns>The newly constructed bounding sphere.</returns>
        public static DBoundingSphere FromPoints(DVector3[] points)
        {
            DBoundingSphere result;
            FromPoints(points, out result);
            return result;
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingSphere"/> from a given box.
        /// </summary>
        /// <param name="box">The box that will designate the extents of the sphere.</param>
        /// <param name="result">When the method completes, the newly constructed bounding sphere.</param>
        public static void FromBox(ref DBoundingBox box, out DBoundingSphere result)
        {
            DVector3.Lerp(ref box.Minimum, ref box.Maximum, 0.5, out result.Center);

            double x = box.Minimum.X - box.Maximum.X;
            double y = box.Minimum.Y - box.Maximum.Y;
            double z = box.Minimum.Z - box.Maximum.Z;

            double distance = (double)(Math.Sqrt((x * x) + (y * y) + (z * z)));
            result.Radius = distance * 0.5;
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingSphere"/> from a given box.
        /// </summary>
        /// <param name="box">The box that will designate the extents of the sphere.</param>
        /// <returns>The newly constructed bounding sphere.</returns>
        public static DBoundingSphere FromBox(DBoundingBox box)
        {
            DBoundingSphere result;
            FromBox(ref box, out result);
            return result;
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingSphere"/> that is the as large as the total combined area of the two specified spheres.
        /// </summary>
        /// <param name="value1">The first sphere to merge.</param>
        /// <param name="value2">The second sphere to merge.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding sphere.</param>
        public static void Merge(ref DBoundingSphere value1, ref DBoundingSphere value2, out DBoundingSphere result)
        {
            DVector3 difference = value2.Center - value1.Center;

            double length = difference.Length();
            double radius = value1.Radius;
            double radius2 = value2.Radius;

            if (radius + radius2 >= length)
            {
                if (radius - radius2 >= length)
                {
                    result = value1;
                    return;
                }

                if (radius2 - radius >= length)
                {
                    result = value2;
                    return;
                }
            }

            DVector3 vector = difference * (1.0 / length);
            double min = Math.Min(-radius, length - radius2);
            double max = (Math.Max(radius, length + radius2) - min) * 0.5;

            result.Center = value1.Center + vector * (max + min);
            result.Radius = max;
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingSphere"/> that is the as large as the total combined area of the two specified spheres.
        /// </summary>
        /// <param name="value1">The first sphere to merge.</param>
        /// <param name="value2">The second sphere to merge.</param>
        /// <returns>The newly constructed bounding sphere.</returns>
        public static DBoundingSphere Merge(DBoundingSphere value1, DBoundingSphere value2)
        {
            DBoundingSphere result;
            Merge(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(DBoundingSphere left, DBoundingSphere right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(DBoundingSphere left, DBoundingSphere right)
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
            return string.Format(CultureInfo.CurrentCulture, "Center:{0} Radius:{1}", Center.ToString(), Radius.ToString());
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
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "Center:{0} Radius:{1}", Center.ToString(format, CultureInfo.CurrentCulture),
                Radius.ToString(format, CultureInfo.CurrentCulture));
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
            return string.Format(formatProvider, "Center:{0} Radius:{1}", Center.ToString(), Radius.ToString());
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
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "Center:{0} Radius:{1}", Center.ToString(format, formatProvider),
                Radius.ToString(format, formatProvider));
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
                return (Center.GetHashCode() * 397) ^ Radius.GetHashCode();
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Vector4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(DBoundingSphere value)
        {
            return Center == value.Center && Radius == value.Radius;
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

            if (!ReferenceEquals(value.GetType(), typeof(DBoundingSphere)))
                return false;

            return Equals((DBoundingSphere)value);
        }
    }
}
