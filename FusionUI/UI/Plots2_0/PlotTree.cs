using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Elements;

namespace FusionUI.UI.Plots2_0
{
    public class PlotTree : ScalableFrame
    {
		protected PlotTree()
		{
		}
		private ScalableFrame TreePanel;
        public PlotTree(FrameProcessor ui, float x, float y, float w, float h, Color backColor) : base(ui, x, y, w, h, "", backColor)
        {
            TreePanel = new ScalableFrame(ui, 0, 0, w, h - UIConfig.UnitMenuButtonHeight, "",
                Color.Zero)
            {                
            };
            BottomPanel = new ScalableFrame(ui, 0, h - UIConfig.UnitMenuButtonHeight, w,
                UIConfig.UnitMenuButtonHeight, "", UIConfig.SettingsColor)
            {
            };
            TreePanel.ActionUpdate += time =>
            {
                TreePanel.UnitHeight = UnitHeight - UIConfig.UnitMenuButtonHeight;
            };
            BottomPanel.ActionUpdate += time =>
            {
                BottomPanel.UnitY = UnitHeight - UIConfig.UnitMenuButtonHeight;
            };
            Add(TreePanel);
            Add(BottomPanel);

            Root = new TreeNode(ui, 0, 0, 0, 0, "", Color.Zero)
            {                
                IsExpand = true,
            };

            Root.MouseWheel += (s, e) =>
            {
                if (Root.GlobalRectangle.Y + Root.Height > TreePanel.GlobalRectangle.Y + TreePanel.Height && e.Wheel < 0)
                {
                    Root.Y += e.Wheel / 5;
                    if (Root.GlobalRectangle.Y > TreePanel.GlobalRectangle.Y)
                        Root.Y = 0;
                }
                if (Root.GlobalRectangle.Y < TreePanel.GlobalRectangle.Y && e.Wheel > 0)
                {
                    Root.Y += e.Wheel / 5;
                }
            };

            TreePanel.Add(Root);            
        }

        protected List<Button> buttons = new List<Button>();

        public Button AddButton(Action<bool> action, Texture pic = null, string text = "", bool active = false,
            bool toggleable = true)
        {
            var unitSize = UIConfig.UnitMenuButtonHeight;
            Button ret;
            if (toggleable)
            {
                ret = new Button(ui, unitSize * BottomPanel.Children.Count, 0, unitSize, unitSize, text,
                    UIConfig.ActiveColor,
                    Color.Zero, pic, pic, action, active)
                {
                    TextAlignment = Alignment.MiddleCenter,
                    ImageMode = FrameImageMode.Centered,
                };
            }
            else
            {
                ret = new Button(ui, unitSize * BottomPanel.Children.Count, 0, unitSize, unitSize, text, Color.Zero,
                    UIConfig.ActiveColor, 200)
                {
                    Image = pic,
                    TextAlignment = Alignment.MiddleCenter,
                    ButtonAction = action,
                    ImageMode = FrameImageMode.Centered,
                };
            }
            BottomPanel.Add(ret);
            buttons.Add(ret);
            return ret;
        }

        public ScalableFrame BottomPanel;

        public TreeNode Root;

        public PlotContainer Plot1d, Plot2d;
		[XmlIgnore]
		public Action<bool> Activate;
		[XmlIgnore]
		public Action OnTreeUpdate;

        public SerializableDictionary<PlotPoint, CheckboxNode> pointNodes = new SerializableDictionary<PlotPoint, CheckboxNode>();

        public void AddPoint(PlotPoint point, bool expand = false)
        {
            var name = point.Name;            
            CheckboxNode pointNode = new CheckboxNode(ui, 0, 0, this.UnitWidth, UIConfig.UnitScenarioConfigLayersHeight, name, UIConfig.SettingsColor,
                b =>
                {
                    point.IsActive = b;
                    OnTreeUpdate?.Invoke();
                })
            {
                Name = name,
                UnitSizeExpandButton = 12,
                ExpandedPicture = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_close-list"),
                CollapsedPicture = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_open-list"),
                UnitImageOffsetX = this.UnitWidth - UIConfig.UnitSettingsPanelSizeCheckbox,
                backColorMainNode = UIConfig.BackColorLayer,
                TextAlignment = Alignment.BaselineLeft,
                OffsetChild = 12,                                
                Tooltip = name,
            };
            pointNodes.Add(point, pointNode);
            pointNode.ActionUpdate += time => { pointNode.Checkbox.IsChecked = point.IsActive; };

            pointNode.ExpandNodes(expand);
            pointNode.IsExpand = expand;

            Root.addNode(pointNode);

            foreach (var plotData in point.Data)
            {
                var node = AddLayer(point, plotData);
                pointNode.addNode(node);
                node.Width = pointNode.Width - node.X;
                //node.ImageOffsetX
            }
        }

        public virtual CheckboxNode AddLayer(PlotPoint point, PlotData data)
        {
            bool selected = false;
            var LayerNode = new CheckboxNode(ui, 0, 0, this.UnitWidth - 3, UIConfig.UnitScenarioConfigLayersHeight, data.Variable.NiceName, Color.Zero,
                b =>
                {
                    data.IsActive = b;
                    OnTreeUpdate?.Invoke();
                })
            {
                Name = data.Variable.NiceName,
                UnitSizeExpandButton = 12,
                ExpandedPicture = data.Depths.Count > 1 ? ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_close-list") : null,
                CollapsedPicture = data.Depths.Count > 1 ? ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_open-list") : null,
                UnitImageOffsetX = this.UnitWidth - UIConfig.UnitSettingsPanelSizeCheckbox - 3,
                backColorMainNode = UIConfig.BackColorLayer,
                TextAlignment = Alignment.BaselineLeft,
                OffsetChild = 12,
                Tooltip = data.Variable.NiceName,
            };
            LayerNode.ActionUpdate += time =>
            {
                LayerNode.Active = data.IsLoaded;
                LayerNode.Checkbox.IsChecked = data.IsActive;
            };
            if (data.Depths.Count > 1)
            {
                foreach (var depth in data.Depths)
                {
                    double d = depth;
                    CheckboxNode depthNode = new CheckboxNode(ui, 0, 0, this.UnitWidth, UIConfig.UnitScenarioConfigLayersHeight, depth.ToString("0.##") + data.DepthUnits, Color.Zero,
                        b =>
                        {
                            if (b) data.ActiveDepths.Add(d);
                            else data.ActiveDepths.Remove(d);
                            OnTreeUpdate?.Invoke();
                        })
                    {
                        Name = depth.ToString("0.##") + data.DepthUnits,
                        TextAlignment = Alignment.BaselineLeft,
                        backColorMainNode = UIConfig.BackColorLayer,
                        UnitImageOffsetX = this.UnitWidth - UIConfig.UnitSettingsPanelSizeCheckbox,
                    };
                    depthNode.ActionUpdate += time =>
                    {
                        depthNode.Active = data.IsLoaded;
                        depthNode.Checkbox.IsChecked = data.ActiveDepths.Contains(d);
                    };
                    LayerNode.addNode(depthNode);
                }                                
            }
            return LayerNode;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Root.GlobalRectangle.Y > TreePanel.GlobalRectangle.Y) Root.Y = 0;
            if (Root.GlobalRectangle.Y < TreePanel.GlobalRectangle.Y && Root.GlobalRectangle.Bottom < TreePanel.GlobalRectangle.Bottom) Root.Y = TreePanel.Height - Root.Height;
        }

        public void RemovePoint(PlotPoint point)
        {
            if (pointNodes.ContainsKey(point))
            {
                var oldNode = pointNodes[point];
                Root.removeNode(oldNode);
                pointNodes.Remove(point);
            }
        }

        public void UpdatePoint(PlotPoint point)
        {
            RemovePoint(point);
            AddPoint(point);            
        }

        public void UpdateAll()
        {
            foreach (var point in pointNodes.Keys)
            {
                UpdatePoint(point);
            }
        }



        #region treeScroll
        private TreeNode selected;
        private int nodeStartPosition, nodeLastPosition;
        public void AddMovableActions(TreeNode rootNode, TreeNode newLayer)
        {
            newLayer.ActionDown += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsAltClick) return;
                selected = newLayer;
                nodeStartPosition = newLayer.Y;
                nodeLastPosition = args.Y;
                flag |= true;
            };

            newLayer.ActionLost += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsAltClick || selected != newLayer) return;
                newLayer.Y = nodeStartPosition;
                selected = null;
            };

            newLayer.ActionDrag += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
            {
                if (selected == newLayer)
                {
                    //int pos = 
                    selected.Y = (int)(selected.Y + args.Position.Y - nodeLastPosition);
                    nodeLastPosition = args.Position.Y;
                    int i = rootNode.listNode.FindIndex(n => n == selected);
                    bool changed = true;
                    changed = false;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        TreeNode node = rootNode.listNode[j] as TreeNode;
                        if (node != null && selected.Y < node.Y && nodeStartPosition > node.Y)
                        {
                            node.Y = node.Y + selected.Height;
                            nodeStartPosition -= node.Height;
                            rootNode.listNode[j] = selected;
                            rootNode.listNode[i] = node;
                            i = j;
                            changed = true;
                            continue;
                        }
                    }
                    for (int j = i + 1; j < rootNode.listNode.Count; j++)
                    {
                        {
                            TreeNode node = rootNode.listNode[j] as TreeNode;
                            if (node != null && selected.Y > node.Y && nodeStartPosition < node.Y)
                            {
                                node.Y = node.Y - selected.Height;
                                nodeStartPosition += node.Height;
                                rootNode.listNode[j] = selected;
                                rootNode.listNode[i] = node;
                                i = j;
                                changed = true;
                                continue;
                            }
                        }
                    }
                    //if (holder != null && changed)
                    //{
                    //    holder.UpdateOrder(rootNode.listNode.ConvertAll(input => input.Name));
                    //}
                    flag = true;
                }
            };
        }
        #endregion
    }
}
