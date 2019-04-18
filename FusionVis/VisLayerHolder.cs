using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.Graph;

namespace FusionVis._0._2
{
    public class VisLayerHolder
    {
        /// <summary>
        /// Gets collection of sprite layers.
        /// </summary>
        public ICollection<SpriteLayer> SpriteLayers
        {
            get;
        } = new List<SpriteLayer>();

        /// <summary>
        /// Gets collection of GIS layers.
        /// </summary>
        public ICollection<Gis.GisLayer> GisLayers
        {
            get;
        } = new List<Gis.GisLayer>();


        /// <summary>
        /// Gets collection of GIS layers.
        /// </summary>
        public ICollection<GraphLayer> GraphLayers
        {
            get;
        } = new List<GraphLayer>();

        public void Clear()
        {
            OnClear?.Values.ToList().ForEach(a => a?.Invoke(this, EventArgs.Empty));
            foreach (var spriteLayer in SpriteLayers)
            {
                spriteLayer.Dispose();
            }

            foreach (var gisLayer in GisLayers)
            {
                gisLayer.Dispose();
            }

            foreach (var graphLayer in GraphLayers)
            {
                graphLayer.Dispose();
            }

            SpriteLayers.Clear();
            GisLayers.Clear();
            GraphLayers.Clear();

        }

        public void Compile()
        {
            OnCompile.Values.ToList().ForEach(a => a?.Invoke(this, EventArgs.Empty));
        }

        public Dictionary<RenderLayer, EventHandler> OnCompile = new Dictionary<RenderLayer, EventHandler>();

        public Dictionary<RenderLayer, EventHandler> OnClear= new Dictionary<RenderLayer, EventHandler>();
    }
}
