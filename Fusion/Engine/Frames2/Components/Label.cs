using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public sealed class Label : UIComponent, IUIMouseAware
    {
        public string Text { get; set; }
        private Graphics.SpritesD2D.Label _label;

        public Label(string text, float x, float y, float width, float height) : base(x, y, width, height)
        {
            Text = text;
            _label = new Graphics.SpritesD2D.Label(
                "text",
                new RectangleF(x, y, width, height),
                new TextFormatD2D("Calibri", 20),
                new SolidBrushD2D(Color4.White)
            );
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(_label);
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
