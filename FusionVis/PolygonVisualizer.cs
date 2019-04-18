using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using FusionData.DataModel.DataTypes;
using FusionData.DataModel.Public;
using DVector2 = Fusion.Engine.Graphics.GIS.GlobeMath.DVector2;
using FDDvector2 = FusionData.DataModel.DataTypes.DVector2;

namespace FusionVis._0._2
{
    public class PolygonVisualizer : IVisualizer
    {
        public bool CheckValidity()
        {
            var iSDict = InputSlots.ToDictionary(a => a.Name, a => a);

            bool flag = iSDict.Values.Where(a => a is IChannelSlot).Cast<IChannelSlot>().All(a =>
            {
                return (a.Content == null && (object) a.Default != null) ||
                       (a.Content?.Source != null && (a.Content.Source) ==
                        ((IChannelSlot) iSDict.Values.First(s => s.Name == PolygonKey)).Content?.Source);
            });

            return flag;

        }

        public void ReCalc()
        {
            var s = InputSlots.Find(a => a.Name.Equals(PolygonGeometryKey));
            bool isMultiPolygons = (bool)InputSlots.Find(a => a.Name == MultiGeometriesKey).Content;
            if (isMultiPolygons)
            {
                if (s == null || s.Content is ChannelSlot<FDDvector2[][]>)
                {
                    InputSlots.Remove(s);
                    InputSlots.Add((ISlot) new ChannelSlot<FDDvector2[][][]>(PolygonGeometryKey));
                }
            }
            else
            {
                if (s == null || s.Content is ChannelSlot<FDDvector2[][][]>)
                {
                    InputSlots.Remove(s);
                    InputSlots.Add((ISlot)new ChannelSlot<FDDvector2[][]>(PolygonGeometryKey));
                }
            }
            VisHolder.Clear();
            if (CheckValidity())
            {
                var isDict = InputSlots.ToDictionary(a => a.Name, a => a);
                foreach (var id in ((IChannelSlot<string>)isDict[PolygonKey]).Content.GetEnumerable())
                {
                    //process feature

                    if (!isMultiPolygons)
                    {
                        var geom = ((IChannelSlot<FDDvector2[][]>)isDict[PolygonGeometryKey]).Get(id.Key);
                        var geomArr = (FDDvector2[][]) geom;
                        //var ga =
                        var layer = PolyGisLayer.CreateExtruded(Game.Instance, geomArr[0].Select(a => new DVector2(a.X, a.Y)).ToArray(),
                            Color.FromHSB(
                                (float) ((IChannelSlot<float>) isDict[PolygonColorHUEKey]).Get(id.Key),
                                (float) ((IChannelSlot<float>) isDict[PolygonColorSatKey]).Get(id.Key),
                                (float) ((IChannelSlot<float>) isDict[PolygonColorVarKey]).Get(id.Key),
                                (float) ((IChannelSlot<float>) isDict[PolygonColorAlphaKey]).Get(id.Key)
                            ),
                            (float)((IChannelSlot<float>)isDict[PolygonHeightKey]).Get(id.Key),
                            false, geomArr.Skip(1).Select(list => list.Select(a => new DVector2(a.X, a.Y)).ToArray()).ToList()
                        );
                        VisHolder.GisLayers.Add(layer);
                    }
                    else
                    {
                        var geom = ((IChannelSlot<FDDvector2[][][]>)isDict[PolygonGeometryKey]).Get(id.Key);
                        var geomArrs = (FDDvector2[][][])geom;
                        foreach (var geomArr in geomArrs)
                        {
                            var layer = PolyGisLayer.CreateExtruded(Game.Instance, geomArr[0].Select(a => new DVector2(a.X, a.Y)).ToArray(),
                                Color.FromHSB(
                                    (float) ((IChannelSlot<float>) isDict[PolygonColorHUEKey]).Get(id.Key),
                                    (float) ((IChannelSlot<float>) isDict[PolygonColorSatKey]).Get(id.Key),
                                    (float) ((IChannelSlot<float>) isDict[PolygonColorVarKey]).Get(id.Key),
                                    (float) ((IChannelSlot<float>) isDict[PolygonColorAlphaKey]).Get(id.Key)
                                ),
                                (float)((IChannelSlot<float>)isDict[PolygonHeightKey]).Get(id.Key),
                                false, geomArr.Skip(1).Select(list => list.Select(a => new DVector2(a.X, a.Y)).ToArray()).ToList()
                            );
                            VisHolder.GisLayers.Add(layer);
                        }
                    }
                }

                var l = (PolyGisLayer) VisHolder.GisLayers.First();
                l.MergeList(VisHolder.GisLayers.Skip(1).Cast<PolyGisLayer>());
                foreach (var gisLayer in VisHolder.GisLayers.Skip(1))
                {
                    ((PolyGisLayer)gisLayer).Dispose();
                }
                VisHolder.GisLayers.Clear();
                VisHolder.GisLayers.Add(l);
            }
        }

        public const string MultiGeometriesKey = "Is MultiPolygon";
        public const string PolygonKey = "PolyKeys";
        public const string PolygonGeometryKey = "PolyGeometries";
        public const string PolygonColorHUEKey = "ColorHUE";
        public const string PolygonColorSatKey = "ColorSaturation";
        public const string PolygonColorVarKey = "ColorVar";
        public const string PolygonColorAlphaKey = "ColorAlpha";
        public const string PolygonHeightKey = "Height";

        public List<ISlot> InputSlots { get; } = new List<ISlot>()
        {
            new ChannelSlot<string>(PolygonKey),
            new ChannelSlot<float>(PolygonColorHUEKey) {Default = 0},
            new ChannelSlot<float>(PolygonColorSatKey) {Default = 1},
            new ChannelSlot<float>(PolygonColorVarKey) {Default = 1},
            new ChannelSlot<float>(PolygonColorAlphaKey) {Default = 1},
            new ChannelSlot<float>(PolygonHeightKey) {Default = 0},
            new ParameterSlot<bool>(MultiGeometriesKey, false),
        };
        public VisLayerHolder VisHolder { get; } = new VisLayerHolder();
        public void UpdateVis(GameTime gameTime)
        {
            foreach (var visHolderGisLayer in VisHolder.GisLayers)
            {
                visHolderGisLayer.Update(gameTime);
            }
        }
    }
}
