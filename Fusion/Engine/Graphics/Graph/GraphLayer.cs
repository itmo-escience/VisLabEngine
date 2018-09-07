using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Fusion;
using Fusion.Input;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Graphics.Display;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;
using Fusion.Engine.Input;
using Keys = Fusion.Engine.Input.Keys;


namespace Fusion.Engine.Graphics.Graph
{
	
	public enum State
	{
		RUN,
		PAUSE
	}

    public class GraphLayer : IDisposable
    {
	    Game Game;

        //Node texture
        Texture2D	texture;
        Texture2D linkTexture;
        Ubershader	shader;


	    [StructLayout(LayoutKind.Explicit)]
	    public struct LinkId
	    {
		    [FieldOffset(0)] public int id;
	    }


		public State State;

		public Graph.Vertice[]	ParticlesCpu;


        public Graph.Link[]		LinksCpu;

		List<List<int>>		linkPtrLists;
	    int[]				linksPtrsCpu;


	    public ConstantBuffer ParamsCb;

	    public StructuredBuffer ParticlesGpu;
	    public StructuredBuffer LinksGpu;
	    public StructuredBuffer LinksPtrGpu;


		public List<string> nodeText;
		public GreatCircleCamera Camera { set; get; }

        enum Flags
        {
            // for geometry shader:
            POINT	= 0x1 << 1,
            LINE	= 0x1 << 2,
            DRAW	= 0x1 << 3,
        }


	    public GraphConfig cfg;


        [StructLayout(LayoutKind.Explicit, Size=160)]
        struct Params
        {
            [FieldOffset(0)] public Matrix View;
            [FieldOffset(64)] public Matrix Projection;
            [FieldOffset(128)] public int MaxParticles;
            [FieldOffset(132)] public float DeltaTime;
            [FieldOffset(136)] public float LinkSize;
            [FieldOffset(140)] public int SelectedId;
            [FieldOffset(144)] public Vector4 Dummy;
        }

        Params param = new Params();

        public Vector4 DummyParams {
            get { return param.Dummy; }
            set { param.Dummy = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        public GraphLayer(Game game)
        {
	        Game = game;
	        cfg = new GraphConfig(game);

		}

		StateFactory factory;
	    public Graph graph;


        public string ShaderPath = "Graph/ParticlesLinksDrawBitmask";
        /// <summary>
        /// инициализация графа
        /// </summary>
        public void Initialize()
        {
            texture	= Game.Content.Load<Texture2D>("Graph/node");
            linkTexture = Game.Content.Load<Texture2D>("Graph/arrow");
            shader = Game.Content.Load<Ubershader>(ShaderPath);
			factory	= new StateFactory( shader, typeof(Flags), (ps,i) => Enum( ps, (Flags)i ) );

            ParamsCb = new ConstantBuffer(Game.GraphicsDevice, typeof(Params));
			State = State.RUN;

            linkPtrLists = new List<List<int>>();

			nodeText = new List<string>();
            
			Log.Message("start graph");


			ParticlesCpu	= new Graph.Vertice[graph.NodesCount]; //буфер для вершин
            LinksCpu		= new Graph.Link[graph.Links.Count]; //буфер для ребер
	        linksPtrsCpu	= new int[0];

			ParticlesCpu	= graph.Nodes.ToArray();
	        LinksCpu		= graph.Links.ToArray();

			Game.Keyboard.KeyDown += Keyboard_KeyDown;
        }


	    public void SetGraph(Graph g)
	    {
		    graph = g;
	    }


		void Keyboard_KeyDown (object sender, Input.KeyEventArgs e)
		{			
		}

			
		//параметры для шейдера
		void Enum ( PipelineState ps, Flags flag )
		{
			ps.Primitive			=	Primitive.PointList; //принимает точки
			ps.RasterizerState		=	RasterizerState.CullNone; //без разницы по или против часовой стрелки

            //if (flag.HasFlag(Flags.LINE))
            //{
                ps.BlendState = BlendState.AlphaBlendKeepDstAlpha; //режим смешивания
                ps.DepthStencilState = DepthStencilState.Readonly;
                Log.Message("LINE");
            //}
            //if (flag.HasFlag(Flags.POINT))
            //{
            //    ps.BlendState = BlendState.Opaque;
            //    ps.DepthStencilState = DepthStencilState.Default;
            //    Log.Message("Point");
            //}
        }

        public void Pause()
        {
	        State = State == State.RUN ? State.PAUSE : State.RUN;
        }


		//добавляем сразу все вершины
	    public void AddMaxParticles()
        {
            linkPtrLists.Clear();
            foreach (var node in graph.Nodes) {
				linkPtrLists.Add(new List<int>());
			}

	        CreateLinks();
        }
		

		//новая итерация происходит тут
        public void CreateLinks()
        {
	        int lInd = 0;
	        foreach (var link in graph.Links) {
		        linkPtrLists[link.Par1].Add(lInd);
		        linkPtrLists[link.Par2].Add(lInd);

				lInd++;
	        }

			var totalCount = linkPtrLists.Sum(x => x.Count);
	        linksPtrsCpu = new int[totalCount];
            UpdateCPUBuffer();
	        int linksCounter = 0;
	        for (int i = 0; i < linkPtrLists.Count; i++) {
		        ParticlesCpu[i].LinksPtr	= linksCounter;
		        ParticlesCpu[i].LinksCount	= linkPtrLists[i].Count;

		        //ParticlesCpu[i].Mass = linkPtrLists[i].Count == 0 ? 15 : linkPtrLists[i].Count;

				Array.Copy(linkPtrLists[i].ToArray(), 0, linksPtrsCpu, linksCounter, linkPtrLists[i].Count);
		        linksCounter += linkPtrLists[i].Count;
	        }

			SetBuffers();
		}


		// задает буферы, которые передадутся в шейдеры
        public void SetBuffers()
        {
			if (graph.NodesCount == 0) return;

	        if (ParticlesCpu.Length != 0) {
		        if (ParticlesGpu == null || ParticlesGpu.StructureCapacity != ParticlesCpu.Length) {
			        ParticlesGpu?.Dispose();
			        ParticlesGpu = new StructuredBuffer(Game.GraphicsDevice, typeof(Graph.Vertice), ParticlesCpu.Length, StructuredBufferFlags.None);
		        }

		        ParticlesGpu.SetData(ParticlesCpu);
	        }


            if (LinksCpu.Length != 0) {
	            if (LinksGpu == null || LinksGpu.StructureCapacity != LinksCpu.Length) {
		            LinksGpu?.Dispose();
					LinksGpu = new StructuredBuffer(Game.GraphicsDevice, typeof(Graph.Link), LinksCpu.Length, StructuredBufferFlags.None);
	            }
				
				LinksGpu.SetData(LinksCpu);
            }


	        if (linksPtrsCpu.Length != 0) {
		        if (LinksPtrGpu == null || LinksPtrGpu.StructureCapacity != linksPtrsCpu.Length) {
					LinksPtrGpu?.Dispose();
			        LinksPtrGpu = new StructuredBuffer(Game.GraphicsDevice, typeof(LinkId), linksPtrsCpu.Length, StructuredBufferFlags.None);
				}

		        LinksPtrGpu.SetData(linksPtrsCpu);
			}
        }

        /// <summary>
        /// чистим память
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose()
        {
			ParamsCb?.Dispose();
			ParticlesGpu?.Dispose();
			LinksGpu?.Dispose();
        }


        public void Update(GameTime gameTime)
        {

        }

        public void UpdateCPUBuffer()
        {
            if (ParticlesGpu != null)
            {
                ParticlesCpu = new Graph.Vertice[graph.NodesCount];
                ParticlesGpu.GetData(ParticlesCpu);
            }
        }

        //выделение узла по клику мышки
		public bool SelectNode(Vector2 cursor, StereoEye eye, float threshold, out int VerticeIndex, out Vector3 VerticePosition, out int TrueIndex)
        {
            VerticeIndex = -1;
			TrueIndex = -1;
            selectedVertice = -1;

            //получаем координаты мыши
            var cam = Camera;
            var viewMatrix = cam.GetViewMatrix(eye);
            var projMatrix = cam.GetProjectionMatrix(eye);
            Vector2 cursorProj = GraphHelper.PixelsToProj(cursor, Game.GraphicsDevice.DisplayBounds.Width, Game.GraphicsDevice.DisplayBounds.Height);

			//определяем узлы-кандидаты
			Dictionary<int, float> candidatesToSelect = new Dictionary<int, float>();
			VerticePosition = new Vector3();
            float minZ = 99999;

            if (ParticlesGpu != null) {
				UpdateCPUBuffer();

				//здесь пересчитываются позиции узлов в точку на экране
				//если узел ближе к клику, чем погрешность, значит его и выбирали
                foreach (var p in ParticlesCpu) {
                    Vector4 posWorld	= new Vector4(p.Position, 1.0f);
                    Vector4 posView		= Vector4.Transform(posWorld, viewMatrix);
                    Vector4 posProj		= Vector4.Transform(posView, projMatrix);
					float delta = p.Size / posProj.W;
					posProj /= posProj.W;

                    Vector2 diff = new Vector2(posProj.X - cursorProj.X, posProj.Y - cursorProj.Y);
					
					if (diff.Length() < threshold + delta) {
						if (minZ > posProj.Z) {
                           minZ			= posProj.Z;
                            
							candidatesToSelect.Add(p.Id, diff.Length());
							//Console.WriteLine(p.Id + " " + diff.Length());
						}
                    }
                }

				//находим ближайший
	            if (candidatesToSelect.Count != 0)
	            {
		            float min = candidatesToSelect.Min((x) => x.Value);
		            int index = candidatesToSelect.First((y) => y.Value <= min).Key;
		            var sVertice = ParticlesCpu.Last((x) => x.Id == index);
					TrueIndex = Array.IndexOf(ParticlesCpu, sVertice);
					selectedVertice = index;
		            VerticeIndex = index;
		            VerticePosition = sVertice.Position;
		           // ls = new LayoutSystem(Game, Game.Content.Load<Ubershader>(@"Graph/NodeInCenter"))
		            //{
			           // NodeId = -1//selectedVertice
		            //};                
	            }
	            else
	            {
		           // ls = new LayoutSystem(Game, Game.Content.Load<Ubershader>(@"Graph/VKRepost")) {NodeId = -1};
		           // state = State.RUN;
	            }
            }
			return (VerticeIndex != -1);
        }

        public void ClearSelection()
        {
            selectedVertice = -1;
        }

        public int selectedVertice = -1;

        public float VelMult = 1;

        /// <summary>
        /// здесь все рисуется
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="stereoEye"></param>
        public void Draw(GameTime gameTime, StereoEye stereoEye)
        {
			var device	= Game.GraphicsDevice;
			var cam		= Camera;

			//параметры для константного буфера
            param.View			= cam.GetViewMatrix(stereoEye);
            param.Projection	= cam.GetProjectionMatrix(stereoEye);
            param.MaxParticles	= 0;
            param.DeltaTime		= gameTime.ElapsedSec * VelMult;
            param.LinkSize		= 0.1f;
            param.SelectedId = selectedVertice;


            device.VertexShaderConstants[0]		= ParamsCb;
            device.GeometryShaderConstants[0]	= ParamsCb;
            device.PixelShaderConstants[0]		= ParamsCb;

            device.PixelShaderSamplers[0]		= SamplerState.LinearWrap;

            //	Simulate : ------------------------------------------------------------------------
            //

            param.MaxParticles = ParticlesCpu.Length;
            ParamsCb.SetData(param);


            device.ComputeShaderConstants[0] = ParamsCb;
				
            // ------------------------------------------------------------------------------------
            //	Render: ---------------------------------------------------------------------------
            //
			device.SetCSRWBuffer(0, null);


			
			device.GeometryShaderResources[1] = ParticlesGpu;
			device.GeometryShaderResources[3] = LinksGpu;
	        device.PixelShaderSamplers[0]	= SamplerState.LinearWrap;
	        device.PixelShaderResources[0]	= texture;
            device.PixelShaderResources[2] = linkTexture;

            // draw lines: --------------------------------------------------------------------------
            //рисуем сначала линии
            //линии и вершины по сути одно и тоже (прямоугольник)
            device.PipelineState = factory[(int)Flags.DRAW | (int)Flags.LINE];
            device.Draw(LinksCpu.Length, 0);

            // draw points: ------------------------------------------------------------------------
            //а теперь вершины
            device.PipelineState = factory[(int)Flags.DRAW | (int)Flags.POINT];
            device.Draw(ParticlesCpu.Length, 0);

           



        }
    }
}

