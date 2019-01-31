using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;
using Fusion.Engine.Input;
using SharpDX.Direct2D1;

namespace Fusion.Engine.Frames2.Components
{
    public sealed class Image : UIComponent
    {
        private readonly float _opacity;
        private readonly DrawBitmap _image;

        public Image(float x, float y, string file, float opacity = 1) : base(x, y)
        {
            _opacity = opacity;

            var source = System.Drawing.Image.FromFile(file);
            Width = source.Width;
            Height = source.Height;

            _image = new DrawBitmap(0, 0, source, _opacity);

        }

        public override void Update(GameTime gameTime) { }

        public override void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(_image);
        }

        internal override void InvokeMouseMove(UIEventProcessor eventProcessor, MoveEventArgs e)
        {
            base.InvokeMouseMove(eventProcessor, e);
            //System.Console.WriteLine($"{Name}.InvokeMouseMove() at ({e.position.X}, {e.position.Y})");
        }

        internal override void InvokeMouseDrag(UIEventProcessor eventProcessor, DragEventArgs e)
        {
            base.InvokeMouseDrag(eventProcessor, e);
            //System.Console.WriteLine($"{Name}.InvokeMouseDrag() at ({e.position.X}, {e.position.Y}) with key {e.key}");
        }

        internal override void InvokeMouseDown(UIEventProcessor eventProcessor, ClickEventArgs e)
        {
            base.InvokeMouseDown(eventProcessor, e);
            //System.Console.WriteLine($"{Name}.InvokeMouseDown() at ({e.position.X}, {e.position.Y}) with key {e.key}");
        }

        internal override void InvokeMouseUp(UIEventProcessor eventProcessor, ClickEventArgs e)
        {
            base.InvokeMouseUp(eventProcessor, e);
            //System.Console.WriteLine($"{Name}.InvokeMouseUp() at ({e.position.X}, {e.position.Y}) with key {e.key}");
        }

        internal override void InvokeClick(UIEventProcessor eventProcessor, ClickEventArgs e)
        {
            base.InvokeClick(eventProcessor, e);
            //System.Console.WriteLine($"{Name}.InvokeClick() at ({e.position.X}, {e.position.Y}) with key {e.key}");
        }

        internal override void InvokeDoubleClick(UIEventProcessor eventProcessor, ClickEventArgs e)
        {
            base.InvokeDoubleClick(eventProcessor, e);
            //System.Console.WriteLine($"{Name}.InvokeDoubleClick() at ({e.position.X}, {e.position.Y}) with key {e.key}");
        }

        internal override void InvokeKeyDown(UIEventProcessor eventProcessor, Managing.KeyEventArgs e)
        {
            base.InvokeKeyDown(eventProcessor, e);
            //System.Console.WriteLine($"{Name}.InvokeKeyDown() with key {e.key}");
        }

        internal override void InvokeKeyUp(UIEventProcessor eventProcessor, Managing.KeyEventArgs e)
        {
            base.InvokeKeyUp(eventProcessor, e);
            //System.Console.WriteLine($"{Name}.InvokeKeyUp() with key {e.key}");
        }

        internal override void InvokeKeyPress(UIEventProcessor eventProcessor, Managing.KeyEventArgs e)
        {
            base.InvokeKeyPress(eventProcessor, e);
            //System.Console.WriteLine($"{Name}.InvokeKeyPress() with key {e.key}");
        }

        internal override void InvokeScroll(UIEventProcessor eventProcessor, ScrollEventArgs e)
        {
            base.InvokeScroll(eventProcessor, e);
            //System.Console.WriteLine($"{Name}.InvokeScroll() at ({e.position.X}, {e.position.Y}) with delta {e.wheelDelta}");
        }

        internal override void InvokeEnter(UIEventProcessor eventProcessor)
        {
            base.InvokeEnter(eventProcessor);
            //System.Console.WriteLine($"{Name}.InvokeEnter()");
        }

        internal override void InvokeLeave(UIEventProcessor eventProcessor)
        {
            base.InvokeLeave(eventProcessor);
            //System.Console.WriteLine($"{Name}.InvokeLeave()");
        }
    }
}
