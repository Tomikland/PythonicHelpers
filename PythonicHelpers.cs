using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pythonic
{
    public static class ListHelpers
    {
        public static List<T> ConcatToList<T>(params object[] toConcat)
        {
            List<T> result = new List<T>();
            foreach (var obj in toConcat)
            {
                if (obj is IEnumerable<T>)
                {
                    result.AddRange(obj as IEnumerable<T>);
                }
                else if (typeof(T).IsAssignableFrom(obj.GetType()))
                {
                    result.Add((T)obj);
                }
                else
                {
                    throw new Exception("Unrecognized type in ConcatAll");
                }
            }

            return result;
        }

        /// <summary>
        /// Dictionary that automatically inserts new keys with default values
        /// </summary>
        [Serializable]
        public class DefaultDict<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
        {
            public new TValue this[TKey key]
            {
                get
                {
                    TValue val;
                    if (!TryGetValue(key, out val))
                    {
                        val = new TValue();
                        Add(key, val);
                    }
                    return val;
                }
                set { base[key] = value; }
            }

            protected DefaultDict(SerializationInfo information, StreamingContext context)
                : base(information, context)
            {

            }
            public DefaultDict()
            {

            }

        }
        
        public static DefaultDict<TKey, TValue>  ToDefaultDict<TKey, TValue> (this IDictionary<TKey, TValue> collection) where TValue : new()
        {
            var d = new DefaultDict<TKey, TValue>();
            foreach (var item in collection)
            {
                d.Add(item.Key, item.Value);
            }
            return d;
        }

        /// <summary>
        /// Gets the first non-null element for a particular key out of a list of dictionaries
        /// On set, it writes to the first dictionary
        /// </summary>
        public class ChainMap<TKey, TValue> : List<IDictionary<TKey, TValue>>
        {

            public List<Dictionary<TKey, TValue>> dicts;
            public List<TKey> Keys => dicts.SelectMany(d => d.Keys).Distinct().ToList();

            public TValue this[TKey key]
            {
                get
                {
                    if(Keys.Contains(key) == false)
                    {
                        throw new KeyNotFoundException($"Key '{key}' is not present in any dictionary inside ChainMap");
                    }
                    return dicts.Where(d => d.ContainsKey(key) && d[key] != null).First()[key];
                }

                set => dicts[0][key] = value;
            }
            public TValue this[TKey key, Type valueType]
            {
                get
                {
                    if (Keys.Contains(key) == false)
                    {
                        throw new KeyNotFoundException($"Key '{key}' is not present in any dictionary inside ChainMap");
                    }
                    return dicts.Where(d => d.ContainsKey(key) && d[key] != null).First()[key];
                }

                set => dicts[0][key] = value;
            }

            public ChainMap()
            {
                dicts = new List<Dictionary<TKey, TValue>>();
            }
            public ChainMap(List<Dictionary<TKey, TValue>> dictionaries)
            {
                dicts = dictionaries;
            }

            /// <summary>
            /// Creates a copy of a ChainMap whose dictionaries are independent of the original but contain the same elements
            /// </summary>
            public ChainMap<TKey, TValue> Clone() => ChainMap.FromList(
                dicts.Select(d => d.ToDictionary(entry => entry.Key, entry => entry.Value)));

        }
        //Non-generic overload in order to utilize type inference for creating Chainmaps
        public class ChainMap 
        {
            public static ChainMap<TKey, TValue> FromDicts<TKey, TValue>(params Dictionary<TKey, TValue>[] dictionaries)
                => new ChainMap<TKey, TValue>() { dicts = dictionaries.ToList() };

            public static ChainMap<TKey, TValue> FromList<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> dictionaries)
                => new ChainMap<TKey, TValue>() { dicts = dictionaries.ToList() };
        }

    }    
    
}
