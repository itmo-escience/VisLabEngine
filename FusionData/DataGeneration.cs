using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FusionData
{
    namespace Data
    {
        /// <summary>
        /// A node that takes inputs and provide outputs
        /// </summary>
        public interface DataProcessor
        {
            Dictionary<string, InputIndexSlot> IndexInput { get; }
            Dictionary<string, InputSlot> ChannelInput { get; }

            Dictionary<string, Indexer> IndexOutput { get; }
            Dictionary<string, DataOutputChannel> ChannelOutput { get; }
        }

        public abstract class Recalculator : DataProcessor
        {
            public Func<Dictionary<string, Indexer>, Dictionary<string, DataOutputChannel>, Tuple<
                Dictionary<string, Indexer>,
                Dictionary<string, DataOutputChannel>>> RecalculateFunction;

            protected Recalculator(
                Func<Dictionary<string, Indexer>, Dictionary<string, DataOutputChannel>, Tuple<
                    Dictionary<string, Indexer>,
                    Dictionary<string, DataOutputChannel>>> function)
            {
                RecalculateFunction = function;
            }

            public Dictionary<string, InputIndexSlot> IndexInput { get; set; }
            public Dictionary<string, InputSlot> ChannelInput { get; set; }

            public Dictionary<string, Indexer> IndexOutput
            {
                get
                {
                    if (Dirty)
                    {
                        indexCache = indexOutput;
                    }

                    return indexCache;
                }
            }

            public Dictionary<string, DataOutputChannel> ChannelOutput
            {
                get
                {
                    if (Dirty)
                    {
                        channelCache = channelOutput;
                    }

                    return channelCache;
                }
            }

            protected abstract Dictionary<string, Indexer> indexOutput { get; }
            protected abstract Dictionary<string, DataOutputChannel> channelOutput { get; }

            public bool Dirty = true;
            protected Dictionary<string, Indexer> indexCache { get; set; }
            protected Dictionary<string, DataOutputChannel> channelCache { get; set; }

            public static class PresetRecFuncsFactory
            {
                public static Func<Dictionary<string, Indexer>, Dictionary<string, DataOutputChannel>, Tuple<
                    Dictionary<string, Indexer>,
                    Dictionary<string, DataOutputChannel>>> TranformValues(string inputIndexName,
                    string inputChannelName, Func<DataElement, DataElement> transform, string outputChannelName,
                    DataType outputChannelType)
                {
                    return (id, cd) =>
                    {
                        var chan = new RecalculatedChannel(outputChannelName, cd[inputChannelName], outputChannelType,
                            transform);
                        return new Tuple<Dictionary<string, Indexer>, Dictionary<string, DataOutputChannel>>(
                            new Dictionary<string, Indexer>()
                            {
                                {inputChannelName, chan.MainIndexer}
                            }, new Dictionary<string, DataOutputChannel>()
                            {
                                {outputChannelName, chan}
                            });
                    };
                }

                internal class UnrollListIndexer : Indexer
                {

                    protected class LUEnumerator : IEnumerator<Index>
                    {
                        protected class LUIndex : Index
                        {
                            private DataOutputChannel listChannel;

                            public uint Ind { get; protected set; }

                            public DataElement GetFromChannel(DataOutputChannel channel)
                            {
                                if (channel.IsChildOf(listChannel))
                                {
                                    return ((List<DataElement>) baseIndex.GetFromChannel(channel).Item)[listIndex];
                                }
                                else
                                {
                                    return baseIndex.GetFromChannel(channel);
                                }
                            }

                            private Index baseIndex;
                            private int listIndex;

                            public LUIndex(Index index, DataOutputChannel listChannel, int listIndex, uint ind)
                            {
                                baseIndex = index;
                                this.listChannel = listChannel;
                                this.listIndex = listIndex;
                                Ind = ind;
                            }
                        }

                        private Indexer baseIndexer;
                        private IEnumerator<Index> indEnum;
                        private int listIndex;
                        private DataOutputChannel listChannel;

                        public void Dispose()
                        {
                            indEnum.Dispose();
                        }

                        private int cc = 0;

                        public LUEnumerator(Indexer baseIndexer, DataOutputChannel listChannel)
                        {
                            this.baseIndexer = baseIndexer;
                            indEnum = baseIndexer.GetEnumerator();
                            listIndex = -1;
                            cc = -1;
                            this.listChannel = listChannel;
                            indEnum.MoveNext();

                        }

                        public bool MoveNext()
                        {
                            var list = (List<DataElement>) listChannel[indEnum.Current].Item;
                            cc++;
                            while (!list.Any() || listIndex == list.Count() - 1)
                            {
                                if (!indEnum.MoveNext()) return false;
                                listIndex = -1;
                                list = (List<DataElement>) listChannel[indEnum.Current].Item;
                            }

                            listIndex++;
                            return true;
                        }

                        public void Reset()
                        {
                            indEnum.Reset();
                            listIndex = -1;
                            cc = -1;
                        }

                        public Index Current
                        {
                            get => new LUIndex(indEnum.Current, listChannel, listIndex, (uint) cc);
                        }

                        object IEnumerator.Current
                        {
                            get { return Current; }
                        }
                    }

                    public UnrollListIndexer(Indexer baseIndexer, DataOutputChannel listChannel)
                    {
                        this.baseIndexer = baseIndexer;
                        this.channel = listChannel;
                    }

                    private Indexer baseIndexer;
                    private DataOutputChannel channel;

                    public IEnumerator<Index> GetEnumerator()
                    {
                        return new LUEnumerator(baseIndexer, channel);
                    }

                    IEnumerator IEnumerable.GetEnumerator()
                    {
                        return GetEnumerator();
                    }
                }

                public static Func<Dictionary<string, Indexer>, Dictionary<string, DataOutputChannel>, Tuple<
                    Dictionary<string, Indexer>,
                    Dictionary<string, DataOutputChannel>>> UnrollList(string listIndexerName, string listChannelName,
                    string outputIndexerName, string outputChannelName, DataType outputChannelType)
                {
                    return (id, cd) =>
                    {
                        var nc = new RecalculatedChannel(outputChannelName, cd[listChannelName],
                            DataType.BasicTypes.Integer, d => new DataElement(d.Item, outputChannelType))
                        {

                        };
                        return new Tuple<Dictionary<string, Indexer>, Dictionary<string, DataOutputChannel>>(
                            new Dictionary<string, Indexer>()
                            {
                                {outputIndexerName, new UnrollListIndexer(id[listIndexerName], nc)}
                            }, new Dictionary<string, DataOutputChannel>()
                            {
                                {outputChannelName, nc}
                            });
                    };
                }

            }
        }

        public class OnlineRecalculator : Recalculator
        {
            protected override Dictionary<string, Indexer> indexOutput
            {
                get
                {
                    var v = RecalculateFunction.Invoke(IndexInput.ToDictionary(kv => kv.Key, kv => kv.Value.Channel),
                        ChannelInput.ToDictionary(kv => kv.Key, kv => kv.Value.Channel));
                    indexCache = v.Item1;
                    channelCache = v.Item2;
                    Dirty = false;
                    return v.Item1;
                }
            }

            protected override Dictionary<string, DataOutputChannel> channelOutput
            {
                get
                {
                    var v = RecalculateFunction.Invoke(IndexInput.ToDictionary(kv => kv.Key, kv => kv.Value.Channel),
                        ChannelInput.ToDictionary(kv => kv.Key, kv => kv.Value.Channel));
                    indexCache = v.Item1;
                    channelCache = v.Item2;
                    Dirty = false;
                    return v.Item2;
                }
            }

            public OnlineRecalculator(
                Func<Dictionary<string, Indexer>, Dictionary<string, DataOutputChannel>,
                    Tuple<Dictionary<string, Indexer>, Dictionary<string, DataOutputChannel>>> function) : base(
                function)
            {
            }
        }


        public static class OnlineRecalculatorFactory
        {
            public static OnlineRecalculator CreateTransformRecalculator(string inputIndexName,
                string inputChannelName, DataType inputChannelType, Func<DataElement, DataElement> transform,
                string outputChannelName,
                DataType outputChannelType)
            {
                OnlineRecalculator rec = new OnlineRecalculator(Recalculator.PresetRecFuncsFactory.TranformValues(
                    inputIndexName,
                    inputChannelName,
                    transform, outputChannelName, outputChannelType))
                {
                    IndexInput =
                        new Dictionary<string, InputIndexSlot>() {{inputIndexName, new InputIndexSlot(inputIndexName)}},
                    ChannelInput = new Dictionary<string, InputSlot>()
                    {
                        {inputChannelName, new InputSlot(inputChannelName, inputChannelType)}
                    }
                };

                return rec;
            }

            public static OnlineRecalculator CreateTransformRecalculatorForSingleChannel(Indexer inputIndex,
                DataOutputChannel inputChannel, Func<DataElement, DataElement> func, string outputChannelName,
                DataType outputChannelType) => CreateRecalculatorForSingleChannel(inputIndex, inputChannel,
                Recalculator.PresetRecFuncsFactory.TranformValues("Index", inputChannel.Name, func, outputChannelName,
                    outputChannelType));

            public static OnlineRecalculator CreateRecalculatorForSingleChannel(Indexer inputIndex,
                DataOutputChannel inputChannel,
                Func<Dictionary<string, Indexer>, Dictionary<string, DataOutputChannel>, Tuple<
                    Dictionary<string, Indexer>,
                    Dictionary<string, DataOutputChannel>>> func)
            {
                OnlineRecalculator rec = new OnlineRecalculator(func)
                {
                    IndexInput =
                        new Dictionary<string, InputIndexSlot>() {{"Index", new InputIndexSlot("Index")}},
                    ChannelInput = new Dictionary<string, InputSlot>()
                    {
                        {inputChannel.Name, new InputSlot(inputChannel.Name, inputChannel.DataType)}
                    }
                };
                rec.IndexInput["Index"].Assign(inputIndex);
                rec.ChannelInput[inputChannel.Name].Assign(inputChannel);

                return rec;
            }

            public static OnlineRecalculator CreateRecalculatorForMultipleChannels(
                Dictionary<string, Indexer> inputIndexes,
                Dictionary<string, DataOutputChannel> inputChannels,
                Func<Dictionary<string, Indexer>, Dictionary<string, DataOutputChannel>, Tuple<
                    Dictionary<string, Indexer>,
                    Dictionary<string, DataOutputChannel>>> func)
            {
                OnlineRecalculator rec = new OnlineRecalculator(func)
                {
                    IndexInput =
                        inputIndexes.ToDictionary(a => a.Key, a => new InputIndexSlot(a.Key)),
                    ChannelInput = inputChannels.ToDictionary(a => a.Key, a => new InputSlot(a.Key, a.Value.DataType))
                };
                foreach (var ii in inputIndexes)
                {
                    rec.IndexInput[ii.Key].Assign(ii.Value);
                }

                foreach (var ic in inputChannels)
                {
                    rec.ChannelInput[ic.Key].Assign(ic.Value);
                }

                return rec;
            }
        }


        /// <summary>
        /// a channel that uses provided inputs and provides a list of outputs
        /// </summary>
        public class RecalculatedChannel : DataOutputChannel
        {
            public DataOutputChannel BaseChannel
            {
                get => Parent;
                protected set => Parent = value;
            }

            public Func<DataElement, DataElement> ConvertFunc { get; protected set; }

            public override Indexer MainIndexer => BaseChannel.MainIndexer;
            public override string Name { get; protected set; }
            public override DataType DataType { get; set; }

            public override DataElement this[BaseIndex index] => ConvertFunc.Invoke(BaseChannel[index]);

            public RecalculatedChannel(string name, DataOutputChannel baseChannel, DataType type,
                Func<DataElement, DataElement> convertFunc)
            {
                Name = name;
                BaseChannel = baseChannel;
                DataType = type;
                ConvertFunc = convertFunc;
            }
        }

        public class ConstChannel : DataOutputChannel
        {
            public override Indexer MainIndexer { get; }
            public override string Name { get; protected set; }

            public override DataType DataType
            {
                get => Value.Type;
                set { }
            }

            public override DataElement this[BaseIndex index] => Value;

            public DataElement Value;

            public ConstChannel(string name, Indexer indexer, DataElement value)
            {
                MainIndexer = indexer;
                Name = name;
                Value = value;
            }
        }
    }
}
