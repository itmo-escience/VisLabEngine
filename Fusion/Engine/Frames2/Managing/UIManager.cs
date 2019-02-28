﻿using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.SpritesD2D;
using System.Text.RegularExpressions;

namespace Fusion.Engine.Frames2.Managing
{
    public class UIManager
    {
        public UIEventProcessor UIEventProcessor { get; }
        public UIContainer Root { get; }

        public bool DebugEnabled { get; set; }

        public UIManager(RenderSystem rs)
        {
            Root = new FreePlacement(0, 0, rs.Width, rs.Height);
            Root.Name = "Root";

            UIEventProcessor = new UIEventProcessor(Root);

            rs.DisplayBoundsChanged += (s, e) =>
            {
                Root.Width = rs.DisplayBounds.Width;
                Root.Height = rs.DisplayBounds.Height;
            };
        }

        public void Update(GameTime gameTime)
        {
            UIEventProcessor.Update(gameTime);

            foreach (var c in UIHelper.DFSTraverse(Root).Where(c => typeof(UIContainer) != c.GetType()))
            {
                c.InternalUpdate(gameTime);
            }
        }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Clear();

            DrawBFS(layer, Root);
        }

        private void DrawBFS(SpriteLayerD2D layer, UIContainer root)
        {
            var queue = new Queue<UIComponent>();
            queue.Enqueue(root);

            while (queue.Any())
            {
                var c = queue.Dequeue();

                if(!c.Visible) continue;

                if (c is UIContainer container)
                {
                    if (container.NeedClipping)
                    {
                        queue.Enqueue(new Components.StartClippingFlag(container.GetClippingGeometry(layer)));
                    }

                    foreach (var child in container.Children)
                    {
                        queue.Enqueue(child);
                    }

                    if (container.NeedClipping)
                    {
                        queue.Enqueue(new Components.EndClippingFlag());
                    }
                }

                layer.Draw(new TransformCommand(c.GlobalTransform));
                c.Draw(layer);
            }

            if (DebugEnabled)
            {
                queue.Enqueue(root);

                while (queue.Any())
                {
                    var c = queue.Dequeue();

                    if (c is UIContainer container)
                    {
                        foreach (var child in container.Children)
                        {
                            queue.Enqueue(child);
                        }
                    }

                    layer.Draw(new TransformCommand(c.GlobalTransform));
                    c.DebugDraw(layer);
                }
            }

            layer.Draw(TransformCommand.Identity);
        }

        public static bool IsComponentNameValid(string name, UIComponent root, UIComponent ignoredComponent = null)
        {
            foreach (UIComponent component in UIHelper.BFSTraverse(root))
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
