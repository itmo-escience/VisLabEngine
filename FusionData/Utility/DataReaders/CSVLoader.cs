using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FusionData.DataModel;
using FusionData;

namespace FusionData.Utility.DataReaders
{
    public class CSVLoader : FileSource<string>.SheetLoader
    {
        public char delimeter, escape, quote;
        public bool hasHeader;
        public List<string> ReadString(string input)
        {
            List<string> data = new List<string>();
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
                    data.Add(item);
                    item = "";
                }
                else
                {
                    item += input[i];
                }
            }
            data.Add(item);

            return data;
        }

        public List<List<string>> ReadData(string file)
        {
            var list= (hasHeader ? File.ReadLines(file).Skip(1) : File.ReadLines(file)).Select((a, i) =>
            {
                var o = ReadString(a);
                return o;
            }).ToList();
            var table = new List<List<string>>();
            for (int ci = 0; ci < list[0].Count; ci++)
            {
                table.Add(new List<string>());
                for (int i = 0; i < list.Count; i++)
                {
                    table[ci].Add(list[i][ci]);
                }
            }

            return table;
        }

        public List<string> ReadHeader(string file)
        {
            return ReadString(File.ReadLines(file).First()).ToList();
        }
    }
}
