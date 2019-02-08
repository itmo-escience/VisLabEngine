using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FusionData.Data;

namespace FusionData.Utility.DataReaders
{
    public class CSVLoader : LocalRAMDatasheet.SheetLoader
    {
        public char delimeter, escape, quote;
        public bool hasHeader;
        public List<DataElement> ReadString(string input)
        {
            List<DataElement> data = new List<DataElement>();
            string item = "";
            bool isEscape = false;
            bool isQuote = false;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == escape)
                {
                    isEscape = true;
                } else if (isEscape)
                {
                    item += char.Parse($"\\{input[i]}");
                } else if (input[i] == quote)
                {
                    if (input[i + 1] == quote) item += quote;
                    else isQuote = !isQuote;
                } else if (input[i] == delimeter && !isQuote)
                {
                    data.Add(new DataElement(item, DataType.BasicTypes.String));
                    item = "";
                }
                else
                {
                    item += input[i];
                }
            }
            data.Add(new DataElement(item, DataType.BasicTypes.String));

            return data;
        }

        public List<List<DataElement>> ReadData(string file)
        {
            var list= (hasHeader ? File.ReadLines(file).Skip(1) : File.ReadLines(file)).Select((a, i) =>
            {
                var o = ReadString(a);
                return o;
            }).ToList();
            var table = new List<List<DataElement>>();
            for (int ci = 0; ci < list[0].Count; ci++)
            {
                table.Add(new List<DataElement>());
                for (int i = 0; i < list.Count; i++)
                {
                    table[ci].Add(list[i][ci]);
                }
            }

            return table;
        }

        public List<Tuple<string, DataType>> ReadHeader(string file)
        {
            return ReadString(File.ReadLines(file).First()).Select(a => new Tuple<string, DataType>((String) a.Item, DataType.BasicTypes.String)).ToList();
        }
    }
}
