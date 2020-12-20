﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventofCode.AoC_2020
{
    public class DicWithDefault<TKey, TValue>
    {
        public DicWithDefault(TValue def = default(TValue))
        {
            _def = def;
        }

        private readonly Dictionary<TKey, TValue> _dic = new Dictionary<TKey, TValue>();
        private readonly TValue _def;
        public IEnumerable<TKey> Keys => _dic.Keys;

        public TValue Get(TKey key)
        {
            if (_dic.TryGetValue(key, out var res))
                return res;
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