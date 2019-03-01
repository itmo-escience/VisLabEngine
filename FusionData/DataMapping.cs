using System;

namespace FusionData
{
    namespace Data
    {
        public interface Slot<T>
        {
            bool Assign(T channel);
            string Name { get; }
            T Channel { get; }
        }

        public class InputSlot : Slot<DataOutputChannel>
        {
            public DataOutputChannel Channel { get; private set; } = null;
            public string Name { get; set; }

            public bool Assign(DataOutputChannel channel)
            {
                //if (Validate == null || Validate(channel))
                if (channel.DataType.IsOfType(Type))
                {
                    Channel = channel;
                    return true;
                }

                return false;

            }

            public InputSlot(string name, DataType type)
            {
                this.Name = name;
                this.Type = type;
            }

            public DataElement this[Index index] => Channel != null
                ? index.GetFromChannel(Channel)
                : (DefaultGen?.Invoke() ?? DataType.BasicTypes.DefaultNull);

            public Func<DataElement> DefaultGen;

            //public Func<DataOutputChannel, bool> Validate;
            public DataType Type;

        }

        public class InputIndexSlot : Slot<Indexer>
        {
            public Indexer Channel { get; private set; }
            public string Name { get; set; }

            public bool Assign(Indexer channel)
            {
                Channel = channel;
                return true;

                return false;
            }

            public InputIndexSlot(string name)
            {
                this.Name = name;
            }
        }
    }
}
