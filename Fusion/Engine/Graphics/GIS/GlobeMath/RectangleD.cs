using System;
using System.Globalization;
using System.Runtime.InteropServices;


namespace Fusion.Engine.Graphics.GIS.GlobeMath
{
    /// <summary>
    /// Define a RectangleD. This structure is slightly different from System.Drawing.RectangleD as it is
    /// internally storing Left,Top,Right,Bottom instead of Left,Top,Width,Height.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RectangleD : IEquatable<RectangleD>
    {
        private double _left;
        private double _top;
        private double _right;
        private double _bottom;

        /// <summary>
        /// An empty rectangle.
        /// </summary>
        public static readonly RectangleD Empty;

        /// <summary>
        /// An infinite rectangle. See remarks.
        /// </summary>
        /// <remarks>
        /// <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/dd372261%28v=vs.85%29.aspx">InfiniteRect function</see>
        /// Any properties that involve computations, like <see cref="Center"/>, <see cref="Width"/> or <see cref="Height"/>
        /// may return incorrect results - <see cref="double.NaN"/>.
        /// </remarks>
        public static readonly RectangleD Infinite;

        static RectangleD()
        {
            Empty = new RectangleD();
            Infinite = new RectangleD
                       {
                           Left = double.NegativeInfinity,
                           Top = double.NegativeInfinity,
                           Right = double.PositiveInfinity,
                           Bottom = double.PositiveInfinity
                       };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RectangleD"/> struct.
        /// </summary>
        /// <param name="x">The left.</param>
        /// <param name="y">The top.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public RectangleD(double x, double y, double width, double height)
        {
            _left = x;
            _top = y;
            _right = x + width;
            _bottom = y + height;
        }

        /// <summary>
        /// Gets or sets the X position of the left edge.
        /// </summary>
        /// <value>The left.</value>
        public double Left
        {
            get { return _left; }
            set { _left = value; }
        }

        /// <summary>
        /// Gets or sets the top.
        /// </summary>
        /// <value>The top.</value>
        public double Top
        {
            get { return _top; }
            set { _top = value; }
        }

        /// <summary>
        /// Gets or sets the right.
        /// </summary>
        /// <value>The right.</value>
        public double Right
        {
            get { return _right; }
            set { _right = value; }
        }

        /// <summary>
        /// Gets or sets the bottom.
        /// </summary>
        /// <value>The bottom.</value>
        public double Bottom
        {
            get { return _bottom; }
            set { _bottom = value; }
        }

        /// <summary>
        /// Gets or sets the X position.
        /// </summary>
        /// <value>The X position.</value>
        public double X
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
        public double Y
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
        public double Width
        {
            get { return _right - _left; }
            set { _right = _left + value; }
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public double Height
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
        public DVector2 Location
        {
            get
            {
                return new DVector2(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// Gets the Point that specifies the center of the rectangle.
        /// </summary>
        /// <value>
        /// The center.
        /// </value>
        public DVector2 Center
        {
            get
            {
                return new DVector2(X + (Width / 2), Y + (Height / 2));
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the rectangle is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is empty]; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get
            {
                return (Width == 0.0) && (Height == 0.0) && (X == 0.0) && (Y == 0.0);
            }
        }

        /// <summary>
        /// Gets or sets the size of the rectangle.
        /// </summary>
        /// <value>The size of the rectangle.</value>
        public Size2D Size
        {
            get
            {
                return new Size2D(Width, Height);
            }
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }

        /// <summary>
        /// Gets the position of the top-left corner of the rectangle.
        /// </summary>
        /// <value>The top-left corner of the rectangle.</value>
        public DVector2 TopLeft { get { return new DVector2(_left, _top); } }

        /// <summary>
        /// Gets the position of the top-right corner of the rectangle.
        /// </summary>
        /// <value>The top-right corner of the rectangle.</value>
        public DVector2 TopRight { get { return new DVector2(_right, _top); } }

        /// <summary>
        /// Gets the position of the bottom-left corner of the rectangle.
        /// </summary>
        /// <value>The bottom-left corner of the rectangle.</value>
        public DVector2 BottomLeft { get { return new DVector2(_left, _bottom); } }

        /// <summary>
        /// Gets the position of the bottom-right corner of the rectangle.
        /// </summary>
        /// <value>The bottom-right corner of the rectangle.</value>
        public DVector2 BottomRight { get { return new DVector2(_right, _bottom); } }

        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="amount">The values to adjust the position of the rectangle by.</param>
        public void Offset(Point amount)
        {
            Offset(amount.X, amount.Y);
        }

        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="amount">The values to adjust the position of the rectangle by.</param>
        public void Offset(DVector2 amount)
        {
            Offset(amount.X, amount.Y);
        }

        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="offsetX">Change in the x-position.</param>
        /// <param name="offsetY">Change in the y-position.</param>
        public void Offset(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        /// <summary>Pushes the edges of the rectangle out by the horizontal and vertical values specified.</summary>
        /// <param name="horizontalAmount">Value to push the sides out by.</param>
        /// <param name="verticalAmount">Value to push the top and bottom out by.</param>
        public void Inflate(double horizontalAmount, double verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>Determines whether this rectangle contains a specified Point.</summary>
        /// <param name="value">The Point to evaluate.</param>
        /// <param name="result">[OutAttribute] true if the specified Point is contained within this rectangle; false otherwise.</param>
        public void Contains(ref DVector2 value, out bool result)
        {
            result = (X <= value.X) && (value.X < Right) && (Y <= value.Y) && (value.Y < Bottom);
        }

        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public bool Contains(RectangleD value)
        {
            return (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }

        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        /// <param name="result">[OutAttribute] On exit, is true if this rectangle entirely contains the specified rectangle, or false if not.</param>
        public void Contains(ref RectangleD value, out bool result)
        {
            result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }

        /// <summary>
        /// Checks, if specified point is inside <see cref="RectangleD"/>.
        /// </summary>
        /// <param name="x">X point coordinate.</param>
        /// <param name="y">Y point coordinate.</param>
        /// <returns><c>true</c> if point is inside <see cref="RectangleD"/>, otherwise <c>false</c>.</returns>
        public bool Contains(double x, double y)
        {
            return (x >= _left && x <= _right && y >= _top && y <= _bottom);
        }

        /// <summary>
        /// Checks, if specified <see cref="DVector2"/> is inside <see cref="RectangleD"/>.
        /// </summary>
        /// <param name="DVector2D">Coordinate <see cref="DVector2"/>.</param>
        /// <returns><c>true</c> if <see cref="DVector2"/> is inside <see cref="RectangleD"/>, otherwise <c>false</c>.</returns>
        public bool Contains(DVector2 DVector2D)
        {
            return Contains(DVector2D.X, DVector2D.Y);
        }

        /// <summary>
        /// Checks, if specified <see cref="Point"/> is inside <see cref="RectangleD"/>.
        /// </summary>
        /// <param name="point">Coordinate <see cref="Point"/>.</param>
        /// <returns><c>true</c> if <see cref="Point"/> is inside <see cref="RectangleD"/>, otherwise <c>false</c>.</returns>
        public bool Contains(Point point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>Determines whether a specified rectangle intersects with this rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public bool Intersects(RectangleD value)
        {
            bool result;
            Intersects(ref value, out result);
            return result;
        }

        /// <summary>
        /// Determines whether a specified rectangle intersects with this rectangle.
        /// </summary>
        /// <param name="value">The rectangle to evaluate</param>
        /// <param name="result">[OutAttribute] true if the specified rectangle intersects with this one; false otherwise.</param>
        public void Intersects(ref RectangleD value, out bool result)
        {
            result = (value.X < Right) && (X < value.Right) && (value.Y < Bottom) && (Y < value.Bottom);
        }

        /// <summary>
        /// Creates a rectangle defining the area where one rectangle overlaps with another rectangle.
        /// </summary>
        /// <param name="value1">The first Rectangle to compare.</param>
        /// <param name="value2">The second Rectangle to compare.</param>
        /// <returns>The intersection rectangle.</returns>
        public static RectangleD Intersect(RectangleD value1, RectangleD value2)
        {
            RectangleD result;
            Intersect(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>Creates a rectangle defining the area where one rectangle overlaps with another rectangle.</summary>
        /// <param name="value1">The first rectangle to compare.</param>
        /// <param name="value2">The second rectangle to compare.</param>
        /// <param name="result">[OutAttribute] The area where the two first parameters overlap.</param>
        public static void Intersect(ref RectangleD value1, ref RectangleD value2, out RectangleD result)
        {
            double newLeft = (value1.X > value2.X) ? value1.X : value2.X;
            double newTop = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            double newRight = (value1.Right < value2.Right) ? value1.Right : value2.Right;
            double newBottom = (value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom;
            if ((newRight > newLeft) && (newBottom > newTop))
            {
                result = new RectangleD(newLeft, newTop, newRight - newLeft, newBottom - newTop);
            }
            else
            {
                result = Empty;
            }
        }

        /// <summary>
        /// Creates a new rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first rectangle to contain.</param>
        /// <param name="value2">The second rectangle to contain.</param>
        /// <returns>The union rectangle.</returns>
        public static RectangleD Union(RectangleD value1, RectangleD value2)
        {
            RectangleD result;
            Union(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Creates a new rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first rectangle to contain.</param>
        /// <param name="value2">The second rectangle to contain.</param>
        /// <param name="result">[OutAttribute] The rectangle that must be the union of the first two rectangles.</param>
        public static void Union(ref RectangleD value1, ref RectangleD value2, out RectangleD result)
        {
            var left = Math.Min(value1.Left, value2.Left);
            var right = Math.Max(value1.Right, value2.Right);
            var top = Math.Min(value1.Top, value2.Top);
            var bottom = Math.Max(value1.Bottom, value2.Bottom);
            result = new RectangleD(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(RectangleD)) return false;
            return Equals((RectangleD)obj);
        }

        /// <inheritdoc/>
        public bool Equals(RectangleD other)
        {
            return DMathUtil.NearEqual(other.Left, Left) &&
                   DMathUtil.NearEqual(other.Right, Right) &&
                   DMathUtil.NearEqual(other.Top, Top) &&
                   DMathUtil.NearEqual(other.Bottom, Bottom);
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
                int result = _left.GetHashCode();
                result = (result * 397) ^ _top.GetHashCode();
                result = (result * 397) ^ _right.GetHashCode();
                result = (result * 397) ^ _bottom.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X:{0} Y:{1} Width:{2} Height:{3}", X, Y, Width, Height);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(RectangleD left, RectangleD right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(RectangleD left, RectangleD right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Performs an explicit conversion to <see cref="Rectangle"/> structure.
        /// </summary>
        /// <remarks>Performs direct double to int conversion, any fractional data is truncated.</remarks>
        /// <param name="value">The source <see cref="RectangleD"/> value.</param>
        /// <returns>A converted <see cref="Rectangle"/> structure.</returns>
        public static explicit operator DRectangle(RectangleD value)
        {
            return new DRectangle((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
        }
    }
}
