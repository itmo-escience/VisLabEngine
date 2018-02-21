using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Fusion.Engine.Graphics.GIS.GlobeMath
{
    /*
     * This class is organized so that the least complex objects come first so that the least
     * complex objects will have the most methods in most cases. Note that not all shapes exist
     * at this time and not all shapes have a corresponding struct. Only the objects that have
     * a corresponding struct should come first in naming and in parameter order. The order of
     * complexity is as follows:
     * 
     * 1. Point
     * 2. DRay
     * 3. Segment
     * 4. DPlane
     * 5. Triangle
     * 6. Polygon
     * 7. Box
     * 8. Sphere
     * 9. Ellipsoid
     * 10. Cylinder
     * 11. Cone
     * 12. Capsule
     * 13. Torus
     * 14. Polyhedron
     * 15. Frustum
    */

    /// <summary>
    /// Contains static methods to help in determining intersections, containment, etc.
    /// </summary>
    public static class DCollision
    {
        /// <summary>
        /// Determines the closest point between a point and a triangle.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="vertex1">The first vertex to test.</param>
        /// <param name="vertex2">The second vertex to test.</param>
        /// <param name="vertex3">The third vertex to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointPointTriangle(ref DVector3 point, ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3, out DVector3 result)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 136

            //Check if P in vertex region outside A
            DVector3 ab = vertex2 - vertex1;
            DVector3 ac = vertex3 - vertex1;
            DVector3 ap = point - vertex1;

            double d1 = DVector3.Dot(ab, ap);
            double d2 = DVector3.Dot(ac, ap);
            if (d1 <= 0.0d && d2 <= 0.0d)
                result = vertex1; //Barycentric coordinates (1,0,0)

            //Check if P in vertex region outside B
            DVector3 bp = point - vertex2;
            double d3 = DVector3.Dot(ab, bp);
            double d4 = DVector3.Dot(ac, bp);
            if (d3 >= 0.0d && d4 <= d3)
                result = vertex2; // Barycentric coordinates (0,1,0)

            //Check if P in edge region of AB, if so return projection of P onto AB
            double vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0d && d1 >= 0.0d && d3 <= 0.0d)
            {
                double v = d1 / (d1 - d3);
                result = vertex1 + v * ab; //Barycentric coordinates (1-v,v,0)
            }

            //Check if P in vertex region outside C
            DVector3 cp = point - vertex3;
            double d5 = DVector3.Dot(ab, cp);
            double d6 = DVector3.Dot(ac, cp);
            if (d6 >= 0.0d && d5 <= d6)
                result = vertex3; //Barycentric coordinates (0,0,1)

            //Check if P in edge region of AC, if so return projection of P onto AC
            double vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0d && d2 >= 0.0d && d6 <= 0.0d)
            {
                double w = d2 / (d2 - d6);
                result = vertex1 + w * ac; //Barycentric coordinates (1-w,0,w)
            }

            //Check if P in edge region of BC, if so return projection of P onto BC
            double va = d3 * d6 - d5 * d4;
            if (va <= 0.0d && (d4 - d3) >= 0.0d && (d5 - d6) >= 0.0d)
            {
                double w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                result = vertex2 + w * (vertex3 - vertex2); //Barycentric coordinates (0,1-w,w)
            }

            //P inside face region. Compute Q through its Barycentric coordinates (u,v,w)
            double denom = 1.0 / (va + vb + vc);
            double v2 = vb * denom;
            double w2 = vc * denom;
            result = vertex1 + ab * v2 + ac * w2; //= u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0 - v - w
        }

        /// <summary>
        /// Determines the closest point between a <see cref="DPlane"/> and a point.
        /// </summary>
        /// <param name="DPlane">The DPlane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointDPlanePoint(ref DPlane DPlane, ref DVector3 point, out DVector3 result)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 126

            double dot;
            DVector3.Dot(ref DPlane.Normal, ref point, out dot);
            double t = dot - DPlane.D;

            result = point - (t * DPlane.Normal);
        }

        /// <summary>
        /// Determines the closest point between a <see cref="DBoundingBox"/> and a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointBoxPoint(ref DBoundingBox box, ref DVector3 point, out DVector3 result)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 130

            DVector3 temp;
            DVector3.Max(ref point, ref box.Minimum, out temp);
            DVector3.Min(ref temp, ref box.Maximum, out result);
        }

        /// <summary>
        /// Determines the closest point between a <see cref="DBoundingSphere"/> and a point.
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects;
        /// or, if the point is directly in the center of the sphere, contains <see cref="DVector3.Zero"/>.</param>
        public static void ClosestPointSpherePoint(ref DBoundingSphere sphere, ref DVector3 point, out DVector3 result)
        {
            //Source: Jorgy343
            //Reference: None

            //Get the unit direction from the sphere's center to the point.
            DVector3.Subtract(ref point, ref sphere.Center, out result);
            result.Normalize();

            //Multiply the unit direction by the sphere's radius to get a vector
            //the length of the sphere.
            result *= sphere.Radius;

            //Add the sphere's center to the direction to get a point on the sphere.
            result += sphere.Center;
        }

        /// <summary>
        /// Determines the closest point between a <see cref="DBoundingSphere"/> and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects;
        /// or, if the point is directly in the center of the sphere, contains <see cref="DVector3.Zero"/>.</param>
        /// <remarks>
        /// If the two spheres are overlapping, but not directly on top of each other, the closest point
        /// is the 'closest' point of intersection. This can also be considered is the deepest point of
        /// intersection.
        /// </remarks>
        public static void ClosestPointSphereSphere(ref DBoundingSphere sphere1, ref DBoundingSphere sphere2, out DVector3 result)
        {
            //Source: Jorgy343
            //Reference: None

            //Get the unit direction from the first sphere's center to the second sphere's center.
            DVector3.Subtract(ref sphere2.Center, ref sphere1.Center, out result);
            result.Normalize();

            //Multiply the unit direction by the first sphere's radius to get a vector
            //the length of the first sphere.
            result *= sphere1.Radius;

            //Add the first sphere's center to the direction to get a point on the first sphere.
            result += sphere1.Center;
        }

        /// <summary>
        /// Determines the distance between a <see cref="DPlane"/> and a point.
        /// </summary>
        /// <param name="DPlane">The DPlane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static double DistancePlanePoint(ref DPlane DPlane, ref DVector3 point)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 127

            double dot;
            DVector3.Dot(ref DPlane.Normal, ref point, out dot);
            return dot - DPlane.D;
        }

        /// <summary>
        /// Determines the distance between a <see cref="DBoundingBox"/> and a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static double DistanceBoxPoint(ref DBoundingBox box, ref DVector3 point)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 131

            double distance = 0d;

            if (point.X < box.Minimum.X)
                distance += (box.Minimum.X - point.X) * (box.Minimum.X - point.X);
            if (point.X > box.Maximum.X)
                distance += (point.X - box.Maximum.X) * (point.X - box.Maximum.X);

            if (point.Y < box.Minimum.Y)
                distance += (box.Minimum.Y - point.Y) * (box.Minimum.Y - point.Y);
            if (point.Y > box.Maximum.Y)
                distance += (point.Y - box.Maximum.Y) * (point.Y - box.Maximum.Y);

            if (point.Z < box.Minimum.Z)
                distance += (box.Minimum.Z - point.Z) * (box.Minimum.Z - point.Z);
            if (point.Z > box.Maximum.Z)
                distance += (point.Z - box.Maximum.Z) * (point.Z - box.Maximum.Z);

            return (double)Math.Sqrt(distance);
        }

        /// <summary>
        /// Determines the distance between a <see cref="DBoundingBox"/> and a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static double DistanceBoxBox(ref DBoundingBox box1, ref DBoundingBox box2)
        {
            //Source:
            //Reference:

            double distance = 0d;

            //Distance for X.
            if (box1.Minimum.X > box2.Maximum.X)
            {
                double delta = box2.Maximum.X - box1.Minimum.X;
                distance += delta * delta;
            }
            else if (box2.Minimum.X > box1.Maximum.X)
            {
                double delta = box1.Maximum.X - box2.Minimum.X;
                distance += delta * delta;
            }

            //Distance for Y.
            if (box1.Minimum.Y > box2.Maximum.Y)
            {
                double delta = box2.Maximum.Y - box1.Minimum.Y;
                distance += delta * delta;
            }
            else if (box2.Minimum.Y > box1.Maximum.Y)
            {
                double delta = box1.Maximum.Y - box2.Minimum.Y;
                distance += delta * delta;
            }

            //Distance for Z.
            if (box1.Minimum.Z > box2.Maximum.Z)
            {
                double delta = box2.Maximum.Z - box1.Minimum.Z;
                distance += delta * delta;
            }
            else if (box2.Minimum.Z > box1.Maximum.Z)
            {
                double delta = box1.Maximum.Z - box2.Minimum.Z;
                distance += delta * delta;
            }

            return (double)Math.Sqrt(distance);
        }

        /// <summary>
        /// Determines the distance between a <see cref="DBoundingSphere"/> and a point.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static double DistanceSpherePoint(ref DBoundingSphere sphere, ref DVector3 point)
        {
            //Source: Jorgy343
            //Reference: None

            double distance;
            DVector3.Distance(ref sphere.Center, ref point, out distance);
            distance -= sphere.Radius;

            return Math.Max(distance, 0d);
        }

        /// <summary>
        /// Determines the distance between a <see cref="DBoundingSphere"/> and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static double DistanceSphereSphere(ref DBoundingSphere sphere1, ref DBoundingSphere sphere2)
        {
            //Source: Jorgy343
            //Reference: None

            double distance;
            DVector3.Distance(ref sphere1.Center, ref sphere2.Center, out distance);
            distance -= sphere1.Radius + sphere2.Radius;

            return Math.Max(distance, 0d);
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DRay"/> and a point.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersect.</returns>
        public static bool RayIntersectsPoint(ref DRay DRay, ref DVector3 point)
        {
            //Source: DRayIntersectsSphere
            //Reference: None

            DVector3 m;
            DVector3.Subtract(ref DRay.Position, ref point, out m);

            //Same thing as DRayIntersectsSphere except that the radius of the sphere (point)
            //is the epsilon for zero.
            double b = DVector3.Dot(m, DRay.Direction);
            double c = DVector3.Dot(m, m) - DMathUtil.ZeroTolerance;

            if (c > 0d && b > 0d)
                return false;

            double discriminant = b * b - c;

            if (discriminant < 0d)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DRay"/> and a <see cref="DRay"/>.
        /// </summary>
        /// <param name="DRay1">The first DRay to test.</param>
        /// <param name="DRay2">The second DRay to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="DVector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersect.</returns>
        /// <remarks>
        /// This method performs a DRay vs DRay intersection test based on the following formula
        /// from Goldman.
        /// <code>s = det([o_2 - o_1, d_2, d_1 x d_2]) / ||d_1 x d_2||^2</code>
        /// <code>t = det([o_2 - o_1, d_1, d_1 x d_2]) / ||d_1 x d_2||^2</code>
        /// Where o_1 is the position of the first DRay, o_2 is the position of the second DRay,
        /// d_1 is the normalized direction of the first DRay, d_2 is the normalized direction
        /// of the second DRay, det denotes the determinant of a matrix, x denotes the cross
        /// product, [ ] denotes a matrix, and || || denotes the length or magnitude of a vector.
        /// </remarks>
        public static bool RayIntersectsRay(ref DRay DRay1, ref DRay DRay2, out DVector3 point)
        {
            //Source: Real-Time Rendering, Third Edition
            //Reference: Page 780

            DVector3 cross;

            DVector3.Cross(ref DRay1.Direction, ref DRay2.Direction, out cross);
            double denominator = cross.Length();

            //Lines are parallel.
            if (DMathUtil.IsZero(denominator))
            {
                //Lines are parallel and on top of each other.
                if (DMathUtil.NearEqual(DRay2.Position.X, DRay1.Position.X) &&
                    DMathUtil.NearEqual(DRay2.Position.Y, DRay1.Position.Y) &&
                    DMathUtil.NearEqual(DRay2.Position.Z, DRay1.Position.Z))
                {
                    point = DVector3.Zero;
                    return true;
                }
            }

            denominator = denominator * denominator;

            //3x3 matrix for the first DRay.
            double m11 = DRay2.Position.X - DRay1.Position.X;
            double m12 = DRay2.Position.Y - DRay1.Position.Y;
            double m13 = DRay2.Position.Z - DRay1.Position.Z;
            double m21 = DRay2.Direction.X;
            double m22 = DRay2.Direction.Y;
            double m23 = DRay2.Direction.Z;
            double m31 = cross.X;
            double m32 = cross.Y;
            double m33 = cross.Z;

            //Determinant of first matrix.
            double dets =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            //3x3 matrix for the second DRay.
            m21 = DRay1.Direction.X;
            m22 = DRay1.Direction.Y;
            m23 = DRay1.Direction.Z;

            //Determinant of the second matrix.
            double dett =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            //t values of the point of intersection.
            double s = dets / denominator;
            double t = dett / denominator;

            //The points of intersection.
            DVector3 point1 = DRay1.Position + (s * DRay1.Direction);
            DVector3 point2 = DRay2.Position + (t * DRay2.Direction);

            //If the points are not equal, no intersection has occurred.
            if (!DMathUtil.NearEqual(point2.X, point1.X) ||
                !DMathUtil.NearEqual(point2.Y, point1.Y) ||
                !DMathUtil.NearEqual(point2.Z, point1.Z))
            {
                point = DVector3.Zero;
                return false;
            }

            point = point1;
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DRay"/> and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <param name="DPlane">The DPlane to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersect.</returns>
        public static bool RayIntersectsPlane(ref DRay DRay, ref DPlane DPlane, out double distance)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 175

            double direction;
            DVector3.Dot(ref DPlane.Normal, ref DRay.Direction, out direction);

            if (DMathUtil.IsZero(direction))
            {
                distance = 0d;
                return false;
            }

            double position;
            DVector3.Dot(ref DPlane.Normal, ref DRay.Position, out position);
            distance = (-DPlane.D - position) / direction;

            if (distance < 0d)
            {
                distance = 0d;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DRay"/> and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <param name="DPlane">The DPlane to test</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="DVector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsPlane(ref DRay DRay, ref DPlane DPlane, out DVector3 point)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 175

            double distance;
            if (!RayIntersectsPlane(ref DRay, ref DPlane, out distance))
            {
                point = DVector3.Zero;
                return false;
            }

            point = DRay.Position + (DRay.Direction * distance);
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DRay"/> and a triangle.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        /// <remarks>
        /// This method tests if the DRay intersects either the front or back of the triangle.
        /// If the DRay is parallel to the triangle's DPlane, no intersection is assumed to have
        /// happened. If the intersection of the DRay and the triangle is behind the origin of
        /// the DRay, no intersection is assumed to have happened. In both cases of assumptions,
        /// this method returns false.
        /// </remarks>
        public static bool RayIntersectsTriangle(ref DRay DRay, ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3, out double distance)
        {
            //Source: Fast Minimum Storage DRay / Triangle Intersection
            //Reference: http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20DRayTriangle%20Intersection.pdf

            //Compute vectors along two edges of the triangle.
            DVector3 edge1, edge2;

            //Edge 1
            edge1.X = vertex2.X - vertex1.X;
            edge1.Y = vertex2.Y - vertex1.Y;
            edge1.Z = vertex2.Z - vertex1.Z;

            //Edge2
            edge2.X = vertex3.X - vertex1.X;
            edge2.Y = vertex3.Y - vertex1.Y;
            edge2.Z = vertex3.Z - vertex1.Z;

            //Cross product of DRay direction and edge2 - first part of determinant.
            DVector3 directioncrossedge2;
            directioncrossedge2.X = (DRay.Direction.Y * edge2.Z) - (DRay.Direction.Z * edge2.Y);
            directioncrossedge2.Y = (DRay.Direction.Z * edge2.X) - (DRay.Direction.X * edge2.Z);
            directioncrossedge2.Z = (DRay.Direction.X * edge2.Y) - (DRay.Direction.Y * edge2.X);

            //Compute the determinant.
            double determinant;
            //Dot product of edge1 and the first part of determinant.
            determinant = (edge1.X * directioncrossedge2.X) + (edge1.Y * directioncrossedge2.Y) + (edge1.Z * directioncrossedge2.Z);

            //If the DRay is parallel to the triangle DPlane, there is no DCollision.
            //This also means that we are not culling, the DRay may hit both the
            //back and the front of the triangle.
            if (DMathUtil.IsZero(determinant))
            {
                distance = 0d;
                return false;
            }

            double inversedeterminant = 1.0d / determinant;

            //Calculate the U parameter of the intersection point.
            DVector3 distanceVector;
            distanceVector.X = DRay.Position.X - vertex1.X;
            distanceVector.Y = DRay.Position.Y - vertex1.Y;
            distanceVector.Z = DRay.Position.Z - vertex1.Z;

            double triangleU;
            triangleU = (distanceVector.X * directioncrossedge2.X) + (distanceVector.Y * directioncrossedge2.Y) + (distanceVector.Z * directioncrossedge2.Z);
            triangleU *= inversedeterminant;

            //Make sure it is inside the triangle.
            if (triangleU < 0d || triangleU > 1d)
            {
                distance = 0d;
                return false;
            }

            //Calculate the V parameter of the intersection point.
            DVector3 distancecrossedge1;
            distancecrossedge1.X = (distanceVector.Y * edge1.Z) - (distanceVector.Z * edge1.Y);
            distancecrossedge1.Y = (distanceVector.Z * edge1.X) - (distanceVector.X * edge1.Z);
            distancecrossedge1.Z = (distanceVector.X * edge1.Y) - (distanceVector.Y * edge1.X);

            double triangleV;
            triangleV = ((DRay.Direction.X * distancecrossedge1.X) + (DRay.Direction.Y * distancecrossedge1.Y)) + (DRay.Direction.Z * distancecrossedge1.Z);
            triangleV *= inversedeterminant;

            //Make sure it is inside the triangle.
            if (triangleV < 0d || triangleU + triangleV > 1d)
            {
                distance = 0d;
                return false;
            }

            //Compute the distance along the DRay to the triangle.
            double DRaydistance;
            DRaydistance = (edge2.X * distancecrossedge1.X) + (edge2.Y * distancecrossedge1.Y) + (edge2.Z * distancecrossedge1.Z);
            DRaydistance *= inversedeterminant;

            //Is the triangle behind the DRay origin?
            if (DRaydistance < 0d)
            {
                distance = 0d;
                return false;
            }

            distance = DRaydistance;
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DRay"/> and a triangle.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="DVector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref DRay DRay, ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3, out DVector3 point)
        {
            double distance;
            if (!RayIntersectsTriangle(ref DRay, ref vertex1, ref vertex2, ref vertex3, out distance))
            {
                point = DVector3.Zero;
                return false;
            }

            point = DRay.Position + (DRay.Direction * distance);
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DRay"/> and a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref DRay DRay, ref DBoundingBox box, out double distance)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 179

            distance = 0f;
            double tmax = double.MaxValue;

            if (DMathUtil.IsZero(DRay.Direction.X))
            {
                if (DRay.Position.X < box.Minimum.X || DRay.Position.X > box.Maximum.X)
                {
                    distance = 0d;
                    return false;
                }
            }
            else
            {
                double inverse = 1.0d / DRay.Direction.X;
                double t1 = (box.Minimum.X - DRay.Position.X) * inverse;
                double t2 = (box.Maximum.X - DRay.Position.X) * inverse;

                if (t1 > t2)
                {
                    double temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0d;
                    return false;
                }
            }

            if (DMathUtil.IsZero(DRay.Direction.Y))
            {
                if (DRay.Position.Y < box.Minimum.Y || DRay.Position.Y > box.Maximum.Y)
                {
                    distance = 0d;
                    return false;
                }
            }
            else
            {
                double inverse = 1.0d / DRay.Direction.Y;
                double t1 = (box.Minimum.Y - DRay.Position.Y) * inverse;
                double t2 = (box.Maximum.Y - DRay.Position.Y) * inverse;

                if (t1 > t2)
                {
                    double temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0d;
                    return false;
                }
            }

            if (DMathUtil.IsZero(DRay.Direction.Z))
            {
                if (DRay.Position.Z < box.Minimum.Z || DRay.Position.Z > box.Maximum.Z)
                {
                    distance = 0d;
                    return false;
                }
            }
            else
            {
                double inverse = 1.0d / DRay.Direction.Z;
                double t1 = (box.Minimum.Z - DRay.Position.Z) * inverse;
                double t2 = (box.Maximum.Z - DRay.Position.Z) * inverse;

                if (t1 > t2)
                {
                    double temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0d;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DRay"/> and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="DVector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref DRay DRay, ref DBoundingBox box, out DVector3 point)
        {
            double distance;
            if (!RayIntersectsBox(ref DRay, ref box, out distance))
            {
                point = DVector3.Zero;
                return false;
            }

            point = DRay.Position + (DRay.Direction * distance);
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DRay"/> and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref DRay DRay, ref DBoundingSphere sphere, out double distance)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 177

            DVector3 m;
            DVector3.Subtract(ref DRay.Position, ref sphere.Center, out m);

            double b = DVector3.Dot(m, DRay.Direction);
            double c = DVector3.Dot(m, m) - (sphere.Radius * sphere.Radius);

            if (c > 0d && b > 0d)
            {
                distance = 0f;
                return false;
            }

            double discriminant = b * b - c;

            if (discriminant < 0d)
            {
                distance = 0d;
                return false;
            }

            distance = -b - (double)Math.Sqrt(discriminant);

            if (distance < 0d)
                distance = 0d;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DRay"/> and a <see cref="DBoundingSphere"/>. 
        /// </summary>
        /// <param name="DRay">The DRay to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="DVector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref DRay DRay, ref DBoundingSphere sphere, out DVector3 point)
        {
            double distance;
            if (!RayIntersectsSphere(ref DRay, ref sphere, out distance))
            {
                point = DVector3.Zero;
                return false;
            }

            point = DRay.Position + (DRay.Direction * distance);
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DPlane"/> and a point.
        /// </summary>
        /// <param name="DPlane">The DPlane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsPoint(ref DPlane DPlane, ref DVector3 point)
        {
            double distance;
            DVector3.Dot(ref DPlane.Normal, ref point, out distance);
            distance += DPlane.D;

            if (distance > 0.0)
                return PlaneIntersectionType.Front;

            if (distance < 0.0)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DPlane"/> and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="DPlane1">The first DPlane to test.</param>
        /// <param name="DPlane2">The second DPlane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool PlaneIntersectsPlane(ref DPlane DPlane1, ref DPlane DPlane2)
        {
            DVector3 direction;
            DVector3.Cross(ref DPlane1.Normal, ref DPlane2.Normal, out direction);

            //If direction is the zero vector, the DPlanes are parallel and possibly
            //coincident. It is not an intersection. The dot product will tell us.
            double denominator;
            DVector3.Dot(ref direction, ref direction, out denominator);

            if (DMathUtil.IsZero(denominator))
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DPlane"/> and a <see cref="DPlane"/>.
        /// </summary>
        /// <param name="DPlane1">The first DPlane to test.</param>
        /// <param name="DPlane2">The second DPlane to test.</param>
        /// <param name="line">When the method completes, contains the line of intersection
        /// as a <see cref="DRay"/>, or a zero DRay if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        /// <remarks>
        /// Although a DRay is set to have an origin, the DRay returned by this method is really
        /// a line in three dimensions which has no real origin. The DRay is considered valid when
        /// both the positive direction is used and when the negative direction is used.
        /// </remarks>
        public static bool PlaneIntersectsPlane(ref DPlane DPlane1, ref DPlane DPlane2, out DRay line)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 207

            DVector3 direction;
            DVector3.Cross(ref DPlane1.Normal, ref DPlane2.Normal, out direction);

            //If direction is the zero vector, the DPlanes are parallel and possibly
            //coincident. It is not an intersection. The dot product will tell us.
            double denominator;
            DVector3.Dot(ref direction, ref direction, out denominator);

            //We assume the DPlanes are normalized, therefore the denominator
            //only serves as a parallel and coincident check. Otherwise we need
            //to divide the point by the denominator.
            if (DMathUtil.IsZero(denominator))
            {
                line = new DRay();
                return false;
            }

            DVector3 point;
            DVector3 temp = DPlane1.D * DPlane2.Normal - DPlane2.D * DPlane1.Normal;
            DVector3.Cross(ref temp, ref direction, out point);

            line.Position = point;
            line.Direction = direction;
            line.Direction.Normalize();

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DPlane"/> and a triangle.
        /// </summary>
        /// <param name="DPlane">The DPlane to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsTriangle(ref DPlane DPlane, ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 207

            PlaneIntersectionType test1 = PlaneIntersectsPoint(ref DPlane, ref vertex1);
            PlaneIntersectionType test2 = PlaneIntersectsPoint(ref DPlane, ref vertex2);
            PlaneIntersectionType test3 = PlaneIntersectsPoint(ref DPlane, ref vertex3);

            if (test1 == PlaneIntersectionType.Front && test2 == PlaneIntersectionType.Front && test3 == PlaneIntersectionType.Front)
                return PlaneIntersectionType.Front;

            if (test1 == PlaneIntersectionType.Back && test2 == PlaneIntersectionType.Back && test3 == PlaneIntersectionType.Back)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DPlane"/> and a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="DPlane">The DPlane to test.</param>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsBox(ref DPlane DPlane, ref DBoundingBox box)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 161

            DVector3 min;
            DVector3 max;

            max.X = (DPlane.Normal.X >= 0.0) ? box.Minimum.X : box.Maximum.X;
            max.Y = (DPlane.Normal.Y >= 0.0) ? box.Minimum.Y : box.Maximum.Y;
            max.Z = (DPlane.Normal.Z >= 0.0) ? box.Minimum.Z : box.Maximum.Z;
            min.X = (DPlane.Normal.X >= 0.0) ? box.Maximum.X : box.Minimum.X;
            min.Y = (DPlane.Normal.Y >= 0.0) ? box.Maximum.Y : box.Minimum.Y;
            min.Z = (DPlane.Normal.Z >= 0.0) ? box.Maximum.Z : box.Minimum.Z;

            double distance;
            DVector3.Dot(ref DPlane.Normal, ref max, out distance);

            if (distance + DPlane.D > 0.0d)
                return PlaneIntersectionType.Front;

            distance = DVector3.Dot(DPlane.Normal, min);

            if (distance + DPlane.D < 0.0d)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DPlane"/> and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="DPlane">The DPlane to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsSphere(ref DPlane DPlane, ref DBoundingSphere sphere)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 160

            double distance;
            DVector3.Dot(ref DPlane.Normal, ref sphere.Center, out distance);
            distance += DPlane.D;

            if (distance > sphere.Radius)
                return PlaneIntersectionType.Front;

            if (distance < -sphere.Radius)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /* This implementation is wrong
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DBoundingBox"/> and a triangle.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsTriangle(ref DBoundingBox box, ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3)
        {
            if (BoxContainsPoint(ref box, ref vertex1) == ContainmentType.Contains)
                return true;

            if (BoxContainsPoint(ref box, ref vertex2) == ContainmentType.Contains)
                return true;

            if (BoxContainsPoint(ref box, ref vertex3) == ContainmentType.Contains)
                return true;

            return false;
        }
        */

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DBoundingBox"/> and a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsBox(ref DBoundingBox box1, ref DBoundingBox box2)
        {
            if (box1.Minimum.X > box2.Maximum.X || box2.Minimum.X > box1.Maximum.X)
                return false;

            if (box1.Minimum.Y > box2.Maximum.Y || box2.Minimum.Y > box1.Maximum.Y)
                return false;

            if (box1.Minimum.Z > box2.Maximum.Z || box2.Minimum.Z > box1.Maximum.Z)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DBoundingBox"/> and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsSphere(ref DBoundingBox box, ref DBoundingSphere sphere)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 166

            DVector3 vector;
            DVector3.Clamp(ref sphere.Center, ref box.Minimum, ref box.Maximum, out vector);
            double distance = DVector3.DistanceSquared(sphere.Center, vector);

            return distance <= sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DBoundingSphere"/> and a triangle.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool SphereIntersectsTriangle(ref DBoundingSphere sphere, ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3)
        {
            //Source: Real-Time DCollision Detection by Christer Ericson
            //Reference: Page 167

            DVector3 point;
            ClosestPointPointTriangle(ref sphere.Center, ref vertex1, ref vertex2, ref vertex3, out point);
            DVector3 v = point - sphere.Center;

            double dot;
            DVector3.Dot(ref v, ref v, out dot);

            return dot <= sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="DBoundingSphere"/> and a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">First sphere to test.</param>
        /// <param name="sphere2">Second sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool SphereIntersectsSphere(ref DBoundingSphere sphere1, ref DBoundingSphere sphere2)
        {
            double radiisum = sphere1.Radius + sphere2.Radius;
            return DVector3.DistanceSquared(sphere1.Center, sphere2.Center) <= radiisum * radiisum;
        }

        /// <summary>
        /// Determines whether a <see cref="DBoundingBox"/> contains a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsPoint(ref DBoundingBox box, ref DVector3 point)
        {
            if (box.Minimum.X <= point.X && box.Maximum.X >= point.X &&
                box.Minimum.Y <= point.Y && box.Maximum.Y >= point.Y &&
                box.Minimum.Z <= point.Z && box.Maximum.Z >= point.Z)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Disjoint;
        }

        /* This implementation is wrong
        /// <summary>
        /// Determines whether a <see cref="DBoundingBox"/> contains a triangle.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsTriangle(ref DBoundingBox box, ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3)
        {
            ContainmentType test1 = BoxContainsPoint(ref box, ref vertex1);
            ContainmentType test2 = BoxContainsPoint(ref box, ref vertex2);
            ContainmentType test3 = BoxContainsPoint(ref box, ref vertex3);

            if (test1 == ContainmentType.Contains && test2 == ContainmentType.Contains && test3 == ContainmentType.Contains)
                return ContainmentType.Contains;

            if (test1 == ContainmentType.Contains || test2 == ContainmentType.Contains || test3 == ContainmentType.Contains)
                return ContainmentType.Intersects;

            return ContainmentType.Disjoint;
        }
        */

        /// <summary>
        /// Determines whether a <see cref="DBoundingBox"/> contains a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsBox(ref DBoundingBox box1, ref DBoundingBox box2)
        {
            if (box1.Maximum.X < box2.Minimum.X || box1.Minimum.X > box2.Maximum.X)
                return ContainmentType.Disjoint;

            if (box1.Maximum.Y < box2.Minimum.Y || box1.Minimum.Y > box2.Maximum.Y)
                return ContainmentType.Disjoint;

            if (box1.Maximum.Z < box2.Minimum.Z || box1.Minimum.Z > box2.Maximum.Z)
                return ContainmentType.Disjoint;

            if (box1.Minimum.X <= box2.Minimum.X && (box2.Maximum.X <= box1.Maximum.X &&
                box1.Minimum.Y <= box2.Minimum.Y && box2.Maximum.Y <= box1.Maximum.Y) &&
                box1.Minimum.Z <= box2.Minimum.Z && box2.Maximum.Z <= box1.Maximum.Z)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines whether a <see cref="DBoundingBox"/> contains a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsSphere(ref DBoundingBox box, ref DBoundingSphere sphere)
        {
            DVector3 vector;
            DVector3.Clamp(ref sphere.Center, ref box.Minimum, ref box.Maximum, out vector);
            double distance = DVector3.DistanceSquared(sphere.Center, vector);

            if (distance > sphere.Radius * sphere.Radius)
                return ContainmentType.Disjoint;

            if ((((box.Minimum.X + sphere.Radius <= sphere.Center.X) && (sphere.Center.X <= box.Maximum.X - sphere.Radius)) && ((box.Maximum.X - box.Minimum.X > sphere.Radius) &&
                (box.Minimum.Y + sphere.Radius <= sphere.Center.Y))) && (((sphere.Center.Y <= box.Maximum.Y - sphere.Radius) && (box.Maximum.Y - box.Minimum.Y > sphere.Radius)) &&
                (((box.Minimum.Z + sphere.Radius <= sphere.Center.Z) && (sphere.Center.Z <= box.Maximum.Z - sphere.Radius)) && (box.Maximum.X - box.Minimum.X > sphere.Radius))))
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines whether a <see cref="DBoundingSphere"/> contains a point.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsPoint(ref DBoundingSphere sphere, ref DVector3 point)
        {
            if (DVector3.DistanceSquared(point, sphere.Center) <= sphere.Radius * sphere.Radius)
                return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether a <see cref="DBoundingSphere"/> contains a triangle.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsTriangle(ref DBoundingSphere sphere, ref DVector3 vertex1, ref DVector3 vertex2, ref DVector3 vertex3)
        {
            //Source: Jorgy343
            //Reference: None

            ContainmentType test1 = SphereContainsPoint(ref sphere, ref vertex1);
            ContainmentType test2 = SphereContainsPoint(ref sphere, ref vertex2);
            ContainmentType test3 = SphereContainsPoint(ref sphere, ref vertex3);

            if (test1 == ContainmentType.Contains && test2 == ContainmentType.Contains && test3 == ContainmentType.Contains)
                return ContainmentType.Contains;

            if (SphereIntersectsTriangle(ref sphere, ref vertex1, ref vertex2, ref vertex3))
                return ContainmentType.Intersects;

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether a <see cref="DBoundingSphere"/> contains a <see cref="DBoundingBox"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="box">The box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsBox(ref DBoundingSphere sphere, ref DBoundingBox box)
        {
            DVector3 vector;

            if (!BoxIntersectsSphere(ref box, ref sphere))
                return ContainmentType.Disjoint;

            double radiussquared = sphere.Radius * sphere.Radius;
            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines whether a <see cref="DBoundingSphere"/> contains a <see cref="DBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsSphere(ref DBoundingSphere sphere1, ref DBoundingSphere sphere2)
        {
            double distance = DVector3.Distance(sphere1.Center, sphere2.Center);

            if (sphere1.Radius + sphere2.Radius < distance)
                return ContainmentType.Disjoint;

            if (sphere1.Radius - sphere2.Radius < distance)
                return ContainmentType.Intersects;

            return ContainmentType.Contains;
        }
    }
}
