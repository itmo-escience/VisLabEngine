﻿using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Frames2.Components
{
    internal sealed class StartClippingFlag : UIComponent
    {
        private PathGeometryD2D _pathGeometry;

        public StartClippingFlag(PathGeometryD2D pathGeometry) : base(0, 0, 0, 0)
        {
            _pathGeometry = pathGeometry;
        }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(new StartClippingAlongGeometry(_pathGeometry, AntialiasModeD2D.Aliased));
        }

        public override void Update(GameTime gameTime) { }
    }

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