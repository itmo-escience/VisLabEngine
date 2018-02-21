// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Fusion.Engine.Graphics.GIS.GlobeMath
{
    /// <summary>
    /// Defines the DViewport dimensions.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DViewport : IEquatable<DViewport>
    {
        /// <summary>
        /// Position of the pixel coordinate of the upper-left corner of the DViewport.
        /// </summary>
        public int X;

        /// <summary>
        /// Position of the pixel coordinate of the upper-left corner of the DViewport.
        /// </summary>
        public int Y;

        /// <summary>
        /// Width dimension of the DViewport.
        /// </summary>
        public int Width;

        /// <summary>
        /// Height dimension of the DViewport.
        /// </summary>
        public int Height;

        /// <summary>
        /// Gets or sets the minimum depth of the clip volume.
        /// </summary>
        public double MinDepth;

        /// <summary>
        /// Gets or sets the maximum depth of the clip volume.
        /// </summary>
        public double MaxDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="DViewport"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the DViewport in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the DViewport in pixels.</param>
        /// <param name="width">The width of the DViewport in pixels.</param>
        /// <param name="height">The height of the DViewport in pixels.</param>
        public DViewport(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = 0f;
            MaxDepth = 1f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DViewport"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the DViewport in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the DViewport in pixels.</param>
        /// <param name="width">The width of the DViewport in pixels.</param>
        /// <param name="height">The height of the DViewport in pixels.</param>
        /// <param name="minDepth">The minimum depth of the clip volume.</param>
        /// <param name="maxDepth">The maximum depth of the clip volume.</param>
        public DViewport(int x, int y, int width, int height, double minDepth, double maxDepth)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = minDepth;
            MaxDepth = maxDepth;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DViewport"/> struct.
        /// </summary>
        /// <param name="bounds">A bounding box that defines the location and size of the DViewport in a render target.</param>
        public DViewport(DRectangle bounds)
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
        public DRectangle Bounds
        {
            get
            {
                return new DRectangle(X, Y, Width, Height);
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
        /// Determines whether the specified <see cref="DViewport"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="DViewport"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="DViewport"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(DViewport other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height && DMathUtil.NearEqual(MinDepth, other.MinDepth) && DMathUtil.NearEqual(MaxDepth, other.MaxDepth);
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
            return obj is DViewport && Equals((DViewport)obj);
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
                int hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
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
        public static bool operator ==(DViewport left, DViewport right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(DViewport left, DViewport right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Retrieves a string representation of this object.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
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
        /// Gets the aspect ratio used by the DViewport.
        /// </summary>
        /// <value>The aspect ratio.</value>
        public double AspectRatio
        {
            get
            {
                if (Height != 0)
                {
                    return Width / (double)Height;
                }
                return 0f;
            }
        }
    }
}
