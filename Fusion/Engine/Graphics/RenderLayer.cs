﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.Graph;


namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Represents entire visible world.
	/// </summary>
	public class RenderLayer : DisposableBase {
		
		protected readonly Game		Game;
		protected readonly RenderSystem	rs;

		/// <summary>
		/// Indicates whether view should be drawn.
		/// Default value is True.
		/// </summary>
		public bool Visible {
			get; set;
		}

		/// <summary>
		/// Indicates in which order view should be drawn.
		/// </summary>
		public int Order {
			get; set;
		}

		/// <summary>
		/// Gets and sets view's camera.
		/// This value is already initialized when View object is created.
		/// </summary>
		public Camera Camera {
			get; set;
		}

		/// <summary>
		/// Gets view target.
		/// Null value indicates that view will be rendered to backbuffer.
		/// Default value is null.
		/// </summary>
		public TargetTexture Target {
			get; set;
		}

		/// <summary>
		/// Indicated whether target buffer should be cleared before rendering.
		/// </summary>
		public bool Clear {	
			get; set;
		}

		/// <summary>
		/// Gets and sets clear color
		/// </summary>
		public Color4 ClearColor {
			get; set;
		}

		/// <summary>
		/// Gets collection of sprite layers.
		/// </summary>
		public ICollection<SpriteLayer>	SpriteLayers {
			get; private set;
		}

		/// <summary>
		/// Gets collection of GIS layers.
		/// </summary>
		public ICollection<Gis.GisLayer> GisLayers {
			get; private set;
		}


		/// <summary>
		/// Gets collection of GIS layers.
		/// </summary>
		public ICollection<Graph.GraphLayer> GraphLayers {
			get; private set;
		}


		public GlobeCamera GlobeCamera {
			get;
			set;
		}


		public Viewport? GlobeViewport { set; get; }


		internal DepthStencil2D GlobeDepthStencil;


		/// <summary>
		/// Creates ViewLayer instance
		/// </summary>
		/// <param name="Game">Game engine</param>
		public RenderLayer ( Game game )
		{
			Game		=	game;
			this.rs		=	Game.RenderSystem;

			Visible		=	true;
			Order		=	0;

			Camera		=	new Camera();

			SpriteLayers	=	new SpriteLayerCollection();
			GisLayers		=	new List<Gis.GisLayer>();
			GraphLayers		=	new List<GraphLayer>();
			GlobeCamera		=	new GlobeCamera(Game);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref GlobeDepthStencil );
			}
			base.Dispose( disposing );
		}

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Rendering :
		 * 
		-----------------------------------------------------------------------------------------*/


		/// <summary>
		/// Renders view
		/// </summary>
		internal virtual void Render ( GameTime gameTime, StereoEye stereoEye )
		{
			var targetSurface = (Target == null) ? rs.Device.BackbufferColor.Surface : Target.RenderTarget.Surface;

			//	clear target buffer if necassary :
			if (Clear) {
				rs.Device.Clear( targetSurface, ClearColor );
			}

			var viewport = GlobeViewport ?? new Viewport( 0,0, targetSurface.Width, targetSurface.Height );


			if (GisLayers.Any() || GraphLayers.Any()) {
				SetTargets(targetSurface);

				Game.GraphicsDevice.SetViewport(viewport);

				//	Render GIS stuff :
				RenderGIS(gameTime, stereoEye, viewport, targetSurface);
				// Render Graph stuff :
				RenderGraph(gameTime, stereoEye, viewport, targetSurface);
			}

			//	draw sprites :
			rs.SpriteEngine.DrawSprites( gameTime, stereoEye, targetSurface, SpriteLayers );

			rs.Filter.FillAlphaOne( targetSurface );
		}


		void SetTargets(RenderTargetSurface targetSurface)
		{
			if (GlobeDepthStencil == null) {
				GlobeDepthStencil = new DepthStencil2D(Game.GraphicsDevice, DepthFormat.D24S8, targetSurface.Width, targetSurface.Height, targetSurface.SampleCount);
			}
			else if (GlobeDepthStencil.Width != targetSurface.Width || GlobeDepthStencil.Height != targetSurface.Height) {
				
				GlobeDepthStencil.Dispose();
				GlobeDepthStencil = new DepthStencil2D(Game.GraphicsDevice, DepthFormat.D24S8, targetSurface.Width, targetSurface.Height, targetSurface.SampleCount);
			}

			rs.Device.Clear(GlobeDepthStencil.Surface);

			Game.GraphicsDevice.SetTargets(GlobeDepthStencil.Surface, targetSurface);
		}

	    public void ClearDepthBuffer(DepthStencil2D ds = null)
	    {
	        ds = ds ?? GlobeDepthStencil;
	        rs.Device.Clear(ds.Surface);
        }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		/// <param name="viewport"></param>
		/// <param name="targetSurface"></param>
		protected void RenderGIS ( GameTime gameTime, StereoEye stereoEye, Viewport viewport, RenderTargetSurface targetSurface )
		{
			if (!GisLayers.Any()) return;
			
			GlobeCamera.Viewport = viewport;
			GlobeCamera.Update(gameTime);

			rs.Gis.Camera = GlobeCamera;
			rs.Gis.Draw(gameTime, stereoEye, GisLayers);
		}

		protected void RenderGraph(GameTime gameTime, StereoEye stereoEye, Viewport viewport, RenderTargetSurface targetSurface)
		{
			if (!GraphLayers.Any()) return;

			foreach (var graphLayer in GraphLayers) {
				graphLayer.Draw(gameTime, stereoEye);
			}

		}
	}
}
