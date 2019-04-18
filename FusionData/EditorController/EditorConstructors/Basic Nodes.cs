using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FusionData.DataModel.Public;
using FusionData.Utility.SupportFactories;

namespace FusionData.EditorConstructors
{
    [NodeFactory]
    public static class Basic_Nodes
    {
        internal class ChannelProcessorNode : IIONode
        {
            public bool CheckValidity()
            {

                var list = InputSlots.Where(a => a is IChannelSlot)
                    .Where(a => a.Content is IDataChannel).Cast<IChannelSlot>().ToList();
                var flag = true;
                if (!list.Any()) {}
                else if (list.All(a => a.Content == null)) {}
                //else if(list.All(a => a.Content == null && a.Default == null)) {flag = false;}
                else
                {
                    var source = list.First(b => b.Content != null).Content?.Source;
                    if (list.Any(a => a.Content == null || a.Content.Source != source)) flag = false;
                    if (!ValidFunc?.Invoke(this) ?? false) flag = false;
                }

                return flag;
            }


            public Dictionary<string, Type> ChannelInputs;
            public Dictionary<string, Type> ParameterInputs;
            public Dictionary<string, Type> Outputs;

            public Func<Dictionary<string, object>, Dictionary<string, object>> ProcFunc;
            public Func<ChannelProcessorNode, bool> ValidFunc;



            private Dictionary<string, object> Process(string id)
            {
                return ProcFunc?.Invoke(InputSlots.ToDictionary(a => a.Name, a => a.Get(id)));
            }

            public void Init()
            {
                InputSlots = ChannelInputs.ToList().Select(a =>
                {
                    var slotType = typeof(ChannelSlot<>).MakeGenericType(a.Value);
                    return Activator.CreateInstance(slotType, a.Key);
                }).Concat(ParameterInputs.ToList().Select(a =>
                {
                    var slotType = typeof(ParameterSlot<>).MakeGenericType(a.Value);
                    return Activator.CreateInstance(slotType, a.Key);
                })).Cast<ISlot>().ToList();


            }
            private class ProvidedChannel : IDataChannel
            {
                public ChannelProcessorNode Provider { get; set; }
                public string Name { get; set; }
                public object Get(string id)
                {
                    return Provider.Process(id)[Name];
                }

                public IEnumerable<KeyValuePair<string, object>> GetEnumerable()
                {
                    return ChannelEnumeratorFactory.GetEnumeratorForChannel(this, i => new KeyValuePair<string, object>(Source.GetKeyByIndex(i), Get(Source.GetKeyByIndex(i))));
                }

                public IDataProvider Source => Provider.KeyChannel?.Source;
                public int Count { get; }
                public Type Type { get; set; }
            }

            private bool _isInit = false;
            public void ReCalc()
            {
                if (!_isInit)
                {
                    Init();
                    _isInit = true;
                }
                if (!CheckValidity()) return;
                OutputChannels = Outputs.ToDictionary(a => a.Key, a => (object)new ProvidedChannel()
                {
                    Provider = this,
                    Type = a.Value,
                    Name = a.Key,
                });
            }


            public bool Dirty { get; set; }
            public bool AlwaysDirty { get; set; }
            public List<ISlot> InputSlots { get; private set; }
            public Dictionary<string, object> OutputChannels { get; private set; }

            public IDataChannel<string> KeyChannel =>
                InputSlots.Any(a => a.Content is IDataChannel) ? ((IDataChannel) InputSlots.First(a => a.Content is IDataChannel)?.Content)?.Source?.KeyChannel : null;

            public string GetKeyByIndex(int index)
            {
                return ((IDataChannel) InputSlots.First(a => a.Content is IDataChannel).Content).Source.GetKeyByIndex(index);
            }
        }

        //[NodeConstructor]
        internal static ChannelProcessorNode CreateProcessorNodeForFunction(Dictionary<string, Type> channelInputs, Dictionary<string, Type> channelOutputs,
            Func<Dictionary<string, object>, Dictionary<string, object>> func, Dictionary<string, Type> paramInputs = null, Func<ChannelProcessorNode, bool> validFunc = null)
        {
            var node = new ChannelProcessorNode()
            {
                Dirty = true,
                ParameterInputs = paramInputs ?? new Dictionary<string, Type>(),
                ChannelInputs = channelInputs,
                Outputs = channelOutputs,
                ProcFunc = func,
                ValidFunc = validFunc,
            };
            node.ReCalc();
            return node;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        [NodeConstructor]
        public static IIONode ChannelConvertNode(Type from, Type to)
        {
            if (from.GetInterfaces().Contains(typeof(IConvertible)) &&
                to.GetInterfaces().Contains(typeof(IConvertible))) ;
            return CreateProcessorNodeForFunction(new Dictionary<string, Type>() {{from.Name, from}},
                new Dictionary<string, Type>() {{to.Name, to}},
                (d) => new Dictionary<string, object>()
                {
                    {
                        to.Name,
                        ((IConvertible)d[from.Name]).ToType(to,NumberFormatInfo.InvariantInfo)
                    }
                });
        }

        [NodeConstructor]
        public static IIONode ChannelCastNode(Type from, Type to)
        {
            return CreateProcessorNodeForFunction(new Dictionary<string, Type>() { { from.Name, from } },
                new Dictionary<string, Type>() { { to.Name, to } },
                (d) => new Dictionary<string, object>()
                {
                    {
                        to.Name,
                        d[from.Name]
                    }
                });
        }

        [NodeConstructor]
        public static IIONode NumericChannelAddNode()
        {
            return CreateProcessorNodeForFunction(
                new Dictionary<string, Type>() {{"A", typeof(IConvertible)}, {"B", typeof(IConvertible)}},
                new Dictionary<string, Type>() {{"Res", typeof(IConvertible)}},
                d =>
                {
                    return new Dictionary<string, object>()
                    {
                        {
                            "Res",
                            ((IConvertible) d["A"]).ToDecimal(NumberFormatInfo.InvariantInfo) +
                            ((IConvertible) d["B"]).ToDecimal(NumberFormatInfo.InvariantInfo)
                        }
                    };
                });
        }

        [NodeConstructor]
        public static IIONode NumericChannelMultiplyNode()
        {
            return CreateProcessorNodeForFunction(
                new Dictionary<string, Type>() { { "A", typeof(IConvertible) }, { "B", typeof(IConvertible) } },
                new Dictionary<string, Type>() { { "Res", typeof(IConvertible) } },
                d =>
                {
                    return new Dictionary<string, object>()
                    {
                        {
                            "Res",
                            ((IConvertible) d["A"]).ToDecimal(NumberFormatInfo.InvariantInfo) *
                            ((IConvertible) d["B"]).ToDecimal(NumberFormatInfo.InvariantInfo)
                        }
                    };
                });
        }

        [NodeConstructor]
        public static IIONode NumericChannelSubstractNode()
        {
            return CreateProcessorNodeForFunction(
                new Dictionary<string, Type>() { { "A", typeof(IConvertible) }, { "B", typeof(IConvertible) } },
                new Dictionary<string, Type>() { { "Res", typeof(IConvertible) } },
                d =>
                {
                    return new Dictionary<string, object>()
                    {
                        {
                            "Res",
                            ((IConvertible) d["A"]).ToDecimal(NumberFormatInfo.InvariantInfo) -
                            ((IConvertible) d["B"]).ToDecimal(NumberFormatInfo.InvariantInfo)
                        }
                    };
                });
        }

        [NodeConstructor]
        public static IIONode NumericChannelDivideNode()
        {
            return CreateProcessorNodeForFunction(
                new Dictionary<string, Type>() { { "A", typeof(IConvertible) }, { "B", typeof(IConvertible) } },
                new Dictionary<string, Type>() { { "Res", typeof(IConvertible) } },
                d =>
                {
                    return new Dictionary<string, object>()
                    {
                        {
                            "Res",
                            ((IConvertible) d["A"]).ToDecimal(NumberFormatInfo.InvariantInfo) /
                            ((IConvertible) d["B"]).ToDecimal(NumberFormatInfo.InvariantInfo)
                        }
                    };
                });
        }

        [NodeConstructor]
        public static IIONode NumericChannelLerpNode()
        {
            return CreateProcessorNodeForFunction(
                new Dictionary<string, Type>() { { "A", typeof(IConvertible) }, { "Min", typeof(IConvertible) }, { "Max", typeof(IConvertible) } },
                new Dictionary<string, Type>() { { "Res", typeof(IConvertible) } },
                d =>
                {
                    var a = ((IConvertible) d["A"]).ToDecimal(NumberFormatInfo.InvariantInfo);
                    var min = ((IConvertible)d["Min"]).ToDecimal(NumberFormatInfo.InvariantInfo);
                    var max = ((IConvertible)d["Max"]).ToDecimal(NumberFormatInfo.InvariantInfo);
                    var res = min + a * (max - min);
                    return new Dictionary<string, object>()
                    {
                        {
                            "Res", res
                        }
                    };
                });
        }

        [NodeConstructor]
        public static IIONode NumericChannelInverseLerpNode()
        {
            return CreateProcessorNodeForFunction(
                new Dictionary<string, Type>() { { "A", typeof(IConvertible) }, { "Min", typeof(IConvertible) }, { "Max", typeof(IConvertible) } },
                new Dictionary<string, Type>() { { "Res", typeof(decimal) } },
                d =>
                {
                    var a = ((IConvertible)d["A"]).ToDecimal(NumberFormatInfo.InvariantInfo);
                    var min = ((IConvertible)d["Min"]).ToDecimal(NumberFormatInfo.InvariantInfo);
                    var max = ((IConvertible)d["Max"]).ToDecimal(NumberFormatInfo.InvariantInfo);
                    var res = (a - min) / (max - min);
                    return new Dictionary<string, object>()
                    {
                        {
                            "Res", res
                        }
                    };
                });
        }

    }
}
