using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class PathGeometryD2D
    {
        internal readonly PathGeometry PathGeometry;

        internal PathGeometryD2D(PathGeometry pathGeometry)
        {
            PathGeometry = pathGeometry;
        }
    }
}
