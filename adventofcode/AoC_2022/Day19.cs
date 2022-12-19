using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
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
        [TestCase(33, null, "Day19_test.txt")]
        [TestCase(9 * 1, 56*1, "Day19_test1.txt")]
        [TestCase(12 * 2, 62*2, "Day19_test2.txt")]
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
            part1 = blueprints.Sum(b => b.QualityLevel);
            // solve part 1 here

            LogAndReset("*1", sw);
            part2 = blueprints.Take(3).Sum(b => b.QualityLevel2);

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

        private int FindBest2(int maxMinutes)
        {
            _parent.Log(() => "===================");
            var builds = new Stack<(Material, int minute)>();
            var inventory = EmptyDic();
            var robotCount = EmptyDic();
            robotCount[Material.ore] = 1;

            foreach (var x in Materials)
            {
                FindBest3(builds, 1, maxMinutes, robotCount, new List<Material>(), inventory, x, new List<string>());
            }

            foreach (var s in _bestLog)
                _parent.Log(() => s);

            return _best;

        }

        private void FindBest3(Stack<(Material, int minute)> builds, int minute, int maxMinutes,
            Dictionary<Material, int> robotCount, List<Material> pendingRobots, Dictionary<Material, int> inventory, Material nextRobot, List<string> log)
        {
            if (minute == maxMinutes - 1)
            {
                // last minute-1 - lets assume we build one more bot;
                var max = inventory[Material.geode] + robotCount[Material.geode] + robotCount[Material.geode] + 1;
                if (max < _best)
                    return;
            }

            if (builds.Count > 20)
                return;
            if (robotCount[Material.ore] > 4)
                return;
            if (minute > maxMinutes)
            {
                if (inventory[Material.geode] > _best || (inventory[Material.geode] == _best && builds.Count < _bestBotCount))
                {
                    _best = inventory[Material.geode];
                    _bestLog = log;
                    _bestBotCount = builds.Count;
                    _parent.Log(() => $"Found a solution for blueprint {Id} that gives {inventory[Material.geode]} geodes with {builds.Count} robots");
                    foreach (var b in builds.Reverse())
                    {
                        _parent.Log(() => $"Minute {b.minute}: Build {b.Item1} robot");
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
                _parent.Log(() => s);

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
                    _parent.Log(() => $"Found a solution that gives {inventory[Material.geode]} geodes");
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

    public enum Material
    {
        geode = 8,
        obsidian = 4,
        clay = 2,
        ore = 1
    }
}