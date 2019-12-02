using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core.Tool
{
    public class MultiDictionary<TKey, TValue>
    {
        private Dictionary<TKey, List<TValue>> _data = new Dictionary<TKey, List<TValue>>();

        public void Add(TKey k, TValue v)
        {
            if (_data.ContainsKey(k))
                _data[k].Add(v);
            else
                _data.Add(k, new List<TValue>() { v });
        }
        
    }
}
