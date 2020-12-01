using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;

namespace AdventofCode
{
    public class BaseBaseDay
    {

        protected bool _addLogHeader;
        protected StringBuilder _log;

        protected BaseBaseDay()
        {
            _log = new StringBuilder();
            LogLevel = 10;
        }
        protected int LogLevel { get; set; }

        protected string[] GetResource(string nameOrValue)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var f = GetType().Namespace;
            var fullName = f + "." + nameOrValue;

            var resourceNames = assembly.GetManifestResourceNames();
            var resourceName = resourceNames.FirstOrDefault(str => str.EndsWith(fullName));
            if (resourceName == null)
            {
                Log("Resource not found, using value literal: " + nameOrValue);
                return new[] { nameOrValue };
            }

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            }
        }

        #region logging
        protected void Log(string s, int logLevel = 0)
        {
            if (logLevel <= LogLevel)
            {
                if (!_addLogHeader)
                {
                    _addLogHeader = true;
                    PrintHeader();
                }

                Debug.WriteLine(s);
                Console.WriteLine(s);
                _log.AppendLine(s);
            }
        }

        public void Log(Func<string> s, int logLevel = 0)
        {
            if (logLevel <= LogLevel)
                Log(s(), logLevel);
        }

        protected void PrintHeader()
        {
            Log("==== " + GetType().Name + " Log ==== ");
        }

        protected void PrintSplitter()
        {
            Log("==== " + GetType().Name + " Summary ==== ");
        }

        protected void PrintFooter(Stopwatch sw)
        {
            Log("==== " + GetType().Name + " Done ==== " + sw.ElapsedMilliseconds + "ms");
            Log("");
        }
        #endregion

        #region input parsing
        protected int[] GetIntInput(string[] input)
        {
            return input.Select(x => int.Parse(x)).ToArray();
        }

        protected IEnumerable<int> GetInts(string s, bool allowNegative = false)
        {
            var temp = "";
            foreach (var c in s)
            {
                if (c >= '0' && c <= '9' || allowNegative && c == '-')
                {
                    temp += c;
                }
                else if (temp != "")
                {
                    yield return int.Parse(temp);
                    temp = "";
                }
            }

            if (temp != "")
            {
                yield return int.Parse(temp);
            }
        }

        protected IEnumerable<float> GetFloats(string s)
        {
            var temp = "";
            foreach (var c in s)
            {
                if (c >= '0' && c <= '9' || c == '.' || c == '-')
                {
                    temp += c;
                }
                else if (temp != "")
                {
                    yield return float.Parse(temp);
                    temp = "";
                }
            }

            if (temp != "")
            {
                yield return int.Parse(temp);
            }
        }

        protected int[] GetIntArr(string s, bool allowNegative = false)
        {
            return GetInts(s, allowNegative).ToArray();
        }
        #endregion
    }

    class TestBaseClass<T, S> : BaseBaseDay
            where S : struct
            where T : struct
    {
        protected void DoAsserts((T? part1, S? part2) actual, T? exp1, S? exp2)
        {
            var (actual1, actual2) = actual;
            if (actual1.HasValue)
                Log("Calculated Part1: " + actual1);

            if (actual2.HasValue)
                Log("Calculated Part2: " + actual2);

            if (actual1.HasValue && exp1.HasValue)
                Assert.That(actual1, Is.EqualTo(exp1), "Incorrect value for Part 1");

            if (actual2.HasValue && exp2.HasValue)
                Assert.That(actual2, Is.EqualTo(exp2.Value), "Incorrect value for Part 2");
        }

        protected (T? part1, S? part2) ComputeWithTimer(string[] source)
        {
            var sw = new Stopwatch();
            sw.Start();
            var res = DoComputeWithTimer(source);
            sw.Stop();
            Log(()=>$"Total Time: {sw.ElapsedMilliseconds} ms");
            return res;
        }

        protected virtual (T? part1, S? part2) DoComputeWithTimer(string[] source)
        {
            throw new NotImplementedException();
        }
    }


    public abstract class BaseDay : BaseBaseDay
    {
        public enum InputSource
        {
            test,
            prod,
        }

        protected InputSource Source { get; set; }

        protected bool UseTestData
        {
            get => Source == InputSource.test;
            set => Source = value ? InputSource.test : InputSource.prod;
        }
        public object Part1Solution { get; set; }
        public object Part2Solution { get; set; }
        protected object Part1TestSolution { get; set; }
        protected object Part2TestSolution { get; set; }

        public object Part1 { get; set; }
        public object Part2 { get; set; }
        private string[] GetInput()
        {
            EnsureFiles();
            var fileName = GetFileName(UseTestData);
            if (UseTestData)
                Log("!!!!!! Running with test input !!!!!!!!!!!!!");

            return File.ReadAllLines(fileName);
        }

        private void EnsureFiles()
        {
            EnsureFile(GetFileName(false));
            EnsureFile(GetFileName(true));
        }

        private void EnsureFile(string fileName)
        {
            if (!File.Exists(fileName))
                File.WriteAllText(fileName, "");
        }

        private string GetFileName(bool useTestData)
        {
            var f = GetType().FullName.Split(new[] { '.' }, 2).Last().Replace('.', '/');
            return "../../" + f + (useTestData ? "_test" : "") + ".txt";
        }

        protected string[] GetTestInput(string suffix = "")
        {
            var fName = GetFileName(suffix);
            return File.ReadAllLines(fName);
        }


        protected string GetFileName(string suffix)
        {
            var f = GetType().FullName.Split(new[] { '.' }, 2).Last().Replace('.', '/');
            return "../../" + f + suffix + ".txt";
        }

        protected string _fileNameSuffix = "";

        public void Run()
        {
            _addLogHeader = false;
            Setup();
            var sw = new Stopwatch();
            sw.Start();
            DoRun(GetInput());
            sw.Stop();
            _addLogHeader = true;
            PrintSplitter();
            var p1 = LogAndCompareExpected("Part1", Part1, UseTestData ? Part1TestSolution : Part1Solution);
            var p2 = LogAndCompareExpected("Part2", Part2, UseTestData ? Part2TestSolution : Part2Solution);
            PrintFooter(sw);
            if (this.Source == InputSource.test &&
                (p1 && Part1 != null && Part1TestSolution != null && Part1Solution == null) ||
                (p2 && Part2 != null && Part2TestSolution != null && Part2Solution == null))
            {
                Log(() => "TEST DATA SEEMS OK, RUNNING WITH PROD DATA TOO!");
                Source = InputSource.prod;
                sw = new Stopwatch();
                sw.Start();
                DoRun(GetInput());
                sw.Stop();
                _addLogHeader = true;
                PrintSplitter();
                LogAndCompareExpected("Part1", Part1, UseTestData ? Part1TestSolution : Part1Solution);
                LogAndCompareExpected("Part2", Part2, UseTestData ? Part2TestSolution : Part2Solution);
                PrintFooter(sw);

            }
            File.WriteAllText(GetType().FullName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + (UseTestData ? "TEST" : "PROD") + _fileNameSuffix + ".log", _log.ToString());
        }

        public static T[][] EmptyArr<T>(int rows, int cols, T def = default(T))
        {
            var res = new T[rows][];
            for (int row = 0; row < rows; row++)
            {
                res[row] = new T[cols];
                for (int col = 0; col < cols; col++)
                    res[row][col] = def;
            }

            return res;
        }


        protected bool LogAndCompareExpected(string label, object value, object expected)
        {
            if (value != null)
            {
                if (expected != null)
                {
                    if (value.Equals(expected))
                    {
                        Log("   " + label + " = " + value + "   OK");
                        _fileNameSuffix += "[OK]";
                        return true;
                    }

                    Log("   ***********************");
                    Log("   *********************** Regression error");
                    Log("   *********************** Expected:" + expected);
                    Log("   *********************** Actual: " + value);
                    Log("   ***********************");
                    _fileNameSuffix += "[XX]";
                    return false;
                }

                Log("   " + label + " = " + value + "   ** " + (Source == InputSource.prod ? "Copied to clipboard" : ""));
                if (Source == InputSource.prod)
                    Clipboard.SetText(value.ToString());
                _fileNameSuffix += "[  ]";
                return true;
            }

            if (expected != null)
            {
                Log("   *** Not complete: " + label + ". Expected: " + expected);
                return false;
            }

            return true;
        }

        protected abstract void DoRun(string[] input);

        protected virtual void Setup()
        {

        }


    }

}
