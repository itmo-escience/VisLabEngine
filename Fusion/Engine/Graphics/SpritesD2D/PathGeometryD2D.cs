using SharpDX.Direct2D1;
using Fusion.Core;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class PathGeometryD2D : DisposableBase
    {
        internal readonly PathGeometry PathGeometry;

        internal PathGeometryD2D(PathGeometry pathGeometry)
        {
            PathGeometry = pathGeometry;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            PathGeometry.Dispose();
        }
    }
}
