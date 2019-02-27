using System;
using System.Collections;
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
    public class SerializableList<T> : IXmlSerializable, INotifyCollectionChanged, IEnumerable<T>
    {
        private SynchronizedCollection<T> _items;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public object SyncRoot { get; set; } = new object();

        public int Count
        {
            get => _items.Count;
        }

        public SerializableList() {
            _items = new SynchronizedCollection<T>(SyncRoot);

        }

        public SerializableList(SerializableList<T> list)
        {
            _items = new SynchronizedCollection<T>(SyncRoot, list._items);
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        private void InvokeAsyncCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                CollectionChanged?.Invoke(this, e);
                Log.Debug(e.Action.ToString());
            });
        }

        public void Add(T item)
        {
            _items.Add(item);
            InvokeAsyncCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            _items.Clear();
            InvokeAsyncCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Insert(Int32 index, T item)
        {
            _items.Insert(index, item);
            InvokeAsyncCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void Remove(T item)
        {
            int index = _items.IndexOf(item);
            bool isRemoved = _items.Remove(item);
            if (isRemoved) InvokeAsyncCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        public void RemoveAt(Int32 index)
        {
            T item = this[index];
            _items.RemoveAt(index);
            InvokeAsyncCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        public void Reverse()
        {
            _items.Reverse();
            InvokeAsyncCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this));
        }

        public T this[Int32 index] {
            get {
                return _items[index];
            }
            set {
                T oldValue = _items[index];
                InvokeAsyncCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue));
            }
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

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
