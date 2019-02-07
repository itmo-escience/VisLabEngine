using System;
using System.Collections.Generic;
using SharpDX.DirectWrite;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class TextFormatD2D : IEquatable<TextFormatD2D>
    {
        public string FontFamily { get; }
        public float Size { get; }

        public TextFormatD2D() { }

        public TextFormatD2D(string fontFamilyName, float fontSize)
        {
            FontFamily = fontFamilyName;
            Size = fontSize;
        }

        public bool Equals(TextFormatD2D other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FontFamily, other.FontFamily) && Size.Equals(other.Size);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextFormatD2D) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FontFamily != null ? FontFamily.GetHashCode() : 0) * 397) ^ Size.GetHashCode();
            }
        }
    }

    internal class TextFormatFactory
    {
        private readonly Factory _factory;
        private readonly Dictionary<TextFormatD2D, TextFormat> _cache = new Dictionary<TextFormatD2D, TextFormat>();

        public TextFormatFactory()
        {
            _factory = new Factory();
        }

        public TextFormat CreateTextFormat(TextFormatD2D format)
        {
            if (!_cache.TryGetValue(format, out var result))
            {
                result = new TextFormat(_factory, format.FontFamily, format.Size);
                _cache[format] = result;
            }
            return result;
        }
    }
}
