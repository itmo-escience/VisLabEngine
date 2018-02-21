using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics.Graph
{
	public class SimpleLayout : LayoutSystem
	{
		Ubershader		shader;
		StateFactory	factory;
		
		struct SimpleParams
		{
			public Vector4 GroupdimXXX;
		}

		SimpleParams cData;
		ConstantBuffer buf;

		enum Flags {
			CalculateForces	= 0x1 << 0,
			MoveVertices	= 0x1 << 1,
		}


		public SimpleLayout(Game game, GraphLayer layer, string shaderName) : base(game, layer)
		{
			shader	= Game.Content.Load<Ubershader>(shaderName);
			factory = new StateFactory(shader, typeof(Flags), (ps, i) => {
				ps.Primitive			= Primitive.PointList; //принимает точки
				ps.BlendState			= BlendState.AlphaBlend; //режим смешивания
				ps.RasterizerState		= RasterizerState.CullNone; //без разницы по или против часовой стрелки
				ps.DepthStencilState	= DepthStencilState.Readonly;
			});

			buf = new ConstantBuffer(Game.GraphicsDevice, typeof(SimpleParams));

			var count = graph.ParticlesCpu.Length;

			int		numGroups	= (count % 1024 != 0) ? ((count / 1024) + 1) : (count / 1024);
			double	secondRoot	= Math.Pow((double)numGroups, (double)(0.5));
					secondRoot	= Math.Ceiling(secondRoot);
			cData.GroupdimXXX.X = (int)secondRoot;

			buf.SetData(cData);
		}


		public override void Update(GameTime time)
		{
		   
		    Game.GraphicsDevice.ComputeShaderResources[00] = graph.LinksPtrGpu;
		    Game.GraphicsDevice.ComputeShaderResources[1] = graph.LinksGpu;
		    Game.GraphicsDevice.SetCSRWBuffer(0, graph.ParticlesGpu, 0);

		    Game.GraphicsDevice.ComputeShaderConstants[0] = graph.ParamsCb;
		    Game.GraphicsDevice.ComputeShaderConstants[1] = buf;

		    //PixHelper.BeginEvent(new SharpDX.Mathematics.Interop.RawColorBGRA(255, 0, 0, 255), "Update Traffic");
		    Game.GraphicsDevice.PipelineState = factory[(int) Flags.CalculateForces];
		    Game.GraphicsDevice.Dispatch((int) cData.GroupdimXXX.X, (int) cData.GroupdimXXX.X, 1);

		    Game.GraphicsDevice.PipelineState = factory[(int) Flags.MoveVertices];
		    Game.GraphicsDevice.Dispatch((int) cData.GroupdimXXX.X, (int) cData.GroupdimXXX.X, 1);
		    //PixHelper.EndEvent();
		}
	}
}
