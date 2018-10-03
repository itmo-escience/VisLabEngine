using Fusion.Engine.Frames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Fusion.Core.Utils
{
	public class FrameSerializer
	{

		public static List<Type> frameTypes = new List<Type>();

		public static void Write<T>( T src, string filename ) where T : Frame
		{
			GetChildTypes(src);


			// передаем в конструктор тип класса
			XmlSerializer formatter = new XmlSerializer(typeof(T), frameTypes.ToArray());

			// получаем поток, куда будем записывать сериализованный объект
			using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
			{
				formatter.Serialize(fs, src);

				Console.WriteLine("Объект сериализован");
			}
		}

		private static void GetChildTypes<T>( T src ) where T:Frame
		{
			if (!frameTypes.Contains(src.GetType()))
			{
				frameTypes.Add(src.GetType());
			}
			foreach (var child in src.Children)
			{
				GetChildTypes(child);
			}
		}

		public static T Read<T>(string filename, out T destination ) where T : Frame
		{
			destination = null;
			XmlSerializer formatter = new XmlSerializer(typeof(T), frameTypes.ToArray());
			// десериализация
			using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
			{
				destination = (T)formatter.Deserialize(fs);
			}

			return destination;
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
