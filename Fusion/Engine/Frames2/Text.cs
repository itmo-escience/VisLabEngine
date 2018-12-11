using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Interfaces;
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Frames2
{
    public sealed class Text : IUIComponent, IUIMouseAware
    {
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

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteLayer layer)
        {

        }

        public event MouseEvent MouseIn;
        public event MouseEvent MouseOver;
        public event MouseEvent MouseMove;
        public event MouseEvent MouseOut;
        public event MouseEvent MouseDrag;
        public event MouseEvent MouseDown;
        public event MouseEvent MouseUp;
        public event MouseEvent MouseClick;
    }
}
