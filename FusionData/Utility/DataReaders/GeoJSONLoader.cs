//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FusionData.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FusionData.DataModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FusionData.DataModel.DataTypes;

namespace FusionData.Utility.DataReaders
{
    public class GeoJSONLoader : FileSource<object>.SheetLoader
    {
        class JSONCOntainer
        {
            public class Rootobject
            {
                public string type { get; set; }
                public Crs crs { get; set; }
                public Feature[] features { get; set; }
            }

            public class Crs
            {
                public string type { get; set; }
                public Properties properties { get; set; }
            }

            public class Properties
            {
                public string name { get; set; }
            }

            public class Feature
            {
                public string type { get; set; }
                public Dictionary<string, Object> properties { get; set; }
                public Geometry geometry { get; set; }
            }

            public class Geometry
            {
                public string type { get; set; }
                public Object coordinates { get; set; }
            }
        }


        /// <summary>
        /// GeometryType types from geojson format
        /// </summary>
        public enum GeometryType
        {
            Points,
            LineStrings,
            Polygons,
            MultiPoints,
            MultiLineStrings,
            MultiPolygons,
        }

        public static GeometryType GetFeatureGeometry(string type)
        {
            switch (type.ToLower())
            {
                case "point":
                    return GeometryType.Points;
                case "linestring":
                    return GeometryType.LineStrings;
                case "multilinestring":
                    return GeometryType.MultiLineStrings;
                case "polygon":
                    return GeometryType.Polygons;
                case "multipolygon":
                    return GeometryType.MultiPolygons;

            }
            throw new NotImplementedException("Geometry type of \"{type}\" is not supported yet");
        }

        private Dictionary<string, bool> _isLoaded= new Dictionary<string, bool>();
        private Dictionary<string, JSONCOntainer.Rootobject> _fileContents= new Dictionary<string, JSONCOntainer.Rootobject>();
        private List<string> _header;
        public GeometryType geometryType;
        public bool IsDegrees;

        private void Load(string path)
        {
            if (_isLoaded.ContainsKey(path)) return;

            var root = this._fileContents[path] =
                JsonConvert.DeserializeObject<JSONCOntainer.Rootobject>(
                    File.ReadAllText(path));

            root.features = root.features.Where(a => GetFeatureGeometry(a.geometry.type) == geometryType).ToArray();

            _header = root.features.Aggregate<JSONCOntainer.Feature, IEnumerable<string>>(new List<string>(),
                (list, feature) => list.Union(feature.properties.Keys)).ToList();
            _header.Add("#Geometry");
        }


        public List<List<object>> ReadData(string file)
        {
            Load(file);
            var Data = new List<List<object>>(_header.Count);
            foreach (var s in _header)
            {
                Data.Add(new List<object>());
            }

            int i = 0;
            foreach (var d in _fileContents)
            {
                foreach (var feature in d.Value.features)
                {
                    int j = 0;
                    foreach (var s in _header)
                    {
                        if (s.Equals("#Geometry"))
                        {
                            switch (geometryType)
                            {
                                case GeometryType.Points:
                                {
                                        var g = ((JArray) feature.geometry.coordinates).ToObject<double[]>();
                                        var v = new DVector2(g[0], g[1]);
                                        Data[j].Add(IsDegrees ? v.ToRadians() : v);
                                }
                                    break;
                                case GeometryType.MultiPoints:
                                    if (feature.geometry.type.Equals("MultiPoint"))
                                    {
                                        var gg = ((JArray) feature.geometry.coordinates).ToObject<double[][]>();
                                        Data[j].Add(gg.Select(g =>
                                        {
                                            var v = new DVector2(g[0], g[1]);
                                            return IsDegrees ? v.ToRadians() : v;
                                        }).ToArray());
                                    }
                                break;
                                case GeometryType.LineStrings:
                                    {
                                        var gg = ((JArray)feature.geometry.coordinates).ToObject<double[][]>();
                                        Data[j].Add(gg.Select(g =>
                                        {
                                            var v = new DVector2(g[0], g[1]);
                                            return IsDegrees ? v.ToRadians() : v;
                                        }).ToArray());
                                    }
                                    break;
                                case GeometryType.MultiLineStrings:
                                    if (feature.geometry.type.Equals("MultiLine"))
                                    {
                                        var ggg = ((JArray)feature.geometry.coordinates).ToObject<double[][][]>();
                                        Data[j].Add(ggg.Select(gg => gg.Select(g =>
                                        {
                                            var v = new DVector2(g[0], g[1]);
                                            return IsDegrees ? v.ToRadians() : v;
                                        }).ToArray()).ToArray());
                                    }
                                break;
                                case GeometryType.Polygons:
                                {
                                    var ggg = ((JArray)feature.geometry.coordinates).ToObject<double[][][]>();
                                    Data[j].Add(ggg.Select(gg => gg.Select(g =>
                                    {
                                        var v = new DVector2(g[0], g[1]);
                                        return IsDegrees ? v.ToRadians() : v;
                                    }).ToArray()).ToArray());
                                }
                                break;

                                case GeometryType.MultiPolygons:
                                {
                                    var gggg = ((JArray)feature.geometry.coordinates).ToObject<double[][][][]>();
                                    Data[j].Add(gggg.Select(ggg => ggg.Select(gg => gg.Select(g =>
                                    {
                                        var v = new DVector2(g[0], g[1]);
                                        return IsDegrees ? v.ToRadians() : v;
                                    }).ToArray()).ToArray()).ToArray());
                                }

                                break;
                            }
                        }
                        else
                        {
                            Data[j].Add(feature.properties[s]);
                        }

                        j++;
                    }

                    i++;
                }
            }

            return Data;
        }

        public List<string> ReadHeader(string file)
        {
            Load(file);
            return _header;
        }
    }
}
