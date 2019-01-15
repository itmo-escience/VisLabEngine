using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    public sealed class Image : UIComponent, IUIMouseAware
    {
        private IBrushD2D _brush = new SolidBrushD2D(Color4.White);
        public Image(float x, float y, float width, float height) : base(x, y, width, height)
        {

        }

        public override void Update(GameTime gameTime) { }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(new Rect(0, 0, Width, Height, _brush));
            layer.Draw(new Line(new Vector2(), new Vector2(Width, Height), _brush));
            layer.Draw(new Line(new Vector2(Width, 0), new Vector2(0, Height), _brush));
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
