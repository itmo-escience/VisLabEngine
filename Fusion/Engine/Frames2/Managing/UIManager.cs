using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.SpritesD2D;
using System.Text.RegularExpressions;

namespace Fusion.Engine.Frames2.Managing
{
    internal class RootSlot : ISlot
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public float X => 0;
        public float Y => 0;
        public float Angle => 0;
        public float Width { get; internal set; }
        public float Height { get; internal set; }
        public float AvailableWidth => Width;
        public float AvailableHeight => Height;
        public float DesiredWidth { get; set; }
        public float DesiredHeight { get; set; }
        public Matrix3x2 Transform => Matrix3x2.Identity;
        public bool Clip => false;
        public bool Visible => true;
        public UIContainer<ISlot> Parent => null;
        public UIComponent Component { get; }
        public SolidBrushD2D DebugBrush => new SolidBrushD2D(Color4.White);
        public TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);

        internal RootSlot(float width, float height, UIContainer<ISlot> rootContainer)
        {
            Width = width;
            Height = height;
            Component = rootContainer;
            rootContainer.Placement = this;
        }
    }

    public class UIManager
    {
        //public UIEventProcessor UIEventProcessor { get; }
        private RootSlot _rootSlot;
        public UIContainer<ISlot> Root { get; }

        public bool DebugEnabled { get; set; }

        public UIManager(RenderSystem rs)
        {
            Root = new FreePlacement();
            Root.Name = "Root";

            _rootSlot = new RootSlot(rs.DisplayBounds.Width, rs.DisplayBounds.Height, Root);

          //  UIEventProcessor = new UIEventProcessor(Root);

            rs.DisplayBoundsChanged += (s, e) =>
            {
                _rootSlot.Width = rs.DisplayBounds.Width;
                _rootSlot.Height = rs.DisplayBounds.Height;
            };
        }

        public void Update(GameTime gameTime)
        {
            //UIEventProcessor.Update(gameTime);

            foreach (var c in UIHelper.DFSTraverse(Root))
            {
                c.Update(gameTime);
            }
        }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Clear();

            layer.Draw(TransformCommand.Identity);
            DrawRecursive(layer, Root, Matrix3x2.Identity);
            layer.Draw(TransformCommand.Identity);
        }

        private void DrawRecursive(SpriteLayerD2D layer, UIComponent component, Matrix3x2 Transform)
        {
            if (!component.Placement.Visible) return;

            var globalTransform = Transform * component.Placement.LocalTransform() * component.Placement.Transform;

            layer.Draw(new TransformCommand(globalTransform));

            if (component.Placement.Clip)
            {
                var geom = component.Placement.GetClippingGeometry(layer);
                layer.Draw(new StartClippingAlongGeometry(geom, AntialiasModeD2D.Aliased));
            }

            component.Draw(layer);

            if (component is UIContainer<ISlot> container)
            {

                foreach (var child in container.Slots)
                {
                    DrawRecursive(layer, child.Component, globalTransform);
                }
            }

            if (component.Placement.Clip)
            {
                layer.Draw(new EndClippingAlongGeometry());
            }
        }

        public static bool IsComponentNameValid(string name, UIComponent root, UIComponent ignoredComponent = null)
        {
            foreach (var component in UIHelper.BFSTraverse(root))
            {
                if (component == root) continue;
                if ((component.Name == name) && (component != ignoredComponent)) return false;
            }
            return true;
        }

        public static void MakeComponentNameValid(UIComponent component, UIComponent root, UIComponent ignoredComponent = null)
        {
            string name = component.Name;

            if (IsComponentNameValid(name, root, ignoredComponent)) return;

            Regex nameEnd = new Regex(@"_\d+$");

            string nameBase;
            int index;
            if (!nameEnd.IsMatch(name))
            {
                nameBase = name;
                index = 1;
            }
            else
            {
                int underscoreIndex = name.LastIndexOf('_');
                nameBase = name.Remove(underscoreIndex, name.Length - underscoreIndex);
                index = int.Parse(name.Substring(underscoreIndex + 1));
            }

            name = nameBase + '_' + index.ToString();
            while (!IsComponentNameValid(name, root, ignoredComponent))
            {
                index++;
                name = nameBase + '_' + index.ToString();
            }

            component.Name = name;
        }
    }
}
