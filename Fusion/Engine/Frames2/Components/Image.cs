using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Managing;
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

        internal override void InvokeMouseMove(UIEventProcessor eventProcessor, MoveEventArgs e)
        {
            base.InvokeMouseMove(eventProcessor, e);
            System.Console.WriteLine($"{Name}.InvokeMouseMove() at ({e.position.X}, {e.position.Y})");
        }

        internal override void InvokeMouseDrag(UIEventProcessor eventProcessor, DragEventArgs e)
        {
            base.InvokeMouseDrag(eventProcessor, e);
            System.Console.WriteLine($"{Name}.InvokeMouseDrag() at ({e.position.X}, {e.position.Y}) with key {e.key}");
        }

        internal override void InvokeMouseDown(UIEventProcessor eventProcessor, ClickEventArgs e)
        {
            base.InvokeMouseDown(eventProcessor, e);
            System.Console.WriteLine($"{Name}.InvokeMouseDown() at ({e.position.X}, {e.position.Y}) with key {e.key}");
        }

        internal override void InvokeMouseUp(UIEventProcessor eventProcessor, ClickEventArgs e)
        {
            base.InvokeMouseUp(eventProcessor, e);
            System.Console.WriteLine($"{Name}.InvokeMouseUp() at ({e.position.X}, {e.position.Y}) with key {e.key}");
        }
    }
}
