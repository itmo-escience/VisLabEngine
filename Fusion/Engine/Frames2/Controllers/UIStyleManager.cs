using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Controllers
{
    public class UIStyleManager
    {
        public static UIStyleManager Instance { get; } = new UIStyleManager();

        public const string DefaultStyle = "Default";
        public const string MissingStyle = "StyleMissing";

        private readonly SerializableDictionary<Type, SerializableDictionary<string, IUIStyle>> _styles = new SerializableDictionary<Type, SerializableDictionary<string, IUIStyle>>();

        private UIStyleManager()
        {
            LoadDefaultStyles();
        }

        public IUIStyle GetStyle(Type type, string name = DefaultStyle)
        {
            if (!_styles.TryGetValue(type, out var typeStyle) ||
                !typeStyle.TryGetValue(name, out var result))
            {
                return new UISimpleStyle(type, MissingStyle);
            }

            return result;
        }

        public void AddStyle(IUIStyle style)
        {
            if(!_styles.ContainsKey(style.ControllerType))
                _styles[style.ControllerType] = new SerializableDictionary<string, IUIStyle>();

            _styles[style.ControllerType][style.Name] = style;
        }

        private void LoadDefaultStyles()
        {
            var buttonStyle = new UISimpleStyle(typeof(ButtonController))
            {
                ["Background"] = new[]
                {
                    new PropertyValueStates("BackgroundColor", Color4.Black)
                    {
                        [ControllerState.Hovered] = new Color4(1.0f, 0.0f, 0.0f, 1.0f),
                        [ButtonController.Pressed] = new Color4(0.0f, 1.0f, 1.0f, 1.0f),
                    },
                    new PropertyValueStates("BorderColor", Color4.White)
                    {
                        [ControllerState.Hovered] = new Color4(1.0f, 0.0f, 1.0f, 1.0f),
                        [ButtonController.Pressed] = new Color4(1.0f, 1.0f, 1.0f, 1.0f),
                    }
                },
                ["Foreground"] = new[]
                {
                    new PropertyValueStates("Text", "Idle")
                    {
                        [ControllerState.Hovered] = "Hovered",
                        [ButtonController.Pressed] = "Pressed",
                    }
                }
            };
            AddStyle(buttonStyle);

            var rbStyle = new UISimpleStyle(typeof(RadioButtonController))
            {
                ["Background"] = new[]
                {
                    new PropertyValueStates("BackgroundColor", Color.Gray.ToColor4())
                    {
                    },
                    new PropertyValueStates("BorderColor", Color4.White)
                    {
                    }
                },
                ["RadioButton"] = new[]
                {
                    new PropertyValueStates("BackgroundColor", new Color4(1.0f, 0.0f, 0.0f, 1.0f))
                    {
                        [ControllerState.Hovered] = new Color4(0.5f, 0.0f, 0.0f, 1.0f),
                        [ControllerState.Disabled] = new Color4(1.0f, 0.5f, 0.5f, 1.0f),
                        [RadioButtonController.Pressed] = new Color4(0.5f, 0.5f, 0.0f, 1.0f),
                        [RadioButtonController.Checked] = new Color4(0.0f, 1.0f, 0.0f, 1.0f),
                        [RadioButtonController.CheckedHovered] = new Color4(0.0f, 0.5f, 0.0f, 1.0f),
                        [RadioButtonController.CheckedDisabled] = new Color4(0.5f, 1.0f, 0.5f, 1.0f)
                    }
                }
            };
            AddStyle(rbStyle);
        }

        /// <summary>
        /// Write all current styles with XmlWriter.
        /// </summary>
        /// <param name="writer"></param>
        /// <example>
        /// <code>
        /// string xmlString;
        /// using (var textWriter = new StringWriter())
        /// {
        ///     using (var writer = XmlWriter.Create(textWriter, new XmlWriterSettings() {Indent = true}))
        ///     {
        ///         WriteStyles(writer);
        ///     }
        ///     xmlString = textWriter.ToString();
        /// }
        /// </code>
        /// </example>
        public void WriteStyles(XmlWriter writer) 
        {
            writer.WriteStartElement("Styles");

            foreach (var controller in _styles.Keys)
            {
                writer.WriteStartElement("Controller");
                writer.WriteAttributeString("Type", controller.FullName);

                var controllerStyles = _styles[controller];
                foreach (var style in controllerStyles.Values)
                {
                    new XmlSerializer(style.GetType()).Serialize(writer, style);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Read new styles with XmlReader.
        /// </summary>
        /// <param name="reader"></param>
        /// <example>
        /// <code>
        /// using(var textReader= new StringReader(xmlString))
        /// {
        ///     using (var reader = XmlReader.Create(textReader))
        ///     {
        ///         ReadStyles(reader);
        ///     }
        /// }
        /// </code>
        /// </example>
        public void ReadStyles(XmlReader reader)
        {
            _styles.Clear();

            reader.ReadStartElement("Styles");

            bool wasEmpty = reader.IsEmptyElement;

            if (wasEmpty)
                return;
            reader.MoveToContent();

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                var type = Type.GetType(reader.GetAttribute("Type"));
                reader.ReadStartElement("Controller");
                reader.MoveToContent();

                var controllerStyles = new SerializableDictionary<string, IUIStyle>();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    var styleType = Type.GetType(reader.GetAttribute("Type"));
                    var styleSerializer = new XmlSerializer(styleType);
                    var style = (IUIStyle) styleSerializer.Deserialize(reader);
                    controllerStyles.Add(style.Name, style);
                    reader.MoveToContent();
                }

                _styles.Add(type, controllerStyles);

                reader.ReadEndElement();
                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }
    }

    public interface IUIStyle : IXmlSerializable
    {
        string Name { get; }
        Type ControllerType { get; }
        IEnumerable<PropertyValueStates> this[string slotName] { get; }
    }

    public class UISimpleStyle : IUIStyle
    {
        // map slotName => props
        private readonly Dictionary<string, List<PropertyValueStates>> _slots = new Dictionary<string, List<PropertyValueStates>>();

        public string Name { get; private set; }
        public Type ControllerType { get; private set; }

        public UISimpleStyle()
        {
        }

        public UISimpleStyle(Type controllerType, string name = UIStyleManager.DefaultStyle)
        {
            Name = name;
            ControllerType = controllerType;
        }

        public IEnumerable<PropertyValueStates> this[string slotName]
        {
            get => _slots.GetOrDefault(slotName, new List<PropertyValueStates>());
            set => _slots[slotName] = value.ToList();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var propertySerializer = new XmlSerializer(typeof(PropertyValueStates));

            Name = reader.GetAttribute("Name");
            ControllerType = Type.GetType(reader.GetAttribute("ControllerType"));

            if (reader.IsEmptyElement) return;

            reader.Read();
            reader.MoveToContent();

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                var slotName = reader.GetAttribute("Name");
                reader.ReadStartElement("Slot");
                reader.MoveToContent();

                _slots[slotName] = new List<PropertyValueStates>();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    _slots[slotName].Add((PropertyValueStates)propertySerializer.Deserialize(reader));
                    reader.MoveToContent();
                }

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var propertySerializer = new XmlSerializer(typeof(PropertyValueStates));

            writer.WriteAttributeString("Type", GetType().FullName);
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("ControllerType", ControllerType.FullName);

            foreach (var slotName in _slots.Keys)
            {
                writer.WriteStartElement("Slot");
                writer.WriteAttributeString("Name", slotName);

                foreach (var property in _slots[slotName])
                {
                    propertySerializer.Serialize(writer, property);
                }

                writer.WriteEndElement();
            }
        }
    }


    [XmlRoot(ElementName = "Property")]
    public sealed class PropertyValueStates : IXmlSerializable
    {
        public string Name { get; private set; }
        public object Default { get; private set; }
        private Type Type { get; set; }

        private readonly Dictionary<string, object> _storedValues = new Dictionary<string, object>();

        public PropertyValueStates() {}

        public PropertyValueStates(string name, object defaultValue)
        {
            Name = name;
            Default = defaultValue;
            Type = defaultValue.GetType();

            _storedValues[ControllerState.Default] = Default;
        }

        public object this[ControllerState s]
        {
            get
            {
                if (!_storedValues.TryGetValue(s, out var result))
                    result = Default;
                return result;
            }
            set => _storedValues[s] = value;
        }

        public override string ToString() => Name;

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("Name");

            var typeName = reader.GetAttribute("Type");
            Type = Type.GetType(typeName);
            var valueSerializer = new XmlSerializer(Type);

            reader.ReadStartElement("Property");
            reader.MoveToContent();

            if (reader.IsEmptyElement)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                var stateName = reader.GetAttribute("Name");
                reader.ReadStartElement("State");
                reader.MoveToContent();

                var value = Convert.ChangeType(valueSerializer.Deserialize(reader), Type);
                reader.MoveToContent();
                _storedValues[stateName] = value;

                if (stateName == ControllerState.Default.Name)
                {
                    Default = value;
                }

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("Type", Type.FullName);

            foreach (var state in _storedValues.Keys)
            {
                writer.WriteStartElement("State");
                writer.WriteAttributeString("Name", state);

                var value = _storedValues[state];
                var valueSerializer = new XmlSerializer(Type);
                valueSerializer.Serialize(writer, value, new XmlSerializerNamespaces(new []{ XmlQualifiedName.Empty }));

                writer.WriteEndElement();
            }
        }
    }
}
