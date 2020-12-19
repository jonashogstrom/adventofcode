using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AdventofCode.AoC_2020
{
    [DebuggerDisplay("({Coords})")]
    public class GenCoord
    {
        private readonly List<int> _values;
        private readonly int _hash;

        public GenCoord(List<int> values)
        {
            _values = values;


            _hash = 0;
            foreach (var t in _values)
                _hash = unchecked(_hash * 347) ^ t;

        }

        protected bool Equals(GenCoord other)
        {
            return _values.SequenceEqual(other._values);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GenCoord)obj);
        }

        public string Coords => string.Join(", ", _values);

        public override int GetHashCode()
        {
            return _hash;
        }

        public IEnumerable<GenCoord> Neighbors()
        {
            return GenNeighborRec(new int[_values.Count], 0);
        }

        private IEnumerable<GenCoord> GenNeighborRec(int[] values, int dim)
        {
            if (dim == _values.Count)
            {
                if (!_values.SequenceEqual(values))
                    yield return new GenCoord(values.ToList());
            }
            else
            {
                for (var i = -1; i <= 1; i++)
                {
                    values[dim] = _values[dim] + i;
                    foreach (var res in GenNeighborRec(values, dim + 1))
                        yield return res;
                }
            }
        }

        public long Dist(GenCoord other)
        {
            var sum = 0L;
            for (int i = 0; i < _values.Count; i++)
                sum += Math.Abs(_values[i] - other._values[i]);
            return sum;
        }
    }
}