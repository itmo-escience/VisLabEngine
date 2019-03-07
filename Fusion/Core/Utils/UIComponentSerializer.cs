﻿using Fusion.Engine.Frames2;
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
		public const string SerializerVersion = "1.5";

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
				try
				{
					formatter.Serialize(fs, holder);
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

		public static string WriteToString(UIComponent src )
		{
			GetChildTypes(src);

			SeralizableObjectHolder holder = new SeralizableObjectHolder(src);

			// передаем в конструктор тип класса
			XmlSerializer formatter = new XmlSerializer(typeof(SeralizableObjectHolder));

			// получаем поток, куда будем записывать сериализованный объект
			using (StringWriter sw = new StringWriter())
			{
				try
				{
					formatter.Serialize(sw, holder);
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

		private static void GetChildTypes(UIComponent src )
		{
			if (!frameTypes.Contains(src.GetType()))
			{
				frameTypes.Add(src.GetType());
			}
			if (src is UIContainer container)
            {
                foreach (var child in container.Children)
                {
                    GetChildTypes(child);
                }
            }
		}

		public static void GetChildTypes(UIComponent src, SerializableDictionary<string, string> list )
		{
			if (!list.Keys.Contains(src.GetType().FullName))
			{
				list.Add(src.GetType().FullName, Assembly.GetAssembly(src.GetType()).FullName);
			}
            if (src is UIContainer container)
            {
                foreach (var child in container.Children)
                {
                    GetChildTypes(child, list);
                }
            }
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
                if (destination is UIContainer container)
                {
                    container.RestoreParents();
                }
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
                if (destination is UIContainer container)
                {
                    container.RestoreParents();
                }
            }
			return destination;
		}
	}

	public class SeralizableObjectHolder : IXmlSerializable
	{
		[XmlElement("Version")]
		public string Version { get; set; } = UIComponentSerializer.SerializerVersion;
		public UIComponent SerializableFrame { get; set; }

		public SerializableDictionary<string,string> FrameTypes = new SerializableDictionary<string, string>();

		public SeralizableObjectHolder(UIComponent frame)
		{
			this.SerializableFrame = frame;
			UIComponentSerializer.GetChildTypes(frame, FrameTypes);
		}

		public SeralizableObjectHolder() { }

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml( XmlReader reader )
		{
			var typesSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
			var versionSerializer = new XmlSerializer(typeof(string));

			var wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

		    var version = (string)versionSerializer.Deserialize(reader);
			Version = version;

			if (version != UIComponentSerializer.SerializerVersion) {
				Console.WriteLine($"Версии текущего сериализатора ({UIComponentSerializer.SerializerVersion}) и десериализуемогог объекта ({version}) не совпадают.");
				return;
			}

			var types = (SerializableDictionary<string, string>)typesSerializer.Deserialize(reader);
			FrameTypes = types;

			var frameTypes = new List<Type>();
			foreach (var keyValuePair in types)
			{
				var assembly = Assembly.Load(keyValuePair.Value);
				frameTypes.Add(assembly.GetType(keyValuePair.Key));
			}

			var frameSerializer = new XmlSerializer(typeof(UIComponent), frameTypes.ToArray());

			var frame = (UIComponent)frameSerializer.Deserialize(reader);
			SerializableFrame = frame;
		}

		public void WriteXml( XmlWriter writer )
		{
			List<Type> frameTypes = new List<Type>();
			foreach (var keyValuePair in FrameTypes)
			{
				var assembly = Assembly.Load(keyValuePair.Value);
				frameTypes.Add(assembly.GetType(keyValuePair.Key));
			}

			XmlSerializer typesSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
			XmlSerializer frameSerializer = new XmlSerializer(typeof(UIComponent), frameTypes.ToArray());
			XmlSerializer versionSerializer = new XmlSerializer(typeof(string));

			versionSerializer.Serialize(writer, this.Version);
			typesSerializer.Serialize(writer, this.FrameTypes);
			frameSerializer.Serialize(writer, this.SerializableFrame);
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
