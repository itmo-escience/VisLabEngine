using System;
using System.IO;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Factories;

namespace FusionUI.UI.Elements.DropDown
{
    public class DropDownSelectorTextureRow : DropDownSelectorRow
    {
        private string value;
        private ScalableFrame front;

        public Func<string, Texture> TextureFunc = s =>
        {
            if (s == "<s>Add")
            {
                return Game.Instance.Content.Load<DiscTexture>(@"UI-new\fv-icons_add");
            }
            else if (s.StartsWith("<u>"))
            {
                string name = s.Substring(3);
                if (!PaletteFactory.CachedPalettes.ContainsKey(name))
                {
                    using (var stream = File.OpenRead(name))
                    {
                        PaletteFactory.CachedPalettes[name] = new UserTexture(Game.Instance.RenderSystem, stream, false);
                    }
                }
                return PaletteFactory.CachedPalettes[name];
            }

            return new DiscTexture(Game.Instance.RenderSystem, Game.Instance.Content.Load<Texture2D>(s));
            //return ApplicationInterface.Instance.Game.Content.Load<DiscTexture>(s);
        };

        public override void Initialize(float x, float y, float w, float h, string text, Color backColor)
        {
            ImageMode = FrameImageMode.Tiled;
            Image = new DiscTexture(Game.Instance.RenderSystem, Game.Instance.Content.Load<Texture2D>(@"ui-new\fv_palette_bg.png"));

            front = new ScalableFrame(0, 0, w, h, "", Color.Zero)
            {
                ImageMode = FrameImageMode.Stretched,
            };
            Add(front);
            base.Initialize (x, y, w, h, text, backColor);
        }

        public override string Value {
            get
            {
                return value;
            }
            set
            {
                front.ImageMode = value.StartsWith("<s>") ? FrameImageMode.Fitted : FrameImageMode.Stretched;
                this.value = value;
                front.Image = TextureFunc(value);
            } }
    }
}
