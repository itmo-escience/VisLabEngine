using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FusionData
{
    namespace Data
    {
        public class DataType
        {
            public Func<DataElement, DataElement> ObjectCast;
            public Func<DataElement> DefaultGen;
            public Func<object, bool> Validate = (o) => true;
            public DataType(string name)
            {
                ObjectCast = o => new DataElement(o, this);

                Name = name;
            }
            public readonly string Name;

            public override string ToString()
            {
                return Name;
            }

            public ISet<DataType> TypeConversions;

            public DataElement RecursiveCast(DataElement obj)
            {

                if (SafeRecursiveCast(obj, this, out var cast))
                {
                    return cast.Value;
                }
                throw new ArgumentException();
            }



            public bool IsOfType(DataType type)
            {
                Queue<DataType> typesQueue = new Queue<DataType>();
                HashSet<DataType> checkedTypes = new HashSet<DataType>();
                typesQueue.Enqueue(this);
                while (typesQueue.Any())
                {
                    var t = typesQueue.Dequeue();
                    checkedTypes.Add(t);
                    if (t == type)
                    {
                        return true;
                    }
                    foreach (var cc in t.TypeConversions)
                    {
                        if (!checkedTypes.Contains(cc))
                        {
                            typesQueue.Enqueue(cc);
                        }
                    }
                }

                return false;
            }

            public static bool SafeRecursiveCast(DataElement obj, DataType target, out DataElement? cast)
            {
                Queue<Tuple<DataType, object>> typesQueue = new Queue<Tuple<DataType, object>>();
                HashSet<DataType> checkedTypes = new HashSet<DataType>();
                typesQueue.Enqueue(new Tuple<DataType, object>(obj.Type, null));
                while (typesQueue.Any())
                {
                    var t = typesQueue.Dequeue();
                    checkedTypes.Add(t.Item1);
                    if (t.Item1 == target)
                    {
                        if (t.Item2 is null)
                        {
                            cast = obj;
                            return true;
                        }

                        if (t.Item2 is Tuple<DataType, object>)
                        {
                            List<DataType> pipeline = new List<DataType>();
                            while (t.Item2 != null)
                            {
                                pipeline.Add(t.Item1);
                                t = t.Item2 as Tuple<DataType, object>;
                            }

                            pipeline.Reverse();
                            cast = pipeline.Aggregate(obj, (o, type) => type.ObjectCast(o));
                            return true;
                        }
                    }

                    foreach (var v in t.Item1.TypeConversions.Where(a => !checkedTypes.Contains(a)))
                    {
                        typesQueue.Enqueue(new Tuple<DataType, object>(v, t));
                    }
                }

                cast = null;
                return false;
            }

            public static class BasicTypes
            {
                public static DataElement DefaultNull => new DataElement(null, Default);

                public static DataType Default = new DataType("Default")
                {
                    TypeConversions = new HashSet<DataType>() {},
                };

                public static DataType Index = new DataType("Index")
                {
                    TypeConversions = new HashSet<DataType>() { Integer },
                };


                public static DataType Quantitive = new DataType("Quantitive")
                {
                    TypeConversions = new HashSet<DataType>() { Default }
                };

                public static DataType Orderable = new DataType("Orderable")
                {
                    TypeConversions = new HashSet<DataType>() { Default }
                };

                public static DataType Numeric = new DataType("Numeric")
                {
                    TypeConversions = new HashSet<DataType>() { Default, Quantitive }
                };

                public static DataType String = new DataType("String")
                {
                    TypeConversions = new HashSet<DataType>() { Default, Orderable },
                    ObjectCast = o => new DataElement(Convert.ToString(o.Item),String),
                };

                public static DataType Integer = new DataType("Integer")
                {
                    TypeConversions = new HashSet<DataType>() { Default, Orderable, Quantitive, Numeric },
                    ObjectCast = o => new DataElement(Convert.ToInt32(o.Item), Integer),
                    DefaultGen= () => new DataElement(0, Integer),
                };

                public static DataType Float = new DataType("Float")
                {
                    TypeConversions = new HashSet<DataType>() { Default, Orderable, Quantitive, Numeric },
                    ObjectCast = o => new DataElement(Convert.ToSingle(o.Item), Float),
                    DefaultGen = () => new DataElement(0, Float),
                };


                static BasicTypes()
                {
                    Quantitive.TypeConversions.Add(Float);
                }
            }

            public static class CompositeTypes
            {
                public class CompositeType : DataType
                {
                    public DataType ElementType { get; set; }

                    public CompositeType(string name) : base(name)
                    {
                    }
                }

                public static CompositeType Pair = new CompositeType("Pair")
                {
                    TypeConversions = new HashSet<DataType>() { },
                    ObjectCast = o => new DataElement(((ICollection<DataElement>)o.Item).Take(2).Select(Pair.ElementType.ObjectCast), Pair),
                    Validate = o => (o is IEnumerable),
                };

                public static CompositeType List = new CompositeType("List")
                {
                    TypeConversions = new HashSet<DataType>() { },
                    ObjectCast = o => new DataElement(((ICollection<DataElement>) o.Item).Select(Pair.ElementType.ObjectCast), List),
                    Validate = o => (o is IEnumerable),
                };
            }
        }

    }

}
