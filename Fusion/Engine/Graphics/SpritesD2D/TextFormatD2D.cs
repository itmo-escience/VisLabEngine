﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Fusion.Core.Utils;
using SharpDX.DirectWrite;

namespace Fusion.Engine.Graphics.SpritesD2D
{
    public class TextFormatD2D : IEquatable<TextFormatD2D>, IXmlSerializable
    {
        public string FontFamily { get; private set; }
        public float Size { get; private set; }
        public VertialAlignment VertialAlignment { get; private set; }
        public HorizontalAlignment HorizontalAlignment { get; private set; }

        public TextFormatD2D() { }

        public TextFormatD2D(string fontFamilyName, float fontSize, 
            VertialAlignment vertialAlignment = VertialAlignment.Up, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left)
        {
            FontFamily = fontFamilyName;
            Size = fontSize;
            VertialAlignment = vertialAlignment;
            HorizontalAlignment = horizontalAlignment;
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

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            FontFamily = UIComponentSerializer.ReadValue<string>(reader);
            Size = UIComponentSerializer.ReadValue<float>(reader);
            VertialAlignment = UIComponentSerializer.ReadValue<VertialAlignment>(reader);
            HorizontalAlignment = UIComponentSerializer.ReadValue<HorizontalAlignment>(reader);
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            UIComponentSerializer.WriteValue(writer, FontFamily);
            UIComponentSerializer.WriteValue(writer, Size);
            UIComponentSerializer.WriteValue(writer, VertialAlignment);
            UIComponentSerializer.WriteValue(writer, HorizontalAlignment);
        }
    }

    public enum VertialAlignment
    {
        Up, Center, Down
    }

    public enum HorizontalAlignment
    {
        Left, Center, Right
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
                switch (format.VertialAlignment)
                {
                    case VertialAlignment.Up:
                        result.ParagraphAlignment = ParagraphAlignment.Near;
                        break;
                    case VertialAlignment.Center:
                        result.ParagraphAlignment = ParagraphAlignment.Center;
                        break;
                    case VertialAlignment.Down:
                        result.ParagraphAlignment = ParagraphAlignment.Far;
                        break;
                }
                switch (format.HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        result.TextAlignment = TextAlignment.Leading;
                        break;
                    case HorizontalAlignment.Center:
                        result.TextAlignment = TextAlignment.Center;
                        break;
                    case HorizontalAlignment.Right:
                        result.TextAlignment = TextAlignment.Trailing;
                        break;
                }
                _cache[format] = result;
            }
            return result;
        }
    }
}
