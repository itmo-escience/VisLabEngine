using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using Fusion.Core.Content;
using Fusion.Core.Mathematics;
using SharpDX.Direct3D;
using Native.Dds;
using Native.Wic;
using Fusion.Core;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics
{
    public class Texture2DArray : ShaderResource
    {

        D3D.Texture2D tex2D;
        ColorFormat format;
        int mipCount;

        [ContentLoader(typeof(Texture2DArray))]
        public class Loader : ContentLoader
        {

            public override object Load(ContentManager content, Stream stream, Type requestedType, string assetPath)
            {
                bool srgb = assetPath.ToLowerInvariant().Contains("|srgb");
                return new Texture2D(content.Game.GraphicsDevice, stream, srgb);
            }
        }



        /// <summary>
        /// Creates texture
        /// </summary>
        /// <param name="device"></param>
        public Texture2DArray(GraphicsDevice device, int width, int height, int elementCount, ColorFormat format, bool mips, bool srgb = false) : base(device)
        {
            this.Width = width;
            this.Height = height;
            this.Depth = 1;
            this.format = format;
            this.mipCount = 1;//mips ? ShaderResource.CalculateMipLevels(Width, Height) : 1;

            var texDesc = new Texture2DDescription();
            texDesc.ArraySize = elementCount;                      
            texDesc.BindFlags = BindFlags.ShaderResource;
            texDesc.CpuAccessFlags = CpuAccessFlags.None;
            texDesc.Format = srgb ? MakeSRgb(Converter.Convert(format)) : Converter.Convert(format);
            texDesc.Height = Height;
            texDesc.MipLevels = mipCount;
            texDesc.OptionFlags = ResourceOptionFlags.None;
            texDesc.SampleDescription.Count = 1;
            texDesc.SampleDescription.Quality = 0;
            texDesc.Usage = ResourceUsage.Default;
            texDesc.Width = Width;

            lock (device.DeviceContext)
            {
                tex2D = new D3D.Texture2D(device.Device, texDesc);
                SRV = new ShaderResourceView(device.Device, tex2D);
            }
        }




        /// <summary>
        /// Returns SRgb version of the current resource.
        /// </summary>
        [Obsolete]
        public ShaderResource SRgb
        {
            get
            {
                return this;
            }
        }


        /// <summary>
        /// Returns linear version of the current resource.
        /// </summary>
        [Obsolete]
        public ShaderResource Linear
        {
            get
            {
                return this;
            }
        }



       


        /// <summary>
        /// Disposes
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SafeDispose(ref tex2D);
                SafeDispose(ref SRV);
                //SafeDispose( ref srgbResource );
                //SafeDispose( ref linearResource );
            }
            base.Dispose(disposing);
        }



        /// <summary>
        /// Sets 2D texture data, specifying a start index, and number of elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="elementCount"></param>
        public void SetData<T>(T[] data, int startIndex, int elementCount, int sliceIndex) where T : struct
        {
            this.SetData(0, null, data, sliceIndex, startIndex, elementCount);
        }



        /// <summary>
        /// Sets 2D texture data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void SetData<T>(T[] data, int sliceIndex) where T : struct
        {
            this.SetData(0, null, data, sliceIndex, 0, data.Length);
        }

        public void SetFromTexture(Texture2D from, int sliceIndex)
        {
            lock (device.DeviceContext)
            {
                device.DeviceContext.CopySubresourceRegion(from.SRV.Resource, 0, null, tex2D, sliceIndex * mipCount);
                device.DeviceContext.GenerateMips(SRV);
            }
        }

        public void SetFromResource(ShaderResource from, int sliceIndex)
        {
            lock (device.DeviceContext)
            {
                device.DeviceContext.CopySubresourceRegion(from.SRV.Resource, 0, null, tex2D, sliceIndex * mipCount);
                device.DeviceContext.GenerateMips(SRV);
            }
        }

        /// <summary>
        /// Sets 2D texture data, specifying a mipmap level, source rectangle, start index, and number of elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level"></param>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="elementCount"></param>
        public void SetData<T>(int level, Rectangle? rect, T[] data, int slice, int startIndex, int elementCount) where T : struct
        {
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                var startBytes = startIndex * elementSizeInByte;
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                int x, y, w, h;
                if (rect.HasValue)
                {
                    x = rect.Value.X;
                    y = rect.Value.Y;
                    w = rect.Value.Width;
                    h = rect.Value.Height;
                }
                else
                {
                    x = 0;
                    y = 0;
                    w = Math.Max(Width >> level, 1);
                    h = Math.Max(Height >> level, 1);

                    // For DXT textures the width and height of each level is a multiple of 4.
                    if (format == ColorFormat.Dxt1 ||
                        format == ColorFormat.Dxt3 ||
                        format == ColorFormat.Dxt5)
                    {
                        w = (w + 3) & ~3;
                        h = (h + 3) & ~3;
                    }
                }

                var box = new SharpDX.DataBox(dataPtr, w * Converter.SizeOf(format), 0);

                var region = new SharpDX.Direct3D11.ResourceRegion();
                region.Top = y;
                region.Front = 0;
                region.Back = 1;
                region.Bottom = y + h;
                region.Left = x;
                region.Right = x + w;

                lock (device.DeviceContext)
                {
                    device.DeviceContext.UpdateSubresource(box, tex2D, slice * mipCount + level, region);
                    device.DeviceContext.GenerateMips(SRV);
                }                
            }
            finally
            {
                dataHandle.Free();
            }
        }


    }
}
