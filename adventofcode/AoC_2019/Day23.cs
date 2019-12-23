using System;
using System.Linq;
using System.Net.Sockets;
using NUnit.Framework;

namespace AdventofCode.AoC_2019
{
    using Part1Type = Int64;
    using Part2Type = Int32;

    [TestFixture]
    class Day23 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        // not 16668
        [Test]
        [TestCase(23815, 16666, "Day23.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = Compute(source);
            DoAsserts(res, exp1, exp2);
        }

        private (Part1Type Part1, Part2Type Part2) Compute(string[] source)
        {
            var computers = new IntCodeComputer[50];
            for (int i = 0; i < computers.Length; i++)
            {
                computers[i] = new IntCodeComputer(source[0]);
                computers[i].AddInput(i);
                computers[i].Execute();
            }

            var ttq = false;
            long lastNaty = -1;
            long lastNatx = -1;
            long naty = -1;
            long natx = -1;
            var firstNatY = -1;
            var part2 = -1;
            while (!ttq)
            {
                var allIdle = true;
                foreach (var c in computers)
                {
                    if (!c.InputQ.Any())
                        c.AddInput(-1);
                    c.Execute();
                    while (c.OutputQ.Any())
                    {
                        var address = c.OutputQ.Dequeue();
                        var x = c.OutputQ.Dequeue();
                        var y = c.OutputQ.Dequeue();
                        if (address == 255)
                        {
                            if (firstNatY == -1)
                                firstNatY = (int)y;
                            Log($"Message sent to NAT: {x}, {y}");
                            naty = y;
                            natx = x;
                     
                        }
                        else
                        {
                            Log($"Message sent to computer {address}: {x}, {y}");
                            allIdle = false;
                            computers[address].AddInput(x);
                            computers[address].AddInput(y);
                        }
                    }
                }

                if (allIdle)
                {
                    Log("Nat: x="+natx + " y=" + naty);
                    computers[0].AddInput(natx);
                    computers[0].AddInput(naty);
                    if (naty == lastNaty)
                        return (firstNatY, (int)naty);
                    lastNaty = naty;
                }

            }
          
            return (-1, 0);
        }
    }
}