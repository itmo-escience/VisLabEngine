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
    /// Define a DDRectangle. This structure is slightly different from System.Drawing.DDRectangle as it is
    /// internally storing Left,Top,Right,Bottom instead of Left,Top,Width,Height.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DRectangle : IEquatable<DRectangle>
    {
        private int _left;
        private int _top;
        private int _right;
        private int _bottom;

        /// <summary>
        /// An empty DRectangle.
        /// </summary>
        public static readonly DRectangle Empty;

        static DRectangle()
        {
            Empty = new DRectangle();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DRectangle"/> struct.
        /// </summary>
        /// <param name="x">The left.</param>
        /// <param name="y">The top.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public DRectangle(int x, int y, int width, int height)
        {
            _left = x;
            _top = y;
            _right = x + width;
            _bottom = y + height;
        }

        /// <summary>
        /// Gets or sets the left.
        /// </summary>
        /// <value>The left.</value>
        public int Left
        {
            get { return _left; }
            set { _left = value; }
        }

        /// <summary>
        /// Gets or sets the top.
        /// </summary>
        /// <value>The top.</value>
        public int Top
        {
            get { return _top; }
            set { _top = value; }
        }

        /// <summary>
        /// Gets or sets the right.
        /// </summary>
        /// <value>The right.</value>
        public int Right
        {
            get { return _right; }
            set { _right = value; }
        }

        /// <summary>
        /// Gets or sets the bottom.
        /// </summary>
        /// <value>The bottom.</value>
        public int Bottom
        {
            get { return _bottom; }
            set { _bottom = value; }
        }

        /// <summary>
        /// Gets or sets the X position.
        /// </summary>
        /// <value>The X position.</value>
        public int X
        {
            get
            {
                return _left;
            }
            set
            {
                _right = value + Width;
                _left = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y position.
        /// </summary>
        /// <value>The Y position.</value>
        public int Y
        {
            get
            {
                return _top;
            }
            set
            {
                _bottom = value + Height;
                _top = value;
            }
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public int Width
        {
            get { return _right - _left; }
            set { _right = _left + value; }
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public int Height
        {
            get { return _bottom - _top; }
            set { _bottom = _top + value; }
        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public Point Location
        {
            get
            {
                return new Point(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// Gets the Point that specifies the center of the DRectangle.
        /// </summary>
        /// <value>
        /// The center.
        /// </value>
        public Point Center
        {
            get
            {
                return new Point(X + (Width / 2), Y + (Height / 2));
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the DRectangle is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is empty]; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get
            {
                return (Width == 0) && (Height == 0) && (X == 0) && (Y == 0);
            }
        }

        /// <summary>
        /// Gets or sets the size of the DRectangle.
        /// </summary>
        /// <value>The size of the DRectangle.</value>
        public Size2 Size
        {
            get
            {
                return new Size2(Width, Height);
            }
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }

        /// <summary>
        /// Gets the position of the top-left corner of the DRectangle.
        /// </summary>
        /// <value>The top-left corner of the DRectangle.</value>
        public Point TopLeft { get { return new Point(_left, _top); } }

        /// <summary>
        /// Gets the position of the top-right corner of the DRectangle.
        /// </summary>
        /// <value>The top-right corner of the DRectangle.</value>
        public Point TopRight { get { return new Point(_right, _top); } }

        /// <summary>
        /// Gets the position of the bottom-left corner of the DRectangle.
        /// </summary>
        /// <value>The bottom-left corner of the DRectangle.</value>
        public Point BottomLeft { get { return new Point(_left, _bottom); } }

        /// <summary>
        /// Gets the position of the bottom-right corner of the DRectangle.
        /// </summary>
        /// <value>The bottom-right corner of the DRectangle.</value>
        public Point BottomRight { get { return new Point(_right, _bottom); } }

        /// <summary>Changes the position of the DRectangle.</summary>
        /// <param name="amount">The values to adjust the position of the DRectangle by.</param>
        public void Offset(Point amount)
        {
            Offset(amount.X, amount.Y);
        }

        /// <summary>Changes the position of the DRectangle.</summary>
        /// <param name="offsetX">Change in the x-position.</param>
        /// <param name="offsetY">Change in the y-position.</param>
        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        /// <summary>Pushes the edges of the DRectangle out by the horizontal and vertical values specified.</summary>
        /// <param name="horizontalAmount">Value to push the sides out by.</param>
        /// <param name="verticalAmount">Value to push the top and bottom out by.</param>
        public void Inflate(int horizontalAmount, int verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>Determines whether this DRectangle contains a specified point represented by its x- and y-coordinates.</summary>
        /// <param name="x">The x-coordinate of the specified point.</param>
        /// <param name="y">The y-coordinate of the specified point.</param>
        public bool Contains(int x, int y)
        {
            return (X <= x) && (x < Right) && (Y <= y) && (y < Bottom);
        }

        /// <summary>Determines whether this DRectangle contains a specified Point.</summary>
        /// <param name="value">The Point to evaluate.</param>
        public bool Contains(Point value)
        {
            bool result;
            Contains(ref value, out result);
            return result;
        }

        /// <summary>Determines whether this DRectangle contains a specified Point.</summary>
        /// <param name="value">The Point to evaluate.</param>
        /// <param name="result">[OutAttribute] true if the specified Point is contained within this DRectangle; false otherwise.</param>
        public void Contains(ref Point value, out bool result)
        {
            result = (X <= value.X) && (value.X < Right) && (Y <= value.Y) && (value.Y < Bottom);
        }

        /// <summary>Determines whether this DRectangle entirely contains a specified DRectangle.</summary>
        /// <param name="value">The DRectangle to evaluate.</param>
        public bool Contains(DRectangle value)
        {
            bool result;
            Contains(ref value, out result);
            return result;
        }

        /// <summary>Determines whether this DRectangle entirely contains a specified DRectangle.</summary>
        /// <param name="value">The DRectangle to evaluate.</param>
        /// <param name="result">[OutAttribute] On exit, is true if this DRectangle entirely contains the specified DRectangle, or false if not.</param>
        public void Contains(ref DRectangle value, out bool result)
        {
            result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }

        /// <summary>
        /// Checks, if specified point is inside <see cref="DRectangle"/>.
        /// </summary>
        /// <param name="x">X point coordinate.</param>
        /// <param name="y">Y point coordinate.</param>
        /// <returns><c>true</c> if point is inside <see cref="DRectangle"/>, otherwise <c>false</c>.</returns>
        public bool Contains(double x, double y)
        {
            return (x >= _left && x <= _right && y >= _top && y <= _bottom);
        }

        /// <summary>
        /// Checks, if specified <see cref="DVector2"/> is inside <see cref="DRectangle"/>.
        /// </summary>
        /// <param name="DVector2D">Coordinate <see cref="DVector2"/>.</param>
        /// <returns><c>true</c> if <see cref="DVector2"/> is inside <see cref="DRectangle"/>, otherwise <c>false</c>.</returns>
        public bool Contains(DVector2 DVector2D)
        {
            return Contains(DVector2D.X, DVector2D.Y);
        }

        /// <summary>Determines whether a specified DRectangle intersects with this DRectangle.</summary>
        /// <param name="value">The DRectangle to evaluate.</param>
        public bool Intersects(DRectangle value)
        {
            bool result;
            Intersects(ref value, out result);
            return result;
        }

        /// <summary>
        /// Determines whether a specified DRectangle intersects with this DRectangle.
        /// </summary>
        /// <param name="value">The DRectangle to evaluate</param>
        /// <param name="result">[OutAttribute] true if the specified DRectangle intersects with this one; false otherwise.</param>
        public void Intersects(ref DRectangle value, out bool result)
        {
            result = (value.X < Right) && (X < value.Right) && (value.Y < Bottom) && (Y < value.Bottom);
        }

        /// <summary>
        /// Creates a DRectangle defining the area where one DRectangle overlaps with another DRectangle.
        /// </summary>
        /// <param name="value1">The first DRectangle to compare.</param>
        /// <param name="value2">The second DRectangle to compare.</param>
        /// <returns>The intersection DRectangle.</returns>
        public static DRectangle Intersect(DRectangle value1, DRectangle value2)
        {
            DRectangle result;
            Intersect(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>Creates a DRectangle defining the area where one DRectangle overlaps with another DRectangle.</summary>
        /// <param name="value1">The first DRectangle to compare.</param>
        /// <param name="value2">The second DRectangle to compare.</param>
        /// <param name="result">[OutAttribute] The area where the two first parameters overlap.</param>
        public static void Intersect(ref DRectangle value1, ref DRectangle value2, out DRectangle result)
        {
            int newLeft = (value1.X > value2.X) ? value1.X : value2.X;
            int newTop = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            int newRight = (value1.Right < value2.Right) ? value1.Right : value2.Right;
            int newBottom = (value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom;
            if ((newRight > newLeft) && (newBottom > newTop))
            {
                result = new DRectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
            }
            else
            {
                result = Empty;
            }
        }

        /// <summary>
        /// Creates a new DRectangle that exactly contains two other DRectangles.
        /// </summary>
        /// <param name="value1">The first DRectangle to contain.</param>
        /// <param name="value2">The second DRectangle to contain.</param>
        /// <returns>The union DRectangle.</returns>
        public static DRectangle Union(DRectangle value1, DRectangle value2)
        {
            DRectangle result;
            Union(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Creates a new DRectangle that exactly contains two other DRectangles.
        /// </summary>
        /// <param name="value1">The first DRectangle to contain.</param>
        /// <param name="value2">The second DRectangle to contain.</param>
        /// <param name="result">[OutAttribute] The DRectangle that must be the union of the first two DRectangles.</param>
        public static void Union(ref DRectangle value1, ref DRectangle value2, out DRectangle result)
        {
            var left = Math.Min(value1.Left, value2.Left);
            var right = Math.Max(value1.Right, value2.Right);
            var top = Math.Min(value1.Top, value2.Top);
            var bottom = Math.Max(value1.Bottom, value2.Bottom);
            result = new DRectangle(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(DRectangle)) return false;
            return Equals((DRectangle)obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="DRectangle"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="DRectangle"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="DRectangle"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(DRectangle other)
        {
            return other._left == _left && other._top == _top && other._right == _right && other._bottom == _bottom;
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
                int result = _left;
                result = (result * 397) ^ _top;
                result = (result * 397) ^ _right;
                result = (result * 397) ^ _bottom;
                return result;
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(DRectangle left, DRectangle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(DRectangle left, DRectangle right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Performs an implicit conversion to the <see cref="DRectangle"/> structure.
        /// </summary>
        /// <remarks>Performs direct converstion from int to double.</remarks>
        /// <param name="value">The source <see cref="DRectangle"/> value.</param>
        /// <returns>The converted structure.</returns>
        public static implicit operator RectangleD(DRectangle value)
        {
            return new RectangleD(value.X, value.Y, value.Width, value.Height);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X:{0} Y:{1} Width:{2} Height:{3}", X, Y, Width, Height);
        }

        internal void MakeXYAndWidthHeight()
        {
            _right = (_right - _left);
            _bottom = (_bottom - _top);
        }
    }
}
