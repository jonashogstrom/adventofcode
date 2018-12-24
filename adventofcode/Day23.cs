using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace adventofcode
{
    internal class Day23 : BaseDay
    {
        private List<Bot[]> _cliques;

        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 2;

            Part1TestSolution = 7;
            Part2TestSolution = null;
            Part1Solution = 164;
            Part2Solution = 122951778;

        }

        protected override void DoRun(string[] input)
        {
            var bots = new List<Bot>();
            foreach (var str in input)
            {
                var ints = GetIntArr(str, true);
                bots.Add(new Bot(ints[0], ints[1], ints[2], ints[3]));
            }

            var strong = bots.OrderByDescending(b => b.Strength).First();
            var inRange = InRangeOfBot(bots, strong);
            Part1 = inRange;

            var ovarlaps = EmptyArr(input.Length, input.Length, false);

            var sb = new StringBuilder();
            for (int i = 0; i < bots.Count; i++)
            {
                var b1 = bots[i];
                b1.BotNum = i;
                if (i >= 1)
                    sb.Append(new string('-', i));
                sb.Append('O');

                for (int i2 = i + 1; i2 < bots.Count; i2++)
                {
                    var b2 = bots[i2];
                    if (Overlaps(b1, b2))
                    {
                        ovarlaps[i][i2] = true;
                        ovarlaps[i2][i] = true;
                        b1.OverLapCount++;
                        b2.OverLapCount++;
                        sb.Append('X');
                    }
                    else
                    {
                        sb.Append('.');
                    }
                }

                sb.AppendLine();
            }

            _cliques = new List<Bot[]>();
            BronKerbosch2(new Bot[0], bots.ToArray(), new Bot[0], bots);
            var cliques = _cliques.OrderByDescending(x => x.Length).ToList();

            var p = FindBoundingBox(cliques.First());
            Part2 = p.x + p.y + p.z;

        }

        private Coord3d FindBoundingBox(Bot[] bots)
        {
            var maxX = int.MaxValue;
            var maxY = int.MaxValue;
            var maxZ = int.MaxValue;
            var minX = int.MinValue;
            var minY = int.MinValue;
            var minZ = int.MinValue;
            foreach (var b in bots)
            {
                maxX = Math.Min(maxX, b.X + b.Strength);
                maxY = Math.Min(maxY, b.Y + b.Strength);
                maxZ = Math.Min(maxZ, b.Z + b.Strength);
                minX = Math.Max(minX, b.X - b.Strength);
                minY = Math.Max(minY, b.Y - b.Strength);
                minZ = Math.Max(minZ, b.Z - b.Strength);
            }

            var c = new Coord3d(
                minX + (maxX - minX) / 2,
                minY + (maxY - minY) / 2,
                minZ + (maxZ - minZ) / 2
            );

            //      c = new Coord3d(47846200, 53749243, 21356335);
            var r = new Random();
            var ok = false;
            var counter = 0;
            while (!ok)
            {

                var miss = bots.Where(b => !IsInRange(b, c)).ToList();
                var oldInRange = bots.Where(b => IsInRange(b, c)).ToList();
                Log("missing : " + miss.Count);
                if (miss.Any())
                {
                    var m = miss.First();
                    var moveStep = -1;
                    var edge = new HashSet<Bot>();
                    while (!IsInRange(m, c))
                    {
                        counter++;
                        var dist = Dist(m, c);
                        var left = dist - m.Strength;
                        if (moveStep == -1)
                            moveStep = Math.Max(1, left / 3);
                        //    var move = 1; // Math.Max(1, left / 10);
                        if (counter % 10000 == 0)
                            Log($"{c.x}, {c.y}, {c.z} => {m.X}, {m.Y}, {m.Z} dist: {dist} Left: {left} MoveStep {moveStep}");
                        var xDiff = c.x - m.X;
                        var yDiff = c.y - m.Y;
                        var zDiff = c.z - m.Z;

                        
                        var xmove = (xDiff > 0 ? -1:1)*moveStep;
                        var ymove = (yDiff > 0 ? -1:1)* moveStep;
                        var zmove = (zDiff > 0 ? -1:1)* moveStep;


                        var xmove2 = !edge.Any() || r.Next(2) == 0 ? xmove  : 0;
                        var ymove2 = !edge.Any() || r.Next(2) == 0 ? ymove  : 0;
                        var zmove2 = !edge.Any() || r.Next(2) == 0 ? zmove  : 0;
                        var tempCoord = new Coord3d(
                            c.x + xmove2,
                            c.y + ymove2,
                            c.z + zmove2);

                        var validate = edge.FirstOrDefault(b => !IsInRange(b, tempCoord));

                        if (validate == null)
                        {
                            validate = oldInRange.FirstOrDefault(b => !IsInRange(b, tempCoord));

                            if (validate == null)
                            {
                                //                  Log("OK: " +  $" {c.x}, {c.y}, {c.z} =>{ tempCoord.x}, { tempCoord.y}, { tempCoord.z} ({xmove2},{ymove2},{zmove2})");

                                c = tempCoord;
                            }
                            else
                            {
                                if (moveStep == 1)
                                    edge.Add(validate);
                                moveStep = Math.Max(1, moveStep / 3);
                            
                                //                Log("!!: " + $" {c.x}, {c.y}, {c.z} =>{ tempCoord.x}, { tempCoord.y}, { tempCoord.z} ({xmove2},{ymove2},{zmove2})");
                            }
                        }
//                        else
//                        {
//                            moveStep = Math.Max(1, moveStep / 3);
//                        }
                    }

                }
                else
                    ok = true;

            }

            var finetune = c;
            while (bots.All(b => IsInRange(b, finetune)))
            {
                Log($"FineTuneX: {c.x}, {c.y}, {c.z}");
                c = finetune;
                finetune = new Coord3d(finetune.x - 1, finetune.y, finetune.z);
            }

            finetune = c;
            while (bots.All(b => IsInRange(b, finetune)))
            {
                Log($"FineTuneY: {c.x}, {c.y}, {c.z}");
                c = finetune;
                finetune = new Coord3d(finetune.x, finetune.y - 1, finetune.z);
            }
            finetune = c;
            while (bots.All(b => IsInRange(b, finetune)))
            {
                Log($"FineTuneZ: {c.x}, {c.y}, {c.z}");
                c = finetune;
                finetune = new Coord3d(finetune.x, finetune.y - 1, finetune.z);
            }

            return c;
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Bron%E2%80%93Kerbosch_algorithm
        /// </summary>
        public void BronKerbosch1(IEnumerable<Bot> r, Bot[] p, Bot[] x, List<Bot> allBots)
        {
            if (!p.Any() && !x.Any())
            {
                _cliques.Add(r.ToArray());
                return;
            }

            foreach (var v in p)
            {
                var Nv = new HashSet<Bot>();
                foreach (var t in allBots)
                {
                    if (Overlaps(v, t) && v != t)
                        Nv.Add(t);
                }
                BronKerbosch1(
                    r.Union(new[] { v }),
                    p.Where(bot => Nv.Contains(bot)).ToArray(),
                    x.Where(bot => Nv.Contains(bot)).ToArray(),
                    allBots);
                p = p.Where(bot => bot != v).ToArray();
                x = x.Union(new[] { v }).ToArray();
            }
        }
        
        public void BronKerbosch2(IEnumerable<Bot> r, Bot[] p, Bot[] x, List<Bot> allBots)
        {
            if (!p.Any() && !x.Any())
            {
                _cliques.Add(r.ToArray());
                return;
            }

            var pivot = p.Union(x).OrderByDescending(bot => bot.OverLapCount).First();
            var Npivot = Connected(allBots, pivot);
            foreach (var v in p.Except(Npivot))
            {
                var Nv = Connected(allBots, v);
                BronKerbosch2(
                    r.Union(new[] { v }),
                    p.Intersect(Nv).ToArray(),
                    x.Intersect(Nv).ToArray(),
                    allBots);
                p = p.Where(bot => bot != v).ToArray();
                x = x.Union(new[] { v }).ToArray();
            }
        }

        private HashSet<Bot> Connected(List<Bot> allBots, Bot v)
        {
            var Nv = new HashSet<Bot>();
            foreach (var t in allBots)
            {
                if (Overlaps(v, t) && v != t)
                    Nv.Add(t);
            }

            return Nv;
        }
        
        private bool Overlaps(Bot b1, Bot b2)
        {
            return Dist(b1, b2) <= b1.Strength + b2.Strength;
        }

        private static int Dist(Bot b1, Bot b2)
        {
            var dist = Math.Abs(b2.X - b1.X) +
                       Math.Abs(b2.Y - b1.Y) +
                       Math.Abs(b2.Z - b1.Z);
            return dist;
        }

        private bool IsInRange(Bot b1, Coord3d c)
        {
            return Dist(b1, c) <= b1.Strength;
        }

        private static int Dist(Bot b1, Coord3d c)
        {
            return Math.Abs(c.x - b1.X) +
                   Math.Abs(c.y - b1.Y) +
                   Math.Abs(c.z - b1.Z);
        }

        private int InRangeOfBot(List<Bot> bots, Bot bot)
        {
            var inRange = 0;
            foreach (var b in bots)
            {
                var dist = Math.Abs(b.X - bot.X) +
                           Math.Abs(b.Y - bot.Y) +
                           Math.Abs(b.Z - bot.Z);
                if (dist <= bot.Strength)
                {
                    inRange++;
                    Log($"In range: {b.X},{b.Y},{b.Z} dist: {dist}", 2);
                }
                else
                {
                    Log($"NOT in range: {b.X},{b.Y},{b.Z} dist: {dist}", 2);
                }
            }

            return inRange;
        }

        public class Bot
        {
            public Bot(int x, int y, int z, int strength)
            {
                X = x;
                Y = y;
                Z = z;
                Strength = strength;
            }

            public int X;
            public int Y;
            public int Z;
            public int Strength;
            public int OverLapCount { get; set; }
            public int BotNum { get; set; }
        }
    }
}