using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS;
using FusionData.DataModel.Public;

namespace FusionVis._0._2
{
    public class GlobeVisualizer : IVisualizer
    {
        public bool CheckValidity()
        {
            return true;
        }

        public void ReCalc()
        {
            if (Tiles == null)
            {
                Tiles = new TilesAtlasLayer(Game.Instance, GlobeCamera.Instance);
            }
            VisHolder.GisLayers.Clear();
            VisHolder.GisLayers.Add(Tiles);

        }

        private TilesAtlasLayer Tiles;

        public List<ISlot> InputSlots { get; } = new List<ISlot>()
        {
            new FiniteSetParameterSlot<TilesGisLayer.MapSource>("MapSource", new List<TilesGisLayer.MapSource>()
            {
                TilesGisLayer.MapSource.BingMap, TilesGisLayer.MapSource.BingMapSatellite, TilesGisLayer.MapSource.DarkV9, TilesGisLayer.MapSource.Yandex, TilesGisLayer.MapSource.YandexSatellite, TilesGisLayer.MapSource.OpenStreetMap
            })
        };
        public VisLayerHolder VisHolder { get; } = new VisLayerHolder();
        public void UpdateVis(GameTime gameTime)
        {
            Tiles.Update(gameTime);
        }
    }
}
