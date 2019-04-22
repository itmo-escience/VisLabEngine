using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using SharpDX.DirectWrite;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class TextLayoutD2D : IEquatable<TextLayoutD2D>
    {
        public string Text { get; }
        public float Width { get; }
        public float Height { get; }
        public float MaxWidth { get; }
        public float MaxHeight { get; }
        public TextFormatD2D TextFormat { get; }

        internal TextLayoutD2D(string text, TextFormatD2D textFormat, float maxWidth, float maxHeight)
        {
            var mw = Math.Max(maxWidth, 1);
            var mh = Math.Max(maxHeight, 1);
            var size = TextLayoutFactory.Instance.MeasureString(text, textFormat, mw, mh);

            Text = text;
            TextFormat = textFormat;
            Width = size.Width;
            Height = size.Height;
            MaxWidth = mw;
            MaxHeight = mh;
        }

        internal TextLayoutD2D(TextLayoutD2D source)
        {
            Text = source.Text;
            Width = source.Width;
            Height = source.Height;
            MaxWidth = source.MaxWidth;
            MaxHeight = source.MaxHeight;
            TextFormat = new TextFormatD2D(source.TextFormat);
        }

        public bool Equals(TextLayoutD2D other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Text, other.Text)
                   && Width.Equals(other.Width)
                   && Height.Equals(other.Height)
                   && MaxWidth.Equals(other.MaxWidth)
                   && MaxHeight.Equals(other.MaxHeight)
                   && TextFormat.Equals(other.TextFormat);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextLayoutD2D) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Text != null ? Text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Width.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                hashCode = (hashCode * 397) ^ MaxWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ MaxHeight.GetHashCode();
                return hashCode;
            }
        }
    }

    internal class TextLayoutFactory
    {
        private static TextLayoutFactory _instance;
        public static TextLayoutFactory Instance => _instance ?? (_instance = new TextLayoutFactory());

        private readonly Factory _factory;
        private readonly TextFormatFactory _formatFactory;
        private readonly Dictionary<TextLayoutD2D, TextLayout> _cache = new Dictionary<TextLayoutD2D, TextLayout>();

        private TextLayoutFactory()
        {
            _factory = new Factory();
            _formatFactory = new TextFormatFactory();
        }

        public Size2F MeasureString(string text, TextFormatD2D textFormat, float maxWidth, float maxHeight)
        {
            var tf = _formatFactory.CreateTextFormat(textFormat);
            var l = new TextLayout(_factory, text, tf, maxWidth, maxHeight);
            var size = new Size2F(l.Metrics.Width, l.Metrics.Height);

            l.Dispose();

            return size;
        }

        public TextLayout CreateTextLayout(TextLayoutD2D layout)
        {
            if (!_cache.TryGetValue(layout, out var result))
            {
                var tf = _formatFactory.CreateTextFormat(layout.TextFormat);
                result = new TextLayout(_factory, layout.Text, tf, layout.MaxWidth, layout.MaxHeight);

                // Create copy to avoid key modification
                _cache[new TextLayoutD2D(layout)] = result;
            }

            return result;
        }
    }
}
