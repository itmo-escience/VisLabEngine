using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;


namespace Fusion.Engine.Graphics.GIS.GlobeMath
{
    /// <summary>
    /// Represents a plane in three dimensional space.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DPlane : IEquatable<DPlane>, IFormattable
    {
        /// <summary>
        /// The normal vector of the plane.
        /// </summary>
        public DVector3 Normal;

        /// <summary>
        /// The distance of the plane along its normal from the origin.
        /// </summary>
        public double D;

        /// <summary>
        /// Initializes a new instance of the <see cref="DPlane"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public DPlane(double value)
        {
            Normal.X = Normal.Y = Normal.Z = D = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DPlane"/> struct.
        /// </summary>
        /// <param name="a">The X component of the normal.</param>
        /// <param name="b">The Y component of the normal.</param>
        /// <param name="c">The Z component of the normal.</param>
        /// <param name="d">The distance of the plane along its normal from the origin.</param>
        public DPlane(double a, double b, double c, double d)
        {
            Normal.X = a;
            Normal.Y = b;
            Normal.Z = c;
            D = d;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DPlane" /> class.
        /// </summary>
        /// <param name="point">Any point that lies along the plane.</param>
        /// <param name="normal">The normal vector to the plane.</param>
        public DPlane(DVector3 point, DVector3 normal)
        {
            this.Normal = normal;
            this.D = -DVector3.Dot(normal, point);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane"/> struct.
        /// </summary>
        /// <param name="value">The normal of the plane.</param>
        /// <param name="d">The distance of the plane along its normal from the origin</param>
        public DPlane(DVector3 value, double d)
        {
            Normal = value;
            D = d;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane"/> struct.
        /// </summary>
        /// <param name="point1">First point of a triangle defining the plane.</param>
        /// <param name="point2">Second point of a triangle defining the plane.</param>
        /// <param name="point3">Third point of a triangle defining the plane.</param>
        public DPlane(DVector3 point1, DVector3 point2, DVector3 point3)
        {
            double x1 = point2.X - point1.X;
            double y1 = point2.Y - point1.Y;
            double z1 = point2.Z - point1.Z;
            double x2 = point3.X - point1.X;
            double y2 = point3.Y - point1.Y;
            double z2 = point3.Z - point1.Z;
            double yz = (y1 * z2) - (z1 * y2);
            double xz = (z1 * x2) - (x1 * z2);
            double xy = (x1 * y2) - (y1 * x2);
            double invPyth = 1.0d / (double)(Math.Sqrt((yz * yz) + (xz * xz) + (xy * xy)));

            Normal.X = yz * invPyth;
            Normal.Y = xz * invPyth;
            Normal.Z = xy * invPyth;
            D = -((Normal.X * point1.X) + (Normal.Y * point1.Y) + (Normal.Z * point1.Z));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DPlane"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the A, B, C, and D components of the plane. This must be an arDRay with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public DPlane(double[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Plane.");

            Normal.X = values[0];
            Normal.Y = values[1];
            Normal.Z = values[2];
            D = values[3];
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the A, B, C, or D component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the A component, 1 for the B component, 2 for the C component, and 3 for the D component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Normal.X;
                    case 1: return Normal.Y;
                    case 2: return Normal.Z;
                    case 3: return D;
                }

                throw new ArgumentOutOfRangeException("index", "Indices for Plane run from 0 to 3, inclusive.");
            }

            set
            {
                switch (index)
                {
                    case 0: Normal.X = value; break;
                    case 1: Normal.Y = value; break;
                    case 2: Normal.Z = value; break;
                    case 3: D = value; break;
                    default: throw new ArgumentOutOfRangeException("index", "Indices for Plane run from 0 to 3, inclusive.");
                }
            }
        }

        /// <summary>
        /// Changes the coefficients of the normal vector of the plane to make it of unit length.
        /// </summary>
        public void Normalize()
        {
            double magnitude = 1.0 / (Math.Sqrt((Normal.X * Normal.X) + (Normal.Y * Normal.Y) + (Normal.Z * Normal.Z)));

            Normal.X *= magnitude;
            Normal.Y *= magnitude;
            Normal.Z *= magnitude;
            D *= magnitude;
        }

        /// <summary>
        /// Creates an arDRay containing the elements of the plane.
        /// </summary>
        /// <returns>A four-element arDRay containing the components of the plane.</returns>
        public double[] ToArDRay()
        {
            return new double[] { Normal.X, Normal.Y, Normal.Z, D };
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref DVector3 point)
        {
            return DCollision.PlaneIntersectsPoint(ref this, ref point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DRay"/>.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DRay DRay)
        {
            double distance;
            return DCollision.RayIntersectsPlane(ref DRay, ref this, out distance);
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
            return DCollision.RayIntersectsPlane(ref DRay, ref this, out distance);
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
            return DCollision.RayIntersectsPlane(ref DRay, ref this, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DPlane plane)
        {
            return DCollision.PlaneIntersectsPlane(ref this, ref plane);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="line">When the method completes, contains the line of intersection
        /// as a <see cref="DRay"/>, or a zero DRay if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref DPlane plane, out DRay line)
        {
            return DCollision.PlaneIntersectsPlane(ref this, ref plane, out line);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3)
        {
            return DCollision.PlaneIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref DBoundingBox box)
        {
            return DCollision.PlaneIntersectsBox(ref this, ref box);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref DBoundingSphere sphere)
        {
            return DCollision.PlaneIntersectsSphere(ref this, ref sphere);
        }

        /// <summary>
        /// Scales the plane by the given scaling factor.
        /// </summary>
        /// <param name="value">The plane to scale.</param>
        /// <param name="scale">The amount by which to scale the plane.</param>
        /// <param name="result">When the method completes, contains the scaled plane.</param>
        public static void Multiply(ref DPlane value, double scale, out DPlane result)
        {
            result.Normal.X = value.Normal.X * scale;
            result.Normal.Y = value.Normal.Y * scale;
            result.Normal.Z = value.Normal.Z * scale;
            result.D = value.D * scale;
        }

        /// <summary>
        /// Scales the plane by the given scaling factor.
        /// </summary>
        /// <param name="value">The plane to scale.</param>
        /// <param name="scale">The amount by which to scale the plane.</param>
        /// <returns>The scaled plane.</returns>
        public static DPlane Multiply(DPlane value, double scale)
        {
            return new DPlane(value.Normal.X * scale, value.Normal.Y * scale, value.Normal.Z * scale, value.D * scale);
        }

        /// <summary>
        /// Calculates the dot product of the specified vector and plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the specified plane and vector.</param>
        public static void Dot(ref DPlane left, ref DVector4 right, out double result)
        {
            result = (left.Normal.X * right.X) + (left.Normal.Y * right.Y) + (left.Normal.Z * right.Z) + (left.D * right.W);
        }

        /// <summary>
        /// Calculates the dot product of the specified vector and plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The dot product of the specified plane and vector.</returns>
        public static double Dot(DPlane left, DVector4 right)
        {
            return (left.Normal.X * right.X) + (left.Normal.Y * right.Y) + (left.Normal.Z * right.Z) + (left.D * right.W);
        }

        /// <summary>
        /// Calculates the dot product of a specified vector and the normal of the plane plus the distance value of the plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of a specified vector and the normal of the Plane plus the distance value of the plane.</param>
        public static void DotCoordinate(ref DPlane left, ref DVector3 right, out double result)
        {
            result = (left.Normal.X * right.X) + (left.Normal.Y * right.Y) + (left.Normal.Z * right.Z) + left.D;
        }

        /// <summary>
        /// Calculates the dot product of a specified vector and the normal of the plane plus the distance value of the plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The dot product of a specified vector and the normal of the Plane plus the distance value of the plane.</returns>
        public static double DotCoordinate(DPlane left, DVector3 right)
        {
            return (left.Normal.X * right.X) + (left.Normal.Y * right.Y) + (left.Normal.Z * right.Z) + left.D;
        }

        /// <summary>
        /// Calculates the dot product of the specified vector and the normal of the plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the specified vector and the normal of the plane.</param>
        public static void DotNormal(ref DPlane left, ref DVector3 right, out double result)
        {
            result = (left.Normal.X * right.X) + (left.Normal.Y * right.Y) + (left.Normal.Z * right.Z);
        }

        /// <summary>
        /// Calculates the dot product of the specified vector and the normal of the plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The dot product of the specified vector and the normal of the plane.</returns>
        public static double DotNormal(DPlane left, DVector3 right)
        {
            return (left.Normal.X * right.X) + (left.Normal.Y * right.Y) + (left.Normal.Z * right.Z);
        }

        /// <summary>
        /// Changes the coefficients of the normal vector of the plane to make it of unit length.
        /// </summary>
        /// <param name="plane">The source plane.</param>
        /// <param name="result">When the method completes, contains the normalized plane.</param>
        public static void Normalize(ref DPlane plane, out DPlane result)
        {
            double magnitude = 1.0 / (Math.Sqrt((plane.Normal.X * plane.Normal.X) + (plane.Normal.Y * plane.Normal.Y) + (plane.Normal.Z * plane.Normal.Z)));

            result.Normal.X = plane.Normal.X * magnitude;
            result.Normal.Y = plane.Normal.Y * magnitude;
            result.Normal.Z = plane.Normal.Z * magnitude;
            result.D = plane.D * magnitude;
        }

        /// <summary>
        /// Changes the coefficients of the normal vector of the plane to make it of unit length.
        /// </summary>
        /// <param name="plane">The source plane.</param>
        /// <returns>The normalized plane.</returns>
        public static DPlane Normalize(DPlane plane)
        {
            double magnitude = 1.0 / (Math.Sqrt((plane.Normal.X * plane.Normal.X) + (plane.Normal.Y * plane.Normal.Y) + (plane.Normal.Z * plane.Normal.Z)));
            return new DPlane(plane.Normal.X * magnitude, plane.Normal.Y * magnitude, plane.Normal.Z * magnitude, plane.D * magnitude);
        }

        /// <summary>
        /// Transforms a normalized plane by a DQuaternion rotation.
        /// </summary>
        /// <param name="plane">The normalized source plane.</param>
        /// <param name="rotation">The DQuaternion rotation.</param>
        /// <param name="result">When the method completes, contains the transformed plane.</param>
        public static void Transform(ref DPlane plane, ref DQuaternion rotation, out DPlane result)
        {
            double x2 = rotation.X + rotation.X;
            double y2 = rotation.Y + rotation.Y;
            double z2 = rotation.Z + rotation.Z;
            double wx = rotation.W * x2;
            double wy = rotation.W * y2;
            double wz = rotation.W * z2;
            double xx = rotation.X * x2;
            double xy = rotation.X * y2;
            double xz = rotation.X * z2;
            double yy = rotation.Y * y2;
            double yz = rotation.Y * z2;
            double zz = rotation.Z * z2;

            double x = plane.Normal.X;
            double y = plane.Normal.Y;
            double z = plane.Normal.Z;

            result.Normal.X = ((x * ((1.0 - yy) - zz)) + (y * (xy - wz))) + (z * (xz + wy));
            result.Normal.Y = ((x * (xy + wz)) + (y * ((1.0 - xx) - zz))) + (z * (yz - wx));
            result.Normal.Z = ((x * (xz - wy)) + (y * (yz + wx))) + (z * ((1.0 - xx) - yy));
            result.D = plane.D;
        }

        /// <summary>
        /// Transforms a normalized plane by a DQuaternion rotation.
        /// </summary>
        /// <param name="plane">The normalized source plane.</param>
        /// <param name="rotation">The DQuaternion rotation.</param>
        /// <returns>The transformed plane.</returns>
        public static DPlane Transform(DPlane plane, DQuaternion rotation)
        {
            DPlane result;
            double x2 = rotation.X + rotation.X;
            double y2 = rotation.Y + rotation.Y;
            double z2 = rotation.Z + rotation.Z;
            double wx = rotation.W * x2;
            double wy = rotation.W * y2;
            double wz = rotation.W * z2;
            double xx = rotation.X * x2;
            double xy = rotation.X * y2;
            double xz = rotation.X * z2;
            double yy = rotation.Y * y2;
            double yz = rotation.Y * z2;
            double zz = rotation.Z * z2;

            double x = plane.Normal.X;
            double y = plane.Normal.Y;
            double z = plane.Normal.Z;

            result.Normal.X = ((x * ((1.0 - yy) - zz)) + (y * (xy - wz))) + (z * (xz + wy));
            result.Normal.Y = ((x * (xy + wz)) + (y * ((1.0 - xx) - zz))) + (z * (yz - wx));
            result.Normal.Z = ((x * (xz - wy)) + (y * (yz + wx))) + (z * ((1.0 - xx) - yy));
            result.D = plane.D;

            return result;
        }

        /// <summary>
        /// Transforms an arDRay of normalized planes by a DQuaternion rotation.
        /// </summary>
        /// <param name="planes">The arDRay of normalized planes to transform.</param>
        /// <param name="rotation">The DQuaternion rotation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="planes"/> is <c>null</c>.</exception>
        public static void Transform(DPlane[] planes, ref DQuaternion rotation)
        {
            if (planes == null)
                throw new ArgumentNullException("planes");

            double x2 = rotation.X + rotation.X;
            double y2 = rotation.Y + rotation.Y;
            double z2 = rotation.Z + rotation.Z;
            double wx = rotation.W * x2;
            double wy = rotation.W * y2;
            double wz = rotation.W * z2;
            double xx = rotation.X * x2;
            double xy = rotation.X * y2;
            double xz = rotation.X * z2;
            double yy = rotation.Y * y2;
            double yz = rotation.Y * z2;
            double zz = rotation.Z * z2;

            for (int i = 0; i < planes.Length; ++i)
            {
                double x = planes[i].Normal.X;
                double y = planes[i].Normal.Y;
                double z = planes[i].Normal.Z;

                /*
                 * Note:
                 * Factor common arithmetic out of loop.
                */
                planes[i].Normal.X = ((x * ((1.0 - yy) - zz)) + (y * (xy - wz))) + (z * (xz + wy));
                planes[i].Normal.Y = ((x * (xy + wz)) + (y * ((1.0 - xx) - zz))) + (z * (yz - wx));
                planes[i].Normal.Z = ((x * (xz - wy)) + (y * (yz + wx))) + (z * ((1.0 - xx) - yy));
            }
        }

        /// <summary>
        /// Transforms a normalized plane by a matrix.
        /// </summary>
        /// <param name="plane">The normalized source plane.</param>
        /// <param name="transformation">The transformation matrix.</param>
        /// <param name="result">When the method completes, contains the transformed plane.</param>
        public static void Transform(ref DPlane plane, ref DMatrix transformation, out DPlane result)
        {
            double x = plane.Normal.X;
            double y = plane.Normal.Y;
            double z = plane.Normal.Z;
            double d = plane.D;

            DMatrix inverse;
            DMatrix.Invert(ref transformation, out inverse);

            result.Normal.X = (((x * inverse.M11) + (y * inverse.M12)) + (z * inverse.M13)) + (d * inverse.M14);
            result.Normal.Y = (((x * inverse.M21) + (y * inverse.M22)) + (z * inverse.M23)) + (d * inverse.M24);
            result.Normal.Z = (((x * inverse.M31) + (y * inverse.M32)) + (z * inverse.M33)) + (d * inverse.M34);
            result.D = (((x * inverse.M41) + (y * inverse.M42)) + (z * inverse.M43)) + (d * inverse.M44);
        }

        /// <summary>
        /// Transforms a normalized plane by a matrix.
        /// </summary>
        /// <param name="plane">The normalized source plane.</param>
        /// <param name="transformation">The transformation matrix.</param>
        /// <returns>When the method completes, contains the transformed plane.</returns>
        public static DPlane Transform(DPlane plane, DMatrix transformation)
        {
            DPlane result;
            double x = plane.Normal.X;
            double y = plane.Normal.Y;
            double z = plane.Normal.Z;
            double d = plane.D;

            transformation.Invert();
            result.Normal.X = (((x * transformation.M11) + (y * transformation.M12)) + (z * transformation.M13)) + (d * transformation.M14);
            result.Normal.Y = (((x * transformation.M21) + (y * transformation.M22)) + (z * transformation.M23)) + (d * transformation.M24);
            result.Normal.Z = (((x * transformation.M31) + (y * transformation.M32)) + (z * transformation.M33)) + (d * transformation.M34);
            result.D = (((x * transformation.M41) + (y * transformation.M42)) + (z * transformation.M43)) + (d * transformation.M44);

            return result;
        }

        /// <summary>
        /// Transforms an arDRay of normalized planes by a matrix.
        /// </summary>
        /// <param name="planes">The arDRay of normalized planes to transform.</param>
        /// <param name="transformation">The transformation matrix.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="planes"/> is <c>null</c>.</exception>
        public static void Transform(DPlane[] planes, ref DMatrix transformation)
        {
            if (planes == null)
                throw new ArgumentNullException("planes");

            DMatrix inverse;
            DMatrix.Invert(ref transformation, out inverse);

            for (int i = 0; i < planes.Length; ++i)
            {
                Transform(ref planes[i], ref transformation, out planes[i]);
            }
        }

        /// <summary>
        /// Scales a plane by the given value.
        /// </summary>
        /// <param name="scale">The amount by which to scale the plane.</param>
        /// <param name="plane">The plane to scale.</param>
        /// <returns>The scaled plane.</returns>
        public static DPlane operator *(double scale, DPlane plane)
        {
            return new DPlane(plane.Normal.X * scale, plane.Normal.Y * scale, plane.Normal.Z * scale, plane.D * scale);
        }

        /// <summary>
        /// Scales a plane by the given value.
        /// </summary>
        /// <param name="plane">The plane to scale.</param>
        /// <param name="scale">The amount by which to scale the plane.</param>
        /// <returns>The scaled plane.</returns>
        public static DPlane operator *(DPlane plane, double scale)
        {
            return new DPlane(plane.Normal.X * scale, plane.Normal.Y * scale, plane.Normal.Z * scale, plane.D * scale);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(DPlane left, DPlane right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(DPlane left, DPlane right)
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
            return string.Format(CultureInfo.CurrentCulture, "A:{0} B:{1} C:{2} D:{3}", Normal.X, Normal.Y, Normal.Z, D);
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
            return string.Format(CultureInfo.CurrentCulture, "A:{0} B:{1} C:{2} D:{3}", Normal.X.ToString(format, CultureInfo.CurrentCulture),
                Normal.Y.ToString(format, CultureInfo.CurrentCulture), Normal.Z.ToString(format, CultureInfo.CurrentCulture), D.ToString(format, CultureInfo.CurrentCulture));
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
            return string.Format(formatProvider, "A:{0} B:{1} C:{2} D:{3}", Normal.X, Normal.Y, Normal.Z, D);
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
            return string.Format(formatProvider, "A:{0} B:{1} C:{2} D:{3}", Normal.X.ToString(format, formatProvider),
                Normal.Y.ToString(format, formatProvider), Normal.Z.ToString(format, formatProvider), D.ToString(format, formatProvider));
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
                return (Normal.GetHashCode() * 397) ^ D.GetHashCode();
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Vector4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(DPlane value)
        {
            return Normal == value.Normal && D == value.D;
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

            if (!ReferenceEquals(value.GetType(), typeof(DPlane)))
                return false;

            return Equals((DPlane)value);
        }
    }
}
