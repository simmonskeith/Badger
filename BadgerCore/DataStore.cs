using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Core
{
    public static class DataStore
    {
        static Dictionary<string, object> _store;

        public static void Initialize()
        {
            _store = new Dictionary<string, object>();
        }

        public static int Count { get { return _store.Count; } }

        public static T Get<T>(string key)
        {
            return (T)_store[key];
        }

        public static string Get(string key)
        {
            return _store[key] as string;
        }

        public static void Add(string key, object value)
        {
            _store[key] = value;
        }

        public static bool Contains(string key)
        {
            return _store.Keys.Contains(key);
        }

        public static void Remove(string key)
        {
            _store.Remove(key);
        }
        
    }
}
