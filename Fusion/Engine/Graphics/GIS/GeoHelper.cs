﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	public static class GeoHelper
	{
		public static readonly double EarthRadius = 6378.137;
		public static readonly double EarthOneDegreeLengthOnEquatorMeters = 111152.8928;


		public static double GetAzimuth(DVector2 p0, DVector2 p1)
		{
			var y = Math.Sin(p1.Y - p0.Y) * Math.Cos(p1.X);
			var x = Math.Cos(p0.X) * Math.Sin(p1.X) - Math.Sin(p0.X) * Math.Cos(p1.X) * Math.Cos(p1.Y - p0.Y);
			return Math.Atan2(x, y);
		}

		public static DVector2 LonLatDegToWebMercatorMet(double lon, double lat)
		{
			var x = lon * 20037508.34 / 180;
			var y = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
			y = y * 20037508.34 / 180;
			return new DVector2(x, y);
		}

		public static DVector2 WebMercatorMet2LonLatDeg(double x, double y)
		{
			var lon = x * 180 / 20037508.34;
			var lat = Math.Atan(Math.Exp(y * Math.PI / 20037508.34)) * 360 / Math.PI - 90;
			return new DVector2(lon, lat);
		}


		public static bool LineIntersection(DVector3 lineOrigin, DVector3 lineEnd, double radius, out DVector3[] intersectionPoints)
		{
			intersectionPoints = new DVector3[0];

			double sphereRadius = radius;
			DVector3 sphereCenter = DVector3.Zero;

			var lineDir = new DVector3(lineEnd.X - lineOrigin.X, lineEnd.Y - lineOrigin.Y, lineEnd.Z - lineOrigin.Z);
			lineDir.Normalize();

			DVector3 w = lineOrigin - sphereCenter;

			double a = DVector3.Dot(lineDir, lineDir); // 1.0f;
			double b = 2 * DVector3.Dot(lineDir, w);
			double c = DVector3.Dot(w, w) - sphereRadius * sphereRadius;
			double d = b * b - 4.0 * a * c;

			if (d < 0) return false;

			if (d == 0.0) {
				double x1 = (-b - Math.Sqrt(d)) / (2.0 * a);

				intersectionPoints = new DVector3[1];
				intersectionPoints[0] = lineOrigin + lineDir * x1;

				return true;
			}

			if (d > 0.0f) {
				double sqrt = Math.Sqrt(d);

				double x1 = (-b - sqrt) / (2.0 * a);
				double x2 = (-b + sqrt) / (2.0 * a);

				intersectionPoints = new DVector3[2];
				intersectionPoints[0] = lineOrigin + lineDir * x1;
				intersectionPoints[1] = lineOrigin + lineDir * x2;

				return true;
			}

			return false;
		}


		public static void CartesianToSpherical(DVector3 cart, out double lon, out double lat)
		{
			var radius = cart.Length();

			if (radius == 0.0) {
				lon = 0;
				lat = 0;
				return;
			}

			lon = Math.Atan2(cart.X, cart.Z);
			lat = Math.Asin(cart.Y / radius);
		}


		public static DVector2 CartesianToSpherical(DVector3 cart)
		{
			double lon, lat;
			CartesianToSpherical(cart, out lon, out lat);
			return new DVector2(lon, lat);
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="lon"></param>
		/// <param name="lat"></param>
		/// <param name="radius"></param>
		/// <param name="cart"></param>
		public static void SphericalToCartesian(double lon, double lat, double radius, out DVector3 cart)
		{
			cart = DVector3.Zero;

			double x, y, z;
			SphericalToCartesian(lon, lat, radius, out x, out y, out z);

			cart.X = x;
			cart.Y = y;
			cart.Z = z;
		}



		public static void SphericalToCartesian(double lon, double lat, double radius, out double x, out double y, out double z)
		{
			z = (radius * Math.Cos(lat) * Math.Cos(lon));
			x = (radius * Math.Cos(lat) * Math.Sin(lon));
			y = (radius * Math.Sin(lat));
		}


		public static DVector3 SphericalToCartesian(DVector2 lonLat, double radius = 6378.137)
		{
			double x, y, z;
			SphericalToCartesian(lonLat.X, lonLat.Y, radius, out x, out y, out z);
			return new DVector3(x, y, z);
		}


		public static double DistanceBetweenTwoPoints(DVector2 lonLatP0, DVector2 lonLatP1, double earthRadius = 6378.137)
		{
			var phi0 = lonLatP0.Y;
			var phi1 = lonLatP1.Y;
			var deltaPhi = phi1 - phi0;
			var deltaLam = lonLatP1.X - lonLatP0.X;

			var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
					Math.Cos(phi0) * Math.Cos(phi1) *
					Math.Sin(deltaLam / 2) * Math.Sin(deltaLam / 2);
			var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			var d = earthRadius * c;

			return d;
		}

	    public static DVector2 MiddlePoint(DVector2 lonLatP0, DVector2 lonLatP1)
        {
            var p0 = SphericalToCartesian(lonLatP0);
            var p1 = SphericalToCartesian(lonLatP1);

            var p = ((p0 + p1) / 2).Normalized() * EarthRadius;

            return CartesianToSpherical(p);
        }

	    public static List<DVector2> LineBetweenTwoPoints(DVector2 from, DVector2 to, int segmentsNumber = 10)
	    {
	        var u = SphericalToCartesian(from).Normalized();
	        var v = SphericalToCartesian(to).Normalized();
	        var w = DVector3.Cross(DVector3.Cross(u, v), u).Normalized();

	        var len = DistanceBetweenTwoPoints(from, to) / (2 * Math.PI * EarthRadius);

	        var result = new List<DVector2>();
	        for (var i = 0; i < segmentsNumber + 1; i++)
	        {
	            var t = 2 * Math.PI * len * i / segmentsNumber;
	            var p = u * Math.Cos(t) + w * Math.Sin(t);
	            var pEarth = p * EarthRadius;

                result.Add(CartesianToSpherical(pEarth));
	        }

	        return result;
	    }

		public static DVector2 RhumbDestinationPoint(DVector2 startPoint, double bearing, double distance, double radius = 6378.137)
		{
			var δ	= distance / radius; // angular distance in radians
			var φ1	= DMathUtil.DegreesToRadians(startPoint.Y);
			var λ1	= DMathUtil.DegreesToRadians(startPoint.X);
			var θ	= DMathUtil.DegreesToRadians(bearing);

			var Δφ = δ * Math.Cos(θ);
			var φ2 = φ1 + Δφ;

			// check for some daft bugger going past the pole, normalise latitude if so
			if (Math.Abs(φ2) > Math.PI / 2) φ2 = φ2 > 0 ? Math.PI - φ2 : -Math.PI - φ2;

			var Δψ = Math.Log(Math.Tan(φ2 / 2 + Math.PI / 4) / Math.Tan(φ1 / 2 + Math.PI / 4));
			var q = Math.Abs(Δψ) > 10e-12 ? Δφ / Δψ : Math.Cos(φ1); // E-W course becomes ill-conditioned with 0/0

			var Δλ = δ * Math.Sin(θ) / q;
			var λ2 = λ1 + Δλ;



			return new DVector2((DMathUtil.RadiansToDegrees(λ2) + 540) % 360 - 180, DMathUtil.RadiansToDegrees(φ2)); // normalise to −180…+180°
		}



		public static DMatrix CalculateBasisOnSurface(DVector2 lonLatRad, bool includeTranslation = false)
		{
			var translation = SphericalToCartesian(lonLatRad, EarthRadius);

			DVector3 up = translation;
			up.Normalize();

			var xAxis = DVector3.TransformNormal(DVector3.UnitX, DMatrix.RotationAxis(DVector3.UnitY, lonLatRad.X));
			xAxis.Normalize();

			var mat = DMatrix.Identity;
			mat.Up = up;
			mat.Right = xAxis;
			mat.Forward = DVector3.Cross(xAxis, up);
			mat.Forward.Normalize();

			if (includeTranslation) mat.TranslationVector = translation;

			return mat;
		}


		public static bool IsPointInPolygon(DVector2[] simPoly, DVector2 point)
		{
			int j = simPoly.Length - 1;
			bool oddNodes = false;

			for (int i = 0; i < simPoly.Length; i++) {
				if ((simPoly[i].Y < point.Y && simPoly[j].Y >= point.Y || simPoly[j].Y < point.Y && simPoly[i].Y >= point.Y) &&
					(simPoly[i].X <= point.X || simPoly[j].X <= point.X))
				{
					oddNodes ^= (simPoly[i].X + (point.Y - simPoly[i].Y) / (simPoly[j].Y - simPoly[i].Y) * (simPoly[j].X - simPoly[i].X) < point.X);
				}
				j = i;
			}

			return oddNodes;
		}


		public static int GetTimeIndex(DateTime curTime, List<DateTime> dates)
		{
			var index = dates.BinarySearch(curTime);

			var timesCount = dates.Count;

			if(index < 0) {
				if(curTime < dates.First()) index = 0;
				else
				if(curTime > dates.Last()) index = timesCount - 1;
				else
					index = Math.Abs(index + 2);
			}

			if(index < timesCount) return index;
			index = timesCount - 1;
			return index;
		}


		public static int GetIndex<T>(T curTime, List<T> dates) where T : IComparable
		{
			var index = dates.BinarySearch(curTime);

			var timesCount = dates.Count;

			if (index < 0) {
				if (curTime.CompareTo(dates.First()) <= 0) index = 0;
				else
				if (curTime.CompareTo(dates.Last()) >= 0) index = timesCount - 1;
				else
					index = Math.Abs(index + 2);
			}

			if (index < timesCount) return index;
			index = timesCount - 1;
			return index;
		}


		public static double DistanceBetweenPointAndLine(DVector3 point, DVector3 line0, DVector3 line1)
		{
			return DVector3.Cross(point - line0, point - line1).Length()/(line1 - line0).Length();
		}


		public static void UtmToLatLon(double utmX, double utmY, string utmZone, out double longitude, out double latitude)
		{
			bool isNorthHemisphere = utmZone.Last() >= 'N';

			var diflat = -0.00066286966871111111111111111111111111;
			var diflon = -0.0003868060578;

			var zone = int.Parse(utmZone.Remove(utmZone.Length - 1));
			var c_sa = 6378137.000000;
			var c_sb = 6356752.314245;

			var e2 = Math.Pow(((c_sa * c_sa) - (c_sb * c_sb)), 0.5) / c_sb;
			var e2cuadrada = (e2 * e2);
			var c = (c_sa * c_sa) / c_sb;

			var x = utmX - 500000;
			var y = isNorthHemisphere ? utmY : utmY - 10000000;

			var s = ((zone * 6.0) - 183.0);
			var lat = y / (c_sa * 0.9996);

			var latCos		= Math.Cos(lat);
			var latCosSqr	= latCos*latCos;
			var v = (c / Math.Pow(1 + (e2cuadrada * latCosSqr), 0.5)) * 0.9996;

			var a = x / v;
			var a1 = Math.Sin(2 * lat);
			var a2 = a1 * Math.Pow((latCos), 2);

			var j2 = lat + (a1 / 2.0);
			var j4 = ((3 * j2) + a2) / 4.0;
			var j6 = ((5 * j4) + Math.Pow(a2 * latCos, 2)) / 3.0;

			var alfa = (3.0 / 4.0) * e2cuadrada;
			var beta = (5.0 / 3.0) * (alfa * alfa);
			var gama = (35.0 / 27.0) * (alfa * alfa * alfa);
			var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
			var b = (y - bm) / v;

			var epsi = ((e2cuadrada * (a*a)) / 2.0) * latCosSqr;
			var eps = a * (1 - (epsi / 3.0));

			var nab = (b * (1 - epsi)) + lat;
			var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
			var delt = Math.Atan(senoheps / (Math.Cos(nab)));
			var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

			longitude	= ((delt * (180.0 / Math.PI)) + s) + diflon;
			latitude	= ((lat + (1 + e2cuadrada * latCosSqr - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * latCos * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
		}

//		double EPSILON = 0.000001

//int triangle_intersection( DVector3   V1,  // Triangle vertices
//						   DVector3   V2,
//						   DVector3   V3,
//						   DVector3    O,  //Ray origin
//						   DVector3    D,  //Ray direction
//								 out float len )
//{
//  DVector3 e1, e2;  //Edge1, Edge2
//  DVector3 P, Q, T;
//  float det, inv_det, u, v;
//  float t;

//  //Find vectors for two edges sharing V1
//  SUB(e1, V2, V1);
//  SUB(e2, V3, V1);
//  //Begin calculating determinant - also used to calculate u parameter
//  CROSS(P, D, e2);
//  //if determinant is near zero, ray lies in plane of triangle
//  det = DOT(e1, P);
//  //NOT CULLING
//  if(det > -EPSILON && det < EPSILON) return 0;
//  inv_det = 1.f / det;

//  //calculate distance from V1 to ray origin
//  SUB(T, O, V1);

//  //Calculate u parameter and test bound
//  u = DOT(T, P) * inv_det;
//  //The intersection lies outside of the triangle
//  if(u < 0.f || u > 1.f) return 0;

//  //Prepare to test v parameter
//  CROSS(Q, T, e1);

//  //Calculate V parameter and test bound
//  v = DOT(D, Q) * inv_det;
//  //The intersection lies outside of the triangle
//  if(v < 0.f || u + v  > 1.f) return 0;

//  t = DOT(e2, Q) * inv_det;

//  if(t > EPSILON) { //ray intersection
//	*out = t;
//	return 1;
//  }

//  // No hit, no win
//  return 0;
//}
	}
}
