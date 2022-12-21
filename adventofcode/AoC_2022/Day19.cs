using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
//using Accord.Collections;
using AdventofCode.Utils;
using NUnit.Framework;

// first guess *1: 1656 - too low
// second guess *2: 1693 - too HIGH!

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
        [TestCase(33, 180, "Day19_test.txt")]
        [TestCase(9 * 1, 56 * 1, "Day19_test1.txt")]
        [TestCase(12 * 2, 62 * 2, "Day19_test2.txt")]
        [TestCase(1681, null, "Day19.txt")]
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
            foreach (var s in source)
            {
                blueprints.Add(Blueprint.Parse(s, this));
            }

            LogAndReset("Parse", sw);
            //part1 = blueprints.Sum(b => b.QualityLevel3);
            // solve part 1 here

            Log($"Part1 = {part1}");
            LogAndReset("*1", sw);
            var sum = 1;
            foreach (var b in blueprints.Take(3))
            //Parallel.ForEach(blueprints.Take(3), b =>
            {
                var res = b.QualityLevelStar2;
                sum *= res;
                Log($"Blueprint {b.Id} = {res}", -1);
            }
            //);
            part2 = sum;

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

        public int QualityLevel => FindBest2(24) * Id;
        public int QualityLevel2 => FindBest2(32) * Id;
        public int QualityLevel3 => FindBestPrioQueue(24) * Id;
        public int QualityLevelStar2 => FindBestPrioQueue(32) * Id;

        private int FindBestPrioQueue(int maxMinutes)
        {
            var initialState = new BotState(0, RobotCosts, null);
            initialState.BotCount[Material.ore] = 1;
            var q = new PriorityQueue<BotState, double>(10);
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

        private void Enqueue(PriorityQueue<BotState, double> queue, BotState state)
        {
            queue.Enqueue(state, state.Prio);
        }

        private int FindBest2(int maxMinutes)
        {
            _parent.Log(() => "===================", 1);
            var builds = new Stack<(Material, int minute)>();
            var inventory = EmptyDic();
            var robotCount = EmptyDic();
            robotCount[Material.ore] = 1;

            foreach (var x in Materials)
            {
                FindBest3(builds, 1, maxMinutes, robotCount, new List<Material>(), inventory, x, new List<string>());
            }

            foreach (var s in _bestLog)
                _parent.Log(() => s, -1);

            return _best;

        }

        private void FindBest3(Stack<(Material, int minute)> builds, int minute, int maxMinutes,
            Dictionary<Material, int> robotCount, List<Material> pendingRobots, Dictionary<Material, int> inventory, Material nextRobot, List<string> log)
        {
            // if (_best > 0 && minute > maxMinutes / 2)
            // {
            //     var clayCount = inventory[Material.clay];
            //     var clayBotCount = robotCount[Material.clay];
            //
            //     var obsBotCost = RobotCosts[Material.obsidian][Material.clay];
            //     var obsCount = inventory[Material.obsidian];
            //     var obsBotCount = robotCount[Material.obsidian];
            //
            //     var geodeBotCost = RobotCosts[Material.geode][Material.obsidian];
            //     var geodeCount = inventory[Material.geode];
            //     var geoBotCount = robotCount[Material.geode];
            //     for (int m = minute; m <= maxMinutes; m++)
            //     {
            //         var oldobsCount = obsCount;
            //         var oldclayCount = clayCount;
            //         geodeCount += geoBotCount;
            //         obsCount += obsBotCount;
            //         clayCount += clayBotCount;
            //         if (oldobsCount >= geodeBotCost)
            //         {
            //             geoBotCount++;
            //             obsCount -= geodeBotCost;
            //         }
            //         else if (oldclayCount >= obsBotCost)
            //         {
            //             obsBotCount++;
            //             clayCount -= obsBotCost;
            //         }
            //         else
            //         {
            //             clayBotCount++;
            //         }
            //     }
            //
            //     if (geodeCount < _best)
            //         return;
            // }

            // if (builds.Count > 20)
            //     return;
            if (robotCount[Material.ore] > 5)
                return;

            if (minute > maxMinutes)
            {
                if (inventory[Material.geode] > _best || (inventory[Material.geode] == _best && builds.Count < _bestBotCount))
                {
                    _best = inventory[Material.geode];
                    _bestLog = log;
                    _bestBotCount = builds.Count;
                    _parent.Log(() => $"Found a solution for blueprint {Id} that gives {inventory[Material.geode]} geodes with {builds.Count} robots", -1);
                    foreach (var b in builds.Reverse())
                    {
                        _parent.Log(() => $"Minute {b.minute}: Build {b.Item1} robot", -1);
                    }
                }

                return;
            }

            if (CanBuild(nextRobot, inventory) && pendingRobots.Count == 0)
            {
                var newInventory = Clone(inventory);
                ReduceInventoryWithBuild(nextRobot, newInventory);
                builds.Push((nextRobot, minute));
                var localPending = new List<Material>(pendingRobots);
                localPending.Add(nextRobot);
                var newLog = new List<string>(log);
                //              newLog.Add($"build {nextRobot}");
                foreach (var x in Materials)
                {
                    FindBest3(builds, minute, maxMinutes, robotCount, localPending, newInventory, x, newLog);
                }
                builds.Pop();
            }
            else
            {
                var newInventory = Clone(inventory);
                var newLog = new List<string>(log);
                UpdateInventory(robotCount, newInventory, newLog);
                var newRobotCount = Clone(robotCount);
                foreach (var m in pendingRobots)
                    newRobotCount[m]++;
                //                newLog.Add($"== Minute {minute+1} ==");
                FindBest3(builds, minute + 1, maxMinutes, newRobotCount, new List<Material>(), newInventory, nextRobot, newLog);
            }
        }

        public int FindBest(int maxMinutes)
        {
            var robotCount = EmptyDic();
            var inventory = EmptyDic();
            robotCount[Material.ore] = 1;
            _parent.Log(() => "===================");
            FindBestRec(1, maxMinutes, robotCount, inventory, new List<string>());
            foreach (var s in _bestLog)
                _parent.Log(() => s, -1);

            return _best;

        }

        private static Dictionary<Material, int> EmptyDic()
        {
            var res = new Dictionary<Material, int>();
            foreach (var m in Materials)
                res[m] = 0;
            return res;
        }

        private static IEnumerable<Material> Materials
        {
            get
            {
                yield return Material.geode;
                yield return Material.obsidian;
                yield return Material.clay;
                yield return Material.ore;
            }
        }

        private void FindBestRec(int min, int maxMinutes, Dictionary<Material, int> robotCount, Dictionary<Material, int> inventory, List<string> log)
        {
            if (min > maxMinutes)
            {
                if (inventory[Material.geode] > _best)
                {
                    _parent.Log(() => $"Found a solution for id {Id} that gives {inventory[Material.geode]} geodes", -1);
                    _best = inventory[Material.geode];
                    _bestLog = log;
                }
                return;
            }

            var best = int.MinValue;
            log.Add("");
            log.Add($"== Minute {min} ==");

            // var localInventory = Clone(inventory);
            // var localRobotCount = Clone(robotCount);

            // var options = FindAllValidBuilds(inventory);
            // foreach (var op in options)
            // {
            //     BuildAndRecurse(min, maxMinutes, robotCount, inventory, log, op);
            // }

            if (CanBuild(Material.geode, inventory))
            {
                BuildAndRecurse(min, maxMinutes, robotCount, inventory, log, Material.geode);
                return;
            }
            if (CanBuild(Material.obsidian, inventory))
            {

                BuildAndRecurse(min, maxMinutes, robotCount, inventory, log, Material.obsidian);
                return;
            }

            if (CanBuild(Material.clay, inventory))
            {
                var localInventory = Clone(inventory);
                var localRobotCount = Clone(robotCount);
                var localLog = new List<string>(log);
                BuildAndRecurse(min, maxMinutes, localRobotCount, localInventory, localLog, Material.clay);
            }
            else if (CanBuild(Material.ore, inventory))
            {
                var localInventory = Clone(inventory);
                var localRobotCount = Clone(robotCount);
                var localLog = new List<string>(log);
                BuildAndRecurse(min, maxMinutes, localRobotCount, localInventory, localLog, Material.ore);
            }

            UpdateInventory(robotCount, inventory, log);
            FindBestRec(min + 1, maxMinutes, robotCount, inventory, log);


        }

        private List<List<Material>> FindAllValidBuilds(Dictionary<Material, int> inventory)
        {
            var res = new List<List<Material>>();
            var start = 0;
            if (CanBuild(Material.obsidian, inventory) || CanBuild(Material.geode, inventory))
                start = 1;
            for (int i = start; i < 15; i++)
            {
                var available = Clone(inventory);
                var builds = new List<Material>();
                var enough = true;
                foreach (var m in Materials)
                {
                    var materialAsByte = (byte)m;
                    if ((i & materialAsByte) == materialAsByte)
                    {
                        foreach (var x in RobotCosts[m])
                        {
                            if (available[x.Key] < x.Value)
                            {
                                enough = false;
                                break;
                            }
                            available[x.Key] -= x.Value;
                            if (available[x.Key] < 0)
                                enough = false;
                        }
                        if (!enough)
                            break;
                        builds.Add(m);
                    }
                }
                if (enough)
                    res.Add(builds);
            }

            return res;
        }

        private void BuildAndRecurse(int min, int maxMinutes, Dictionary<Material, int> robotCount, Dictionary<Material, int> inventory, List<string> log, Material robotType)
        {
            log.Add($"Spend xxx to start building a {robotType}-collecting robot.");
            ReduceInventoryWithBuild(robotType, inventory);
            UpdateInventory(robotCount, inventory, log);
            robotCount[robotType]++;
            FindBestRec(min + 1, maxMinutes, robotCount, inventory, log);
        }

        private void BuildAndRecurse(int min, int maxMinutes, Dictionary<Material, int> robotCount,
            Dictionary<Material, int> inventory, List<string> log, List<Material> robotTypesToBuild)
        {
            var tempLog = new List<string>(log);
            foreach (var robotType in robotTypesToBuild)
            {
                tempLog.Add($"Spend xxx to start building a {robotType}-collecting robot.");
                ReduceInventoryWithBuild(robotType, inventory);
            }
            var localInventory = Clone(inventory);
            UpdateInventory(robotCount, localInventory, tempLog);

            var localRobotCount = Clone(robotCount);
            foreach (var robotType in robotTypesToBuild)
            {
                localRobotCount[robotType]++;
            }

            FindBestRec(min + 1, maxMinutes, localRobotCount, localInventory, tempLog);
        }

        private void UpdateInventory(Dictionary<Material, int> robotCount, Dictionary<Material, int> inventory, List<string> log)
        {
            foreach (var mat in Materials)
            {
                if (robotCount[mat] > 0)
                {
                    inventory[mat] += robotCount[mat];
                    //                    log.Add($"{robotCount[mat]} {mat}-collecting robot collects {robotCount[mat]} {mat}; you now have {inventory[mat]} {mat}");
                }
            }
        }

        private bool CanBuild(Material mat, Dictionary<Material, int> inventory)
        {
            var cost = RobotCosts[mat];
            foreach (var m in cost.Keys)
            {
                if (inventory[m] < cost[m])
                    return false;
            }
            return true;
        }

        private void ReduceInventoryWithBuild(Material mat, Dictionary<Material, int> inventory)
        {
            foreach (var m in RobotCosts[mat].Keys)
            {
                inventory[m] -= RobotCosts[mat][m];
            }
        }

        private Dictionary<Material, int> Clone(Dictionary<Material, int> source)
        {
            var res = new Dictionary<Material, int>();
            foreach (var m in Materials)
                res[m] = source[m];
            return res;
        }

        public int OreRobots { get; set; }

        public static Blueprint Parse(string s, Day19 parent)
        {
            /*
  Blueprint 1:
  Each ore robot costs 4 ore.
  Each clay robot costs 2 ore.
  Each obsidian robot costs 3 ore and 14 clay.
  Each geode robot costs 2 ore and 7 obsidian.
             */
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

    internal class BotState
    {
        private readonly Dictionary<Material, Dictionary<Material, int>> _robotCosts;
        public int Minute { get; }
        public DicWithDefault<Material, int> BotCount { get; } = new DicWithDefault<Material, int>();
        public DicWithDefault<Material, int> Inventory { get; } = new DicWithDefault<Material, int>();

        public BotState Parent { get; }
        public double Prio
        {
            get
            {
                var sum = Minute < 15 ? 1 : 0;
                sum <<= 8 + Inventory[Material.geode];
                sum <<= 8 + BotCount[Material.geode];
                sum <<= 8 + Inventory[Material.obsidian];
                sum <<= 8 + BotCount[Material.obsidian];
                sum <<= 8 + Inventory[Material.clay];
                sum <<= 8 + BotCount[Material.clay];
                sum <<= 8 + Minute;

                return sum;
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


        // private static Dictionary<Material, int> EmptyDic()
        // {
        //     var res = new Dictionary<Material, int>();
        //     foreach (var m in Materials)
        //         res[m] = 0;
        //     return res;
        // }
        //
        // private static IEnumerable<Material> Materials
        // {
        //     get
        //     {
        //         yield return Material.geode;
        //         yield return Material.obsidian;
        //         yield return Material.clay;
        //         yield return Material.ore;
        //     }
        // }



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