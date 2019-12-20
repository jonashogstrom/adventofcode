using System;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int32;
    using Part2Type = Int32;

    [TestFixture]
    class Day19 : TestBaseClass<Part1Type, Part2Type>
    {
        private int _checkCounter;
        private IntCodeComputer _comp;
        public bool Debug { get; set; }

        [Test]
        [TestCase(186, 9231141, "Day19.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var program = IntCodeComputer.ParseProgram(source[0]);
            var res = Compute(program);
            DoAsserts(res, exp1, exp2);

            // nope: 980120, 920113, 3970554
        }

        private (Part1Type Part1, Part2Type Part2) Compute(long[]source)
        {

            var world = new SparseBuffer<char>(' ');
            var affected = CalcPart1(source, world, 50);

            var part2Size = 100;
            var p = Coord.FromXY(part2Size, 0);
            while (CheckCoord(source, p, world) != '#') 
                p = p.Move(Coord.S);

            while (CheckCoord(source, Coord.FromXY(p.X - (part2Size-1), p.Y + (part2Size-1)),world) == '.')
            {
                while (CheckCoord(source, p.Move(Coord.E), world) == '#')
                    p = p.Move(Coord.E);
                p = p.Move(Coord.E);

                while (CheckCoord(source, p.Move(Coord.S), world) == '.')
                    p = p.Move(Coord.S);
                p = p.Move(Coord.S);
            }

            var Part2 = p.Move(Coord.W, part2Size-1);


            for (int x = Part2.X; x < Part2.X + (part2Size); x++)
                for (int y = Part2.Y; y < Part2.Y + (part2Size); y++)
                {
                    var c = Coord.FromXY(x, y);
                    if (world[c] == ' ')
                        world[c] = 'O';
                }
            world[Part2] = 'O';

            PrintWorld(world);
            Log($"Total checks: {_checkCounter}");

            return (affected, Part2.X * 10000 + Part2.Y);
        }

        private int CalcPart1(long[] source, SparseBuffer<char> world, int size)
        {
            int affected = 0;
            for (int x = 0; x < size; x++)
            {
                var foundbeam = false;
                for (int y = 0; y < size; y++)
                {
                    var c = Coord.FromXY(x, y);
                    if (CheckCoord(source, c, world) == '#')
                    {
                        affected++;
                        foundbeam = true;
                    }
                    else if (foundbeam)
                        break;
                }

                PrintWorld(world);
            }

            PrintWorld(world);
            return affected;
        }

        private char CheckCoord(long[] source, Coord c, SparseBuffer<char> world)
        {
            var res = CheckCoord(source, c);

            world[c] = res == 1 ? '#' : '.';

            return world[c];
        }
        private long CheckCoord(long[] source, Coord c)
        {
            _checkCounter++;
            if (_comp == null)
                _comp = new IntCodeComputer(source) {SupportRestore = true};
            else
            {
                _comp.Reset();
            }
            _comp.AddInput(c.X);
            _comp.AddInput(c.Y);
            _comp.Execute();
            var res = _comp.OutputQ.Dequeue();
            return res;
        }

        private void PrintWorld(SparseBuffer<char> world)
        {
            Log(world.ToString(c => c.ToString()));
        }
    }
}