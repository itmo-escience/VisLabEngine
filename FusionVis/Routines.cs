using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Graphics;

namespace FusionVis._0._2
{
    public class Routines
    {
        public static void AddVisLayers(RenderLayer layer, VisLayerHolder holder)
        {
            foreach (var holderGisLayer in holder.GisLayers)
            {
                layer.GisLayers.Add(holderGisLayer);
            }
            foreach (var holderSpriteLayer in holder.SpriteLayers)
            {
                layer.SpriteLayers.Add(holderSpriteLayer);
            }
            foreach (var holderGraphLayer in holder.GraphLayers)
            {
                layer.GraphLayers.Add(holderGraphLayer);
            }

            holder.OnClear.Add(layer, (sender, args) =>
            {
                foreach (var holderGisLayer in holder.GisLayers)
                {
                    layer.GisLayers.Remove(holderGisLayer);
                }
                foreach (var holderSpriteLayer in holder.SpriteLayers)
                {
                    layer.SpriteLayers.Remove(holderSpriteLayer);
                }
                foreach (var holderGraphLayer in holder.GraphLayers)
                {
                    layer.GraphLayers.Remove(holderGraphLayer);
                }
            });

            holder.OnCompile.Add(layer, (sender, args) =>
            {
                foreach (var holderGisLayer in holder.GisLayers)
                {
                    layer.GisLayers.Add(holderGisLayer);
                }

                foreach (var holderSpriteLayer in holder.SpriteLayers)
                {
                    layer.SpriteLayers.Add(holderSpriteLayer);
                }

                foreach (var holderGraphLayer in holder.GraphLayers)
                {
                    layer.GraphLayers.Add(holderGraphLayer);
                }
            });
        }

        public void RemoveVisLayers(RenderLayer layer, VisLayerHolder holder)
        {
            holder.Clear();
        }
    }
}
