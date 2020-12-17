using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day17 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(112, 848, "Day17_test.txt")]
        [TestCase(273, 1504, "Day17.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            var z0 = source.ToSparseBuffer('.');

            var genWorld3d = new DicWithDefault<GenCoord, char>('.');
            var genWorld4d = new DicWithDefault<GenCoord, char>('.');
            for (int x = 0; x < z0.Width; x++)
                for (int y = 0; y < z0.Height; y++)
                {
                    var coord3d = new GenCoord(new List<int>() { x, y, 0 });
                    var coord4d = new GenCoord(new List<int>() { x, y, 0, 0 });
                    genWorld3d[coord3d] = z0[Coord.FromXY(x, y)];
                    genWorld4d[coord4d] = z0[Coord.FromXY(x, y)];
                }



            var world = new DicWithDefault<int, SparseBuffer<char>>();
            world[0] = z0;

            //
            // LogAndReset("Parse", sw);
            // for (int i = 0; i < 6; i++)
            // {
            //     world = Execute3dLife(world, 0, z0.Width, i);
            // }
            //
            // var sum = 0;
            // foreach (var layer in world.Keys)
            // {
            //     sum += world[layer].Count('#');
            // }

            for (int i = 0; i < 6; i++)
            {
                genWorld3d = ExecuteGenDLife(genWorld3d);
            }
            part1 = genWorld3d.Count('#');
            //
            // var sum2 = 0;
            // foreach (var c in genWorld3d.Keys)
            // {
            //     if (genWorld3d[c] == '#')
            //         sum2++;
            // }




            //part1 = sum2;

            LogAndReset("*1", sw);
            for (int i = 0; i < 6; i++)
            {
                genWorld4d = ExecuteGenDLife(genWorld4d);
            }

            // sum2 = 0;
            // foreach (var c in genWorld4d.Keys)
            // {
            //     if (genWorld4d[c] == '#')
            //         sum2++;
            // }

            part2 = genWorld4d.Count('#');



            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private DicWithDefault<GenCoord, char> ExecuteGenDLife(DicWithDefault<GenCoord, char> world)
        {
            var res = new DicWithDefault<GenCoord, char>('.');
            var neighborCounter = new DicWithDefault<GenCoord, int>(0);
            foreach (var c in world.Keys)
            {
                if (world[c] == '#')
                    foreach (var n in c.Neighbors())
                    {
                        neighborCounter[n]++;
                    }
            }

            foreach (var c in neighborCounter.Keys)
            {
                var neighbors = neighborCounter[c];
                if (neighbors == 3 && world[c] == '.')
                    res[c] = '#';
                else if ((neighbors == 2 || neighbors == 3) && world[c] == '#')
                    res[c] = '#';
            }

            return res;
        }


        private DicWithDefault<int, SparseBuffer<char>> Execute3dLife(DicWithDefault<int, SparseBuffer<char>> world,
                int gen0Min, int gen0Max, int gen)
        {

            Log("=====================================");
            Log("Generation: " + gen);
            var nextWorld = new DicWithDefault<int, SparseBuffer<char>>();

            var nextLayerNumbers = world.Keys;//.Append(world.Keys.Max()+1).Append(world.Keys.Min()-1).ToList();
            var boundMin = gen0Min - gen;
            var boundMax = gen0Max + gen;
            foreach (var z in Enumerable.Range(-gen - 1, boundMax - boundMin + 1))
            {
                var layer = world[z];
                var lnext = world[z + 1];
                var lprev = world[z - 1];
                var nextLayer = new SparseBuffer<char>('.');
                nextWorld[z] = nextLayer;
                foreach (var x in Enumerable.Range(boundMin, boundMax - boundMin + 1))
                    foreach (var y in Enumerable.Range(boundMin, boundMax - boundMin + 1))
                    {
                        var k = Coord.FromXY(x, y);
                        var neigh = 0;
                        foreach (var c in k.GenAdjacent8())
                        {
                            if (layer != null && layer[c] == '#')
                                neigh++;
                            if (lnext != null && lnext[c] == '#')
                                neigh++;
                            if (lprev != null && lprev[c] == '#')
                                neigh++;
                        }
                        if (lnext != null && lnext[k] == '#')
                            neigh++;
                        if (lprev != null && lprev[k] == '#')
                            neigh++;

                        var oldchar = layer?[k] ?? '.';
                        if (oldchar == '.' && neigh == 3)
                            nextLayer[k] = '#';
                        else if (oldchar == '#' && (neigh == 2 || neigh == 3))
                            nextLayer[k] = '#';
                    }

                Log($"z={z}");
                Log(nextLayer.ToString(c => c.ToString()));
            }

            return nextWorld;
        }
    }

    [DebuggerDisplay("({Coords})")]
    public class GenCoord
    {
        private readonly List<int> _values;

        public GenCoord(List<int> values)
        {
            _values = values;
        }

        protected bool Equals(GenCoord other)
        {
            if (other._values.Count != _values.Count)
                return false;
            return !_values.Where((t, i) => other._values[i] != t).Any();
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
            var res = 0;
            foreach (var t in _values)
                res = unchecked(res * 347) ^ t;

            return res;
        }

        public IEnumerable<GenCoord> Neighbors()
        {
            return GenNeighborRec(new List<int>()).Where(c => !c.Equals(this));
        }

        private IEnumerable<GenCoord> GenNeighborRec(List<int> values)
        {
            if (values.Count == _values.Count)
            {
                yield return new GenCoord(values);
            }
            else
            {
                foreach (var i in Enumerable.Range(-1, 3))
                {
                    var newList = new List<int>(values);
                    newList.Add(_values[newList.Count] + i);
                    foreach (var res in GenNeighborRec(newList))
                        yield return res;
                }
            }
        }
    }
}

// var comp = new IntCodeComputer(source[0]);
// comp.Execute();
// var part1 = (int)comp.LastOutput;
// return (part1, 0);
