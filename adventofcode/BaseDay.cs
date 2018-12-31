using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace adventofcode
{
    public enum InputSource
    {
        test,
        prod,
    }
    public abstract class BaseDay
    {
        protected BaseDay()
        {
            _log = new StringBuilder();
            LogLevel = 10;
        }

        protected InputSource Source { get; set; }

        protected bool UseTestData {
            get => Source == InputSource.test;
            set => Source = value ? InputSource.test : InputSource.prod;
        }
        protected int LogLevel { get; set; }
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

            //            https://adventofcode.com/2018/day/8/input

            return File.ReadAllLines(fileName);
        }

        private void EnsureFiles()
        {
            if (!File.Exists(GetFileName(true)))
                File.WriteAllText(GetFileName(true), "");
            if (!File.Exists(GetFileName(false)))
                File.WriteAllText(GetFileName(false), "");
        }

        private string GetFileName(bool useTestData)
        {
            return "../../" + GetType().Name + (useTestData ? "_test" : "") + ".txt";
        }

        protected int[] GetIntInput(string[] input)
        {
            return GetInput().Select(x => int.Parse(x)).ToArray();
        }

        protected bool _addLogHeader;
        protected StringBuilder _log;
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
            LogAndCompareExpected("Part1", Part1, UseTestData ? Part1TestSolution : Part1Solution);
            LogAndCompareExpected("Part2", Part2, UseTestData ? Part2TestSolution : Part2Solution);
            PrintFooter(sw);
            File.WriteAllText(GetType().Name + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + (UseTestData ? "TEST" : "PROD") + _fileNameSuffix + ".log", _log.ToString());
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

                Log("   " + label + " = " + value);
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

        public void Log(Func<string> s, int logLevel = 0)
        {
            if (logLevel <= LogLevel)
                Log(s(), logLevel);
        }

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
    }

}
