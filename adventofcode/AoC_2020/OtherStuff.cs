using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using NUnit.Framework;

namespace AdventofCode.AoC_2020
{
    class OtherStuff : TestBaseClass<int, int>
    {
        [Test]
        public void TestFibb()
        {
            var c = new Cache<BigInteger, int>();
            var count = 500;
            var fib = c.Do(fibb, count);

            Console.WriteLine(fib);
            Assert.That(fib, Is.EqualTo(fib));
        }

        public BigInteger fibb(int i, Cache<BigInteger, int> c)
        {
            switch (i)
            {
                case 0:
                    return 0;
                case 1:
                    return 1;
                default:
                    return c.Do(fibb, i - 1) + c.Do(fibb, i - 2);
            }
        }


        public long fibb(int i)
        {
            switch (i)
            {
                case 0:
                    return 0;
                case 1:
                    return 1;
                default:
                    return fibb(i - 1) + fibb(i - 2);
            }
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
                foreach (var x in continents.Keys.ToArray())
                    if (volume >= continents[x])
                    {
                        Log($"Bag size {side} with volume {volume} is large enough for {x}. Adding {side * 8} units of fabric");
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
                    var s = new string(clear.Select(b => (char)b).ToArray());
                    Log(s);

                }
            }
        }

        [Test]
        public void Mission002()
        {
            var data = File.ReadAllText("c:\\temp\\data.txt").Trim();
            var parts = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                    foreach (var r in reverseRules.Keys)
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
            //            var code = "YZZWXVAVYZ";
            //          REENFUAURE
            // V = 55 = U
            // W = 4E = N
            // X = 46 = F
            // Y = 52 = R
            // Z = 45 = E
        }



    }
}