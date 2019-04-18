using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FusionData.Utility.SupportFactories;
using FusionData.DataModel.Public;

namespace FusionData.DataModel
{
    public class FileSource<T> : IDataProvider
    {
        /// <summary>
        /// Interface for loaders which process files
        /// </summary>
        public interface SheetLoader
        {
            List<List<T>> ReadData(string file);

            List<string> ReadHeader(string file);
        }

        /// <summary>
        /// File header with column names
        /// </summary>
        public List<string> ChannelNames { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Dictionary<string, object> OutputChannels { get; private set; }

        public IDataChannel<string> KeyChannel { get; private set; }
        public string GetKeyByIndex(int index)
        {
            return Data[keyColumnIndex][index].ToString();
        }

        private List<List<T>> Data;
        private Dictionary<string, int> keyIndices;
        private int keyColumnIndex = -1;
        public string KeyColumn { get; }

        private SheetLoader _loader;
        private string _filePath;
        private bool _hasHeader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loader"> <see cref="SheetLoader "/> to proccess input file</param>
        /// <param name="filePath"> path to the input file </param>
        /// <param name="hasHeader"> is input file has header (if no, set header as "1", "2", "3"... </param>
        /// <param name="keyColumn"> header of the key column. If not present in the header, use line number instead </param>
        public FileSource(SheetLoader loader, string filePath, bool hasHeader, string keyColumn)
        {
            _loader = loader;
            _filePath = filePath;
            _hasHeader = hasHeader;
            KeyColumn = keyColumn;


        }

        public void ReCalc()
        {
            if (Dirty || AlwaysDirty)
            {
                Data = _loader.ReadData(_filePath);
                Size = Data[0].Count;
                ChannelNames =
                    _hasHeader ? _loader.ReadHeader(_filePath) : Data.Select((a, i) => i.ToString()).ToList();
                keyColumnIndex = ChannelNames.IndexOf(KeyColumn);
                keyIndices = GetKeyColumn().Select((a, i) => new KeyValuePair<string, int>(a, i))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                OutputChannels = ChannelNames
                    .Select((a, i) =>
                        new KeyValuePair<string, object>(a, new FileStaticChannel<T>(this, i)))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                KeyChannel = new FileKeyChannel(this);
                OutputChannels.Add("#key", KeyChannel);
                Dirty = false;
            }

        }

        public bool Dirty { get; set; } = true;
        public bool AlwaysDirty { get; set; }

        private int Size;

        private List<string> GetKeyColumn()
        {
            if (keyColumnIndex >= 0) return Data[keyColumnIndex].Select(a => a.ToString()).ToList();
            return Enumerable.Range(0, Size - 1).Select(a => a.ToString()).ToList();
        }

        private class FileStaticChannel<T> : IDataChannel<T>
        {
            public FileStaticChannel(FileSource<T> source, int channelIndex)
            {
                _source = source;
                _sourceChannelIndex = channelIndex;
            }



            private FileSource<T> _source;
            private int _sourceChannelIndex;
            object IDataChannel.Get(string id)
            {
                if (_source.keyIndices.ContainsKey(id))
                {
                    Source.ReCalc();
                    return _source.Data[_sourceChannelIndex][_source.keyIndices[id]];
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }

            IEnumerable<KeyValuePair<string, object>> IDataChannel.GetEnumerable()
            {
                return ChannelEnumeratorFactory.GetEnumeratorForChannel(this,
                    index => new KeyValuePair<string, object>(this._source.GetKeyColumn()[index],
                        Get(_source.GetKeyColumn()[index])));
            }

            public T Get(string id)
            {
                return (T)((IDataChannel)this).Get(id);
            }

            public IEnumerable<KeyValuePair<string, T>> GetEnumerable()
            {
                return ChannelEnumeratorFactory.GetEnumeratorForChannel(this,
                    index => new KeyValuePair<string, T>(this._source.GetKeyColumn()[index],
                        Get(_source.GetKeyColumn()[index])));
            }



            public IDataProvider Source => _source;
            public int Count => _source.keyIndices.Count;
            public Type Type => typeof(T);
        }

        /// <summary>
        /// A channel which uses integer key for iteration and based on a keys column
        /// </summary>
        private class FileKeyChannel : IDataChannel<string>
        {
            object IDataChannel.Get(string id)
            {
                return Get(id);
            }

            IEnumerable<KeyValuePair<string, object>> IDataChannel.GetEnumerable()
            {
                return ChannelEnumeratorFactory.GetEnumeratorForChannel(this, i => new KeyValuePair<string, object>(Get(i.ToString()), Get(i.ToString())));
            }

            public string Get(string id)
            {
                return _source.GetKeyColumn()[Convert.ToInt32(id)];
            }

            public IEnumerable<KeyValuePair<string, string>> GetEnumerable()
            {
                return ChannelEnumeratorFactory.GetEnumeratorForChannel(this, i => new KeyValuePair<string, string>(Get(i.ToString()), Get(i.ToString())));
            }

            private FileSource<T> _source;
            public IDataProvider Source => _source;
            public int Count { get => _source.GetKeyColumn().Count; }
            public Type Type => typeof(string);

            public FileKeyChannel(FileSource<T> source)
            {
                _source = source;
            }
        }

        public bool CheckValidity()
        {
            return true;
        }
    }
}
