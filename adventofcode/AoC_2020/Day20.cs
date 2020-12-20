using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using NUnit.Framework;


namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day20 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(20899048083289, 273, "Day20_test.txt")]
        [TestCase(45443966642567, 1607, "Day20.txt")]
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

            var tiles = new Dictionary<int, Tile>();
            foreach (var x in source.AsGroups())
            {
                var tile = ParseTile(x);
                tiles[tile.index] = tile;
            }

            LogAndReset("Parse", sw);
            var edgeId = new DicWithDefaultFunc<string, List<Tile>>(() => new List<Tile>());
            foreach (var t in tiles.Values)
            {

                foreach (var edge in t.Edges)
                {
                    edgeId[edge].Add(t);
                    edgeId[new string(edge.Reverse().ToArray())].Add(t);
                }
            }

            var unique = new DicWithDefault<int, int>();

            foreach (var t in edgeId.Keys)
            {
                if (edgeId[t].Count == 1)
                {
                    //                    Log($"UniqueEdge found in {edgeId[t].First().index}: {t}");
                    unique[edgeId[t].First().index]++;
                }
            }

            var xx = 1L;

            var corners = new List<Tile>();
            foreach (var x in unique.Keys)
            {
                //                Log($"UniqueEdges for {x} = {unique[x]}");
                if (unique[x] == 4)
                {
                    xx *= x;
                    corners.Add(tiles[x]);
                }
            }


            part1 = xx;
            LogAndReset("*1", sw);

            var topLeft = corners.First();
            var temp = topLeft.AllOrientations().Where(x => edgeId[x.SN].Count == 1 && edgeId[x.SW].Count == 1).ToList();
            topLeft = temp.First();

            var side = (int)Math.Sqrt(tiles.Count);
            var allTiles = new SparseBuffer<Tile>();
            allTiles[Coord.FromXY(0, 0)] = topLeft;
            for (var row = 0; row < side; row++)
                for (var col = 0; col < side; col++)
                {
                    var c = Coord.FromXY(col, row);
                    if (allTiles[c] == null)
                    {
                        var northNeighbour = allTiles[c.Move(Coord.N)];
                        var westNeighbour = allTiles[c.Move(Coord.W)];
                        string northEdge = null;
                        Tile t = null;
                        if (northNeighbour != null)
                        {
                            northEdge = northNeighbour.SS;
                            t = edgeId[northEdge].First(x => x.index != northNeighbour.index);
                        }
                        string westEdge = null;
                        if (t == null && westNeighbour != null)
                        {
                            westEdge = westNeighbour.SE;
                            t = edgeId[westEdge].First(x => x.index != westNeighbour.index);
                        }

                        foreach (var t2 in t.AllOrientations())
                            if ((northEdge == null || t2.SN == northEdge) &&
                                (westEdge == null || t2.SW == westEdge))
                            {
                                allTiles[c] = t2;
                                break;
                            }
                        //                        Log($"Tile x={c.X} y={c.Y} index: {allTiles[c].index}");
                    }
                }

            LogMidTime("FoundAllPieces", sw);

            var bigMap = new SparseBuffer<char>('-');
            // foreach (var c in allTiles.Keys)
            // {
            //     var tile = allTiles[c];
            //     for (int col = 0; col < tile.data.Width; col++)
            //         for (int row = 0; row < tile.data.Height; row++)
            //         {
            //             var bigCoord = Coord.FromXY(c.X * (tile.data.Width + 1) + col - 1, c.Y * (tile.data.Height + 1) + row - 1);
            //             bigMap[bigCoord] = tile.data[Coord.FromXY(col, row)];
            //         }
            // }
            // LogMidTime("built bitmap1", sw);

            //            Log(bigMap.ToString(c=>c.ToString()));

            bigMap = new SparseBuffer<char>('-');
            foreach (var c in allTiles.Keys)
            {
                var tile = allTiles[c];
                for (int col = 1; col < tile.data.Width - 1; col++)
                    for (int row = 1; row < tile.data.Height - 1; row++)
                    {
                        var bigCoord = Coord.FromXY(c.X * (tile.data.Width - 2) + col - 1, c.Y * (tile.data.Height - 2) + row - 1);
                        bigMap[bigCoord] = tile.data[Coord.FromXY(col, row)];
                    }
            }

            LogMidTime("built bitmap2", sw);
            //            Log(bigMap.ToString(c => c.ToString()));

            var monster = GetMonster();
            var maxMonsters = 0;
            foreach (var m in monster.AllOrientations())
            {
                var res = FindMonsterInMap(m, bigMap);
                maxMonsters = Math.Max(maxMonsters, res);
                if (maxMonsters > 0)
                    break;
//                LogMidTime("looking for more monsters", sw);
            }
            LogMidTime("found all monsters", sw);


            part2 = bigMap.Count('#') - maxMonsters * monster.data.Count('#');
            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private int FindMonsterInMap(Tile monster, SparseBuffer<char> bigMap)
        {
            var foundMonsters = 0;
            foreach (var topLeft in bigMap.Keys)
            {
                var match = true;
                foreach (var monstercoord in monster.data.Keys)
                {
                    if (monster.data[monstercoord] == '#')
                        if (bigMap[topLeft.Move(monstercoord)] != '#')
                        {
                            match = false;
                            break;
                        }

                }
                if (match)
                    foundMonsters++;
            }

            return foundMonsters;
        }

        private Tile GetMonster()
        {
            var s = new List<string>
            {
                "                  # ",
                "#    ##    ##    ###",
                " #  #  #  #  #  #   "
            };
            return new Tile(s.ToArray().ToSparseBuffer(), -1);
        }

        private Tile ParseTile(IList<string> list)
        {
            var index = GetInts(list.First()).First();
            var data = list.Skip(1).ToArray().ToSparseBuffer('.');
            return new Tile(data, index);

        }
    }

    internal class Tile
    {
        public string Description { get; }
        public SparseBuffer<char> data;
        public int index;
        private List<Tile> _allOrientations;
        public string SN { get; }
        public string SE { get; }
        public string SW { get; }
        public string SS { get; }
        public string[] Edges { get; }

        public Tile(SparseBuffer<char> data, int index, string description = "")
        {
            Description = description;
            this.data = data;
            this.index = index;
            SN = "";
            SE = "";
            SW = "";
            SS = "";
            for (int i = 0; i < data.Width; i++)
            {
                SN += data[Coord.FromXY(i, 0)];
                SS += data[Coord.FromXY(i, data.Height - 1)];
                SW += data[Coord.FromXY(0, i)];
                SE += data[Coord.FromXY(data.Width - 1, i)];
            }

            Edges = new[] { SN, SE, SS, SW };
        }

        public IEnumerable<Tile> AllOrientations()
        {
            // if (_allOrientations == null)
            // {
            _allOrientations = new List<Tile>();
            var temp = this;
            var temp2 = FlipV();
            for (int i = 0; i < 4; i++)
            {
                temp = temp.Rot90();
                yield return temp;
                //                    _allOrientations.Add(temp);
                temp2 = temp2.Rot90();
                yield return temp2;
                //                    _allOrientations.Add(temp2);
            }
            // }

            //          return _allOrientations;
        }

        public Tile FlipH()
        {
            var res = new SparseBuffer<char>();
            for (var x = 0; x < data.Width; x++)
                for (var y = 0; y < data.Height; y++)
                {
                    res[Coord.FromXY(x, y)] = data[Coord.FromXY(data.Width - x - 1, y)];
                }
            return new Tile(res, index, Description + "FlipH");
        }

        public Tile FlipV()
        {
            var res = new SparseBuffer<char>();
            for (var x = 0; x < data.Width; x++)
                for (var y = 0; y < data.Height; y++)
                {
                    res[Coord.FromXY(x, y)] = data[Coord.FromXY(x, data.Height - y - 1)];
                }
            return new Tile(res, index, Description + "FlipV");
        }
        public Tile Rot90()
        {
            var res = new SparseBuffer<char>();
            for (var x = 0; x < data.Height; x++)
                for (var y = 0; y < data.Width; y++)
                {
                    res[Coord.FromXY(x, y)] = data[Coord.FromXY(y, data.Height - x - 1)];
                }
            return new Tile(res, index, Description + "Rot90");
        }
    }
}
