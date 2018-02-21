using System;
using System.Runtime.InteropServices;

namespace Fusion.Engine.Graphics.GIS.GlobeMath
{
    /// <summary>
    /// Frustum camera parameters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DFrustumCameraParams
    {
        /// <summary>
        /// Position of the camera.
        /// </summary>
        public DVector3 Position;

        /// <summary>
        /// Looking at direction of the camera.
        /// </summary>
        public DVector3 LookAtDir;

        /// <summary>
        /// Up direction.
        /// </summary>
        public DVector3 UpDir;

        /// <summary>
        /// Field of view.
        /// </summary>
        public double FOV;

        /// <summary>
        /// Z near distance.
        /// </summary>
        public double ZNear;

        /// <summary>
        /// Z far distance.
        /// </summary>
        public double ZFar;

        /// <summary>
        /// Aspect ratio.
        /// </summary>
        public double AspectRatio;
    }
}