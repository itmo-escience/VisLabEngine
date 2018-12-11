using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Interfaces;
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Frames2
{
    public class UIContainer : IUIComponent
    {
        public IEnumerable<IUIComponent> Children;
        public ChildrenLayout Layout { get; internal set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public RectangleF BoundingBox { get; }
        public bool Visible { get; set; }
        public Matrix Transform { get; set; }
        public object Tag { get; set; }
        public string Name { get; }
        public IEnumerable<IUIController> Controllers { get; }

        public void Add(IUIComponent child)
        {

        }

        public bool Remove(IUIComponent child)
        {
            return false;
        }

        public bool Remove(int index)
        {
            return false;
        }

        public void Draw(SpriteLayer layer) { }

        public void Update(GameTime gameTime)
        {

        }
    }

    public enum ChildrenLayout
    {
        Free,
        Grid,
        HStack,
        VStack
    }
}
