using Fusion.Engine.Common;
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Frames2.Managing
{
    public class UIManager
    {
        public UIPainter UIPainter { get; }
        public UIEventProcessor UIEventProcessor { get; }
        public UIContainer Root;

        internal UIManager()
        {
            UIPainter = new UIPainter(Root);
            UIEventProcessor = new UIEventProcessor(Root);


            var device = Game.Instance.GraphicsDevice.Device.QueryInterface<SharpDX.Direct3D11.Device1>();

            // Query for the adapter and more advanced DXGI objects.
            SharpDX.DXGI.Device2 dxgiDevice2 = device.QueryInterface<SharpDX.DXGI.Device2>();
            SharpDX.DXGI.Adapter dxgiAdapter = dxgiDevice2.Adapter;
            SharpDX.DXGI.Factory2 dxgiFactory2 = dxgiAdapter.GetParent<SharpDX.DXGI.Factory2>();

            SharpDX.Direct2D1.Device d2dDevice = new SharpDX.Direct2D1.Device(dxgiDevice2);
            var d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);


        }

        public void Update(GameTime gameTime)
        {
            UIEventProcessor.Update(gameTime);
        }

        public void Draw(SpriteLayer layer)
        {
            layer.Clear();

            UIPainter.Draw(layer);
        }
    }
}
