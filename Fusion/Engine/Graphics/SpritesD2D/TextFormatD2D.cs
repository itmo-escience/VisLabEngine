using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Fusion.Core.Utils;
using SharpDX.DirectWrite;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class TextFormatD2D : INotifyPropertyChanged, IEquatable<TextFormatD2D>, IXmlSerializable
    {
        public string FontFamily { get; set; }
        public float Size { get; set; }
        public TextVerticalAlignment VerticalAlignment { get; set; }
        public TextHorizontalAlignment HorizontalAlignment { get; set; }

        public enum TextVerticalAlignment
        {
            Up, Center, Down
        }

        public enum TextHorizontalAlignment
        {
            Left, Center, Right
        }

        public TextFormatD2D() { }

        public TextFormatD2D(string fontFamilyName, float fontSize,
            TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Up,
            TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left)
        {
            FontFamily = fontFamilyName;
            Size = fontSize;
            VerticalAlignment = verticalAlignment;
            HorizontalAlignment = horizontalAlignment;
        }

        internal TextFormatD2D(TextFormatD2D source)
        {
            FontFamily = source.FontFamily;
            Size = source.Size;
            VerticalAlignment = source.VerticalAlignment;
            HorizontalAlignment = source.HorizontalAlignment;
        }

		public event PropertyChangedEventHandler PropertyChanged;

		public bool Equals(TextFormatD2D other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FontFamily, other.FontFamily)
                   && Size.Equals(other.Size)
                   && VerticalAlignment.Equals(other.VerticalAlignment)
                   && HorizontalAlignment.Equals(other.HorizontalAlignment);
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

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            FontFamily = UIComponentSerializer.ReadValue<string>(reader);
            Size = UIComponentSerializer.ReadValue<float>(reader);
            VerticalAlignment = UIComponentSerializer.ReadValue<TextVerticalAlignment>(reader);
            HorizontalAlignment = UIComponentSerializer.ReadValue<TextHorizontalAlignment>(reader);
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            UIComponentSerializer.WriteValue(writer, FontFamily);
            UIComponentSerializer.WriteValue(writer, Size);
            UIComponentSerializer.WriteValue(writer, VerticalAlignment);
            UIComponentSerializer.WriteValue(writer, HorizontalAlignment);
        }

        public override string ToString() =>
            $"TextFormatD2D: {FontFamily}:{Size} {HorizontalAlignment},{VerticalAlignment}";
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
                switch (format.VerticalAlignment)
                {
                    case TextFormatD2D.TextVerticalAlignment.Up:
                        result.ParagraphAlignment = ParagraphAlignment.Near;
                        break;
                    case TextFormatD2D.TextVerticalAlignment.Center:
                        result.ParagraphAlignment = ParagraphAlignment.Center;
                        break;
                    case TextFormatD2D.TextVerticalAlignment.Down:
                        result.ParagraphAlignment = ParagraphAlignment.Far;
                        break;
                }
                switch (format.HorizontalAlignment)
                {
                    case TextFormatD2D.TextHorizontalAlignment.Left:
                        result.TextAlignment = TextAlignment.Leading;
                        break;
                    case TextFormatD2D.TextHorizontalAlignment.Center:
                        result.TextAlignment = TextAlignment.Center;
                        break;
                    case TextFormatD2D.TextHorizontalAlignment.Right:
                        result.TextAlignment = TextAlignment.Trailing;
                        break;
                }

                // Create copy to avoid key modification
                _cache[new TextFormatD2D(format)] = result;
            }
            return result;
        }
    }
}
