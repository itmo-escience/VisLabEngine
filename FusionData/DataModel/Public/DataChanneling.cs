using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionData.DataModel.Public
{
    public interface IDataChannel
    {

        /// <summary>
        /// Get an object for the key
        /// </summary>
        /// <param name="id">Key to identify object</param>
        /// <returns>corrsponding object in this channel</returns>
        object Get(string id);

        /// <summary>
        /// Get Enumerable to iterate through all object in this channel
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<string, object>> GetEnumerable();

        IDataProvider Source { get; }
        int Count { get; }

        Type Type { get; }
    }

    public interface IDataChannel<TData> : IDataChannel
    {
        /// <summary>
        /// Get an object for the key
        /// </summary>
        /// <param name="id">Key to identify object</param>
        /// <returns>corrsponding object in this channel</returns>
        TData Get(string id);

        /// <summary>
        /// Get Enumerable to iterate through all object in this channel
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<string, TData>> GetEnumerable();


    }



    public interface ISlot : IDisposable
    {
        string Name { get; }
        bool IsAssigned { get; }
        object Content { get; set; }

        object Default { get; set; }

        Type Type { get; }

        object Get(string id);

    }

    public interface ISlot<T> : ISlot
    {
        new T Content { get; set; }
    }

    public interface IChannelSlot : ISlot
    {
        new IDataChannel Content { get; set; }
    }

    public interface IChannelSlot<T> : ISlot<IDataChannel>, IChannelSlot
    {
        bool AllowConstParam { get; }
        new T Default { get; set; }
        new IDataChannel Content { get; set; }
    }

    public interface IParameterSlot : ISlot
    {

    }

    public interface IParameterSlot<T> : ISlot<T>, IParameterSlot
    {
        new T Default { get; set; }
    }


    public class ChannelSlot<T> : IChannelSlot<T>
    {
        public string Name { get; }

        public IDataChannel Content
        {
            get => _content;
            set { if (_content.Type == Type) _content = value; }
        }


        public bool IsAssigned => _content != null || Default != null;

        object ISlot.Content
        {
            get => _content;
            set
            {
                if (value is IDataChannel channel && channel.Type == Type) _content = channel;
            }
        }

        public ChannelSlot(string name)
        {
            Name = name;
        }

        private T _default = default(T);


        public T Default
        {
            get => _default;
            set => _default = value;
        }

        object ISlot.Default
        {
            get => (object)_default;
            set
            {
                if (value is T val) _default = val;
            }
        }

        private IDataChannel _content;

        public Type Type { get; set; } = typeof(T);

        public object Get(string id)
        {
            return (Content != null) ? Content.Get(id) : Default;
        }



        public bool AllowConstParam { get; set; }

        public void Dispose()
        {
        }
    }

    public class ParameterSlot<T> : IParameterSlot<T>
    {
        public string Name { get; }
        public bool IsAssigned => (_data != null ? _data : (_data = _defaultValue)) != null;

        public ParameterSlot(string name, T defaultValue)
        {
            Name = name;
            _defaultValue = defaultValue;
        }

        public T Content
        {
            get => _data;
            set => _data = value;
        }

        public T Default
        {
            get => _defaultValue;
            set => _defaultValue = value;
        }

        public Type Type => typeof(T);

        public object Get(string id)
        {
            return Content;
        }

        private T _data;

        object ISlot.Content
        {
            get { return Content; }
            set
            {
                if (value is T val) Content = val;
            }
        }

        object ISlot.Default
        {
            get => _defaultValue;
            set
            {
                if (value is T val) _defaultValue = val;
            }
        }

        private T _defaultValue;

        public void Dispose()
        {

        }
    }

    public class FiniteSetParameterSlot<T> : IParameterSlot<T>
    {
        public string Name { get; }
        public bool IsAssigned => true;

        public T Content
        {
            get => ListValues[_index];
            set
            {
                int tmpIndex = ListValues.FindIndex(a => a.Equals(value));
                if (tmpIndex >= 0) _index = tmpIndex;
            }
        }

        object ISlot.Content
        {
            get { return Content; }
            set
            {
                int tmpIndex = ListValues.FindIndex(a => a.Equals(value));
                if (tmpIndex >= 0) _index = tmpIndex;
            }
        }

        private int _index = 0;
        private T _default => ListValues.First();

        public object Default
        {
            get => _default;
            set { }
        }


        T IParameterSlot<T>.Default
        {
            get => _default;
            set { }
        }

        public Type Type { get; set; }

        public object Get(string id)
        {
            return Content;
        }

        public FiniteSetParameterSlot(string name, ICollection<T> values)
        {
            Name = name;
            if (!values.Any())
            {
                throw new ArgumentException("Value list should contain at least one value");
            }

            ListValues = values.ToList();
        }

        public List<T> ListValues { get; private set; }

        public void Dispose()
        {
        }
    }
}
