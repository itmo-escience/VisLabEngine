using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Fusion.Core.Utils
{
    public class SerializableList<T> : List<T>, IXmlSerializable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public new void Add(T item)
        {
            base.Add(item);
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void AddRange(IEnumerable<T> range)
        {
            base.AddRange(range);
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void Clear()
        {
            base.Clear();
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void ForEach(Action<T> action)
        {
            base.ForEach(action);
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void Insert(Int32 index, T item)
        {
            base.Insert(index, item);
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void InsertRange(Int32 index, IEnumerable<T> range)
        {
            base.InsertRange(index, range);
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void Remove(T item)
        {
            base.Remove(item);
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void RemoveAt(Int32 index)
        {
            base.RemoveAt(index);
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void Reverse()
        {
            base.Reverse();
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void Reverse(Int32 startIndex, Int32 endIndex)
        {
            base.Reverse(startIndex, endIndex);
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void Sort()
        {
            base.Sort();
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void Sort(Comparison<T> comparer)
        {
            base.Sort(comparer);
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void Sort(IComparer<T> comparer)
        {
            base.Sort(comparer);
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public new void Sort(Int32 startIndex, Int32 endIndex, IComparer<T> comparer)
        {
            base.Sort(startIndex, endIndex, comparer);
            PropertyChanged?.Invoke(null, (PropertyChangedEventArgs)EventArgs.Empty);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            XmlSerializer typesSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
            var types = (SerializableDictionary<string, string>)typesSerializer.Deserialize(reader);

            var frameTypes = new List<Type>();
            foreach (var keyValuePair in types)
            {
                var assembly = Assembly.Load(keyValuePair.Value);
                frameTypes.Add(assembly.GetType(keyValuePair.Key));
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T), frameTypes.ToArray());

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                object item = serializer.Deserialize(reader);
                Add((T)item);
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            SerializableDictionary<string, string> typeList = new SerializableDictionary<string, string>();
            foreach (T value in this)
            {
                if (!typeList.Keys.Contains(value.GetType().FullName))
                {
                    typeList.Add(value.GetType().FullName, Assembly.GetAssembly(value.GetType()).FullName);
                }
            }

            XmlSerializer typesSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
            typesSerializer.Serialize(writer, typeList);

            List<Type> frameTypes = new List<Type>();
            foreach (var keyValuePair in typeList)
            {
                var assembly = Assembly.Load(keyValuePair.Value);
                frameTypes.Add(assembly.GetType(keyValuePair.Key));
            }

            XmlSerializer valuesSerializer = new XmlSerializer(typeof(T), frameTypes.ToArray());
            foreach (T value in this)
            {
                writer.WriteStartElement("item");
                valuesSerializer.Serialize(writer, value);
                writer.WriteEndElement();
            }
        }
    }
}
