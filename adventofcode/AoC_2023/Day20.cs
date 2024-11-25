using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using AdventofCode.Utils;
using System.Linq;
using Accord;


namespace AdventofCode.AoC_2023
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day20 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(32000000, null, "Day20_test.txt")]
        [TestCase(11687500, null, "Day20_test2.txt")]
        [TestCase(1020211150, 238815727638557, "Day20.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;
            var sw = Stopwatch.StartNew();

            var modules = new Dictionary<string, Module>();
            foreach (var s in source)
            {
                var m = ParseModule(s, modules);
                modules.Add(m.Name, m);
            }
            foreach (var m in modules.Values)
                foreach (var m2Name in m.Targets)
                {
                    if (modules.TryGetValue(m2Name, out var m2))
                        if (m2.ModuleType == ModuleType.Conjunction)
                            m2.RegisterInput(m);
                }

            LogAndReset("Parse", sw);

            var highCount = 0L;
            var lowCount = 0L;
            var nodes = new HashSet<string> { "gl", "hr", "nr", "gk" };
            var cycles = new Dictionary<string, long>();
            var round = 1;
            while (part2 == 0)
            {
                Log(() => $"=========== Round {round} ================");
                var q = new Queue<(string from, string to, bool value)>();
                q.Enqueue(("button", "broadcaster", false));

                while (q.Any())
                {
                    var x = q.Dequeue();
                    var value = x.value ? "high" : "low";

                    if (nodes.Contains(x.from))
                    {
                        if (!x.value)
                            if (!cycles.ContainsKey(x.from))
                            {
                                cycles[x.from] = round;
                                if (cycles.Count == nodes.Count)
                                {
                                    part2 = cycles.Values.Multiply();

                                    break;
                                }
                            }
                    }
                    // if (x.from == "gk" && !x.value)
                    //     Log(() => $"{x.from}: {round} sending {value} to {x.to}", -1);

                    Log(() => $"{x.from} -{value}-> {x.to}");

                    if (modules.TryGetValue(x.to, out var m))
                    {
                        m.Receive(x.from, x.value, q);
                    }

                    if (x.value)
                        highCount++;
                    else
                        lowCount++;
                }

                if (round == 1000)
                {
                    part1 = lowCount * highCount;
                }


                round++;
            }

            LogAndReset("*1", sw);

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }

        private Module ParseModule(string s, Dictionary<string, Module> modules)
        {
            var parts = s.Split(" -> ");
            var targets = parts[1].Split(", ");

            switch (parts[0][0])
            {
                case '%': return new Module(parts[0][1..], targets, ModuleType.FlipFlop, modules);
                case '&': return new Module(parts[0][1..], targets, ModuleType.Conjunction, modules);
                default: return new Module(parts[0], targets, ModuleType.Broadcaster, modules);
            }
        }

    }

    internal enum Pulse
    {
        Low
    }

    internal enum ModuleType
    {
        FlipFlop,
        Broadcaster,
        Conjunction
    }

    internal class Module
    {
        private readonly Dictionary<string, Module> _modules;
        public string Name { get; }
        public string[] Targets { get; }
        public ModuleType ModuleType { get; }
        private Dictionary<string, bool> _memory = new();

        public Module(string name, string[] targets, ModuleType moduleType, Dictionary<string, Module> modules)
        {
            _modules = modules;
            Name = name;
            Targets = targets;
            ModuleType = moduleType;
            State = false;
            PulsesSent = new DicWithDefault<bool, int>();
        }

        public DicWithDefault<bool, int> PulsesSent { get; }

        public bool State { get; set; }

        public void Receive(string source, bool pulse, Queue<(string from, string to, bool value)> q)
        {
            switch (ModuleType)
            {
                case ModuleType.FlipFlop:
                    if (pulse == false)
                    {
                        State = !State;
                        SendToReceivers(State, q);
                    }
                    break;
                case ModuleType.Conjunction:
                    _memory[source] = pulse;
                    var allHigh = _memory.Values.All(v => v);
                    SendToReceivers(!allHigh, q);
                    break;
                case ModuleType.Broadcaster:
                    SendToReceivers(pulse, q);
                    break;

            }
        }

        private void SendToReceivers(bool state, Queue<(string from, string to, bool value)> q)
        {
            foreach (var t in Targets)
                q.Enqueue((Name, t, state));
        }

        public void RegisterInput(Module module)
        {
            _memory[module.Name] = false;
        }
    }
}