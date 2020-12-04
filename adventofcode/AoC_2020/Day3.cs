using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms.VisualStyles;
using AdventofCode.AoC_2018;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day3 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(7, 336, "Day3_test.txt")]
        [TestCase(200, 3737923200L, "Day3.txt")]
        public void Test1(Part1Type exp1, Part2Type? exp2, string resourceName)
        {
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2);
        }

        protected override (Part1Type? part1, Part2Type? part2) DoComputeWithTimer(string[] source)
        {
            Log("Part1");
            var part1 = CheckSlope(source, 3, 1);
            Log("Part2");
            var part2 =
                CheckSlope(source, 1, 1) *
                CheckSlope(source, 3, 1) *
                CheckSlope(source, 5, 1) *
                CheckSlope(source, 7, 1) *
                CheckSlope(source, 1, 2);
            return (part1, part2);
        }

        private long CheckSlope(string[] source, int right, int down)
        {
            var width = source[0].Length;
            var pos = 0;
            var trees = 0;
            for (int row=0; row < source.Length; row += down)
            {
                if (source[row][pos % width] == '#')
                    trees++;
                pos += right;
            }
            Log($"Right: {right} Down: {down} Trees: {trees}");

            return trees;
        }

        /*
Asia 4,541,638,755
Africa 1,340,884,126
Europe  747,697,967
South America  430,819,421
North America  369,015,287
Oceania   42,721,994
         */

        [Test]
        public void Testinfini()
        {
            var continents = new Dictionary<string, long>();
            continents["Asia"] = 4541638755L;
            continents["Africa"] = 1340884126;
            continents["Europe"] = 747697967;
            continents["South America"] = 430819421;
            continents["North America"] = 369015287;
            continents["Oceania"] = 42721994;
            var side = 1L;
            var fabric = 0L;
            while (continents.Any())
            {
                long volume = 5 * side * side + 4 * side * (side - 1) / 2;
                foreach(var x in continents.Keys.ToArray())
                    if (volume >= continents[x])
                    {
                        Log($"Bag size {side} with volume {volume} is large enough for {x}. Adding {side*8} units of fabric");
                        fabric += side * 8;
                        continents.Remove(x);
                    }

                side++;
            }
            Log($"Total fabric: {fabric}");
        }


        // https://tracking-game.reaktor.com/
        [Test]
        public void Mission001()
        {
            var base64 = File.ReadAllText("c:\\temp\\base64.bin").Trim();

            for (int i = 0; i < base64.Length - 16; i++)
            {
                var values = new HashSet<char>();
                for (int j = 0; j < 16; j++)
                {
                    values.Add(base64[j + i]);
                }

                if (values.Count == 16)
                {
                    var msg = base64.Substring(i, 16); 
                    Log(msg);
                    var clear = Convert.FromBase64String(msg);
                    var s = new string(clear.Select(b=>(char)b).ToArray());
                    Log(s);

                }
            }
        }

        [Test]
        public void Mission002()
        {
            var data = File.ReadAllText("c:\\temp\\data.txt").Trim();
            var parts = data.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (var p in parts)
            {
                var b = Convert.ToInt32(p, 2);
                sb.AppendLine(b.ToString());
            }
            File.WriteAllText("c:\\temp\\data.csv", sb.ToString());
        }

        [Test]
        public void MainChamber()
        {
            // https://reaktor-svalbard-challenge.herokuapp.com/
            // /tunnel = ROOTSOFLIFE (jsfuck, and some debugging
            // /mainchamber = HUNTORGATHER (reverse apply the rules)

            var final = "HHGNHHGNGNRHNTTGHHGNHHGNGNRHNTTGGNRHNTTGRHHHHHGNNTTGTGTGTGTGGNRHUNTTGTGTGTGTGGNRHTGTGGNRHTGTGGNRHTGTGGNRHTGTGGNRHGNRHNTTGRHHHHHGNTGTGGNRHTGTGGNRHGNRHNTTGRHHHHHGNTGTGGNRHTGTGGNRHGNRHNTTGRHHHHHGNORHHHHHGNHHGNHHGNHHGNHHGNGNRHNTTGHHGNHHGNGNRHNTTGHHGNHHGNGNRHNTTGGNRHNTTGRHHHHHGNNTTGTGTGTGTGGNRHRHHHHHGNHHGNHHGNHHGNHHGNGNRHNTTGATGTGGNRHTGTGGNRHGNRHNTTGRHHHHHGNTGTGGNRHTGTGGNRHGNRHNTTGRHHHHHGNHHGNHHGNGNRHNTTGHHGNHHGNGNRHNTTGGNRHNTTGRHHHHHGNNTTGTGTGTGTGGNRHERHHHHHGNHHGNHHGNHHGNHHGNGNRHNTTGHHGNHHGNGNRHNTTGHHGNHHGNGNRHNTTG";
            var rules = new Dictionary<string, string>();
            rules["G"] = "GN";
            rules["R"] = "NT";
            rules["N"] = "RH";
            rules["T"] = "HH";
            rules["H"] = "TG";
            rules["A"] = "A";
            rules["I"] = "I";
            rules["U"] = "U";
            rules["E"] = "E";
            rules["O"] = "O";
            var reverseRules = new Dictionary<string, string>();
            foreach (var x in rules.Keys)
                reverseRules[rules[x]] = x;
            var s = final;
            for (int i = 0; i < 6; i++)
            {
                var p = 0;
                var newstring = "";
                while (p < s.Length)
                {
                    foreach(var r in reverseRules.Keys)
                        if (s.Substring(p, r.Length) == r)
                        {
                            newstring += reverseRules[r];
                            p += r.Length;
                            break;
                        }


                }

                s = newstring;
            }
            Log(s);

            // TACGATGCATGGCTACREENFUAURETTAGACTAGCACTCGA
            var code = "YZZWXVAVYZ";
            //          REENFUAURE
            // V = 55 = U
            // W = 4E = N
            // X = 46 = F
            // Y = 52 = R
            // Z = 45 = E
        }


    }

    // var comp = new IntCodeComputer(source[0]);
    // comp.Execute();
    // var part1 = (int)comp.LastOutput;
    // return (part1, 0);

}