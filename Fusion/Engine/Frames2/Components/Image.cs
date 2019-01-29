﻿using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;
using Fusion.Engine.Input;

namespace Fusion.Engine.Frames2.Components
{
    public sealed class Image : UIComponent
    {
        private string _file;
        private float _opacity;
        public Image(float x, float y, float width, float height, string file, float opacity = 1) : base(x, y, width, height)
        {
            _file = file;
            _opacity = opacity;
        }

        public override void Update(GameTime gameTime) { }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(new DrawBitmap(0, 0, Width, Height, _file, _opacity));
        }
    }
}
