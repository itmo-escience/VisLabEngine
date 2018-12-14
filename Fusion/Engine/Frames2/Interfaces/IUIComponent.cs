using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Frames2.Interfaces
{
    public interface IUIComponent
    {
        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        RectangleF BoundingBox { get; }
        bool Visible { get; set; }
        Matrix Transform { get; set; }
        object Tag { get; set; }
        string Name { get; }
        // TODO: Anchors

        IEnumerable<IUIController> Controllers { get; }

        void Update(GameTime gameTime);
        void Draw(SpriteLayer layer);
    }
}
