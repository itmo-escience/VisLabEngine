using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements.TextFormatting
{
    public enum ValidationResponceType
    {
        None,
        LogWarning,
        Exception
    }
    [Flags]
    public enum ValidationStrictness
    {
        DisallowAll = 0,
        AllowUncompleteTags = 1,
        AllowUnsupportedTags = 2,
        AllowEmptyImages = 4,
        AllowUnsupportedParams = 8,                
        
    }

    public enum ValidationInvokeType
    {
        NoValidation,
        OnSet,
        OnGo
    }

    public partial class FormatTextBlock : RichTextBlock
    {
        private class TagStack : ICloneable
        {
            private List<Tag> stack = new List<Tag>();
            private Dictionary<Tag.TagType, Stack<Tag>> lastTags = new Dictionary<Tag.TagType, Stack<Tag>>();
            public void Push(Tag t)
            {
                stack.Add(t);
                if (!lastTags.ContainsKey(t.Type))
                {
                    lastTags.Add(t.Type, new Stack<Tag>());
                }
                lastTags[t.Type].Push(t);
            }

            public Tag Peek()
            {
                
                return stack.Any() ? stack[stack.Count - 1] : null;
            }

            public Tag Pop()
            {
                var ans = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                lastTags[ans.Type].Pop();
                return ans;
            }

            public Tag LastByType(Tag.TagType type)
            {
                // if (!stack.Any()) return null;
                //for (int i = stack.Count - 1; i >= 0; i--)
                //{
                //    if (stack[i].Type == type)
                //        return stack[i];
                //}

                return (lastTags.ContainsKey(type) && lastTags[type].Any()) ? lastTags[type].Peek() : null;

                return null;

            }

            public object Clone()
            {
                TagStack n = new TagStack();
                n.stack = new List<Tag>(this.stack);
                n.lastTags = lastTags
                        .Select(a => new KeyValuePair<Tag.TagType, Stack<Tag>>(a.Key, new Stack<Tag>(a.Value)))
                        .ToDictionary(a => a.Key, a => a.Value);
                return n;                
            }
        }

        public FormatTextBlock(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor, UIConfig.FontHolder font, float offsetLine = 0, float minHeight = 0, bool isShortText = false, int maxWidth = 0) : base(ui, x, y, w, h, text, backColor, font, offsetLine, minHeight, isShortText, maxWidth)
        {
            BaseFont = font;
            ItalicFont = font;
            BoldFont = font;
            BoldItalicFont = font;

            init();
            Resize += (sender, args) => init();
        }

        public static class ValidationControls
        {
            
        }

        public ValidationInvokeType ValidationInvokeType = ValidationInvokeType.OnSet;
        public ValidationResponceType ValidationResponceType = ValidationResponceType.LogWarning;
        public ValidationStrictness ValidationStrictness = ValidationStrictness.DisallowAll;


        private class Tag
        {
            public enum TagType
            {
                Bold, //[b];[bold] [/b];[/bold]
                Italic, //[i];[italic];[em] [/i];[/italic];[/em]
                Underline, //[u];[underline] [/u][/underline]
                Striketrough, //[s][striketrough] [/s][/striketrough]
                Size, //[size=###] [/size] 
                Color, //[color=###] [/color]              
                Alignment, //[align=###][center][left][right][justify] [/align][/center][/left][/right][/justify]
                Image, //[img=###] [/img]
                Resource, //[r] [/r]
                NoTag, //[notag][nobb] [notag][/nobb]
                Break, //[break]
                Unsupported,
            }

            public Tag(string tag)
            {
                tag = tag.Replace(" ", "");
                if (tag[1] == '/')
                {
                    tag = tag.Remove(1, 1);
                }

                if (!(tag.StartsWith("[") && tag.EndsWith("]")))
                    throw new ArgumentException("Input argument should be bbcode tag");
                string s = tag.Trim("[]".ToCharArray());
                if (s.Equals("b", StringComparison.OrdinalIgnoreCase) || s.Equals("bold", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Bold;
                    OpenString = s;
                    return;
                }
                if (s.Equals("r", StringComparison.OrdinalIgnoreCase) || s.Equals("resource", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Resource;
                    OpenString = s;
                    return;
                }
                if (s.Equals("i", StringComparison.OrdinalIgnoreCase) || s.Equals("italic", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Italic;
                    OpenString = s;
                    return;
                }
                if (s.Equals("u", StringComparison.OrdinalIgnoreCase) || s.Equals("underline", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Underline;
                    OpenString = s;
                    return;
                }
                if (s.Equals("s", StringComparison.OrdinalIgnoreCase) || s.Equals("striketrough", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Underline;
                    OpenString = s;
                    return;
                }

                if (s.StartsWith("size", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Size;
                    OpenString = "size";
                    Param = s.Substring(5);
                    return;
                }

                if (s.StartsWith("color", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Color;
                    OpenString = "color";
                    Param = s.Substring(6);
                    return;
                }

                if (s.StartsWith("align", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Alignment;
                    OpenString = "align";
                    Param = s.Substring(6);
                    return;
                }

                if (s.Equals("center", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Alignment;
                    OpenString = "center";
                    Param = "center";
                    return;
                }
                if (s.Equals("left", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Alignment;
                    OpenString = "left";
                    Param = "left";
                    return;
                }
                if (s.Equals("right", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Alignment;
                    OpenString = "right";
                    Param = "right";
                    return;
                }
                if (s.Equals("justify", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.Alignment;
                    OpenString = "justify";
                    Param = "justify";
                    return;
                }

                if (s.Equals("notag", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("nobb", StringComparison.OrdinalIgnoreCase))
                {
                    Type = TagType.NoTag;
                    OpenString = s;
                    return;
                }

                if (s.StartsWith("image"))
                {
                    Type = TagType.Image;
                    OpenString = "image";
                    //if (s.Length > 6)
                    //{
                    //    Param = s.Substring(6);
                    //}
                    return;
                }

                Type = TagType.Unsupported;
                OpenString = s;
            }

            public static bool IsTag(string s, bool supportedOnly = true)
            {
                return s.StartsWith("[") && s.EndsWith("]") && (!supportedOnly || (s[1] == '/' || new Tag(s).Type != TagType.Unsupported));
            }

            public bool CheckTag(string tag)
            {
                tag = tag.Replace(" ", "");
                if (!(tag.StartsWith("[") && tag.EndsWith("]")))
                    throw new ArgumentException("Input argument should be bbcode tag");
                string s = tag.Trim("[/]".ToCharArray());
                return (s.StartsWith(OpenString));
            }

            public TagType Type;
            public string Param;
            public string OpenString;
        }

        //public bool CheckString(string stringToCheck)
        //{
        //    Stack<Tag> ts;
        //    return CheckString(stringToCheck, false, out ts);
        //}

        //private bool CheckString(string stringToCheck, bool passOpen, out Stack<Tag> tagStack)
        //{   
        //    tagStack = new Stack<Tag>();
        //    int last = 0;
        //    for (int i = 0; i < stringToCheck.Length; i = stringToCheck.IndexOf("[", i))
        //    {
        //        string s = stringToCheck.Substring(i);
        //        s = s.Substring(0, s.IndexOf("]"));
        //        if (s[1] == '/')
        //        {
        //            Tag t1 = tagStack.Peek();
        //            if (t1.CheckTag(s))
        //            {
        //                tagStack.Pop();
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            Tag t = new Tag(s);
        //            if ((ValidationStrictness & ValidationStrictness.AllowUnsupportedTags) == 0 &&
        //                t.Type == Tag.TagType.Unsupported) return false;
        //            if ((ValidationStrictness & ValidationStrictness.AllowUnsupportedParams) == 0)
        //            {
        //                if (t.Type == Tag.TagType.Alignment)
        //                {
        //                    if (!(t.Param.Equals("justify", StringComparison.OrdinalIgnoreCase)
        //                          || t.Param.Equals("left", StringComparison.OrdinalIgnoreCase)
        //                          || t.Param.Equals("right", StringComparison.OrdinalIgnoreCase)
        //                          || t.Param.Equals("center", StringComparison.OrdinalIgnoreCase)))
        //                    {
        //                        return false;
        //                    }
        //                }
        //            }
        //            if (t.Type == Tag.TagType.Break)
        //            {
        //                if (tagStack.Any() && (ValidationStrictness & ValidationStrictness.AllowUncompleteTags) == 0) return false;
        //                tagStack.Clear();
        //            }
        //            else
        //            {
        //                tagStack.Push(t);
        //            }
        //        }
        //        last = i;
        //    }

        //    return (ValidationStrictness & ValidationStrictness.AllowUncompleteTags) != 0 || passOpen || !tagStack.Any();
        //}

        public override void init()
        {            
            BaseHeight = this.Height;
            //UpdateGlobalRect(0, 0);
            splitByString();
            var textOffset = strings.Sum(a => a.Rect.Height);
            //UpdateGlobalRect(0, 0);
            //foreach (var str in strForDraw)
            //{
            //    textOffset += this.Font.CapHeight + OffsetLine;
            //}
            //this.Height = textOffset + this.Font.CapHeight;

            //this.Height = Math.Max(this.Height, this.MinHeight);
            //if (IsShortText)
            //{
            //    this.Height = BaseHeight;
            //}
            //this.Height = !IsShortText ? textOffset : BaseHeight;

        }
    }
}
