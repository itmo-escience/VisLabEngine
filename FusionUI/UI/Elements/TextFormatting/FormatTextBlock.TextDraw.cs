using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Test;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.Concurrent;

namespace FusionUI.UI.Elements.TextFormatting
{
    partial class FormatTextBlock
    {
        private class SingleString
        {
            public TagStack InTags;
            public List<string> Words = new List<string>();
            public FormatTextBlock block;

            public SingleString(FormatTextBlock block)
            {
                this.block = block;
            }

            public bool AddWord(string word)
            {

                if (!string.IsNullOrEmpty(word))
                {
                    Words.Add(word);
                    return true;
                }

                return false;
            }

            public override string ToString()
            {
                return Words.Aggregate("", (s, s1) => s + " " + s1);
            }

            public void Prepare()
            {
                var tagStack = (TagStack)(InTags?.Clone() ?? new TagStack());
                var wsw = 0;
                var textWidth = 0;
                var xOffset = 0;
                if (Words.Any() && String.IsNullOrWhiteSpace(Words.Last()) && Words.Last() != "\n") Words = Words.Take(Words.Count - 1).ToList();
                string alignment = tagStack.LastByType(Tag.TagType.Alignment)?.Param.ToLower() ?? block.DefaultAlignment;
                textWidth = 0;
                var height = 0;
                string stp = "";
                foreach (var word in Words)
                {
                    if (Tag.IsTag(word))
                    {
                        if (word[1] == '/')
                        {
                            if (tagStack.Peek().CheckTag(word))
                            {
                                tagStack.Pop();
                            }
                        }
                        else if (new Tag(word).Type == Tag.TagType.Image)
                        {
                            if (tagStack.Peek().Type == Tag.TagType.Image)
                            {
                                if (block.ImageCache.ContainsKey(tagStack.Peek().Param))
                                {
                                    var img = block.ImageCache[tagStack.Peek().Param];
                                    Rect = new Rectangle(0, 0, block.Width, block.Width * img.Height / img.Width);
                                    return;
                                }
                            }

                        } else
                        {
                            tagStack.Push(new Tag(word));
                        }

                    }
                    else
                    {
                        Rectangle r;
                        if (alignment == "justify")
                        {
                            var currentFont = block.GetFont(tagStack);
                            r = currentFont.MeasureString(word.Trim());
                            //textWidth += r.Width;
                        }
                        else
                        {
                            var currentFont = block.GetFont(tagStack);
                            if (word == "\n")
                            {
                                r = currentFont.MeasureString(" ");
                            } else if (string.IsNullOrWhiteSpace(word))
                            {
                                r = Rectangle.Empty;
                            } else if (word != TextWords.Last())
                            {
                                r = currentFont.MeasureString(word + " ");
                            }
                            else
                            {
                                r = currentFont.MeasureString(word);
                            }
                        }

                        textWidth += r.Width;
                        height = Math.Max(height, r.Height);
                    }
                }

                if (alignment == "justify" && TextWords.Count(a => !string.IsNullOrWhiteSpace(a)) > 1)
                {
                    WhiteSpaceWidth = (block.Width - textWidth) / (TextWords.Count(a => !string.IsNullOrWhiteSpace(a)) - 1);
                }

                if (!TextWords.Any())
                {
                    height = 0;
                }
                Rect = new Rectangle(0, 0, textWidth, height + block.OffsetLine);
            }

            public Rectangle Rect;
            public int WhiteSpaceWidth;

            public IEnumerable<string> TextWords => Words.Where(a => !Tag.IsTag(a));
            public IEnumerable<string> TagWords => Words.Where(a => Tag.IsTag(a));
        }

        private List<SingleString> strings;

        public string DefaultAlignment = "left";

        public UIConfig.FontHolder BaseFont, BoldFont, ItalicFont, BoldItalicFont;

        public Dictionary<string, Texture> ImageCache;

        private int lastWidth = 0;

        protected override void splitByString()
        {
            if (LastText == Text && strings != null && lastWidth == this.Width|| Text == null ) return;
            MaxLineWidth = 0;
            lastWidth = this.Width;
            ImageCache = new Dictionary<string, Texture>();
            LastText = Text;
            float width = 0;
            strings = new List<SingleString>();
            TagStack tagStack = new TagStack();
            SingleString currentString = new SingleString(this);
            string currentWord = "";
            SpriteFont currentFont = GetFont(tagStack);
            Tag lastTag;
            float Width = this.Width - PaddingLeft - PaddingRight;
            //string align = DefaultAlignment;
            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] == '[')
                {
                    width += currentFont.MeasureString(currentWord + " ").Width;
                    if (width > Width - 10)
                    {
                        strings.Add(currentString);
                        width = 0;
                        currentString.Prepare();
                        currentString = new SingleString(this)
                        {
                            InTags = (TagStack)tagStack.Clone() ?? new TagStack(),
                        };
                    }

                    if (!string.IsNullOrWhiteSpace(currentWord))
                    {
                        currentString.AddWord(currentWord);
                    }
                    currentWord = "";

                    if (Text[i + 1] == '/')
                    {
                        var s = Text.Substring(i, Text.IndexOf("]", i) - i + 1);
                        var tag = tagStack.Peek();
                        if (tag == null)
                        {
                            i = Text.IndexOf("]", i);
                            continue;
                        }
                        if (tag.CheckTag(s))
                        {
                            lastTag = tagStack.Pop();
                        }

                        currentString.AddWord(s);
                        if (!(tagStack.LastByType(Tag.TagType.Alignment)?.Param ?? DefaultAlignment)
                                .Equals(tag.Param, StringComparison.OrdinalIgnoreCase) &&
                            tag.Type == Tag.TagType.Alignment)
                        {
                            currentString.AddWord("\n");
                            strings.Add(currentString);
                            //tagStack.Push(tag);
                            width = 0;
                            currentString.Prepare();
                            currentString = new SingleString(this)
                            {
                                InTags = (TagStack) tagStack.Clone() ?? new TagStack(),
                            };
                        }
                    }
                    else
                    {
                        var s = Text.Substring(i, Text.IndexOf("]", i) - i + 1).Trim();
                        var tag = new Tag(s);

                        if (tag.Type == Tag.TagType.Unsupported)
                        {
                            currentWord += s;
                        }
                        else
                        {


                            currentWord = "";
                            if (tag.Type == Tag.TagType.Alignment)
                            {
                                currentString.AddWord(s);
                                if (!(tagStack.LastByType(Tag.TagType.Alignment)?.Param ?? DefaultAlignment)
                                    .Equals(tag.Param, StringComparison.OrdinalIgnoreCase))
                                {
                                    //currentString.AddWord("\n");
                                    strings.Add(currentString);
                                    tagStack.Push(tag);
                                    width = 0;
                                    currentString.Prepare();
                                    currentString = new SingleString(this)
                                    {
                                        InTags = (TagStack) tagStack.Clone() ?? new TagStack(),
                                    };
                                }
                                else
                                {
                                    tagStack.Push(tag);
                                }
                            }
                            else if (tag.Type == Tag.TagType.Image)
                            {

                                var ei = Text.IndexOf("[/image]", i, StringComparison.OrdinalIgnoreCase);
                                tag.Param = Text.Substring(i + 7,
                                    ei - i - 7);
                                i = ei;
                                tagStack.Push(tag);
                                Gis.ResourceWorker.Post(r =>
                                {
                                    r.ProcessQueue.Post(t =>
                                    {
                                        try
                                        {

                                            if (tag.Param.Contains(":\\") && char.IsLetter(tag.Param[0]))
                                            {
                                                t.DiskWRQueue.Post(t1 =>
                                                {
                                                    if (!File.Exists(tag.Param))
                                                    {
                                                        using (var stream = File.OpenRead(tag.Param))
                                                        {
                                                            Texture Image = new UserTexture(Game.Instance.RenderSystem, stream, false);
                                                            ImageCache.Add(tag.Param, Image);
                                                        }
                                                    }
                                                }, null);

                                            }
                                            else
                                            {

                                                var client = new WebClient();
                                                client.Headers.Add("user-agent",
                                                    "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                                                byte[] data = client.DownloadData(tag.Param);

                                                Texture Image = new UserTexture(Game.Instance.RenderSystem, data,
                                                    false);
                                                ImageCache.Add(tag.Param, Image);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Warning($"{e.Message}Url: {tag.Param}");
                                        }
                                    }, null);
                                }, null);
                                width = 0;
                                currentString.InTags = currentString.InTags ?? new TagStack();
                                currentString.AddWord("\n");
                                strings.Add(currentString);
                                currentString.Prepare();
                                currentString = new SingleString(this)
                                {
                                    InTags = (TagStack) tagStack.Clone() ?? new TagStack(),
                                };
                                currentString.InTags.Push(tag);
                                currentString.AddWord(s);
                                strings.Add(currentString);
                                currentString.Prepare();
                                tagStack.Pop();
                                currentString = new SingleString(this)
                                {
                                    InTags = (TagStack)tagStack.Clone() ?? new TagStack(),
                                };
                            }
                            else if (tag.Type == Tag.TagType.Resource)
                            {
                                var ei = Text.IndexOf($"[/{tag.OpenString}]", i, StringComparison.OrdinalIgnoreCase);
                                var param = Text.Substring(i + 2 + tag.OpenString.Length, ei - i - 2 - tag.OpenString.Length);
                                var p1 = TryGetText(param);
                                Text = Text.Replace($"[{tag.OpenString}]" + param + $"[/{tag.OpenString}]", "]" + p1);
                            }
                            else
                            {
                                currentString.AddWord(s);
                                tagStack.Push(tag);
                            }
                        }

                        if (tag.Type == Tag.TagType.Break)
                        {
                            if (!(tagStack.LastByType(Tag.TagType.Alignment)?.Param ?? DefaultAlignment).Equals(
                                DefaultAlignment, StringComparison.OrdinalIgnoreCase))
                            {
                                strings.Add(currentString);
                                width = 0;
                                currentString.Prepare();
                                currentString = new SingleString(this)
                                {
                                    InTags = (TagStack)tagStack.Clone() ?? new TagStack(),
                                };
                            }
                        }
                    }

                    i = Text.IndexOf("]", i);

                    continue;
                }

                if (Text[i] == '\n')
                {
                    currentFont = GetFont(tagStack);
                    width += currentFont.MeasureString(currentWord + " ").Width;
                    if (width > Width - 10)
                    {
                        strings.Add(currentString);
                        width = currentFont.MeasureString(currentWord + " ").Width;
                        ;
                        currentString.Prepare();
                        currentString = new SingleString(this)
                        {
                            InTags = (TagStack)tagStack.Clone() ?? new TagStack(),
                        };
                    }

                    currentString.AddWord(currentWord);
                    currentWord = "";
                    currentString.AddWord("\n");
                    strings.Add(currentString);
                    width = 0;
                    currentString.Prepare();
                    currentString = new SingleString(this)
                    {
                        InTags = (TagStack)tagStack.Clone() ?? new TagStack(),
                    };
                    continue;
                }

                if (char.IsWhiteSpace(Text[i]) && !string.IsNullOrEmpty(currentWord))
                {
                    currentFont = GetFont(tagStack);
                    width += currentFont.MeasureString(currentWord + " ").Width;
                    if (width > Width - 10)
                    {
                        strings.Add(currentString);
                        width = currentFont.MeasureString(currentWord + " ").Width;
                        ;
                        currentString.Prepare();
                        currentString = new SingleString(this)
                        {
                            InTags = (TagStack)tagStack.Clone() ?? new TagStack(),
                        };
                    }

                    currentString.AddWord(currentWord);
                    currentString.AddWord(" ");
                    currentWord = "";
                    continue;
                }

                currentWord += Text[i];
            }

            if (!string.IsNullOrWhiteSpace(currentWord))
            {
                currentFont = GetFont(tagStack);
                width += currentFont.MeasureString(currentWord + " ").Width;
                if (width > Width - 10)
                {
                    strings.Add(currentString);
                    width = currentFont.MeasureString(currentWord + " ").Width;
                    ;
                    currentString.Prepare();
                    currentString = new SingleString(this)
                    {
                        InTags = (TagStack)tagStack.Clone() ?? new TagStack(),
                    };
                }

                currentString.AddWord(currentWord);
                currentString.AddWord(" ");
                currentWord = "";
            }
            if (currentString.Words.Any())
            {
                strings.Add(currentString);
                currentString.Prepare();
            }
            LastText = Text;
            TextHeight = strings.Sum(a => a.Rect.Height) + 15;
            this.Height = Math.Max(TextHeight, MinHeight);
        }

        private int TextHeight = 0;

        private Color GetColor(TagStack stack)
        {
            var c = stack.LastByType(Tag.TagType.Color);
            if (c != null)
                return Color.FromString(stack.LastByType(Tag.TagType.Color).Param);
            else
                return ForeColor;
        }

        private SpriteFont GetFont(TagStack stack)
        {
            bool isItalic = stack.LastByType(Tag.TagType.Italic) != null;
            bool isBold = stack.LastByType(Tag.TagType.Bold) != null;
            float size = float.Parse(stack.LastByType(Tag.TagType.Size)?.Param ?? "1");
            if (isItalic)
            {
                if (isBold)
                {
                    return BoldItalicFont[ApplicationInterface.uiScale * size];
                }
                else
                {
                    return ItalicFont[ApplicationInterface.uiScale * size];
                }
            }
            else
            {
                if (isBold)
                {
                    return BoldFont[ApplicationInterface.uiScale * size];
                }
                else
                {
                    return BaseFont[ApplicationInterface.uiScale * size];
                }
            }

        }

        private bool IsStriketrough(TagStack stack)
        {
            return stack.LastByType(Tag.TagType.Striketrough) != null;
        }

        private bool IsUnderline(TagStack stack)
        {
            return stack.LastByType(Tag.TagType.Underline) != null;
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            splitByString();
            float th = 0;
            var textOffset = IsShortText ?  (BaseHeight - strings.TakeWhile(a => (th += a.Rect.Height) < BaseHeight).Sum(a => a.Rect.Height)) * 0.5f:
                TextAlignment == Alignment.MiddleLeft || TextAlignment == Alignment.MiddleCenter || TextAlignment == Alignment.MiddleRight ? (Height - TextHeight)/2  :
                TextAlignment == Alignment.BottomLeft || TextAlignment == Alignment.BottomCenter || TextAlignment == Alignment.BottomRight ? Height - TextHeight : PaddingTop;
            DefaultAlignment = TextAlignment == Alignment.MiddleLeft || TextAlignment == Alignment.BottomLeft ||
                               TextAlignment == Alignment.TopLeft ? "left" :
                TextAlignment == Alignment.MiddleRight || TextAlignment == Alignment.BottomRight ||
                TextAlignment == Alignment.TopRight ? "right" : "center";
            var toOld = textOffset;
            var rect = GlobalRectangle;
            Frame frame = this.parent;
            while (frame != null)
            {
                rect = Rectangle.Intersect(rect, frame.GlobalRectangle);
                frame = frame.Parent;
            }
            foreach (var str in strings)
            {
                 if (textOffset > GlobalRectangle.Height) continue;
                 TagStack tagStack = (TagStack)str.InTags?.Clone() ?? new TagStack();
                string alignment = tagStack.LastByType(Tag.TagType.Alignment)?.Param.ToLower() ?? DefaultAlignment;
                if (IsShortText)
                {
                    if (!str.Words.Any()) continue;
                    if (!str.TextWords.Any())
                    {
                        foreach (var strTagWord in str.TagWords)
                        {
                            var s = strTagWord.Replace(" ", "");
                            if (s.StartsWith("[/"))
                            {
                                if (tagStack.Peek().CheckTag(s)) tagStack.Pop();
                                else
                                {
                                    //todo: handle tag mismatch
                                }
                            }
                        }
                    }

                    if (textOffset > BaseHeight - OffsetLine * 2 - this.Font.CapHeight * 2)
                    {
                        ;
                        if (alignment.Equals("left", StringComparison.OrdinalIgnoreCase) ||
                            alignment.Equals("justify", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Font.DrawString(sb, str.Words[0] + "...", this.GlobalRectangle.X,
                                this.GlobalRectangle.Y + textOffset,
                                ForeColor, 0, 0, false);
                        }
                        else
                        {
                            var r = Font.MeasureString(str.Words[0] + "...");
                            if (alignment.Equals("right", StringComparison.OrdinalIgnoreCase))
                            {
                                this.Font.DrawString(sb, str.Words[0] + "...",
                                    this.GlobalRectangle.X + Width - r.Width - PaddingRight,
                                    this.GlobalRectangle.Y + textOffset,
                                    ForeColor, 0, 0, false);
                            }
                            else if (alignment.Equals("center", StringComparison.OrdinalIgnoreCase))
                            {
                                this.Font.DrawString(sb, str.Words[0] + "...",
                                    this.GlobalRectangle.X + Width / 2 - r.Width / 2,
                                    this.GlobalRectangle.Y + textOffset,
                                    ForeColor, 0, 0, false);
                            }
                        }
                    }
                    tagStack = (TagStack)str.InTags?.Clone() ?? new TagStack();
                }

                float wsw = 0;
                var textWidth = 0;
                var xOffset = 0;

                textWidth = str.Rect.Width;
                var height = str.Rect.Height - OffsetLine;
                string stp = "";
                var lastFont = GetFont(tagStack);
                var lastColor = GetColor(tagStack);

                if (GlobalRectangle.Top + textOffset + str.Rect.Height > rect.Top && GlobalRectangle.Y + textOffset < rect.Bottom)//(textOffset < GlobalRectangle.Height && textOffset >= 0)
                {

                    if (alignment == "justify" && !str.TextWords.Last().Equals("\n"))
                    {
                        wsw = (float)((Width - textWidth) / Math.Max(str.TextWords.Count(a => string.IsNullOrWhiteSpace(a)), 1));
                    }


                    if (alignment == "right")
                    {
                        xOffset = (int) (Width - textWidth - PaddingRight -PaddingRight);
                    }

                    if (alignment == "center")
                    {
                        xOffset = (int) (Width - textWidth) / 2;
                    }

                    xOffset += PaddingLeft;
                    foreach (var word in str.Words)
                    {
                        if (Tag.IsTag(word))
                        {
                            if (word[1] == '/')
                            {
                                if (tagStack.Peek().CheckTag(word))
                                {
                                    tagStack.Pop();
                                }
                            }
                            else
                            {
                                if (tagStack.Peek()?.Type == Tag.TagType.Image)
                                {
                                    var tag = tagStack.Pop();
                                    if (ImageCache.ContainsKey(tag.Param))
                                    {
                                        var Image = ImageCache[tag.Param];
                                        var w = Width;
                                        var h = w * Image.Height / Image.Width;
                                        sb.Draw(Image, GlobalRectangle.Left, GlobalRectangle.Top + textOffset, w, h, Color.White, clipRectIndex);
                                        this.Height += h - str.Rect.Height;
                                        str.Rect = new Rectangle(0, 0, w, h);
                                        //textOffset += h;
                                    }
                                    else
                                    {
                                        var currentFont = GetFont(tagStack);
                                        var currentColor = GetColor(tagStack);
                                        var r = currentFont.MeasureString(tag.Param);
                                        var h = this.GlobalRectangle.Y + textOffset - r.Bottom + height;
                                        currentFont.DrawString(sb, tag.Param, this.GlobalRectangle.X + xOffset,
                                            this.GlobalRectangle.Y + textOffset - r.Bottom + height, currentColor,
                                            clipRectIndex, 0, false);
                                    }
                                }
                                else
                                {
                                    tagStack.Push(new Tag(word));
                                }

                            }

                        }
                        else
                        {
                            var currentFont = GetFont(tagStack);
                            var currentColor = GetColor(tagStack);
                            var w = word;
                            if (word == "\n") continue;
                            if (alignment == "justify")
                            {
                                stp += !string.IsNullOrWhiteSpace(w) ? w : "";
                            }
                            else
                            {
                                stp += w;
                            }

                            if (string.IsNullOrWhiteSpace(w))
                            {
                                xOffset += (int)(wsw > 0 ? wsw : currentFont.MeasureString(w).Width);
                            }
                            else
                            {

                                //if (currentColor != lastColor || currentFont != lastFont || word == str.Words.Last())
                                //{
                                var r = currentFont.MeasureString(stp);
                                var h = this.GlobalRectangle.Y + textOffset - r.Bottom + height;
                                //if (h + r.Height > GlobalRectangle.Top && h < GlobalRectangle.Bottom)
                                //{
                                    currentFont.DrawString(sb, stp, this.GlobalRectangle.X + xOffset,
                                        this.GlobalRectangle.Y + textOffset - r.Bottom + height, currentColor,
                                        clipRectIndex, 0, false);
                                //}

                                var whiteTex = Game.Instance.RenderSystem.WhiteTexture;
                                if (IsUnderline(tagStack))
                                {
                                    sb.DrawBeam(whiteTex,
                                        new Vector2(this.GlobalRectangle.X + xOffset,
                                            this.GlobalRectangle.Y + textOffset - r.Bottom + height + r.Height),
                                        new Vector2(this.GlobalRectangle.X + xOffset + r.Width,
                                            this.GlobalRectangle.Y + textOffset - r.Bottom + height + r.Height), currentColor,currentColor, 2);
                                }
                                if (IsStriketrough(tagStack))
                                {
                                    sb.DrawBeam(whiteTex,
                                        new Vector2(this.GlobalRectangle.X + xOffset,
                                            this.GlobalRectangle.Y + textOffset - r.Bottom + height + r.Height/2),
                                        new Vector2(this.GlobalRectangle.X + xOffset + r.Width,
                                            this.GlobalRectangle.Y + textOffset - r.Bottom + height + r.Height/2), currentColor, currentColor, 2);
                                }

                                xOffset += r.Width;
                                height = Math.Max(height, r.Height);

                                stp = "";
                            }

                            lastFont = currentFont;
                            lastColor = currentColor;
                        }
                    }

                    //textOffset += height + OffsetLine;
                }

                textOffset += str.Rect.Height;
                //this.Height = (int) textOffset;
            }

            TextHeight = (int)(textOffset - toOld);
        }
    }
}
