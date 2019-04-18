//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using FusionData.Data;

//namespace VisTest
//{
//    class BSPBGraphListJSONLoader : LocalRAMDatasheet.SheetLoader
//    {
//        public List<List<DataElement>> ReadData(string file)
//        {
//            var lines = File.ReadLines(file);
//            int ind = 0;
//            List<List<DataElement>> data = new List<List<DataElement>>();
//            data.Add(new List<DataElement>());
//            data.Add(new List<DataElement>());
//            foreach (var line in lines)
//            {
//                var s = line.Trim("{}".ToCharArray()).Split(':');
//                string id = s[0].Trim('"');
//                var linkIds = s[1].Trim(" []".ToCharArray()).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
//                    .Select(a => a.Trim()).ToList();
//                data[0].Add(new DataElement(id, DataType.BasicTypes.String));
//                data[1].Add(new DataElement(linkIds.Select(a => new DataElement(Convert.ToInt32(a), DataType.BasicTypes.Integer)).ToList(), DataType.CompositeTypes.List));
//                ind++;
//            }
//            return data;
//        }

//        public List<Tuple<string, DataType>> ReadHeader(string file)
//        {
//            return new List<Tuple<string, DataType>>() {new Tuple<string, DataType>("Id", DataType.BasicTypes.Integer), new Tuple<string, DataType>("ListId", DataType.CompositeTypes.List)};
//        }
//    }
//}
