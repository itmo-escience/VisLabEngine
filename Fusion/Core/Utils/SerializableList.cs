using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public class SerializableList<T> : List<T>, IXmlSerializable, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public SerializableList() : base()
        {
            CollectionChanged += (s, e) =>
            {
                int i;
            };
        }

        public SerializableList(SerializableList<T> list) : base(list)
        {
            CollectionChanged += (s, e) =>
            {
                int i;
            };
        }

        public new void Add(T item)
        {
            base.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public new void AddRange(IEnumerable<T> range)
        {
            base.AddRange(range);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, range));
        }

        public new void Clear()
        {
            base.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public new void Insert(Int32 index, T item)
        {
            base.Insert(index, item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public new void InsertRange(Int32 index, IEnumerable<T> range)
        {
            base.InsertRange(index, range);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, range, index));
        }

        public new void Remove(T item)
        {
            bool isRemoved = base.Remove(item);
            if (isRemoved) CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }

        public new void RemoveAt(Int32 index)
        {
            T item = this[index];
            base.RemoveAt(index);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        public new void Reverse()
        {
            base.Reverse();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this));
        }

        public new void Reverse(Int32 startIndex, Int32 endIndex)
        {
            base.Reverse(startIndex, endIndex);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, GetRange(startIndex, endIndex - startIndex + 1)));
        }

        public new void Sort()
        {
            base.Sort();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this));
        }

        public new void Sort(Comparison<T> comparer)
        {
            base.Sort(comparer);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this));
        }

        public new void Sort(IComparer<T> comparer)
        {
            base.Sort(comparer);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this));
        }

        public new void Sort(Int32 startIndex, Int32 endIndex, IComparer<T> comparer)
        {
            base.Sort(startIndex, endIndex, comparer);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, GetRange(startIndex, endIndex - startIndex + 1)));
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
