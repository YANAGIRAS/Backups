﻿using System;
using System.Collections.Generic;
using System.Linq;
namespace Implem.Libraries.Utilities
{
    public static class Linqs
    {
        public static void RemoveAll<K, V>(
            this IDictionary<K, V> self, Func<K, V, bool> peredicate)
        {
            foreach (var key in self.Keys.ToArray().Where(key => peredicate(key, self[key])))
            {
                self.Remove(key);
            }
        }

        public static Dictionary<K, V> AddRange<K, V>(
            this IDictionary<K, V> self, IDictionary<K, V> data)
        {
            foreach (var dataPart in data)
            {
                self.Add(dataPart);
            }
            return self.ToDictionary(o => o.Key, o => o.Value);
        }
    }
}
