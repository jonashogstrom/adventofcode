using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventofCode.Utils
{
    public class DicWithDefault<TKey, TValue>
    {
        public DicWithDefault(TValue def = default(TValue))
        {
            _def = def;
        }
        
        public DicWithDefault(Func<TValue> factory)
        {
            _factory = factory;
        }

        private readonly Dictionary<TKey, TValue> _dic = new Dictionary<TKey, TValue>();
        private readonly TValue _def;
        private readonly Func<TValue> _factory;
        public IEnumerable<TKey> Keys => _dic.Keys;

        public TValue Get(TKey key)
        {
            if (_dic.TryGetValue(key, out var res))
                return res;
            if (_factory != null)
            {
                _dic[key] = _factory();
                return _dic[key];
            }
            return _def;
        }

        public void Set(TKey key, TValue value)
        {
            if (value.Equals(_def))
            {
                _dic.Remove(key);
            }
            else
            {
                _dic[key] = value;
            }
        }

        public TValue this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public long Count(TValue c)
        {
            return _dic.Values.Count(x => x.Equals(c));
        }
        public IEnumerable<TValue> Values => _dic.Values;

        public void RemoveKey(TKey c)
        {
            _dic.Remove(c);
        }
    }
    public class DicWithDefaultFunc<TKey, TValue>
    {
        private readonly Func<TValue> _f;

        public DicWithDefaultFunc(Func<TValue> f)
        {
            _f = f;
        }

        private readonly Dictionary<TKey, TValue> _dic = new Dictionary<TKey, TValue>();
        public IEnumerable<TKey> Keys => _dic.Keys;

        public TValue Get(TKey key)
        {
            if (!_dic.TryGetValue(key, out var res))
            {
                res = _f();
                _dic[key] = res;
            }
            return res;
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