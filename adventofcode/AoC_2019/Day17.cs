using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AdventofCode.AoC_2018;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day17 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]

        [TestCase(11372, 1155497, "Day17.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = Compute(source);
            DoAsserts(res, exp1, exp2);
        }

        private (Part1Type Part1, Part2Type Part2) Compute(string[] source)
        {
            // part 1
            var c1 = new IntCodeComputer(source[0]);
            var world1 = new SparseBuffer<char>();
            c1.Execute();

            var bot1 = ParseOutputAsWorld(c1, world1);
            Log(world1.ToString(ch => ch.ToString()));

            var sum1 = CountIntersections(world1);

            // part 2
            var c2 = new IntCodeComputer(source[0]);
            var world2 = new SparseBuffer<char>();
            c2.WriteMemory(0, 2);

            c2.Execute();

            var bot2 = ParseOutputAsWorld(c2, world2);
            Log(world2.ToString(ch => ch.ToString()));

            var program = new List<int>();
            var dist = 0;
            var done = false;
            var sb = new StringBuilder();
            while (!done)
            {
                var ahead = bot2.pos.Move(bot2.dir);
                if (world2[ahead] == '#')
                {
                    dist += 1;
                    bot2.pos = ahead;
                }
                else if (world2[bot2.pos.Move(bot2.dir.RotateCCW90())] == '#')
                {
                    // L
                    dist = AddDistToProgram(dist, program, sb);
                    program.Add((byte)'L');
                    sb.Append("L,");
                    bot2.dir = bot2.dir.RotateCCW90();
                }
                else if (world2[bot2.pos.Move(bot2.dir.RotateCW90())] == '#')
                {
                    // R
                    dist = AddDistToProgram(dist, program, sb);
                    program.Add((byte)'R');
                    sb.Append("R,");
                    bot2.dir = bot2.dir.RotateCW90();
                }
                else
                {
                    dist = AddDistToProgram(dist, program, sb);
                    done = true;
                }
            }
            Log(sb.ToString());


            var main = "A,B,A,B,C,A,B,C,A,C";
            var A = "R,6,L,10,R,8";
            var B = "R,8,R,12,L,8,L,8";
            var C = "L,10,R,6,R,6,L,8";
            var input = new List<long>();
            input.AddRange(FormatInput(main));
            input.AddRange(FormatInput(A));
            input.AddRange(FormatInput(B));
            input.AddRange(FormatInput(C));
            input.AddRange(FormatInput("y"));
            foreach (var i in input)
                c2.AddInput(i);
            c2.Execute();
            var x = "";
            while (c2.OutputQ.Any())
            {
                x += (char)c2.OutputQ.Dequeue();
            }

            var collectedDust = c2.LastOutput;
            Log(x);

            return (sum1, (int)collectedDust);
        }

        private IEnumerable<long> FormatInput(string seq)
        {
            for (var i = 0; i < seq.Length; i++)
            {
                yield return seq[i];
            }
            yield return 10;
        }

        private static int AddDistToProgram(int dist, List<int> program, StringBuilder sb)
        {
            if (dist > 0)
            {
                program.Add(dist);
                sb.Append(dist);
                sb.Append(',');
                dist = 0;
            }
            return dist;
        }

        private static int CountIntersections(SparseBuffer<char> world)
        {
            var sum = FindIntersections(world).Aggregate(0, (s, coord) => s + coord.X * coord.Y);
            return sum;
        }

        private static IEnumerable<Coord> FindIntersections(SparseBuffer<char> world)
        {
            return world.Keys.Where(coord => world[coord] == '#' && coord.GenAdjacent4().All(adj => world[adj] == '#'));
        }

        private (Coord pos, Coord dir) ParseOutputAsWorld(IntCodeComputer c, SparseBuffer<char> world)
        {
            var pos = Coord.Origin;

            Coord botPos = null;
            Coord botDir = null;
            while (c.OutputQ.Any())
            {
                var ascii = c.OutputQ.Dequeue();
                var ch = (char)ascii;
                if (Debug)
                    Log($"{ascii}, [{ch}]");
                if (ascii == 10) // newline
                    pos = Coord.FromXY(0, pos.Y + 1);
                else
                {
                    if (Coord.trans2Coord.Keys.Any(k => k == ch))
                    {
                        // found the bot, its on a scaffold
                        botPos = pos;
                        botDir = Coord.trans2Coord[ch];
                        ch = '#';
                    }

                    // write to world, move to next square
                    world[pos] = ch;
                    pos = pos.Move(Coord.E);
                }
            }

            return (botPos, botDir);
        }
    }
}