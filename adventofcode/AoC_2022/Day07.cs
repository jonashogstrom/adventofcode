using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Accord.Collections;
using NUnit.Framework;

namespace AdventofCode.AoC_2022
{
    // to use string-types, change baseclass to TestBaseClass2 and remove a bunch of ? in the methods below
    using Part1Type = Int64;
    using Part2Type = Int64;

    [TestFixture]
    class Day07 : TestBaseClass<Part1Type, Part2Type>
    {
        public bool Debug { get; set; }

        [Test]
        [TestCase(95437, 24933642, "Day07_test.txt")]
        [TestCase(1077191, 5649896, "Day07.txt")]
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
            var root = new Folder("/", null);
            Folder currDir = null;
            foreach (var line in source)
            {
                var parts = line.Split(' ');
                if (line.StartsWith("$ cd /"))
                {
                    currDir = root;
                }
                else if (line.StartsWith("$ cd .."))
                {
                    currDir = currDir.Parent;
                }
                else if (line.StartsWith("$ cd "))
                {
                    currDir = currDir.GetSubDir(line.Split(' ')[2]);
                }
                else if (line.StartsWith("dir"))
                {
                    currDir.GetSubDir(parts[1]);
                }
                else if (int.TryParse(parts[0], out var size))
                {
                    currDir.CreateFile(size, parts[1]);
                }
            }
            LogAndReset("Parse", sw);

            var folders = new List<Folder>();
            FindFoldersBelowSize(root, 100000, folders);
            part1 = folders.Sum(f => f.TotalSize);
            LogAndReset("*1", sw);

            long free = 70000000 - root.TotalSize;
            var needed = 30000000 - free;
            folders = new List<Folder>();
            FindFoldersAboveSize(root, needed, folders);
            part2 = folders.OrderBy(f => f.TotalSize).First().TotalSize;
            LogAndReset("*2", sw);

            return (part1, part2);
        }


        private void FindFoldersBelowSize(Folder root, long max, List<Folder> folders)
        {
            if (root.TotalSize <= max)
                folders.Add(root);
            foreach (var f in root.SubFolders)
                FindFoldersBelowSize(f, max, folders);
        }

        private void FindFoldersAboveSize(Folder root, long max, List<Folder> folders)
        {
            if (root.TotalSize >= max)
                folders.Add(root);
            foreach (var f in root.SubFolders)
                FindFoldersAboveSize(f, max, folders);
        }
    }

    internal class Folder
    {
        private readonly Dictionary<string, Folder> _subFolders = new Dictionary<string, Folder>();
        private List<(int size, string fileName)> _files = new List<(int size, string fileName)>();
        public int LocalSize { get; private set; }
        public string Path { get; }
        public Folder Parent { get; }

        public Folder(string path, Folder parent)
        {
            Path = path;
            Parent = parent;
        }

        public Folder GetSubDir(string s)
        {
            if (_subFolders.TryGetValue(s, out var res))
                return res;
            res = new Folder(s, this);
            _subFolders[s] = res;
            return res;
        }

        public long TotalSize
        {
            get
            {
                return LocalSize + _subFolders.Values.Sum(f => f.TotalSize);
            }
        }

        public IEnumerable<Folder> SubFolders => _subFolders.Values;

        public void CreateFile(int size, string fileName)
        {
            _files.Add((size, fileName));
            LocalSize += size;
        }
    }

    internal class Files
    {
    }
}