using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FusionData
{
    namespace Data
    {
        public interface Index
        {
            /// <summary>
            /// Always should represent number of this index in iterative sequence
            /// </summary>
            uint Ind { get; }
            //DataBlock Sheet { get; set; }
            DataElement GetFromChannel(DataOutputChannel channel);
        }

        public class BaseIndex : Index
        {

            public uint Ind { get; set; }
            public DataBlock Sheet { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is BaseIndex i)
                {
                    return Ind.Equals(i.Ind) && Sheet.Equals(i.Sheet);
                }
                else return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public DataElement GetFromChannel(DataOutputChannel channel) => channel[this];

            internal BaseIndex(DataBlock sheet, uint index)
            {
                Sheet = sheet;
                Ind = index;
            }
        }

        public interface Indexer : IEnumerable<Index>
        {
            //public DataElement this[Index index]
            //{
            //    get => new DataElement();
            //}

            //List<Index> ExistingIndices = new List<Index>();
            //private uint lastIndex = 0;
            //public DataBlock DataBlock;
            //public virtual Index AddIndex()
            //{
            //    var i =  new Index(DataBlock, lastIndex++);
            //    ExistingIndices.Add(i);
            //    return i;
            //}

        }


        //has one indexer output and list of DataOutputChannels
        public abstract class DataBlock
        {
            public Dictionary<string, DataOutputChannel> Outputs => outputs.ToDictionary(a => a.Name, a => a);

            public abstract ICollection<DataOutputChannel> outputs { get; protected set; }
            protected abstract void Init();
            public abstract Indexer Indexer { get; protected set; }
        }

        public class LocalRAMDatasheet : DataBlock
        {
            protected List<List<DataElement>> table;
            protected List<string> rowsHeader;

            public class DataOutputChannel : Data.DataOutputChannel
            {
                private LocalRAMDatasheet sheet;

                public ICollection<DataElement> ColumnContent
                {
                    get { return sheet.table[sheet.rowsHeader.IndexOf(Name)]; }
                    protected set { }
                }

                public override Indexer MainIndexer { get; }
                public override string Name { get; protected set; }
                public override DataType DataType { get; set; }

                public override DataElement this[BaseIndex index]
                {
                    get => sheet.table[sheet.rowsHeader.IndexOf(Name)][(int)(index).Ind];
                }

                public DataOutputChannel(LocalRAMDatasheet sheet, string header)
                {
                    this.sheet = sheet;
                    this.Name = header;
                }
            }

            private LocalRAMDatasheet()
            {
            }

            public static LocalRAMDatasheet CreateWithLoader(SheetLoader loader, string filePath)
            {
                var sheet = new LocalRAMDatasheet();
                var header = loader.ReadHeader(filePath);
                sheet.rowsHeader = header.Select(a => a.Item1).ToList();
                sheet.headerTypes = header.ToDictionary(a => a.Item1, a => a.Item2);
                sheet.table = loader.ReadData(filePath);
                sheet.Init();

                return sheet;
            }

            public override Indexer Indexer
            {
                get => indexer;
                protected set { }
            }
            private Dictionary<string, DataType> headerTypes;
            public override ICollection<Data.DataOutputChannel> outputs { get; protected set; }
            protected SheetIndexer indexer;
            protected class SheetIndexer : Indexer
            {
                public SheetIndexer(LocalRAMDatasheet sheet)
                {
                    this.sheet = sheet;
                }

                private LocalRAMDatasheet sheet;

                protected class LocalRamDatasheetEnumerator : IEnumerator<Index>
                {
                    private LocalRAMDatasheet sheet;
                    private int index;


                    public void Dispose()
                    {
                    }

                    public bool MoveNext()
                    {
                        return (++index < sheet.table[0].Count);
                    }

                    public void Reset()
                    {
                        index = -1;
                    }

                    public Index Current { get => new BaseIndex(sheet, (uint)index); }

                    public LocalRamDatasheetEnumerator(LocalRAMDatasheet sheet)
                    {
                        this.sheet = sheet;
                        index = -1;
                    }

                    object IEnumerator.Current
                    {
                        get { return Current; }
                    }
                }

                public IEnumerator<Index> GetEnumerator()
                {
                    return new LocalRamDatasheetEnumerator(sheet);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }

            protected override void Init()
            {
                indexer = new SheetIndexer(this);
                var columnsList = new List<Data.DataOutputChannel>();
                outputs = columnsList;
                foreach (var columnName in rowsHeader)
                {
                    columnsList.Add(new DataOutputChannel(this, columnName)
                    {
                        DataType = headerTypes[columnName],
                    });
                }

            }

            public interface SheetLoader
            {
                List<List<DataElement>> ReadData(string file);

                List<Tuple<string, DataType>> ReadHeader(string file);
            }
        }
    }
}
