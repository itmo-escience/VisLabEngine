using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.Graph;
using FusionData.Data;

namespace FusionVis
{
    public class GraphVisualiser : IVisualizer
    {

        private static string nodeIdKey = "NodeId";
        private static string nodeColorHUEKey = "NodeColorHUE";
        private static string nodeColorSaturationKey = "NodeColorSaturation";
        private static string nodeColorBrightnessKey = "NodeColorBrightness";
        private static string nodeColorAlphaKey = "NodeColorAlpha";
        private static string nodeSizeKey = "NodeSize";
        private static string nodeMassKey = "NodeMass";
        private static string linkFirstNodeKey = "LinksInd1";
        private static string linkSecondNodeKey = "LinksInd2";
        private static string linkStrengthKey = "LinksStrength";
        private static string linkLengthKey = "LinksLength";
        private static string linkWidthKey = "LinksWidth";
        private static string linkColorHUEKey = "LinkColorHUE";
        private static string linkColorSaturationKey = "LinkColorSaturation";
        private static string linkColorBrightnessKey = "LinkColorBrightness";
        private static string linkColorAlphaKey = "LinkColorAlpha";

        private static string nodesSheetKey = "Vertices";
        private static string linksSheetKey = "Edges";

        public Dictionary<string, InputIndexSlot> IndexInputs { get; }= new List<InputIndexSlot>()
        {
            new InputIndexSlot(nodesSheetKey),

            new InputIndexSlot(linksSheetKey),
        }.ToDictionary(a => a.Name, a => a);

        public Dictionary<string, InputSlot> Inputs { get; }=
            new List<InputSlot>()
            {
                new InputSlot(nodeIdKey, DataType.BasicTypes.Integer)
                {
                },
                new InputSlot(nodeColorHUEKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(0.0f, DataType.BasicTypes.Float)
                },
                new InputSlot(nodeColorSaturationKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(0.0f, DataType.BasicTypes.Float)
                },
                new InputSlot(nodeColorBrightnessKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(1.0f, DataType.BasicTypes.Float)
                },
                new InputSlot(nodeColorAlphaKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(1.0f, DataType.BasicTypes.Float)
                },
                new InputSlot(nodeSizeKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(10.0f, DataType.BasicTypes.Float)
                },
                new InputSlot(nodeMassKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(1.0f, DataType.BasicTypes.Float)
                },


                new InputSlot(linkFirstNodeKey, DataType.BasicTypes.Integer)
                {
                },
                new InputSlot(linkSecondNodeKey, DataType.BasicTypes.Integer)
                {
                },
                new InputSlot(linkStrengthKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(1.0f, DataType.BasicTypes.Float)
                },
                new InputSlot(linkLengthKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(100.0f, DataType.BasicTypes.Float)
                },
                new InputSlot(linkWidthKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(0.2f, DataType.BasicTypes.Float)
                },
                new InputSlot(linkColorHUEKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(0.0f, DataType.BasicTypes.Float)
                },
                new InputSlot(linkColorSaturationKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(0.0f, DataType.BasicTypes.Float)
                },
                new InputSlot(linkColorBrightnessKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(1.0f, DataType.BasicTypes.Float)
                },
                new InputSlot(linkColorAlphaKey, DataType.BasicTypes.Float)
                {
                    DefaultGen = () => new DataElement(1.0f, DataType.BasicTypes.Float)
                },

            }.ToDictionary(a => a.Name, a => a);

        public RenderLayer VisLayer { get; protected set; }

        protected GraphLayer graph;
        protected SimpleLayout layout;
        public string LayoutName = "Graph/Shaders/SimpleLayout";

        public void Prepare()
        {
            renderReady = false;
            try
            {
                VisLayer = new RenderLayer(Game.Instance);

                graph = new GraphLayer(Game.Instance);
                graph.graph = new Graph();
                LoadData();
                graph.Camera = new GreatCircleCamera();
                graph.Initialize();
                VisLayer.GraphLayers.Add(graph);
                graph.AddMaxParticles();
                layout = new SimpleLayout(Game.Instance, graph, LayoutName);
                renderReady = true;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        private Dictionary<int, Index> nodeIndexesById;

        public void LoadData()
        {

            dataReady = false;
            if (!ValidateInputs()) return;

            try
            {
                Dictionary<Index, Graph.Vertice> nodes = new Dictionary<Index, Graph.Vertice>();
                nodeIndexesById = new Dictionary<int, Index>();
                foreach (var i in IndexInputs[nodesSheetKey].Channel)
                {
                    nodeIndexesById.Add((int)DataType.BasicTypes.Integer.RecursiveCast(Inputs[nodeIdKey][i]).Item, i);
                    nodes.Add(i, new Graph.Vertice()
                    {
                        Size = Convert.ToSingle(DataType.BasicTypes.Float.RecursiveCast(Inputs[nodeSizeKey][i]).Item),
                        Mass = Convert.ToSingle(DataType.BasicTypes.Float.RecursiveCast(Inputs[nodeMassKey][i]).Item),
                        Color = Color.FromHSB(
                            Convert.ToSingle(DataType.BasicTypes.Float.RecursiveCast(Inputs[nodeColorHUEKey][i]).Item),
                                Convert.ToSingle(DataType.BasicTypes.Float.RecursiveCast(Inputs[nodeColorSaturationKey][i])
                                .Item),
                                    Convert.ToSingle(DataType.BasicTypes.Float.RecursiveCast(Inputs[nodeColorBrightnessKey][i])
                                .Item)).ToVector4(),
                        Force = Vector3.Zero,
                        Acceleration = Vector3.Zero,
                        Velocity = Vector3.Zero,
                        Id = Convert.ToInt32(i.Ind),
                        Position = (GraphHelper.RadialRandomVector3D() * 100),
                        //Dummy = new Vector3(int.Parse(userData[kv.Key][0]), 0, 0),
                    });
                }

                List<Graph.Link> links = new List<Graph.Link>();
                foreach (var i in IndexInputs[linksSheetKey].Channel)
                {
                    links.Add(new Graph.Link
                    {
                        Par1 = (int) nodeIndexesById[Convert.ToInt32(Inputs[linkFirstNodeKey][i].Item)].Ind,
                        Par2 = (int) nodeIndexesById[Convert.ToInt32(Inputs[linkSecondNodeKey][i].Item)].Ind,
                        Strength = Convert.ToSingle(DataType.BasicTypes.Float.RecursiveCast(Inputs[linkStrengthKey][i])
                            .Item),
                        Width = Convert.ToSingle(DataType.BasicTypes.Float.RecursiveCast(Inputs[linkWidthKey][i]).Item),
                        Length = Convert.ToSingle(DataType.BasicTypes.Float.RecursiveCast(Inputs[linkLengthKey][i]).Item),
                        Color = Color.FromHSB(
                            Convert.ToSingle(DataType.BasicTypes.Float.RecursiveCast(Inputs[linkColorHUEKey][i]).Item),
                            Convert.ToSingle(DataType.BasicTypes.Float.RecursiveCast(Inputs[linkColorSaturationKey][i])
                                .Item),
                            Convert.ToSingle(DataType.BasicTypes.Float.RecursiveCast(Inputs[linkColorBrightnessKey][i])
                                .Item)).ToVector4(),
                    });
                }

                graph.graph.Nodes = nodes.Values.ToList();
                graph.graph.Links = links;
                graph.graph.NodesCount = graph.graph.Nodes.Count;
                dataReady = true;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

        }

        public void UpdateFrame(GameTime gameTime)
        {
            graph.Camera.Update(gameTime);
            if (graph.State == State.RUN)
            {
                layout.Update(gameTime);
            }
        }

        private bool renderReady;
        private bool dataReady;

        public bool Ready => renderReady && dataReady;
        public TargetTexture Render => VisLayer.Target;

        public void SetScreenArea(int x, int y, int width, int height)
        {
            var oldTarget = VisLayer.Target;
            oldTarget.Dispose();
            var newTarget = VisLayer.Target = new TargetTexture(Game.Instance.RenderSystem, width, height,
                TargetFormat.LowDynamicRangeMSAA);
        }


        public bool ValidateInputs()
        {
            var b = true;
            b &= Inputs[linkFirstNodeKey].Channel.DataType.IsOfType(DataType.BasicTypes.Integer);
            b &= Inputs[linkSecondNodeKey].Channel.DataType.IsOfType(DataType.BasicTypes.Integer);

            return b;
        }
    }

}
