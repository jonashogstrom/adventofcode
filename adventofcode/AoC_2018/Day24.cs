using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AdventofCode.AoC_2018
{
    internal class Day24 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 0;

            Part1TestSolution = 5216;
            Part2TestSolution = 51;
            Part1Solution = 10538;
            Part2Solution = 9174;
        }

        protected override void DoRun(string[] input)
        {
            var groups = new List<Group>();
            ParseInput(input, groups);

            PlayToEnd(groups);

            Part1 = groups.Where(g => g.Units > 0).Sum(g => g.Units);



            var boost = UseTestData ? 1569 : 0;
            var winner = Side.Infection;

            while (winner == Side.Infection)
            {
                groups = new List<Group>();
                ParseInput(input, groups);
                boost += 1;
                Log($"=============== Playing with Boost: {boost}");
                foreach (var g in groups.Where(g => g.Side == Side.ImmuneSystem))
                    g.Boost = boost;
                if (PlayToEnd(groups))
                {
                    winner = groups.First(g => g.Units > 0).Side;
                    var unitsLeft = groups.Where(g => g.Units > 0).Sum(g => g.Units);
                    Log($"Boost: {boost}, winner is {winner}, unitsLeft {unitsLeft}");
                    if (winner == Side.ImmuneSystem)
                        Part2 = unitsLeft;
                }
            }
        }

        private bool PlayToEnd(List<Group> groups)
        {
            var round = 1;
            var units = int.MaxValue;
            while (groups.Where(g => g.Units > 0).Select(g => g.Side).Distinct().Count() > 1)
            {
                Log(() => $"=== ROUND {round}", 2);
                foreach (var g in groups.Where(x => x.Units > 0).OrderBy(x => x.Side))
                    Log(() => $"{g.Side} {g.Index} contains {g.Units} units", 2);

                Log(() => $"=== SELECT {round}", 2);
                Select(groups);
                Log(() => $"=== FIGHT {round}", 2);
                Fight(groups);
                groups = groups.Where(g => g.Units > 0).ToList();

                Log(() => $"=== AFTER ROUND {round}", 2);
                foreach (var g in groups.Where(x => x.Units > 0).OrderBy(x => x.Side))
                    Log(() => $"{g.Side} {g.Index} contains {g.Units} units", 2);

                var newUnits = groups.Where(g => g.Units > 0).Sum(g => g.Units);
                if (units == newUnits)
                {
                    Log("StaleMate");
                    return false;
                }

                units = newUnits;

                round++;
            }

            return true;
        }

        private void Fight(List<Group> groups)
        {
            foreach (var g in groups.Where(g => g.Target != null).OrderByDescending(g => g.Initiative))
            {
                var damage = CalcDamage(g, g.Target);
                var unitsKilled = damage / g.Target.HP;
                Log($"{g.Side} group {g.Index} attacks defending group {g.Target.Index}, killing {unitsKilled} units", 2);
                g.Target.Units -= unitsKilled;
            }
        }

        private void Select(List<Group> groups)
        {
            // reset targets
            groups.ForEach(g => g.TargetedBy = null);
            groups.ForEach(g => g.Target = null);

            foreach (var g in groups.Where(g => g.Units > 0).OrderByDescending(g => g.EffectivePower).ThenByDescending(g => g.Initiative))
            {
                var enemies = groups.Where(e => e.Units > 0).Where(e => e.Side != g.Side).Where(e => e.TargetedBy == null).ToList();
                var orderedEnemies = enemies.OrderByDescending(e => CalcDamage(g, e))
                        .ThenByDescending(e => e.EffectivePower)
                        .ThenByDescending(e => e.Initiative).ToList();

                foreach (var e in orderedEnemies)
                {
                    Log($"{g.Side} group {g.Index} would deal defending group {e.Index} {CalcDamage(g, e)} damage", 2);
                }

                var enemy = orderedEnemies.FirstOrDefault();

                if (enemy != null && CalcDamage(g, enemy) > 0)
                {
                    g.Target = enemy;
                    enemy.TargetedBy = g;
                }
            }
        }

        private int CalcDamage(Group attacker, Group target)
        {
            if (target.ImmuneTo.Contains(attacker.Weapon))
                return 0;
            if (target.WeakTo.Contains(attacker.Weapon))
                return attacker.EffectivePower * 2;
            else
            {
                return attacker.EffectivePower;
            }

        }

        private void ParseInput(string[] input, List<Group> groups)
        {
            var side = Side.ImmuneSystem;
            foreach (var s in input)
            {
                if (s.Trim() == "Infection:")
                    side = Side.Infection;
                else if (s.Trim() == "Immune System:")
                    side = Side.ImmuneSystem;
                else if (!string.IsNullOrEmpty(s))
                {
                    groups.Add(ParseGroup(s, side, groups.Count(g => g.Side == side) + 1));
                }
            }
        }

        private Group ParseGroup(string s, Side side, int groupIndex)
        {
            var g = new Group();
            var num = GetIntArr(s);
            g.Units = num[0];
            g.HP = num[1];
            g.Index = groupIndex;
            g.OrgDamage = num[2];
            g.Initiative = num[3];
            g.Side = side;
            var words = s.Split(' ');
            var x = s.Split('(', ')');
            if (x.Length > 1)
            {
                var parts = x[1].Split(';');
                foreach (var p in parts)
                {
                    var wordsInParen = p.Replace(",", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 2; i < wordsInParen.Length; i++)
                    {
                        var ww = wordsInParen[i];
                        if (Weapon.TryParse(ww, out Weapon weakness))
                        {
                            if (wordsInParen[0] == "immune")
                                g.ImmuneTo.Add(weakness);
                            else
                                g.WeakTo.Add(weakness);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
            }

            var www = words[words.Length - 5];
            if (Weapon.TryParse(www, out Weapon w))
                g.Weapon = w;
            return g;

        }

        internal enum Weapon
        {
            INVALID,
            cold,
            slashing,
            radiation,
            bludgeoning,
            fire
        };
        internal enum Side
        {
            ImmuneSystem,
            Infection
        }

        [DebuggerDisplay("{Side} {Index} {Units} units, {HP} HP")]
        internal class Group
        {
            public int HP;

            public Group()
            {
                ImmuneTo = new List<Weapon>();
                WeakTo = new List<Weapon>();
            }

            public int Units { get; set; }
            public int OrgDamage { get; set; }
            public int Damage => OrgDamage + Boost;
            public int Initiative { get; set; }
            public List<Weapon> WeakTo { get; set; }
            public Weapon Weapon { get; set; }
            public Side Side { get; set; }
            public int EffectivePower => Units * Damage;
            public Group TargetedBy { get; set; }
            public Group Target { get; set; }
            public List<Weapon> ImmuneTo { get; set; }
            public int Index { get; set; }
            public int Boost { get; set; }
        }
    }


}