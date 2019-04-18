using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.Graph;
using FusionData.DataModel.Public;
using FusionData;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.OpenStreetMaps;

namespace FusionVis._0._2
{
    class GraphVisualizer : IVisualizer
    {

        #region keys

        private const string nodeIdKey = "NodeId";
        private const string nodeColorHUEKey = "NodeColorHUE";
        private const string nodeColorSaturationKey = "NodeColorSaturation";
        private const string nodeColorBrightnessKey = "NodeColorBrightness";
        private const string nodeColorAlphaKey = "NodeColorAlpha";
        private const string nodeSizeKey = "NodeSize";
        private const string nodeMassKey = "NodeMass";

        private static List<string> NodeChannelNames { get; }= new List<string>()
        {
            nodeIdKey,
            nodeColorHUEKey,
            nodeColorSaturationKey,
            nodeColorBrightnessKey,
            nodeColorAlphaKey,
            nodeSizeKey,
            nodeMassKey
        };

        private const string linkFirstNodeKey = "LinksId1";
        private const string linkSecondNodeKey = "LinksId2";
        private const string linkStrengthKey = "LinksStrength";
        private const string linkLengthKey = "LinksLength";
        private const string linkWidthKey = "LinksWidth";
        private const string linkColorHUEKey = "LinkColorHUE";
        private const string linkColorSaturationKey = "LinkColorSaturation";
        private const string linkColorBrightnessKey = "LinkColorBrightness";
        private const string linkColorAlphaKey = "LinkColorAlpha";

        private List<string> LinkChannelNames { get; } = new List<string>()
        {
            linkFirstNodeKey,
            linkSecondNodeKey,
            linkStrengthKey,
            linkLengthKey,
            linkWidthKey,
            linkColorHUEKey,
            linkColorSaturationKey,
            linkColorBrightnessKey,
            linkColorAlphaKey,
        };

        #endregion

        private const string layoutShaderKey = "LinkColorAlpha";

        public GraphVisualizer()
        {
            InputSlots = new List<ISlot>();
            InputSlots.Add(new ChannelSlot<string>(nodeIdKey));
            InputSlots.Add(new ChannelSlot<float>(nodeColorHUEKey)
            {
                Default = 0,
            });
            InputSlots.Add(new ChannelSlot<float>(nodeColorSaturationKey)
            {
                Default = 1,
            });
            InputSlots.Add(new ChannelSlot<float>(nodeColorBrightnessKey)
            {
                Default = 1,
            });
            InputSlots.Add(new ChannelSlot<float>(nodeColorAlphaKey)
            {
                Default = 1,
            });
            InputSlots.Add(new ChannelSlot<float>(nodeSizeKey)
            {
                Default = 1,
            });
            InputSlots.Add(new ChannelSlot<float>(nodeMassKey)
            {
                Default = 1,
            });

            InputSlots.Add(new ChannelSlot<string>(linkFirstNodeKey));
            InputSlots.Add(new ChannelSlot<string>(linkSecondNodeKey));
            InputSlots.Add(new ChannelSlot<float>(linkStrengthKey)
            {
                Default = 1,
            });
            InputSlots.Add(new ChannelSlot<float>(linkLengthKey)
            {
                Default = 1,
            });
            InputSlots.Add(new ChannelSlot<float>(linkWidthKey)
            {
                Default = 1,
            });
            InputSlots.Add(new ChannelSlot<float>(linkColorHUEKey)
            {
                Default = 1,
            });
            InputSlots.Add(new ChannelSlot<float>(linkColorSaturationKey)
            {
                Default = 1,
            });
            InputSlots.Add(new ChannelSlot<float>(linkColorBrightnessKey)
            {
                Default = 1,
            });
            InputSlots.Add(new ChannelSlot<float>(linkColorAlphaKey)
            {
                Default = 1,
            });


            InputSlots.Add(new ParameterSlot<string>(layoutShaderKey, "Graph/Shaders/SimpleLayout"));
        }


        public bool CheckValidity()
        {
            var iSDict = InputSlots.ToDictionary(a => a.Name, a => a);

            var linkSlots = LinkChannelNames.Select(a => iSDict[a]).ToList();
            var nodeSlots = NodeChannelNames.Select(a => iSDict[a]).ToList();

            return linkSlots.Cast<IChannelSlot>().All(a =>
                       a.IsAssigned && (a.Content.Source == null || a.Content.Source == linkSlots.Cast<IChannelSlot>()
                                            .First(c => c.Content.Source != null).Content.Source)) &&
                   nodeSlots.Cast<IChannelSlot>().All(a =>
                       a.IsAssigned && (a.Content.Source == null || a.Content.Source == nodeSlots.Cast<IChannelSlot>()
                                            .First(c => c.Content.Source != null).Content.Source));

        }

        public void ReCalc()
        {
            InitGraph();
            if (Dirty || AlwaysDirty)
            {
                Dirty = false;
                if (!CheckValidity()) return;
                var iSDict = InputSlots.ToDictionary(a => a.Name, a => a);
                var nodeKeyChannel = ((IChannelSlot) iSDict[nodeIdKey]).Content.Source.KeyChannel;
                var nodeEnum = nodeKeyChannel.GetEnumerable();
                Dictionary<object, Graph.Vertice> nodes = new Dictionary<object, Graph.Vertice>();
                int i = 0;
                foreach (var key in nodeEnum)
                {
                    nodes.Add(((IChannelSlot)iSDict[nodeIdKey]), new Graph.Vertice()
                    {
                        Size = (float)((IChannelSlot)iSDict[nodeSizeKey]).Content.Get(key.Key),
                        Mass = (float)((IChannelSlot)iSDict[nodeMassKey]).Content.Get(key.Key),
                        Color = Color.FromHSB(
                            (float)((IChannelSlot)iSDict[nodeColorHUEKey]).Content.Get(key.Key),
                            (float)((IChannelSlot)iSDict[nodeColorSaturationKey]).Content.Get(key.Key),
                            (float)((IChannelSlot)iSDict[nodeColorBrightnessKey]).Content.Get(key.Key)
                                ).ToVector4(),
                        Force = Vector3.Zero,
                        Acceleration = Vector3.Zero,
                        Velocity = Vector3.Zero,
                        Id = i++,
                        Position = (GraphHelper.RadialRandomVector3D() * 100),
                    });
                }

                var linkKeyChannel = ((IChannelSlot)iSDict[linkFirstNodeKey]).Content.Source.KeyChannel;
                var linkEnum = linkKeyChannel.GetEnumerable();
                Dictionary<object, Graph.Link> links = new Dictionary<object, Graph.Link>();
                foreach (var key in linkEnum)
                {
                    links.Add(key, new Graph.Link
                    {
                        Par1 = nodes[((IChannelSlot)iSDict[linkFirstNodeKey]).Content.Get(key.Key)].Id,
                        Par2 = nodes[((IChannelSlot)iSDict[linkSecondNodeKey]).Content.Get(key.Key)].Id,
                        Strength = (float)((IChannelSlot)iSDict[linkStrengthKey]).Content.Get(key.Key),
                        Width = (float)((IChannelSlot)iSDict[linkWidthKey]).Content.Get(key.Key),
                        Length = (float)((IChannelSlot)iSDict[linkLengthKey]).Content.Get(key.Key),
                        Color = Color.FromHSB(
                            (float)((IChannelSlot)iSDict[linkColorHUEKey]).Content.Get(key.Key),
                            (float)((IChannelSlot)iSDict[linkColorSaturationKey]).Content.Get(key.Key),
                            (float)((IChannelSlot)iSDict[linkColorBrightnessKey]).Content.Get(key.Key)
                                ).ToVector4(),
                    });
                }

                graph.graph.Nodes = nodes.Values.OrderBy(a => a.Id).ToList();
                graph.graph.Links = links.Values.ToList();
                graph.graph.NodesCount = graph.graph.Nodes.Count;
                graph.AddMaxParticles();
            }
        }

        protected GraphLayer graph;
        protected SimpleLayout layout;
        public string LayoutName => (string)InputSlots.Find(a => a.Name == layoutShaderKey).Content;

        void InitGraph()
        {
            try
            {
                graph = new GraphLayer(Game.Instance);
                graph.graph = new Graph();

                graph.Camera = new GreatCircleCamera();
                VisHolder.GraphLayers.Add(graph);
                layout = new SimpleLayout(Game.Instance, graph, LayoutName);
                graph.Initialize();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }


        public void UpdateVis(GameTime gameTime)
        {
            graph.Camera.Update(gameTime);
            if (graph.State == State.RUN)
            {
                layout.Update(gameTime);
            }
        }

        public bool Dirty { get; set; }
        public bool AlwaysDirty { get; set; }
        public List<ISlot> InputSlots { get; }
        public VisLayerHolder VisHolder { get; }

    }
}
