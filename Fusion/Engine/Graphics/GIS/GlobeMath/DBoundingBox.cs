using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Fusion.Core.Mathematics;


namespace Fusion.Engine.Graphics.GIS.GlobeMath
{
    /// <summary>
    /// Represents an axis-aligned bounding box in three dimensional space.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DBoundingBox : IEquatable<DBoundingBox>, IFormattable
    {
        /// <summary>
        /// The minimum point of the box.
        /// </summary>
        public DVector3 Minimum;

        /// <summary>
        /// The maximum point of the box.
        /// </summary>
        public DVector3 Maximum;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBoundingBox"/> struct.
        /// </summary>
        /// <param name="minimum">The minimum vertex of the bounding box.</param>
        /// <param name="maximum">The maximum vertex of the bounding box.</param>
        public DBoundingBox(DVector3 minimum, DVector3 maximum)
        {
            this.Minimum = minimum;
            this.Maximum = maximum;
        }

        /// <summary>
        /// Retrieves the eight corners of the bounding box.
        /// </summary>
        /// <returns>An arDRay of points representing the eight corners of the bounding box.</returns>
        public DVector3[] GetCorners()
        {
            DVector3[] results = new DVector3[8];
            GetCorners(results);
            return results;
        }

        /// <summary>
        /// Retrieves the eight corners of the bounding box.
        /// </summary>
        /// <returns>An arDRay of points representing the eight corners of the bounding box.</returns>
        public void GetCorners(DVector3[] corners)
        {
            corners[0] = new DVector3(Minimum.X, Maximum.Y, Maximum.Z);
            corners[1] = new DVector3(Maximum.X, Maximum.Y, Maximum.Z);
            corners[2] = new DVector3(Maximum.X, Minimum.Y, Maximum.Z);
            corners[3] = new DVector3(Minimum.X, Minimum.Y, Maximum.Z);
            corners[4] = new DVector3(Minimum.X, Maximum.Y, Minimum.Z);
            corners[5] = new DVector3(Maximum.X, Maximum.Y, Minimum.Z);
            corners[6] = new DVector3(Maximum.X, Minimum.Y, Minimum.Z);
            corners[7] = new DVector3(Minimum.X, Minimum.Y, Minimum.Z);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DRay"/>.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DRay DRay)
        {
            double distance;
            return DCollision.RayIntersectsBox(ref DRay, ref this, out distance);
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
            return DCollision.RayIntersectsBox(ref DRay, ref this, out distance);
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
            return DCollision.RayIntersectsBox(ref DRay, ref this, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="DPlane">The DPlane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref DPlane DPlane)
        {
            return DCollision.PlaneIntersectsBox(ref DPlane, ref this);
        }

        /* This implementation is wrong
        /// <summary>
        /// Determines if there is an intersection between the current object and a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3)
        {
            return DCollision.BoxIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
        }
        */

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DBoundingBox box)
        {
            return DCollision.BoxIntersectsBox(ref this, ref box);
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
            return DCollision.BoxIntersectsSphere(ref this, ref sphere);
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
            return DCollision.BoxContainsPoint(ref this, ref point);
        }

        /// <summary>
        /// Determines whether the current objects contains a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(DVector3 point)
        {
            return Contains(ref point);
        }

        /* This implementation is wrong
        /// <summary>
        /// Determines whether the current objects contains a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3)
        {
            return DCollision.BoxContainsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
        }
        */

        /// <summary>
        /// Determines whether the current objects contains a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref DBoundingBox box)
        {
            return DCollision.BoxContainsBox(ref this, ref box);
        }

        /// <summary>
        /// Determines whether the current objects contains a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(DBoundingBox box)
        {
            return Contains(ref box);
        }

        /// <summary>
        /// Determines whether the current objects contains a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref DBoundingSphere sphere)
        {
            return DCollision.BoxContainsSphere(ref this, ref sphere);
        }

        /// <summary>
        /// Determines whether the current objects contains a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(DBoundingSphere sphere)
        {
            return Contains(ref sphere);
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingBox"/> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the box.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is <c>null</c>.</exception>
        public static void FromPoints(DVector3[] points, out DBoundingBox result)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            DVector3 min = new DVector3(double.MaxValue);
            DVector3 max = new DVector3(double.MinValue);

            for (int i = 0; i < points.Length; ++i)
            {
                DVector3.Min(ref min, ref points[i], out min);
                DVector3.Max(ref max, ref points[i], out max);
            }

            result = new DBoundingBox(min, max);
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingBox"/> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the box.</param>
        /// <returns>The newly constructed bounding box.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is <c>null</c>.</exception>
        public static DBoundingBox FromPoints(DVector3[] points)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            DVector3 min = new DVector3(double.MaxValue);
            DVector3 max = new DVector3(double.MinValue);

            for (int i = 0; i < points.Length; ++i)
            {
                DVector3.Min(ref min, ref points[i], out min);
                DVector3.Max(ref max, ref points[i], out max);
            }

            return new DBoundingBox(min, max);
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingBox"/> from a given sphere.
        /// </summary>
        /// <param name="sphere">The sphere that will designate the extents of the box.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
        public static void FromSphere(ref DBoundingSphere sphere, out DBoundingBox result)
        {
            result.Minimum = new DVector3(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius);
            result.Maximum = new DVector3(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius);
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingBox"/> from a given sphere.
        /// </summary>
        /// <param name="sphere">The sphere that will designate the extents of the box.</param>
        /// <returns>The newly constructed bounding box.</returns>
        public static DBoundingBox FromSphere(DBoundingSphere sphere)
        {
            DBoundingBox box;
            box.Minimum = new DVector3(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius);
            box.Maximum = new DVector3(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius);
            return box;
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingBox"/> that is as large as the total combined area of the two specified boxes.
        /// </summary>
        /// <param name="value1">The first box to merge.</param>
        /// <param name="value2">The second box to merge.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
        public static void Merge(ref DBoundingBox value1, ref DBoundingBox value2, out DBoundingBox result)
        {
            DVector3.Min(ref value1.Minimum, ref value2.Minimum, out result.Minimum);
            DVector3.Max(ref value1.Maximum, ref value2.Maximum, out result.Maximum);
        }

        /// <summary>
        /// Constructs a <see cref="DBoundingBox"/> that is as large as the total combined area of the two specified boxes.
        /// </summary>
        /// <param name="value1">The first box to merge.</param>
        /// <param name="value2">The second box to merge.</param>
        /// <returns>The newly constructed bounding box.</returns>
        public static DBoundingBox Merge(DBoundingBox value1, DBoundingBox value2)
        {
            DBoundingBox box;
            DVector3.Min(ref value1.Minimum, ref value2.Minimum, out box.Minimum);
            DVector3.Max(ref value1.Maximum, ref value2.Maximum, out box.Maximum);
            return box;
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(DBoundingBox left, DBoundingBox right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(DBoundingBox left, DBoundingBox right)
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
            return string.Format(CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", Minimum.ToString(), Maximum.ToString());
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

            return string.Format(CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", Minimum.ToString(format, CultureInfo.CurrentCulture),
                Maximum.ToString(format, CultureInfo.CurrentCulture));
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
            return string.Format(formatProvider, "Minimum:{0} Maximum:{1}", Minimum.ToString(), Maximum.ToString());
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

            return string.Format(formatProvider, "Minimum:{0} Maximum:{1}", Minimum.ToString(format, formatProvider),
                Maximum.ToString(format, formatProvider));
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
                return (Minimum.GetHashCode() * 397) ^ Maximum.GetHashCode();
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Vector4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(DBoundingBox value)
        {
            return Minimum == value.Minimum && Maximum == value.Maximum;
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

            if (!ReferenceEquals(value.GetType(), typeof(DBoundingBox)))
                return false;

            return Equals((DBoundingBox)value);
        }

        public BoundingBox ToBoundingBox()
        {
            return new BoundingBox(Minimum.ToVector3(), Maximum.ToVector3());
        }
    }
}
