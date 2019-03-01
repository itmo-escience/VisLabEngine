using System;

namespace FusionData
{
    namespace Data
    {
        public struct DataElement
        {
            public object Item { get; }
            public DataType Type { get; }
            //public Index Index;

            public static DataElement NullElement = new DataElement(null, DataType.BasicTypes.Default);

            public DataElement(object item, DataType type)
            {
                Item = item;
                Type = type;
            }
        }



        public abstract class DataOutputChannel
        {
            public abstract Indexer MainIndexer{ get; }
            public abstract string Name { get; protected set; }
            public abstract DataType DataType { get; set; }
            public abstract DataElement this[BaseIndex index]
            {
                get;
            }

            public DataElement this[Index index] => index.GetFromChannel(this);

            public DataElement Default =>
                DefaultGen?.Invoke() ?? DataType.DefaultGen?.Invoke() ?? DataType.BasicTypes.DefaultNull;

            public Func<DataElement> DefaultGen;

            public virtual int Size { get; }

            public virtual DataOutputChannel Parent { get; set; }

            public bool IsParentOf(DataOutputChannel c)
            {
                DataOutputChannel p = c;
                while (p != null)
                {
                    if (p == this) return true;
                    p = p.Parent;
                }

                return false;
            }

            public bool IsChildOf(DataOutputChannel c) => c.IsParentOf(this);

        }

    }
}
