using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Fusion.Core.Utils
{
	public class UIComponentSerializer
	{
		public const string SerializerVersion = "1.6";

		public static List<Type> frameTypes = new List<Type>();

        private static XmlSerializer _formatter = SerializersStorage.GetSerializer(typeof(SeralizableObjectHolder));

		public static void Write(IUIComponent src, string filename )
		{
			GetChildTypes(src);

			SeralizableObjectHolder holder = new SeralizableObjectHolder(src);

			// получаем поток, куда будем записывать сериализованный объект
			using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
			{
				try
				{
					_formatter.Serialize(fs, holder);
				}
				catch (Exception ex)
				{
					Log.Error($"---------------ERROR---------------");
					Log.Error($"Could not serialize \"{holder.SerializableFrame.Name}\".\n");
					Log.Error($"Next exception is thrown:\n{ex.Message}\n");
					Log.Error($"Exception stack trace:\n{ex.StackTrace}");
					Log.Error($"---------------ERROR---------------\n");
				}
				Console.WriteLine("Объект сериализован");
			}
		}

		public static string WriteToString(IUIComponent src )
		{
			GetChildTypes(src);

			SeralizableObjectHolder holder = new SeralizableObjectHolder(src);

			// получаем поток, куда будем записывать сериализованный объект
			using (StringWriter sw = new StringWriter())
			{
				try
				{
					_formatter.Serialize(sw, holder);
				}
				catch (Exception ex)
				{
					Log.Error($"---------------ERROR---------------");
					Log.Error($"Could not serialize \"{holder.SerializableFrame.Name}\".\n");
					Log.Error($"Next exception is thrown:\n{ex.Message}\n");
					Log.Error($"Exception stack trace:\n{ex.StackTrace}");
					Log.Error($"---------------ERROR---------------\n");
				}
				Console.WriteLine("Объект сериализован в строку");
				return sw.ToString();
			}
		}

		private static void GetChildTypes(IUIComponent src )
		{
			if (!frameTypes.Contains(src.GetType()))
			{
				frameTypes.Add(src.GetType());
			}
			/*if (src is UIContainer container)
            {
                foreach (var child in container.Children)
                {
                    GetChildTypes(child);
                }
            }*/
		}

		public static IUIComponent Read(string filename, out IUIComponent destination )
		{
			destination = null;
			// десериализация
			using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
			{
				var holder = (SeralizableObjectHolder)_formatter.Deserialize(fs);
				destination = holder.SerializableFrame;
                /*if (destination is UIContainer container)
                {
                    container.RestoreParents();
                }*/
			}
			return destination;
		}

		public static IUIComponent ReadFromString(string xmlFrame)
		{
            IUIComponent destination = null;
			// десериализация
			using (StringReader sr = new StringReader(xmlFrame))
			{
				var holder = (SeralizableObjectHolder)_formatter.Deserialize(sr);
				destination = holder.SerializableFrame;
                /*if (destination is UIContainer container)
                {
                    container.RestoreParents();
                }*/
            }
			return destination;
		}

        public static void WriteValue(XmlWriter writer, object value)
        {
            var serializer = SerializersStorage.GetSerializer(value.GetType());
            serializer.Serialize(writer, value, new XmlSerializerNamespaces(new []{ XmlQualifiedName.Empty }));
        }

        public static T ReadValue<T>(XmlReader writer)
        {
            var serializer = SerializersStorage.GetSerializer(typeof(T));
            return (T)serializer.Deserialize(writer);
        }
	}

	public class SeralizableObjectHolder : IXmlSerializable
	{
		[XmlElement("Version")]
		public string Version { get; set; } = UIComponentSerializer.SerializerVersion;
		public IUIComponent SerializableFrame { get; set; }

		public string FrameTypeName;
        public string FrameAssemblyName;

		public SeralizableObjectHolder(IUIComponent frame)
		{
			this.SerializableFrame = frame;
            FrameTypeName = frame.GetType().FullName;
            FrameAssemblyName = Assembly.GetAssembly(frame.GetType()).FullName;
		}

		public SeralizableObjectHolder() { }

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml( XmlReader reader )
		{
			var stringSerializer = SerializersStorage.GetSerializer(typeof(string));
			reader.ReadStartElement("SeralizableObjectHolder");

		    var version = (string)stringSerializer.Deserialize(reader);
			Version = version;

			if (version != UIComponentSerializer.SerializerVersion) {
				Console.WriteLine($"Версии текущего сериализатора ({UIComponentSerializer.SerializerVersion}) и десериализуемогог объекта ({version}) не совпадают.");
				return;
			}

            FrameTypeName = (string)stringSerializer.Deserialize(reader);
            FrameAssemblyName = (string)stringSerializer.Deserialize(reader);

			var assembly = Assembly.Load(FrameAssemblyName);
			var frameType = assembly.GetType(FrameTypeName);

			var frameSerializer = SerializersStorage.GetSerializer(frameType);

			var frame = (IUIComponent)frameSerializer.Deserialize(reader);
			SerializableFrame = frame;

            reader.ReadEndElement();
		}

		public void WriteXml( XmlWriter writer )
		{
			var assembly = Assembly.Load(FrameAssemblyName);
            var frameType = assembly.GetType(FrameTypeName);

			XmlSerializer frameSerializer = SerializersStorage.GetSerializer(frameType);
			XmlSerializer stringSerializer = SerializersStorage.GetSerializer(typeof(string));

            stringSerializer.Serialize(writer, Version);
            stringSerializer.Serialize(writer, FrameTypeName);
            stringSerializer.Serialize(writer, FrameAssemblyName);
			frameSerializer.Serialize(writer, SerializableFrame, new XmlSerializerNamespaces(new []{ XmlQualifiedName.Empty }));
		}
	}

	[XmlRoot("dictionary")]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
	{
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml( System.Xml.XmlReader reader )
		{
			XmlSerializer keySerializer = SerializersStorage.GetSerializer(typeof(TKey));
			XmlSerializer valueSerializer = SerializersStorage.GetSerializer(typeof(TValue));

			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
			{
				reader.ReadStartElement("item");

				reader.ReadStartElement("key");
				TKey key = (TKey)keySerializer.Deserialize(reader);
				reader.ReadEndElement();

				reader.ReadStartElement("value");
				TValue value = (TValue)valueSerializer.Deserialize(reader);
				reader.ReadEndElement();

				this.Add(key, value);

				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		public void WriteXml( System.Xml.XmlWriter writer )
		{
			XmlSerializer keySerializer = SerializersStorage.GetSerializer(typeof(TKey));
			XmlSerializer valueSerializer = SerializersStorage.GetSerializer(typeof(TValue));

			foreach (TKey key in this.Keys)
			{
				writer.WriteStartElement("item");

				writer.WriteStartElement("key");
				keySerializer.Serialize(writer, key);
				writer.WriteEndElement();

				writer.WriteStartElement("value");
				TValue value = this[key];
				valueSerializer.Serialize(writer, value);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}
	}

    public class SerializersStorage
    {
        private static readonly Dictionary<Type, XmlSerializer> _serializers = new Dictionary<Type, XmlSerializer>();

        public static XmlSerializer GetSerializer(Type type)
        {
            if (!_serializers.ContainsKey(type))
            {
                _serializers.Add(type, new XmlSerializer(type));
            }

            return _serializers[type];
        }
    }
}
