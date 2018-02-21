using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Fusion.Engine.Graphics.GIS.GlobeMath
{
    /// <summary>
    /// Defines the viewport dimensions using double coordinates for (X, Y, Width, Height).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ViewportD : IEquatable<ViewportD>
    {
        /// <summary>
        /// Position of the pixel coordinate of the upper-left corner of the viewport.
        /// </summary>
        public double X;

        /// <summary>
        /// Position of the pixel coordinate of the upper-left corner of the viewport.
        /// </summary>
        public double Y;

        /// <summary>
        /// Width dimension of the viewport.
        /// </summary>
        public double Width;

        /// <summary>
        /// Height dimension of the viewport.
        /// </summary>
        public double Height;

        /// <summary>
        /// Gets or sets the minimum depth of the clip volume.
        /// </summary>
        public double MinDepth;

        /// <summary>
        /// Gets or sets the maximum depth of the clip volume.
        /// </summary>
        public double MaxDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewportD"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="width">The width of the viewport in pixels.</param>
        /// <param name="height">The height of the viewport in pixels.</param>
        public ViewportD(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = 0f;
            MaxDepth = 1f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewportD"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="width">The width of the viewport in pixels.</param>
        /// <param name="height">The height of the viewport in pixels.</param>
        /// <param name="minDepth">The minimum depth of the clip volume.</param>
        /// <param name="maxDepth">The maximum depth of the clip volume.</param>
        public ViewportD(double x, double y, double width, double height, double minDepth, double maxDepth)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = minDepth;
            MaxDepth = maxDepth;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewportD"/> struct.
        /// </summary>
        /// <param name="bounds">A bounding box that defines the location and size of the viewport in a render target.</param>
        public ViewportD(RectangleD bounds)
        {
            X = bounds.X;
            Y = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;
            MinDepth = 0f;
            MaxDepth = 1f;
        }

        /// <summary>
        /// Gets the size of this resource.
        /// </summary>
        /// <value>The bounds.</value>
        public RectangleD Bounds
        {
            get
            {
                return new RectangleD(X, Y, Width, Height);
            }

            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }

    	/// <summary>
        /// Determines whether the specified <see cref="ViewportD"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="ViewportD"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="ViewportD"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ViewportD other)
        {
            return DMathUtil.NearEqual(X, other.X) && DMathUtil.NearEqual(Y, other.Y) && DMathUtil.NearEqual(Width, other.Width) && DMathUtil.NearEqual(Height, other.Height) && DMathUtil.NearEqual(MinDepth, other.MinDepth)
                   && DMathUtil.NearEqual(MaxDepth, other.MaxDepth);
        }
	
        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is ViewportD && Equals((ViewportD)obj);
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
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Width.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                hashCode = (hashCode * 397) ^ MinDepth.GetHashCode();
                hashCode = (hashCode * 397) ^ MaxDepth.GetHashCode();
                return hashCode;
            }
        }

	/// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ViewportD left, ViewportD right)
        {
            return left.Equals(right);
        }

	/// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ViewportD left, ViewportD right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Retrieves a string representation of this object.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{X:{0} Y:{1} Width:{2} Height:{3} MinDepth:{4} MaxDepth:{5}}}", X, Y, Width, Height, MinDepth, MaxDepth);
        }

        /// <summary>
        /// Projects a 3D vector from object space into screen space.
        /// </summary>
        /// <param name="source">The vector to project.</param>
        /// <param name="projection">The projection DMatrix.</param>
        /// <param name="view">The view DMatrix.</param>
        /// <param name="world">The world DMatrix.</param>
        /// <returns>The projected vector.</returns>
        public DVector3 Project(DVector3 source, DMatrix projection, DMatrix view, DMatrix world)
        {
            DMatrix DMatrix;
            DMatrix.Multiply(ref world, ref view, out DMatrix);
            DMatrix.Multiply(ref DMatrix, ref projection, out DMatrix);

            DVector3 vector;
            Project(ref source, ref DMatrix, out vector);
            return vector;
        }

        /// <summary>
        /// Projects a 3D vector from object space into screen space.
        /// </summary>
        /// <param name="source">The vector to project.</param>
        /// <param name="DMatrix">A combined WorldViewProjection DMatrix.</param>
        /// <param name="vector">The projected vector.</param>
        public void Project(ref DVector3 source, ref DMatrix DMatrix, out DVector3 vector)
        {
            DVector3.Transform(ref source, ref DMatrix, out vector);
            double a = (((source.X * DMatrix.M14) + (source.Y * DMatrix.M24)) + (source.Z * DMatrix.M34)) + DMatrix.M44;

            if (!DMathUtil.IsOne(a))
            {
                vector = (vector / a);
            }

            vector.X = (((vector.X + 1f) * 0.5) * Width) + X;
            vector.Y = (((-vector.Y + 1f) * 0.5) * Height) + Y;
            vector.Z = (vector.Z * (MaxDepth - MinDepth)) + MinDepth;
        }

        /// <summary>
        /// Converts a screen space point into a corresponding point in world space.
        /// </summary>
        /// <param name="source">The vector to project.</param>
        /// <param name="projection">The projection DMatrix.</param>
        /// <param name="view">The view DMatrix.</param>
        /// <param name="world">The world DMatrix.</param>
        /// <returns>The unprojected Vector.</returns>
        public DVector3 Unproject(DVector3 source, DMatrix projection, DMatrix view, DMatrix world)
        {
            DMatrix DMatrix;
            DMatrix.Multiply(ref world, ref view, out DMatrix);
            DMatrix.Multiply(ref DMatrix, ref projection, out DMatrix);
            DMatrix.Invert(ref DMatrix, out DMatrix);

            DVector3 vector;
            Unproject(ref source, ref DMatrix, out vector);
            return vector;
        }

        /// <summary>
        /// Converts a screen space point into a corresponding point in world space.
        /// </summary>
        /// <param name="source">The vector to project.</param>
        /// <param name="DMatrix">An inverted combined WorldViewProjection DMatrix.</param>
        /// <param name="vector">The unprojected vector.</param>
        public void Unproject(ref DVector3 source, ref DMatrix DMatrix, out DVector3 vector)
        {
            vector.X = (((source.X - X) / (Width)) * 2f) - 1f;
            vector.Y = -((((source.Y - Y) / (Height)) * 2f) - 1f);
            vector.Z = (source.Z - MinDepth) / (MaxDepth - MinDepth);

            double a = (((vector.X * DMatrix.M14) + (vector.Y * DMatrix.M24)) + (vector.Z * DMatrix.M34)) + DMatrix.M44;
            DVector3.Transform(ref vector, ref DMatrix, out vector);

            if (!DMathUtil.IsOne(a))
            {
                vector = (vector / a);
            }
        }

        /// <summary>
        /// Gets the aspect ratio used by the viewport.
        /// </summary>
        /// <value>The aspect ratio.</value>
        public double AspectRatio
        {
            get
            {
                if (!DMathUtil.IsZero(Height))
                {
                    return Width / Height;
                }
                return 0f;
            }
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="DViewport"/> to <see cref="ViewportD"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator ViewportD(DViewport value)
        {
            return new ViewportD(value.X, value.Y, value.Width, value.Height, value.MinDepth, value.MaxDepth);
        }

		#if false
        void IDataSerializable.Serialize(BinarySerializer serializer)
        {
            // Write optimized version without using Serialize methods
            if (serializer.Mode == SerializerMode.Write)
            {
                serializer.Writer.Write(X);
                serializer.Writer.Write(Y);
                serializer.Writer.Write(Width);
                serializer.Writer.Write(Height);
                serializer.Writer.Write(MinDepth);
                serializer.Writer.Write(MaxDepth);
            }
            else
            {
                X = serializer.Reader.ReadSingle();
                Y = serializer.Reader.ReadSingle();
                Width = serializer.Reader.ReadSingle();
                Height = serializer.Reader.ReadSingle();
                MinDepth = serializer.Reader.ReadSingle();
                MaxDepth = serializer.Reader.ReadSingle();
            }
        }
		#endif
    }
}
