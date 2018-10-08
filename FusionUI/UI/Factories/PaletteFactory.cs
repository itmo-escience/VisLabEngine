using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI.Elements;
using FusionUI.UI.Elements.DropDown;
using Button = FusionUI.UI.Elements.Button;
using Color = Fusion.Core.Mathematics.Color;
using System.Drawing;
using System.Drawing.Drawing2D;
using Fusion.Drivers.Graphics;

namespace FusionUI.UI.Factories
{
    public class HorizontalPaletteRender : ScalableFrame
    {        
        private FrameProcessor ui;

        public float maxPosition;
        public float minPosition;

        public Texture PaletteImage;

        public HorizontalPaletteRender(FrameProcessor ui, float x, float y, float w, float h, string textureName) : base(ui, x, y, w, h, "", Color.Zero)
        {
            this.ui = ui;
            maxPosition = 1;
            minPosition = 0;
            PaletteImage = new DiscTexture(ui.Game.RenderSystem, ui.Game.Content.Load<Texture2D> (textureName));
            Image = new DiscTexture(Game.RenderSystem, ui.Game.Content.Load<Texture2D>(@"ui-new/fv_palette_bg.png"));
            ImageMode = FrameImageMode.Tiled;
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            base.DrawFrame(gameTime, sb, clipRectIndex);
            sb.DrawUV(PaletteImage, GlobalRectangle.X, GlobalRectangle.Y,  minPosition * Width, Height, Color.White, 0.5f / PaletteImage.Width, 0, 0, 0);

            sb.DrawFreeUV(PaletteImage, new Vector2(GlobalRectangle.X + minPosition * Width, GlobalRectangle.Y), new Vector2(GlobalRectangle.X + minPosition * Width, GlobalRectangle.Y + Height),
                                    new Vector2(GlobalRectangle.X + maxPosition * Width, GlobalRectangle.Y), new Vector2(GlobalRectangle.X + maxPosition * Width, GlobalRectangle.Y + Height),
                    Color.White, new Vector2(2*0.5f / PaletteImage.Width, 1), new Vector2(2*0.5f / PaletteImage.Width, 0), new Vector2(1- 0.5f / PaletteImage.Width, 0), new Vector2(1- 0.5f / PaletteImage.Width, 0));

            sb.DrawUV(PaletteImage, GlobalRectangle.X + maxPosition * Width, GlobalRectangle.Y, Width - maxPosition * Width, Height, Color.White, 1 - 0.5f / PaletteImage.Width, 1, 0, 0);
        }
    }    

    public class HorizontalPaletteHolder : ScalableFrame
    {
        public HorizontalPaletteRender palette;

        private List<string> paletteList;
        public DropDownSelector<DropDownSelectorTextureRow> selector;

        public Action<float, float> ChangeAction, MinMaxUpadteAction;
        public Action<string> PaletteChangeAction;
        public string CurrentTextureName;

        public void SetPalette(string paletteFileName)
        {
            //palette.PaletteImage = ui.Game.Content.Load<DiscTexture>(paletteFileName);
            var th = this;
            PaletteFactory.GetTextureByString(paletteFileName, ref th);
            selector.Current = paletteFileName;
            PaletteChangeAction(selector.Current);
            CurrentTextureName = paletteFileName.Split('.')[0];
        }

        public void AddPalette(string textureName)
        {
            selector.AddValue(textureName);
        }

        public HorizontalPaletteHolder(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
        }

        public ScalableFrame scrollHolder;
        public Scroll minScroll, maxScroll;
        public ScalableFrame minScrollFrame, maxScrollFrame, minValueLabel, maxValueLabel;
        public Button PaletteChangeButton;
        public float minValue, maxValue;

        public Editbox minEdit, maxEdit;

        public void SetValues(float min, float max, float cMin, float cMax)
        {
            MinMaxUpadteAction?.Invoke(min, max);
            minValueLabel.Text = $"{min:0.##}";
            maxValueLabel.Text = $"{max:0.##}";
            minValue = min;
            maxValue = max;
            SetScrolls(cMin, cMax);
            if (minEdit != null) minEdit.Text = $"{min}";
            if (maxEdit != null) maxEdit.Text = $"{max}";
        }

        public void Init()
        {            
            minScroll.actionForMove += (x, y) =>
            {
                if (minScroll.UnitX + minScroll.UnitWidth > scrollHolder.UnitWidth - maxScroll.UnitWidth)
                {
                    minScroll.UnitX = scrollHolder.UnitWidth - maxScroll.UnitWidth - minScroll.UnitWidth;
                }
                if (minScroll.UnitX + minScroll.UnitWidth > maxScroll.UnitX)
                {
                    maxScroll.UnitX = minScroll.UnitX + minScroll.UnitWidth;
                }
                this.palette.minPosition = (minScroll.UnitX) / (this.palette.UnitWidth - maxScroll.UnitWidth);
                this.palette.maxPosition = (maxScroll.UnitX) / (this.palette.UnitWidth - minScroll.UnitWidth);

                minValueLabel.UnitX = this.palette.minPosition < 0.5 && this.palette.maxPosition - this.palette.minPosition > 0.25f
                    ? minScroll.UnitX + minScroll.UnitWidth
                    : minScroll.UnitX - scrollHolder.UnitWidth / 2;
                minValueLabel.TextAlignment = this.palette.minPosition < 0.5 && this.palette.maxPosition - this.palette.minPosition > 0.25f ? Alignment.MiddleLeft : Alignment.MiddleRight;

                maxValueLabel.UnitX = this.palette.maxPosition > 0.5 && this.palette.maxPosition - this.palette.minPosition > 0.25f
                    ? maxScroll.UnitX - scrollHolder.UnitWidth / 2
                    : maxScroll.UnitX + maxScroll.UnitWidth;
                maxValueLabel.TextAlignment = this.palette.maxPosition > 0.5 && this.palette.maxPosition - this.palette.minPosition > 0.25f ? Alignment.MiddleRight : Alignment.MiddleLeft;

                float min = MathUtil.Lerp(minValue, maxValue, this.palette.minPosition),
                    max = MathUtil.Lerp(minValue, maxValue, this.palette.maxPosition);
                minValueLabel.Text = $"{min:0.##}";
                maxValueLabel.Text = $"{max:0.##}";

                this.ChangeAction(this.palette.minPosition, this.palette.maxPosition);
            };

            maxScroll.actionForMove += (x, y) =>
            {
                if (maxScroll.UnitX < minScroll.UnitWidth)
                {
                    maxScroll.UnitX = minScroll.UnitWidth;
                }
                if (maxScroll.UnitX < minScroll.UnitX + minScroll.UnitWidth)
                {
                    minScroll.UnitX = maxScroll.UnitX - minScroll.UnitWidth;
                }
                this.palette.minPosition = (minScroll.UnitX) / (this.palette.UnitWidth - maxScroll.UnitWidth);
                this.palette.maxPosition = (maxScroll.UnitX) / (this.palette.UnitWidth - minScroll.UnitWidth);

                minValueLabel.UnitX = this.palette.minPosition < 0.5 && this.palette.maxPosition - this.palette.minPosition > 0.25f
                    ? minScroll.UnitX + minScroll.UnitWidth
                    : minScroll.UnitX - scrollHolder.UnitWidth / 2;
                minValueLabel.TextAlignment = this.palette.minPosition < 0.5 && this.palette.maxPosition - this.palette.minPosition > 0.25f ? Alignment.MiddleLeft : Alignment.MiddleRight;

                maxValueLabel.UnitX = this.palette.maxPosition > 0.5 && this.palette.maxPosition - this.palette.minPosition > 0.25f
                    ? maxScroll.UnitX - scrollHolder.UnitWidth / 2
                    : maxScroll.UnitX + maxScroll.UnitWidth;
                maxValueLabel.TextAlignment = this.palette.maxPosition > 0.5 && this.palette.maxPosition - this.palette.minPosition > 0.25f ? Alignment.MiddleRight : Alignment.MiddleLeft;

                float min = MathUtil.Lerp(minValue, maxValue, this.palette.minPosition),
                    max = MathUtil.Lerp(minValue, maxValue, this.palette.maxPosition);
                minValueLabel.Text = $"{min:0.##}";
                maxValueLabel.Text = $"{max:0.##}";

                this.ChangeAction(this.palette.minPosition, this.palette.maxPosition);
            };
        }

        public void SetScrolls(float minPosition, float maxPosition)
        {
            minPosition = MathUtil.Clamp(minPosition, 0, 1);
            maxPosition = MathUtil.Clamp(maxPosition, 0, 1);
            minScroll.UnitX = minPosition * (this.palette.UnitWidth - maxScroll.UnitWidth);            
            maxScroll.UnitX = maxPosition * (this.palette.UnitWidth - minScroll.UnitWidth);
            minScroll.actionForMove(minScroll.X, minScroll.Y);
            maxScroll.actionForMove(maxScroll.X, maxScroll.Y);
            ChangeAction(minPosition, maxPosition);
        }
    }

    public class PaletteFactory
    {

        public static void GetTextureByString (string s, ref HorizontalPaletteHolder holder) {
            if (s == "<s>Add")
            {
                FileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = @"Image files|*.BMP;*.JPG;*.GIF;*.PNG;*.JPEG;*.TGA";

                fileDialog.AddExtension = true;
                fileDialog.CheckFileExists = true;
                fileDialog.Title = "Select new palette file";

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filename = fileDialog.FileName;

                    if (!Directory.Exists(UIConfig.CustomPalettePath))
                    {
                        Directory.CreateDirectory(UIConfig.CustomPalettePath);
                    }
                    var fsplit = filename.Split(@"\/".ToCharArray());
                    var file = UIConfig.CustomPalettePath + fsplit.Last();
                    File.Copy(filename, file);
                    filename = file;
                    fsplit = filename.Split(@"\/".ToCharArray());
                    var name = fsplit.Last();
                    if (!name.StartsWith("[FVP]"))
                    {
                        name = "[FVP]" + name;
                        if (!CachedPalettes.ContainsKey(name))
                        {
                            if (!UIConfig.ListPalettes.Contains("<u>" + name))
                            {
                                using (var srcImage = Image.FromFile(file))
                                {
                                    if (!(srcImage.Width <= 256 && srcImage.Height <= 50))
                                    {
                                        var newWidth = (int) (256);
                                        var newHeight = (int) (50);
                                        using (var newImage = new Bitmap(newWidth, newHeight))
                                        using (var graphics = Graphics.FromImage(newImage))
                                        {
                                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                            graphics.DrawImage(srcImage,
                                                new System.Drawing.RectangleF(0, 0, newWidth, newHeight));
                                            file = String.Join("/", fsplit.Take(fsplit.Length - 1)) + "/" + name;
                                            newImage.Save(file);
                                        }
                                    }
                                }
                                File.Delete(filename);

                                UIConfig.ListPalettes.Add("<u>" + file);
                            }

                            using (var stream = File.OpenRead(file))
                            {
                                CachedPalettes[file] = new UserTexture(ApplicationInterface.Instance.Game.RenderSystem,
                                    stream, false);
                            }

                            holder.selector.AddValue("<u>" + file);
                            holder.selector.UpdateList(UIConfig.ListPalettes.Concat(new[] { "<s>Add" }).ToList());
                            holder.selector.Current = "<u>" + file;

                            holder.palette.PaletteImage = CachedPalettes[file];
                            holder.PaletteChangeAction(holder.selector.Current);
                            holder.CurrentTextureName = "<u>" + file;
                        }
                    }                                                           
                }                
            }
            else if (!s.StartsWith("<u>"))
            {
                holder.palette.PaletteImage = new DiscTexture(Game.Instance.RenderSystem, ApplicationInterface.Instance.Game.Content.Load<Texture2D>(s));
                holder.PaletteChangeAction(holder.selector.Current);
                holder.CurrentTextureName = s;
            }
            else
            {
                string name = s.Substring(3);
                if (!CachedPalettes.ContainsKey(name))
                {
                    using (var stream = File.OpenRead(name))
                    {
                        CachedPalettes[name] =
                            new UserTexture(ApplicationInterface.Instance.Game.RenderSystem, stream, false);
                    }
                }
                holder.palette.PaletteImage = CachedPalettes[name];
                holder.PaletteChangeAction(holder.selector.Current);
                holder.CurrentTextureName = s;
            }
        }

        public static HorizontalPaletteHolder HorizontalPaletteHolder(FrameProcessor ui, float OffsetX, float OffsetY,
            ScalableFrame parent, string label, List<string> textureNames, float minValue, float maxValue, Action<float, float> changeAction, Action<string> paletteChangeAction, Action<float, float> minmaxChangeAction, out HorizontalPaletteRender paletteRender, bool minMaxSelector = true)
        {

            textureNames = new List<string>(textureNames);
            textureNames.Add("<s>Add");

            HorizontalPaletteHolder holder = new HorizontalPaletteHolder(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight,OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ElementOffset + UIConfig.UnitPalette2ElementHeight, "", Color.Zero);
            holder.ChangeAction = changeAction;
            holder.MinMaxUpadteAction = minmaxChangeAction;
            holder.PaletteChangeAction = paletteChangeAction;
            holder.minValue = minValue;
            holder.maxValue = maxValue;
            ScalableFrame labelFrame = new ScalableFrame(ui, OffsetX, OffsetY, holder.UnitWidth, UIConfig.UnitPalette2LabelHeight, label, Color.Zero)
            {
                TextAlignment = Alignment.BaselineLeft,
                UnitTextOffsetY = 4,
                FontHolder = UIConfig.FontBody,
            };
            holder.Add(labelFrame);            
            holder.palette = new HorizontalPaletteRender(ui, OffsetX,
                OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ElementOffset,
                holder.UnitWidth - OffsetX*2 - UIConfig.UnitPalette2ButtonWidth, UIConfig.UnitPalette2ElementHeight, textureNames[0]);
            holder.Add(holder.palette);

            ScalableFrame scrollHolder = new ScalableFrame(ui, OffsetX,
                OffsetY + UIConfig.UnitPalette2LabelHeight,
                holder.UnitWidth - OffsetX * 2 - UIConfig.UnitPalette2ButtonWidth, UIConfig.UnitPalette2ElementHeight + 2 * UIConfig.UnitPalette2ElementOffset, "", Color.Zero)
            {
            };
            holder.scrollHolder = scrollHolder;

            Scroll minScroll = new Scroll(ui, 0, 0, UIConfig.UnitPalette2ScrollWidth, UIConfig.UnitPalette2ScrollHeight, "", Color.Zero)
            {
                ImageMode = FrameImageMode.Stretched,
                IsFixedY = true,
                
            };
            holder.minScroll = minScroll;
            ScalableFrame minScrollFrame, maxScrollFrame;
            minScroll.Add(minScrollFrame = new ScalableFrame(ui, 1, 2, 2, 8, "", Color.Blue) {Border = 1, BorderColor = Color.White});
            holder.minScrollFrame = minScrollFrame;
            Scroll maxScroll = new Scroll(ui, scrollHolder.UnitWidth - UIConfig.UnitPalette2ScrollWidth, 0, UIConfig.UnitPalette2ScrollWidth, UIConfig.UnitPalette2ScrollHeight, "", Color.Zero)
            {
                ImageMode = FrameImageMode.Stretched,
                IsFixedY = true,
            };
            holder.maxScroll = maxScroll;
            maxScroll.Add(maxScrollFrame = new ScalableFrame(ui, 1, 2, 2, 8, "", Color.Red) { Border = 1, BorderColor = Color.White });
            holder.maxScrollFrame = maxScrollFrame;
            scrollHolder.Add(minScroll);
            scrollHolder.Add(maxScroll);

            holder.Add(scrollHolder);

            var minValueLabel = new ScalableFrame(ui, minScroll.UnitX + minScroll.UnitWidth, 0, scrollHolder.UnitWidth/2,
                scrollHolder.UnitHeight, $"{minValue}", Color.Zero)
            {
                TextAlignment = Alignment.MiddleLeft,
                FontHolder = UIConfig.FontBody,
            };
            holder.minValueLabel = minValueLabel;
            var maxValueLabel = new ScalableFrame(ui, maxScroll.UnitX - scrollHolder.UnitWidth / 2, 0, scrollHolder.UnitWidth/2,
                scrollHolder.UnitHeight, $"{maxValue}", Color.Zero)
            {
                TextAlignment = Alignment.MiddleRight,
                FontHolder = UIConfig.FontBody,
            };
            holder.maxValueLabel = maxValueLabel;
            scrollHolder.Add(maxValueLabel);
            scrollHolder.Add(minValueLabel);

            

            holder.selector = new DropDownSelector<DropDownSelectorTextureRow>(
                ui, OffsetX,
                OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ElementOffset + UIConfig.UnitPalette2ElementHeight,
                holder.UnitWidth - OffsetX * 2 - UIConfig.UnitPalette2ButtonWidth, UIConfig.UnitPalette2ElementHeight,
                Color.Zero, textureNames, s =>
                {
                    GetTextureByString(s, ref holder);                    
                }, Color.Zero, false)
            {
                Visible = false,                
            };
            holder.Add(holder.selector);

            Button button = new Button(ui, holder.UnitWidth - OffsetX - UIConfig.UnitPalette2ButtonWidth, OffsetY + UIConfig.UnitPalette2LabelHeight, UIConfig.UnitPalette2ButtonWidth, UIConfig.UnitPalette2ButtonHeight, "", Color.Zero, UIConfig.ActiveColor, 200,
                () =>
                {
                    if (!holder.selector.IsOpen) holder.selector.OpenList();                    
                })
            {
                Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_palette"),
                ImageMode = FrameImageMode.Fitted,
            };
            holder.PaletteChangeButton = button;
            paletteRender = holder.palette;
            holder.Add(button);
            holder.Init();
            //TODO: temporary editboxes for min/max
            #region MinMaxSelector
            if (minMaxSelector)
            {
                holder.UnitHeight += OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ElementOffset;
                float EditboxWidth = 25;
                Editbox minEdit = holder.minEdit = new Editbox(ui, OffsetX,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight, EditboxWidth,
                    UIConfig.UnitPalette2LabelHeight, $"{minValue}", Color.Black)
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                    BorderActive = UIConfig.ActiveColor,
                    HoverColor = Color.White,
                    PaddingLeft = (int) (2 * ApplicationInterface.ScaleMod),
                    //PaddingBottom = (int) (2*AppInterface.ScaleMod),
                    TextAlignment = Alignment.MiddleLeft
                };
                minEdit.ActionOut += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
                {
                    if (args.IsClick) minEdit.SetActiveStatus(false);
                };
                Editbox maxEdit = holder.maxEdit = new Editbox(ui, holder.UnitWidth - OffsetX - EditboxWidth,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight, EditboxWidth,
                    UIConfig.UnitPalette2LabelHeight, $"{maxValue}", Color.Black)
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                    BorderActive = UIConfig.ActiveColor,
                    HoverColor = Color.White,
                    PaddingLeft = (int) (2 * ApplicationInterface.ScaleMod),
                    //PaddingBottom = (int)(2 * AppInterface.ScaleMod),
                    TextAlignment = Alignment.MiddleLeft
                };
                maxEdit.ActionOut += (ControllableFrame.ControlActionArgs args, ref bool flag) =>
                {
                    if (args.IsClick) maxEdit.SetActiveStatus(false);
                };
                Button setMinMaxButton = new Button(ui, OffsetX + EditboxWidth,
                    2 * OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ButtonHeight,
                    holder.UnitWidth - 2 * (OffsetX + EditboxWidth), UIConfig.UnitPalette2LabelHeight, "Set",
                    UIConfig.ButtonColor, UIConfig.ActiveColor, 200,
                    () =>
                    {
                        float newMin = float.Parse(minEdit.Text), newMax = float.Parse(maxEdit.Text);
                        holder.MinMaxUpadteAction?.Invoke(newMin, newMax);
                        minValue = newMin;
                        maxValue = newMax;
                        float min = MathUtil.Lerp(minValue, maxValue, holder.palette.minPosition),
                            max = MathUtil.Lerp(minValue, maxValue, holder.palette.maxPosition);
                        minValueLabel.Text = $"{min:0.##}";
                        maxValueLabel.Text = $"{max:0.##}";
                        holder.minValue = minValue;
                        holder.maxValue = maxValue;                
                        holder.ChangeAction(holder.palette.minPosition, holder.palette.maxPosition);
                    })
                {
                    Border = 1,
                    BorderColor = UIConfig.BorderColor,
                };
                holder.Add(minEdit);
                holder.Add(maxEdit);
                holder.Add(setMinMaxButton);
            }
            #endregion
            return holder;
        }

        public static Dictionary<string, Texture> CachedPalettes = new Dictionary<string, Texture>();

        public static HorizontalPaletteHolder HorizontalPaletteHolderSimple(FrameProcessor ui, float OffsetX,
            float OffsetY, float height,
            ScalableFrame parent, List<string> textureNames, float minValue, float maxValue,
            Action<float, float> changeAction, Action<string> paletteChangeAction,
            Action<float, float> minmaxChangeAction, out HorizontalPaletteRender paletteRender, bool addAdder = true)
        {
            if (addAdder)
            {
                textureNames = new List<string>(textureNames);
                textureNames.Add("<s>Add");
            }

            HorizontalPaletteHolder holder = new HorizontalPaletteHolder(ui, parent.UnitPaddingLeft, 0,
                parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight,
                height, "", Color.Zero);
            holder.ChangeAction = changeAction;
            holder.MinMaxUpadteAction = minmaxChangeAction;
            holder.PaletteChangeAction = paletteChangeAction;
    
            holder.palette = new HorizontalPaletteRender(ui, OffsetX,
                holder.UnitHeight/2 - UIConfig.UnitPalette2ElementHeight/2,
                holder.UnitWidth - OffsetX * 2 - UIConfig.UnitPalette2ButtonWidth, UIConfig.UnitPalette2ElementHeight,
                textureNames[0]);
            holder.Add(holder.palette);

            ScalableFrame scrollHolder = new ScalableFrame(ui, OffsetX,
                holder.UnitHeight / 2 - UIConfig.UnitPalette2ElementHeight /2 - UIConfig.UnitPalette2ElementOffset, holder.UnitWidth - OffsetX * 2 - UIConfig.UnitPalette2ButtonWidth,
                UIConfig.UnitPalette2ElementHeight + 2 * UIConfig.UnitPalette2ElementOffset, "", Color.Zero)
            {
            };
            holder.scrollHolder = scrollHolder;

            Scroll minScroll = new Scroll(ui, 0, 0, UIConfig.UnitPalette2ScrollWidth, UIConfig.UnitPalette2ScrollHeight,
                "", Color.Zero)
            {
                ImageMode = FrameImageMode.Stretched,
                IsFixedY = true,

            };
            holder.minScroll = minScroll;
            ScalableFrame minScrollFrame, maxScrollFrame;
            minScroll.Add(minScrollFrame =
                new ScalableFrame(ui, 1, 2, 2, 8, "", Color.Blue) {Border = 1, BorderColor = Color.White});
            holder.minScrollFrame = minScrollFrame;
            Scroll maxScroll = new Scroll(ui, scrollHolder.UnitWidth - UIConfig.UnitPalette2ScrollWidth, 0,
                UIConfig.UnitPalette2ScrollWidth, UIConfig.UnitPalette2ScrollHeight, "", Color.Zero)
            {
                ImageMode = FrameImageMode.Stretched,
                IsFixedY = true,
            };
            holder.maxScroll = maxScroll;
            maxScroll.Add(maxScrollFrame =
                new ScalableFrame(ui, 1, 2, 2, 8, "", Color.Red) {Border = 1, BorderColor = Color.White});
            holder.maxScrollFrame = maxScrollFrame;
            scrollHolder.Add(minScroll);
            scrollHolder.Add(maxScroll);

            holder.Add(scrollHolder);

            var minValueLabel = new ScalableFrame(ui, minScroll.UnitX + minScroll.UnitWidth, 0,
                scrollHolder.UnitWidth / 2,
                scrollHolder.UnitHeight, $"{minValue}", Color.Zero)
            {
                TextAlignment = Alignment.MiddleLeft,
                FontHolder = UIConfig.FontBody,
            };
            holder.minValueLabel = minValueLabel;
            var maxValueLabel = new ScalableFrame(ui, maxScroll.UnitX - scrollHolder.UnitWidth / 2, 0,
                scrollHolder.UnitWidth / 2,
                scrollHolder.UnitHeight, $"{maxValue}", Color.Zero)
            {
                TextAlignment = Alignment.MiddleRight,
                FontHolder = UIConfig.FontBody,
            };
            holder.maxValueLabel = maxValueLabel;
            scrollHolder.Add(maxValueLabel);
            scrollHolder.Add(minValueLabel);
            
            if (textureNames.Count > 1)
            {
                holder.selector = new DropDownSelector<DropDownSelectorTextureRow>(
                    ui, OffsetX,
                    OffsetY + UIConfig.UnitPalette2LabelHeight + UIConfig.UnitPalette2ElementOffset +
                    UIConfig.UnitPalette2ElementHeight,
                    holder.UnitWidth - OffsetX * 2 - UIConfig.UnitPalette2ButtonWidth,
                    UIConfig.UnitPalette2ElementHeight,
                    Color.Zero, textureNames, s =>
                    {
                        GetTextureByString (s, ref holder);                                               
                    }, Color.Zero, false)
                {
                    Visible = false,
                };
                holder.Add(holder.selector);

                Button button = new Button(ui, holder.UnitWidth - OffsetX - UIConfig.UnitPalette2ButtonWidth,
                    OffsetY + holder.UnitHeight / 2 - UIConfig.UnitPalette2ButtonHeight / 2,
                    UIConfig.UnitPalette2ButtonWidth,
                    UIConfig.UnitPalette2ButtonHeight, "", Color.Zero, UIConfig.ActiveColor, 200,
                    () =>
                    {
                        if (!holder.selector.IsOpen) holder.selector.OpenList();
                    })
                {
                    Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_palette"),
                    ImageMode = FrameImageMode.Fitted,
                };
                holder.PaletteChangeButton = button;
                holder.Add(button);
            }
            paletteRender = holder.palette;            
            holder.minValue = minValue;
            holder.maxValue = maxValue;
            holder.Init();
            return holder;
        }
    }
}
