//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Fusion.Engine.Graphics.GIS.GlobeMath;
//using FusionData.Data;
//using Newtonsoft.Json;

//namespace VisTest
//{

//    public class GridJSON
//    {
//        public class Rootobject
//        {
//            public Class1[] Property1 { get; set; }
//        }

//        public class Class1
//        {
//            public Topleft TopLeft { get; set; }
//            public Botright BotRight { get; set; }
//            public Post[] Posts { get; set; }
//        }

//        public class Topleft
//        {
//            public double X { get; set; }
//            public double Y { get; set; }
//            public int Weight { get; set; }
//            public object Content { get; set; }
//        }

//        public class Botright
//        {
//            public double X { get; set; }
//            public double Y { get; set; }
//            public int Weight { get; set; }
//            public object Content { get; set; }
//        }

//        public class Post
//        {
//            public string ID { get; set; }
//            public string Shortcode { get; set; }
//            public string ImageURL { get; set; }
//            public bool IsVideo { get; set; }
//            public string Caption { get; set; }
//            public int CommentsCount { get; set; }
//            public int Timestamp { get; set; }
//            public int LikesCount { get; set; }
//            public bool IsAd { get; set; }
//            public string AuthorID { get; set; }
//            public string LocationID { get; set; }
//        }
//    }

//    class VisgridLoader : LocalRAMDatasheet.SheetLoader
//    {
//        public List<List<DataElement>> ReadData(string file)
//        {
//            var son = JsonConvert.DeserializeObject<GridJSON.Rootobject>(File.ReadAllText(file));
//            List<List<DataElement>> data = new List<List<DataElement>>();
//            data.Add(new List<DataElement>());
//            data.Add(new List<DataElement>());
//            data.Add(new List<DataElement>());
//            foreach (var f in son.Property1)
//            {
//                data[0].Add(new DataElement(new DVector2(f.TopLeft.X, f.TopLeft.Y), DataType.CompositeTypes.DVector2));
//                data[1].Add(new DataElement(new DVector2(f.BotRight.X, f.BotRight.Y), DataType.CompositeTypes.DVector2));
//                data[2].Add(new DataElement(f.Posts.Select(a =>
//                        $"{a.ID};{a.ImageURL};{a.Caption};{a.Timestamp};{a.LikesCount};{a.AuthorID};{a.LocationID}"),
//                    DataType.CompositeTypes.List));
//            }
//            return data;
//        }

//        public List<Tuple<string, DataType>> ReadHeader(string file)
//        {
//            return new List<Tuple<string, DataType>>() { new Tuple<string, DataType>("TopLeft", DataType.BasicTypes.String), new Tuple<string, DataType>("BotRight", DataType.BasicTypes.String), new Tuple<string, DataType>("Posts", DataType.CompositeTypes.List) };
//        }
//    }
//}
