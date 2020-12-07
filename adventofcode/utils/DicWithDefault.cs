using System.Collections.Generic;

namespace AdventofCode.AoC_2020
{
    public class DicWithDefault<TKey, TValue>
    {
        public DicWithDefault(TValue def = default(TValue))
        {
            _def = def;
        }

        private Dictionary<TKey, TValue> _dic = new Dictionary<TKey, TValue>();
        private TValue _def;
        public IEnumerable<TKey> Keys => _dic.Keys;

        public TValue Get(TKey key)
        {
            if (_dic.TryGetValue(key, out var res))
                return res;
            return _def;
        }

        public void Set(TKey key, TValue value)
        {
            _dic[key] = value;
        }

        public TValue this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }
    }
}