using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventofCode.Utils;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day21 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(5, -1389192152, "Day21_test.txt")]
        [TestCase(2280, -1879313247, "Day21.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1 = 0;
            Part2Type part2 = 0;

            var sw = Stopwatch.StartNew();
            var foodList = new List<Food>();
            foreach (var s in source)
                foodList.Add(ParseFood(s));



            LogAndReset("Parse", sw);

            var allergeneToFoodDic = new DicWithDefaultFunc<string, List<Food>>(() => new List<Food>());
            var allAllergenes = new HashSet<string>();
            var allingredients = new HashSet<string>();

            foreach (var f in foodList)
            {
                foreach (var a in f.Allergenes)
                {
                    allergeneToFoodDic[a].Add(f);
                    allAllergenes.Add(a);
                }

                foreach (var i in f.Ingredients)
                {
                    allingredients.Add(i);
                }
            }


            var safeIngredients = new HashSet<string>(allingredients);
            foreach (var a in allAllergenes)
            {
                var allergenesIsInFood = allergeneToFoodDic[a];
                var possibleIngredients = allergenesIsInFood.First().Ingredients;
                foreach (var f in allergeneToFoodDic[a])
                {
                    possibleIngredients = possibleIngredients.Intersect(f.Ingredients).ToHashSet();
                }

                foreach (var i in possibleIngredients)
                {
                    safeIngredients.Remove(i);
                }
            }

            foreach (var i in safeIngredients)
            {
                foreach (var f in foodList)
                {
                    if (f.Ingredients.Contains(i))
                        part1++;
                }
            }

            LogAndReset("*1", sw);
            foreach (var i in safeIngredients)
                EliminateIngredient(i, foodList);

            // foreach (var f in foodList)
            // {
            //     var xx = string.Join(",", f.Ingredients);
            //     var yy = string.Join(",", f.Allergenes);
            //     Log($"{f.Ingredients.Count}=>{f.Allergenes.Count}  {xx} => {yy}");
            // }

            var identifiedIngredients = new List<(string i, string a)>();
            while (identifiedIngredients.Count < allAllergenes.Count)
            {
                foreach (var a in allAllergenes)
                {
                    var allergenesIsInFood = allergeneToFoodDic[a];
                    var possibleIngredients = allergenesIsInFood.First().Ingredients;
                    foreach (var f in allergeneToFoodDic[a])
                    {
                        possibleIngredients = possibleIngredients.Intersect(f.Ingredients).ToHashSet();
                    }
 
                    if (possibleIngredients.Count == 1)
                    {
                        var identifiedIngredient = possibleIngredients.First();
                        EliminateIngredient(identifiedIngredient, foodList);
                        EliminateAllergene(a, foodList);
                        identifiedIngredients.Add((identifiedIngredient, a));
//                        Log($"{a} => {identifiedIngredient}");
                    }
                }
            }

            foreach (var x in identifiedIngredients)
            {
                Log($"{x.a} => {x.i}");
            }

            var res = string.Join(",", identifiedIngredients.OrderBy(x=>x.a).Select(x=>x.i));
            Log($"Part1: {res}");
            part2 = res.GetHashCode();

            LogAndReset("*2", sw);
            return (part1, part2);
        }

        private void EliminateAllergene(string allergene, List<Food> foodList)
        {
            foreach (var f in foodList)
                f.Allergenes.Remove(allergene);
        }

        private void EliminateIngredient(string ingredient, List<Food> foodList)
        {
            foreach (var f in foodList)
                f.Ingredients.Remove(ingredient);
        }

        private Food ParseFood(string s)
        {
            s = s.Replace(" (", "").Replace(")", "").Replace(",", "");
            var parts = s.Split(new[] { "contains " }, StringSplitOptions.RemoveEmptyEntries);
            return new Food(parts[0].Split(' '), parts[1].Split(' '));
        }
    }
    
    internal class Food
    {
        public HashSet<string> Ingredients { get; }
        public HashSet<string> Allergenes { get; }

        public Food(string[] ingredients, string[] allergenes)
        {
            Ingredients = ingredients.ToHashSet();
            Allergenes = allergenes.ToHashSet();
        }
    }
}

// var comp = new IntCodeComputer(source[0]);
// comp.Execute();
// var part1 = (int)comp.LastOutput;
// return (part1, 0);
