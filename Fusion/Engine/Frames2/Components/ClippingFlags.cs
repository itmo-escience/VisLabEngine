using System.ComponentModel;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Components
{
    internal sealed class StartClippingFlag : UIComponent
    {
        private readonly PathGeometryD2D _pathGeometry;

        public StartClippingFlag(PathGeometryD2D pathGeometry)
        {
            _pathGeometry = pathGeometry;
        }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(new StartClippingAlongGeometry(_pathGeometry, AntialiasModeD2D.Aliased));
        }

        public ISlot Placement { get; set; }
        public object Tag { get; set; }
        public string Name { get; set; }
        public void Update(GameTime gameTime) { }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    internal sealed class EndClippingFlag : UIComponent
    {
        public void Draw(SpriteLayerD2D layer)
        {
            layer.Draw(new EndClippingAlongGeometry());
        }

        public ISlot Placement { get; set; }
        public object Tag { get; set; }
        public string Name { get; set; }
        public void Update(GameTime gameTime) { }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
