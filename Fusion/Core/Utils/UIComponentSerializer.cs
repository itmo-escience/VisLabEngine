using Fusion.Engine.Frames2;
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

		public static void Write(UIComponent src, string filename )
		{
			GetChildTypes(src);

			SeralizableObjectHolder holder = new SeralizableObjectHolder(src);

			// передаем в конструктор тип класса
			XmlSerializer formatter = new XmlSerializer(typeof(SeralizableObjectHolder));

			// получаем поток, куда будем записывать сериализованный объект
			using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
			{
				formatter.Serialize(fs, holder);
				Console.WriteLine("Объект сериализован");
			}
		}

		public static string WriteToString(UIComponent src )
		{
			GetChildTypes(src);

			SeralizableObjectHolder holder = new SeralizableObjectHolder(src);

			// передаем в конструктор тип класса
			XmlSerializer formatter = new XmlSerializer(typeof(SeralizableObjectHolder));

			// получаем поток, куда будем записывать сериализованный объект
			using (StringWriter sw = new StringWriter())
			{
				formatter.Serialize(sw, holder);
				Console.WriteLine("Объект сериализован в строку");
				return sw.ToString();
			}
		}

		private static void GetChildTypes(UIComponent src )
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

		public static UIComponent Read(string filename, out UIComponent destination )
		{
			destination = null;
			XmlSerializer formatter = new XmlSerializer(typeof(SeralizableObjectHolder));
			// десериализация
			using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
			{
				var holder = (SeralizableObjectHolder)formatter.Deserialize(fs);
				destination = holder.SerializableFrame;
                /*if (destination is UIContainer container)
                {
                    container.RestoreParents();
                }*/
			}
			return destination;
		}

		public static UIComponent ReadFromString(string xmlFrame)
		{
            UIComponent destination = null;
			XmlSerializer formatter = new XmlSerializer(typeof(SeralizableObjectHolder));
			// десериализация
			using (StringReader sr = new StringReader(xmlFrame))
			{
				var holder = (SeralizableObjectHolder)formatter.Deserialize(sr);
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
            var serializer = new XmlSerializer(value.GetType());
            serializer.Serialize(writer, value, new XmlSerializerNamespaces(new []{ XmlQualifiedName.Empty }));
        }

        public static T ReadValue<T>(XmlReader writer)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(writer);
        }
	}

	public class SeralizableObjectHolder : IXmlSerializable
	{
		[XmlElement("Version")]
		public string Version { get; set; } = UIComponentSerializer.SerializerVersion;
		public UIComponent SerializableFrame { get; set; }

		public string FrameTypeName;
        public string FrameAssemblyName;

		public SeralizableObjectHolder(UIComponent frame)
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
			var stringSerializer = new XmlSerializer(typeof(string));

			var wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

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

			var frameSerializer = new XmlSerializer(frameType);

			var frame = (UIComponent)frameSerializer.Deserialize(reader);
			SerializableFrame = frame;
		}

		public void WriteXml( XmlWriter writer )
		{
			var assembly = Assembly.Load(FrameAssemblyName);
            var frameType = assembly.GetType(FrameTypeName);

			XmlSerializer frameSerializer = new XmlSerializer(frameType);
			XmlSerializer stringSerializer = new XmlSerializer(typeof(string));

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
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

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
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

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
}
