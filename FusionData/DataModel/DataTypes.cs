using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionData.DataModel
{
    namespace DataTypes
    {
        public struct DVector2
        {
            public double X { get; set; }
            public double Y { get; set; }

            public DVector2(double x, double y)
            {
                X = x;
                Y = y;
            }

            public DVector2 ToRadians()
            {
                return new DVector2(X * Math.PI/ 180, Y * Math.PI / 180);
            }
        }


        public struct Vector2
        {
            public float X { get; set; }
            public float Y { get; set; }


        }

    }
}
