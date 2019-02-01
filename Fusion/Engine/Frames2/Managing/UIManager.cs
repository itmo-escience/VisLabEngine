using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.SpritesD2D;

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

            UIEventProcessor = new UIEventProcessor(Root);
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
                    foreach (var child in container.Children)
                    {
                        queue.Enqueue(child);
                    }

                    if (container.NeedClipping)
                    {
                        layer.Draw(new StartClippingAlongGeometry(container.GetClippingGeometry(layer), AntialiasModeD2D.Aliased));
                        queue.Enqueue(new Components.EndClippingFlag());
                    }
                }

                layer.Draw(new TransformCommand(c.GlobalTransform));
                c.Draw(layer);

                if (DebugEnabled)
                {
                    c.DebugDraw(layer);
                }
            }

            layer.Draw(TransformCommand.Identity);
        }
    }
}
