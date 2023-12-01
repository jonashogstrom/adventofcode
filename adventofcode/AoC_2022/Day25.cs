using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = String;
    using Part2Type = String;

    [TestFixture]
    class Day25 : TestBaseClass2<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase("2=-1=0", null, "Day25_test.txt")]
        [TestCase("2---1010-0=1220-=010", null, "Day25.txt")]
        public void Test1(Part1Type? exp1, Part2Type? exp2, string resourceName)
        {
            LogLevel = resourceName.Contains("test") ? 20 : -1;
            var source = GetResource(resourceName);
            var res = ComputeWithTimer(source);
            DoAsserts(res, exp1, exp2, resourceName);
        }

        [Test]
        public void Test2()
        {
            Assert.That(ParseSnafu("222"), Is.EqualTo(62));
            Assert.That(ParseSnafu("1==="), Is.EqualTo(63));
            Assert.That(ToSnafu(3), Is.EqualTo("1="));
            Assert.That(ToSnafu(50), Is.EqualTo("200"));
            Assert.That(ToSnafu(2023), Is.EqualTo("1=110="));

            Assert.That(ParseSnafu("10"), Is.EqualTo(5));
            Assert.That(ToSnafu(12345), Is.EqualTo("1-0---0"));
            Assert.That(ToSnafu(314159265), Is.EqualTo("1121-1110-1=0"));
        }



        protected override (Part1Type part1, Part2Type part2) DoComputeWithTimer(string[] source)
        {
            Part1Type part1;
            var sw = Stopwatch.StartNew();

            // parse input here

            LogAndReset("Parse", sw);

            // solve part 1 here

            var sum = 0L;
            foreach (var s in source)
            {
                sum += ParseSnafu(s);

            }
            part1 = ToSnafu(sum);
            LogAndReset("*1", sw);

            // solve part 2 here

            return (part1, null);
        }

        private long ParseSnafu(string s)
        {
            var sum = 0L;
            var trans = new Dictionary<char, int>();
            trans['2'] = 2; 
            trans['1'] = 1; 
            trans['0'] = 0; 
            trans['-'] = -1; 
            trans['='] = -2; 

            for (int i=0; i<s.Length; i++)
            {
                var factor = (long)Math.Pow(5, s.Length - i-1);
                var value = factor * trans[s[i]];
                sum += value;
            }

            return sum;
        }

        private string ToSnafu(long value)
        {
            var trans = new Dictionary<char, int>();
            trans['2'] = 2;
            trans['1'] = 1;
            trans['0'] = 0;
            trans['-'] = -1;
            trans['='] = -2;

            var trans2 = new Dictionary<long, char>();
            trans2[0] = '0';
            trans2[1] = '1';
            trans2[2] = '2';
            trans2[3] = '=';
            trans2[4] = '-';


            var factor = 1;
            var s = "";
            while (value > 0)
            {
                var mod = value % 5;
                s = trans2[mod]+s;
                var digValue = trans[trans2[mod]];
                value -= factor * digValue;
                value = value / 5;
            }
            return s;

        }
    }
}