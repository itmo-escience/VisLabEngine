using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
    public class MCBatch : Gis.GisLayer
    {
        Ubershader shader;
        StateFactory factory;   

        [Flags]
        public enum FieldFlags : int 
        {
            None = 0,
            DrawIsoSurface = 1 << 0,
            LerpBuffers = 1 << 1,
        }

        SamplerState Sampler = SamplerState.PointClamp;  

        protected struct ConstData
        {    
            public double Lat;
            public double Lon;
            public Vector2 FieldSize;            
            public float IsolineValue;
            public float LerpValue;
            public uint dimX;
            public uint dimY;
            public uint dimZ; 
            public uint Dummy;
            public Vector4 Color;
            public Vector4 Right;
            public Vector4 Forward;   
        }
        protected ConstData parameters;


        public void SetWholeData(float[] data, int dimX, int dimY, int dimZ)
        {
            DataFirstFrameGpu?.Dispose();
            DataFirstFrameGpu = new Texture3D(Game.GraphicsDevice, dimX, dimY, dimZ, ColorFormat.R32F, false);
            DataFirstFrameGpu.SetData(data); 

            DataSecondFrameGpu?.Dispose();
            DataSecondFrameGpu = new Texture3D(Game.GraphicsDevice, dimX, dimY, dimZ, ColorFormat.R32F, false);
            DataSecondFrameGpu.SetData(data);

            dataFrameSize = (dimX - 1) * (dimY - 1) * (dimZ - 1);

            parameters.dimX = (uint)dimX;
            parameters.dimY = (uint)dimY;
            parameters.dimZ = (uint)dimZ;
        }

        public void AddFrame(float[] data, int dimX, int dimY, int dimZ)
        {
            DataFirstFrameGpu.Dispose();
            DataFirstFrameGpu = DataSecondFrameGpu;

            DataSecondFrameGpu = new Texture3D(Game.GraphicsDevice, dimX, dimY, dimZ, ColorFormat.R32F, false);
            DataSecondFrameGpu.SetData(data); 
            dataFrameSize = (dimX - 1) * (dimY - 1) * (dimZ - 1);
        }

        private int dataFrameSize;

        public float Bordervalue
        {
            get { return parameters.IsolineValue; }
            set { parameters.IsolineValue = value; }
        }

        public Color Color 
        {
            get { return (Color) parameters.Color; }
            set { parameters.Color = value.ToVector4(); }
        }

        public Vector2 FieldSize
        {
            get { return parameters.FieldSize; }
            set { parameters.FieldSize = value; }
        }

        public double Lat
        {
            get { return parameters.Lat; }
            set { parameters.Lat = value; }
        }

        public double Lon
        {
            get { return parameters.Lon; }
            set { parameters.Lon = value; }
        }

        public float Lerp
        {
            get { return parameters.LerpValue; }
            set { parameters.LerpValue = value; }
        }

        void EnumFunc(PipelineState ps, int flag)
        {
            var flags = (FieldFlags) flag;
             
            ps.VertexInputElements = null;
            //ps.BlendState = flags.HasFlag(FieldFlags.XRAY) ? BlendState.Additive : BlendState.AlphaBlend;
            //ps.DepthStencilState = flags.HasFlag(FieldFlags.NO_DEPTH) ? DepthStencilState.None : DepthStencilState.Default;
            //ps.RasterizerState = flags.HasFlag(FieldFlags.CULL_NONE) ? RasterizerState.CullNone : RasterizerState.CullCW;

            ps.BlendState = BlendState.AlphaBlend; 
            ps.DepthStencilState = DepthStencilState.None;         
            ps.RasterizerState = RasterizerState.CullNone;


            //if (flags.HasFlag(FieldFlags.DrawIsoSurface)) ps.BlendState = BlendState.Opaque;

            ps.Primitive = Primitive.PointList;   
        } 


        protected ConstantBuffer cB;

        Texture3D DataFirstFrameGpu;
        Texture3D DataSecondFrameGpu;        
        private float[] depths;

        public float[] DataDepths 
        { 
            set
            { 
                depths = new float[parameters.dimZ];

                for (int i = 0; i < parameters.dimZ; i++)
                {
                    if (value.Length >  i)
                    {
                        depths[i] = value[i];                    
                    } else if (i == 0)
                    {
                        depths[i] = 0;
                    } else if (i == 1)
                    {
                        depths[i] = 1;
                    } else
                    {
                        depths[i] = depths[i - 1] + (depths[i - 1] - depths[i - 2]);
                    }
                           
                }
                
                depthBuffer?.Dispose();
                depthBuffer = new StructuredBuffer(Game.GraphicsDevice, sizeof(float), depths.Length, StructuredBufferFlags.None);
                depthBuffer.SetData(depths);
            }
        }

        private StructuredBuffer depthBuffer;   
        
                   
        public MCBatch(Game engine) : base(engine)
        {
            shader = Game.Content.Load<Ubershader>("globe.marchingCubes.hlsl");
            factory = shader.CreateFactory(typeof(FieldFlags), EnumFunc);
             
            cB = new ConstantBuffer(Game.GraphicsDevice, typeof(ConstData)); 
        }

        public override void Dispose()
        {
            //shader.Dispose();	
            factory.Dispose();
            //factoryXray.Dispose();

            DataFirstFrameGpu.Dispose();   
            DataSecondFrameGpu.Dispose();

            cB.Dispose();

            base.Dispose();  
        }  

        public FieldFlags Flags = FieldFlags.DrawIsoSurface | FieldFlags.LerpBuffers;    

        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
            Game.GraphicsDevice.GeometryShaderConstants[0] = constBuffer;   
            Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;
            //var m = GeoHelper.CalculateBasisOnSurface(new DVector2(parameters.Lon, parameters.Lat));
            //parameters.Right = new Vector4(m.Right.ToVector3(), 0);
            //parameters.Forward = new Vector4(m.Forward.ToVector3(), 0);              
            cB.SetData(parameters);   
            Game.GraphicsDevice.VertexShaderConstants[1] = cB;
            Game.GraphicsDevice.GeometryShaderConstants[1] = cB;
            Game.GraphicsDevice.PixelShaderConstants[1] = cB;

            Game.GraphicsDevice.GeometryShaderSamplers[0] = Sampler;

            Game.GraphicsDevice.GeometryShaderResources[1] = DataFirstFrameGpu; 
            Game.GraphicsDevice.GeometryShaderResources[2] = DataSecondFrameGpu; 
            Game.GraphicsDevice.GeometryShaderResources[3] = depthBuffer;

            Game.GraphicsDevice.PipelineState = factory[(int)Flags];

            Game.GraphicsDevice.SetupVertexInput(null, null);    

            Game.GraphicsDevice.Draw(dataFrameSize, 0);        
               
        } 

    }
}
