using System;
using System.Collections;
using System.Collections.Generic;
using FusionData.DataModel.Public;
using FusionData;

namespace FusionData.Utility.SupportFactories
{
    internal static class ChannelEnumeratorFactory
    {
        private class ChannelEnumerator<T> : IEnumerator<KeyValuePair<string, T>>
        {
            private int index = 0;
            private IDataChannel _channel;

            private Func<int, KeyValuePair<string, T>> _getByIndex;
            private int _maxIndex;

            public bool MoveNext()
            {
                index++;
                return index < _maxIndex;
            }

            public void Reset()
            {
                index = 0;
            }

            public KeyValuePair<string, T> Current
            {
                get => _getByIndex(index);
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public ChannelEnumerator(IDataChannel channel, int maxIndex, Func<int, KeyValuePair<string, T>> iterFunc)
            {
                _channel = channel;
                _maxIndex = maxIndex;
                _getByIndex = iterFunc;
            }

            public void Dispose()
            {
            }
        }

        private class ChannelEnumerable<T> : IEnumerable<KeyValuePair<string, T>>
        {
            private IDataChannel _channel;

            private Func<int, KeyValuePair<string, T>> _getByIndex;
            private int _maxIndex;

            public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
            {
                return new ChannelEnumerator<T>(_channel, _maxIndex, _getByIndex);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public ChannelEnumerable(IDataChannel channel, int maxIndex,
                Func<int, KeyValuePair<string, T>> iterFunc)
            {
                _channel = channel;
                _maxIndex = maxIndex;
                _getByIndex = iterFunc;
            }
        }

        public static IEnumerable<KeyValuePair<string, T>> GetEnumeratorForChannel<T>(IDataChannel channel,
            Func<int, KeyValuePair<string, T>> indexerFunction)
        {
            return new ChannelEnumerable<T>(channel, channel.Count, indexerFunction);
        }
    }
}
