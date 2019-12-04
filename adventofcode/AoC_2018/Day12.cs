using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AdventofCode.AoC_2018
{
    internal class Day12 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = false;
            Part1TestSolution = 325;
            Part2TestSolution = null;
            Part1Solution = 4818;
            Part2Solution = 5100000001377;
        }

        protected override void DoRun(string[] input)
        {
            var initial = ParseState(input[0].Split(' ')[2]);
            // the rules array indicates which rules spawns a plant, each rule is represented by its bit-value
            var rules = new bool[32];
            for (int i = 2; i < input.Length; i++)
            {
                var parts = input[i].Split(new[] { " => " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts[1] == "#")
                    rules[ParseRule(parts[0])] = true;
            }
            // the states-list is only used for debugging
            var states = new List<HashSet<int>>();
            if (UseTestData)
                states.Add(initial);

            var state = initial;
            var gen = 0;
            var diffs = new int[10];
            var stable = false;
            var sw = new Stopwatch();
            sw.Start();
            var lastStateSum = 0;

            while (!stable)
            {
                state = Mutate(state, rules);
                gen++;
                var stateSum = state.Sum();

                // keep the result for part 1 after 20 iterations
                if (gen == 20)
                    Part1 = stateSum;

                if (UseTestData)
                    states.Add(state);

                // keep track of the past 10 diffs to check when they stabilize
                diffs[gen % diffs.Length] = stateSum - lastStateSum;
                if (diffs.Distinct().Count() == 1)
                    stable = true;

                lastStateSum = stateSum;
            }

            if (UseTestData)
                PrintStates(states);

            Part2 = state.Sum() + (50000000000 - gen) * diffs.Last();
        }

        private void PrintStates(List<HashSet<int>> states)
        {
            var min = states.Min(s => s.Min());
            var max = states.Max(s => s.Max());
            for (int gen = 0; gen < states.Count; gen++)
            {
                var str = "";
                for (int i = min; i < max + 3; i++)
                {
                    str += states[gen].Contains(i) ? "#" : ".";
                }
                var diff = gen == 0 ? 0 : (states[gen].Sum() - states[gen - 1].Sum());
                Log(str + " =>" + gen + "=" + states[gen].Sum() + "(" + diff + ")");
            }
        }

        private HashSet<int> Mutate(HashSet<int> state, bool[] rules)
        {
            var newState = new HashSet<int>();
            var runningbitValue = 0;
            // Iterate over all positions where there are plants.
            // it is actually a linear operation to get the Min and Max this way too, that could be optimized to
            // keep track of the current min and max value.
            for (int i = state.Min() - 2; i <= state.Max() + 2; i++)
            {
                // for each plant position, shift the bit value from the last position one step up
                // and add a one if the position two steps to the right has a plant, remove any bits above 32 (2^5)
                runningbitValue = (runningbitValue << 1 | (state.Contains(i + 2) ? 1 : 0)) % 32;
                // if there is a rule with the current bit value, then spawn a plant in this place in the next generation
                if (rules[runningbitValue])
                    newState.Add(i);
            }

            return newState;
        }

        private int ParseRule(string s)
        {
            // calculate the bit value for each rule that generates a new plant
            var ruleValue = 0;
            foreach (var t in s)
                ruleValue = (ruleValue << 1) + (t == '#' ? 1 : 0);

            return ruleValue;
        }

        private HashSet<int> ParseState(string s)
        {
            // Add a plant for each character in the initial setup that is a #
            var res = new HashSet<int>();
            for (var i = 0; i < s.Length; i++)
                if (s[i] == '#')
                    res.Add(i);
            return res;
        }

    }
}