using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Graphics.GIS
{
    public class VPBatch : Gis.GisLayer  
    {
        public VPBatch(Game engine) : base(engine)
        {
            shader = Game.Content.Load<Ubershader>("globe.VolumPoints.hlsl");    
            factory = shader.CreateFactory(typeof(FieldFlags), EnumFunc);

            cB = new ConstantBuffer(Game.GraphicsDevice, typeof(ConstData));    

            confBuffer = new ConstantBuffer(Game.GraphicsDevice, typeof(ConfData));             
        }
         
        Ubershader shader;    
        StateFactory factory;   

        [Flags]
        public enum FieldFlags : int 
        {
            None = 0,
            Draw_points = 1 << 0,
            LerpBuffers = 1 << 1,
            UsePalette = 1 << 2,
            MoveVertices = 1 << 3,
            Depth_Calc = 1 << 4,
            Depth_Sort_Transpose = 1 << 5,            
            Depth_Sort_FirstMerge = 1 << 6,             
        }

        SamplerState Sampler = SamplerState.LinearClamp;
        #region FieldData
        public struct  ConstData
        {    
            public double Lat;
            public double Lon;
            public Vector3 FieldSize;            
            public float LerpValue;
            public uint dimX;
            public uint dimY;
            public uint dimZ; 
            public uint dimW;            
            public Vector4 Right;
            public Vector4 Forward;
            public Matrix View;
            public Matrix Proj;
            public Vector2 MinMax;
            public Vector2 Dummy; 
        }

        public Matrix View
        {
            get => parameters.View;
            set => parameters.View = value;
        }

        public Matrix Proj
        {
            get => parameters.Proj;
            set => parameters.Proj = value;
        }

        public float Min
        {
            get { return parameters.MinMax.X; }
            set { parameters.MinMax.X = value; }
        }

        public Vector4 Right
        {
            get { return parameters.Right; }
            set { parameters.Right = value; }
        }

        public Vector4 Forward
        {
            get { return parameters.Forward; }
            set { parameters.Forward = value; }
        }

        public float Max
        {
            get { return parameters.MinMax.Y; }
            set { parameters.MinMax.Y = value; }
        }

        public Vector3 FieldSize
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

        #endregion

        #region ConfData

        public struct ConfData
        {
            public float MinSize;
            public float MaxSize;
            public float LowSizeValue;
            public float HighSizeValue;            
            public float MinTransp;
            public float MaxTransp;
            public float LowTranspValue;
            public float HighTranspValue;
            public float TransparencyMult;
            public float TransparencyPower;
            public Vector2 Dummy;
        }

        public float MinPointSize
        {
            get => configParameters.MinSize;
            set => configParameters.MinSize = value;
        }

        public float MaxPointSize
        {
            get => configParameters.MaxSize;
            set => configParameters.MaxSize = value;
        }

        public float MinPointSizeValue  
        {
            get => configParameters.LowSizeValue;
            set => configParameters.LowSizeValue = value;
        }

        public float MaxPointSizeValue
        {
            get => configParameters.HighSizeValue;
            set => configParameters.HighSizeValue = value;
        }

        public float MinPointValue
        {
            set
            {
                configParameters.LowSizeValue = value;
                configParameters.LowTranspValue = value;
            }            
        }

        public float MaxPointValue
        {
            set
            {
                configParameters.HighSizeValue = value;
                configParameters.HighTranspValue = value;
            }
        }
        
        public float MinPointTransp 
        {
            get => configParameters.MinTransp;
            set => configParameters.MinTransp = value;
        }

        public float MaxPointTransp
        {
            get => configParameters.MaxTransp;
            set => configParameters.MaxTransp = value;
        }

        public float MinPointTranspValue
        {
            get => configParameters.LowTranspValue;
            set => configParameters.LowTranspValue = value;
        }

        public float MaxPointTranspValue
        {
            get => configParameters.HighTranspValue;
            set => configParameters.HighTranspValue = value;
        }

        public float TransparencyMult
        {
            get => configParameters.TransparencyMult;
            set => configParameters.TransparencyMult = value;
        }

        public float TransparencyPower
        {
            get => configParameters.TransparencyPower;
            set => configParameters.TransparencyPower = value;
        }

        #endregion
        protected ConstData parameters;
        public ConfData configParameters;
        
        public void SetWholeData(float[] data, int dimX, int dimY, int dimZ)   
        {
            DataFirstFrameGpu?.Dispose();
            DataFirstFrameGpu = new Texture3D(Game.GraphicsDevice, dimX, dimY, dimZ, ColorFormat.R32F, false);
            DataFirstFrameGpu.SetData(data); 

            DataSecondFrameGpu?.Dispose();
            DataSecondFrameGpu = new Texture3D(Game.GraphicsDevice, dimX, dimY, dimZ, ColorFormat.R32F, false);
            DataSecondFrameGpu.SetData(data);

            dataFrameSize = dimX * dimY * dimX;

            parameters.dimX = (uint)dimX;
            parameters.dimY = (uint)dimY;
            parameters.dimZ = (uint)dimX; 
            parameters.dimW = (uint)dimZ;


            var cc = 1 << (int)Math.Ceiling(Math.Log(dataFrameSize, 2)); 
            distBuffer?.Dispose();
            indBuffer?.Dispose();
            posBuffer?.Dispose();
            indecies?.Dispose();
            distBuffer = new StructuredBuffer(Game.GraphicsDevice, typeof(float), cc, StructuredBufferFlags.None);
            posBuffer = new StructuredBuffer(Game.GraphicsDevice, typeof(Vector4), cc, StructuredBufferFlags.None);
            indBuffer = new StructuredBuffer(Game.GraphicsDevice, typeof(uint), cc, StructuredBufferFlags.None);            
            indecies = new IndexBuffer(Game.GraphicsDevice, cc);
        }

        private IndexBuffer indecies;
        public void AddFrame(float[] data, int dimX, int dimY, int dimZ) 
        {
            DataFirstFrameGpu.Dispose();
            DataFirstFrameGpu = DataSecondFrameGpu;

            DataSecondFrameGpu = new Texture3D(Game.GraphicsDevice, dimX, dimY, dimZ, ColorFormat.R32F, false);
            DataSecondFrameGpu.SetData(data); 
            dataFrameSize = dimX * dimY * dimX;            
        }

        private int dataFrameSize;       

        void EnumFunc(PipelineState ps, int flag)
        {
            var flags = (FieldFlags) flag;
             
            ps.VertexInputElements = null;

            ps.BlendState = BlendState.AlphaBlend; 
            ps.DepthStencilState = DepthStencilState.Readonly;                  
            ps.RasterizerState = RasterizerState.CullCW;              

            ps.Primitive = Primitive.PointList;   
        } 


        protected ConstantBuffer cB, confBuffer;  

        Texture3D DataFirstFrameGpu;
        Texture3D DataSecondFrameGpu;        
        private float[] depths;

        public float[] DataDepths 
        { 
            set
            { 
                depths = new float[parameters.dimW]; 

                for (int i = 0; i < parameters.dimW; i++)
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


        private StructuredBuffer distBuffer, indBuffer, posBuffer;  

        private const int BITONIC_BLOCK_SIZE = 1024;
        void GPUSort(ConstantBuffer constBuffer)
        {
            int count = dataFrameSize;
            
            var cc = 1 << (int) Math.Ceiling(Math.Log(count, 2)); 

            //var data1 = new float[cc];
            //uint[] indecies = new uint[cc];            
            //for (int i = 0; i < cc; i++)
            //{
            //    indecies[i] = (uint) i;
            //}
            //indBuffer.SetData(indecies);
             
            using (ConstantBuffer sortCB = new ConstantBuffer(Game.GraphicsDevice, sizeof(uint) * 4))
            {         
                Game.GraphicsDevice.SetCSRWBuffer(0, distBuffer, 0);
                Game.GraphicsDevice.SetCSRWBuffer(1, indBuffer, 0);
                Game.GraphicsDevice.SetCSRWBuffer(2, posBuffer, 0);

                Game.GraphicsDevice.PipelineState = factory[(int)FieldFlags.Depth_Calc];
                Game.GraphicsDevice.Dispatch((int)Math.Ceiling((float)dataFrameSize / BITONIC_BLOCK_SIZE), 1, 1); 

                float[] dist = new float[cc];
                //int[] inds = new int[cc];
                sortCB.SetData(new uint[] { (uint)cc, 0, 0, 0 }); 
                Game.GraphicsDevice.ComputeShaderConstants[2] = sortCB; 
                for (uint level = 2; level <= cc; level <<= 1)   
                {
                    sortCB.SetData(new uint[] {(uint) level, 0, 0, 0}); 
                    Game.GraphicsDevice.PipelineState = factory[(int) FieldFlags.Depth_Sort_FirstMerge];
                    Game.GraphicsDevice.Dispatch((int) Math.Ceiling((float) cc / BITONIC_BLOCK_SIZE), 1, 1);
                    if (level > BITONIC_BLOCK_SIZE)
                    {
                        for (uint l = level / 2; l >= BITONIC_BLOCK_SIZE; l >>= 1)     
                        {
                            sortCB.SetData(new uint[] {(uint) level, l, 0, 0}); 
                            Game.GraphicsDevice.PipelineState = factory[(int) FieldFlags.Depth_Sort_Transpose];
                            Game.GraphicsDevice.Dispatch((int) Math.Ceiling((float) cc / BITONIC_BLOCK_SIZE), 1, 1);
                        }    
                    }  

                    //distBuffer.GetData(dist);
                    //if (float.IsNaN(dist.Take((int)level).Aggregate(float.PositiveInfinity, (a, b) => a >= b ? b : float.NaN)))
                    //{
                    //    Log.Error($"{level}: No!");       
                    //} 
                    //else
                    //{
                    //    Log.Message($"{level}: Yes!");
                    //}
                }     
                Game.GraphicsDevice.SetCSRWBuffer(0, null, 0);
                Game.GraphicsDevice.SetCSRWBuffer(1, null, 0);
                Game.GraphicsDevice.SetCSRWBuffer(2, null, 0);

                distBuffer.GetData(dist);
                //indBuffer.GetData(inds);
                 
                if (float.IsNaN(dist.Aggregate(float.PositiveInfinity, (a, b) => a >= b ? b : float.NaN)))  
                { 
                    Log.Error("No!");
                } 
                //else
                //{
                //    Log.Message("Yes!");
                //}
            }
            
          
        }

        public override void Dispose()
        {
            //shader.Dispose();	
            factory.Dispose();
            //factoryXray.Dispose();

            DataFirstFrameGpu.Dispose();   
            DataSecondFrameGpu.Dispose();

            distBuffer?.Dispose();
            indBuffer?.Dispose();
            posBuffer?.Dispose();

            indecies?.Dispose();

            depthBuffer?.Dispose();
            cB.Dispose();
             
            base.Dispose();   
        }  

        public FieldFlags Flags = FieldFlags.Draw_points | FieldFlags.LerpBuffers;    

        public Texture2D Palette
        {
            get => palette;
            set
            {
                //palette?.Dispose(); 
                palette = value;
            }
        }
        private Texture2D palette;
        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
            Game.GraphicsDevice.GeometryShaderConstants[0] = constBuffer;
            Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;
            Game.GraphicsDevice.ComputeShaderConstants[0] = constBuffer;
            parameters.Dummy.X = 0;//(float)gameTime.Total.TotalSeconds / 100;  
            cB.SetData(parameters);       
            confBuffer.SetData(configParameters);   
            Game.GraphicsDevice.VertexShaderConstants[1] = cB;     
            Game.GraphicsDevice.GeometryShaderConstants[1] = cB;      
            Game.GraphicsDevice.PixelShaderConstants[1] = cB;

            Game.GraphicsDevice.VertexShaderConstants[3] = confBuffer;
            Game.GraphicsDevice.GeometryShaderConstants[3] = confBuffer;
            Game.GraphicsDevice.PixelShaderConstants[3] = confBuffer;  

            Game.GraphicsDevice.ComputeShaderConstants[1] = cB;
            Game.GraphicsDevice.GeometryShaderSamplers[0] = Sampler;


            var count = 1 << (int)Math.Ceiling(Math.Log(dataFrameSize, 2));

            
            

            GPUSort(constBuffer);
            Game.GraphicsDevice.GeometryShaderResources[0] = Palette;
            Game.GraphicsDevice.GeometryShaderResources[1] = DataFirstFrameGpu;  
            Game.GraphicsDevice.GeometryShaderResources[2] = DataSecondFrameGpu;
            Game.GraphicsDevice.GeometryShaderResources[3] = depthBuffer;            
            Game.GraphicsDevice.GeometryShaderResources[4] = posBuffer;  
            if (Palette != null)  
            {
                Flags = FieldFlags.Draw_points | FieldFlags.LerpBuffers | FieldFlags.UsePalette | FieldFlags.MoveVertices;
               
            }
            else
            {
                Flags = FieldFlags.Draw_points | FieldFlags.LerpBuffers | FieldFlags.MoveVertices;          
            }                                      
            Game.GraphicsDevice.PipelineState = factory[(int)Flags];                
            Game.GraphicsDevice.DeviceContext.CopyResource(indBuffer.SRV.Resource, indecies.Buffer);  
            Game.GraphicsDevice.SetupVertexInput(null, indecies);                
            Game.GraphicsDevice.DrawIndexed(dataFrameSize, 0, 0); 
            //Game.GraphicsDevice.Draw(dataFrameSize, 0);
        } 

    }
}
