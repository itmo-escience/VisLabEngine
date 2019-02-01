using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Frames2.Components
{
    internal sealed class EndClippingFlag : UIComponent
    {
        public EndClippingFlag() : base(0, 0, 0, 0)
        {
        }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(new EndClippingAlongGeometry());
        }

        public override void Update(GameTime gameTime) { }
    }
}
