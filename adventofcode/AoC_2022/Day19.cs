using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
//using Accord.Collections;
using AdventofCode.Utils;
using NUnit.Framework;

// first guess *1: 1656 - too low
// second guess *2: 1693 - too HIGH! -- must have been an error...

// Brute: 1688 ToolBar high

// 1672 - fel!



namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day19 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(33, 56 * 62, "Day19_test.txt")]
        [TestCase(9 * 1, 56 * 1, "Day19_test1.txt")]
        [TestCase(12 * 2, 62, "Day19_test2.txt")]
        [TestCase(1681, 5394, "Day19.txt")]
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

            var blueprints = new List<Blueprint>();
            var sum1 = 0;
            foreach (var s in source)
            {
                blueprints.Add(Blueprint.Parse(s, this));
            }

            LogAndReset("Parse", sw);
            Parallel.ForEach(blueprints, b =>
            {
                var res = b.QualityLevel;
                sum1 += res;
            });
            part1 = sum1;

            Log($"Part1 = {part1}");
            LogAndReset("*1", sw);
            var res2 = 1L;
            // foreach (var b in blueprints.Take(3))
            Parallel.ForEach(blueprints.Take(3), b =>
            {
                var res = b.FindBestPrioQueue(32);
                res2 *= res;
                Log($"Blueprint {b.Id} = {res}", -1);
            });
            part2 = res2;

            // solve part 2 here

            LogAndReset("*2", sw);

            return (part1, part2);
        }
    }

    internal class Blueprint
    {
        private readonly Day19 _parent;
        private Dictionary<Material, Dictionary<Material, int>> RobotCosts = new Dictionary<Material, Dictionary<Material, int>>();
        private int _best = Int32.MinValue;
        private List<string> _bestLog;
        private int _bestBotCount = int.MaxValue;

        public Blueprint(int id, Day19 parent)
        {
            _parent = parent;
            Id = id;
        }

        public int QualityLevel => FindBestPrioQueue(24) * Id;

        public int FindBestPrioQueue(int maxMinutes)
        {
            var initialState = new BotState(0, RobotCosts, null);
            initialState.BotCount[Material.ore] = 1;
            var q = new PriorityQueue<BotState,BotState>(10, new BotComparer());
            Enqueue(q, initialState);
            var best = 0;
            var stateCounter = 0L;
            while (q.Count > 0)
            {
                stateCounter++;
                var state = q.Dequeue();
                if (state.MaxPossible(maxMinutes) < best)
                {
                    continue;
                }
                if (state.Minute == maxMinutes)
                {
                    if (state.Inventory[Material.geode] > best)
                    {
                        best = state.Inventory[Material.geode];
                        _parent.Log(() => $"Found new result for {Id}: {best}", -1);
                    }
                }
                else if (state.CanBuild(Material.geode))
                {
                    Enqueue(q, state.CreateNewState(Material.geode));
                    if (state.CanBuild(Material.obsidian))
                        Enqueue(q, state.CreateNewState(Material.obsidian));
                }
                else if (state.CanBuild(Material.obsidian))
                {
                    Enqueue(q, state.CreateNewState(Material.obsidian));
                    if (state.CanBuild(Material.clay))
                        Enqueue(q, state.CreateNewState(Material.clay));
                    // if (state.CanBuild(Material.ore))
                    //     Enqueue(q, state.CreateNewState(Material.ore));
                }
                else
                {
                    if (state.CanBuild(Material.clay))
                        Enqueue(q, state.CreateNewState(Material.clay));
                    if (state.CanBuild(Material.ore))
                        Enqueue(q, state.CreateNewState(Material.ore));

                    Enqueue(q, state.Clone().AddBotYield());
                }
            }
            _parent.Log(() => $"Completed blueprint {Id} in {stateCounter} states, result: {best}");
            return best;
        }

        private void Enqueue(PriorityQueue<BotState, BotState> queue, BotState state)
        {
            queue.Enqueue(state, state);
        }


        public static Blueprint Parse(string s, Day19 parent)
        {
            var parts = s.Split(new[] { ' ', ':', }, StringSplitOptions.RemoveEmptyEntries).Where(x => int.TryParse(x, out var z)).Select(int.Parse).ToArray();
            var result = new Blueprint(parts[0], parent);
            result.RobotCosts[Material.ore] = new Dictionary<Material, int>() { { Material.ore, parts[1] } };
            result.RobotCosts[Material.clay] = new Dictionary<Material, int>() { { Material.ore, parts[2] } };
            result.RobotCosts[Material.obsidian] = new Dictionary<Material, int>()
            {
                { Material.ore, parts[3] },
                { Material.clay, parts[4] }
            };
            result.RobotCosts[Material.geode] = new Dictionary<Material, int>()
            {
                {Material.ore, parts[5] },
                { Material.obsidian, parts[6] }
            };
            ;
            return result;
        }

        public int Id { get; }
    }

    internal class BotComparer : IComparer<BotState>
    {
        public int Compare(BotState x, BotState y)
        {
            return 0;
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;


            var res = x.Minute.CompareTo(y.Minute);
            if (res == 0)
                CompareMaterial(x, y, Material.geode);
            if (res == 0)
                CompareMaterial(x, y, Material.obsidian);
            if (res == 0)
                CompareMaterial(x, y, Material.clay);

            return res;
        }

        private static void CompareMaterial(BotState x, BotState y, Material material)
        {
            int res;
            res = CompareInventory(x, y, material);
            if (res == 0)
                res = CompareBotCount(x, y, material);
        }

        private static int CompareInventory(BotState x, BotState y, Material material)
        {
            return -x.BotCount[material].CompareTo(y.BotCount[material]);
        }
        private static int CompareBotCount(BotState x, BotState y, Material material)
        {
            return -x.Inventory[material].CompareTo(y.Inventory[material]);
        }
    }

    internal class BotState
    {
        private readonly Dictionary<Material, Dictionary<Material, int>> _robotCosts;
        public int Minute { get; }
        public DicWithDefault<Material, int> BotCount { get; } = new();
        public DicWithDefault<Material, int> Inventory { get; } = new();

        public BotState Parent { get; }
        public double Prio
        {
            get
            {
                var x = 3 << 4;
                x = 7 >> 4;
                long sum = Minute < 15 ? 1 : 0;
                sum = sum << 8 + Inventory[Material.geode];
                sum <<= 8 + BotCount[Material.geode];
                sum <<= 8 + Inventory[Material.obsidian];
                sum <<= 8 + BotCount[Material.obsidian];
                sum <<= 8 + Inventory[Material.clay];
                sum <<= 8 + BotCount[Material.clay];
                sum <<= 8 + Minute;

                return -sum;
            }
        }

        public long MaxPossible(int maxMinutes)
        {
            var clayCount = Inventory[Material.clay];
            var clayBotCount = BotCount[Material.clay];

            var obsBotCost = _robotCosts[Material.obsidian][Material.clay];
            var obsCount = Inventory[Material.obsidian];
            var obsBotCount = BotCount[Material.obsidian];

            var geodeBotCost = _robotCosts[Material.geode][Material.obsidian];
            var geodeCount = Inventory[Material.geode];
            var geoBotCount = BotCount[Material.geode];
            for (int m = Minute + 1; m <= maxMinutes; m++)
            {
                var oldobsCount = obsCount;
                var oldclayCount = clayCount;
                geodeCount += geoBotCount;
                obsCount += obsBotCount;
                clayCount += clayBotCount;
                if (oldobsCount >= geodeBotCost)
                {
                    geoBotCount++;
                    obsCount -= geodeBotCost;
                }
                else if (oldclayCount >= obsBotCost)
                {
                    obsBotCount++;
                    clayCount -= obsBotCost;
                }
                else
                {
                    clayBotCount++;
                }
            }

            return geodeCount;
        }

        public BotState(int minute, Dictionary<Material, Dictionary<Material, int>> robotCosts, BotState parent = null)
        {
            _robotCosts = robotCosts;
            Minute = minute;
            Parent = parent;
        }

        public bool CanBuild(Material mat)
        {
            foreach (var m in _robotCosts[mat])
            {
                if (Inventory[m.Key] < m.Value)
                    return false;
            }

            return true;
        }

        public BotState CreateNewState(Material botType)
        {
            var newState = Clone();
            foreach (var m in _robotCosts[botType])
            {
                newState.Inventory[m.Key] -= m.Value;
            }

            newState.AddBotYield();
            newState.BotCount[botType]++;
            return newState;
        }

        private void CopyTo(DicWithDefault<Material, int> source, DicWithDefault<Material, int> target)
        {
            foreach (var m in source.Keys)
                target[m] = source[m];
        }

        public BotState Clone()
        {
            var res = new BotState(Minute + 1, _robotCosts, this);
            CopyTo(BotCount, res.BotCount);
            CopyTo(Inventory, res.Inventory);
            return res;
        }

        public BotState AddBotYield()
        {
            foreach (var m in BotCount.Keys)
                Inventory[m] += BotCount[m];
            return this;
        }
    }

    public enum Material
    {
        geode = 8,
        obsidian = 4,
        clay = 2,
        ore = 1
    }
}