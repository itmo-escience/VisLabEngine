//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FusionData.Data;

//namespace FusionData.Utility.DataReaders
//{
//    class GeoJSONLoader : LocalRAMDatasheet.SheetLoader
//    {
//        class JSONCOntainer
//        {
//            public class Rootobject
//            {
//                public string type { get; set; }
//                public Crs crs { get; set; }
//                public Feature[] features { get; set; }
//            }

//            public class Crs
//            {
//                public string type { get; set; }
//                public Properties properties { get; set; }
//            }

//            public class Properties
//            {
//                public string name { get; set; }
//            }

//            public class Feature
//            {
//                public string type { get; set; }
//                public Dictionary<string, Object> properties { get; set; }
//                public Geometry geometry { get; set; }
//            }

//            public class Geometry
//            {
//                public string type { get; set; }
//                public Object coordinates { get; set; }
//            }
//        }

//        public List<List<DataElement>> ReadData(string file)
//        {
//            throw new NotImplementedException();
//        }


//        public List<string> ReadHeader(string file)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
