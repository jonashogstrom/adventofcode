using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace adventofcode
{
    internal class Day20 : BaseDay
    {
        protected override void Setup()
        {
            Source = InputSource.test;
            Source = InputSource.prod;

            LogLevel = UseTestData ? 5 : 1;

            Part1TestSolution = null;
            Part2TestSolution = null;
            Part1Solution = 4239;
            Part2Solution = 8205;
        }

        protected override void DoRun(string[] input)
        {
            foreach (var s in input)
            {
                var str = s.Split(':');
                var regex = ParseRegex(str.Length == 1 ? s : str[1]);
                var rooms = TraceRoom(regex);
                var res = rooms.OrderBy(r => r.Dist).Last().Dist;
                if (str.Length == 2)
                {
                    if (res != int.Parse(str[0]))
                        throw new Exception();
                }

                Part1 = res;
                res = rooms.Count(r => r.Dist >= 1000);
                Part2 = res;
            }
        }

        private IEnumerable<Room> TraceRoom(Node node)
        {
            var rooms = new Dictionary<Coord, Room>();
            var start = new Room(new Coord(0, 0));

            rooms[start.Coord] = start;
            TraceTree(rooms, new List<Room> { start }, node);
            ExpandDistances(rooms, new Coord(0, 0));

            return rooms.Values;
        }

        private List<Room> TraceTree(IDictionary<Coord, Room> rooms, List<Room> startRooms, Node node)
        {
            var res = new List<Room>();
            if (startRooms.Count > 1)
                startRooms = startRooms.Distinct().ToList();
            foreach (var start in startRooms)
            {
                if (node is Leaf leaf)
                {
                    var dir = Coord.CharToDir(leaf.C);
                    var newCoord = start.Coord.Move(dir);
                    if (!rooms.TryGetValue(newCoord, out var newRoom))
                    {
                        newRoom = new Room(newCoord);
                        rooms[newCoord] = newRoom;
                    }
                    start.Doors[leaf.C] = newRoom;

                    var otherDoor = dir.RotateCCW90().RotateCCW90().DirToNESW();
                    newRoom.Doors[otherDoor] = start;
                    res.Add(newRoom);
                }
                else if (node is ListNode l)
                {
                    if (l.Mode == Mode.Choice)
                    {
                        foreach (var n in l.Nodes)
                        {
                            res.AddRange(TraceTree(rooms, new List<Room> { start }, n));
                        }
                    }
                    else
                    {
                        var newRooms = new List<Room> { start };
                        foreach (var n in l.Nodes)
                        {
                            newRooms = TraceTree(rooms, newRooms, n);
                        }

                        res.AddRange(newRooms);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Flood fill distances in a breadth first approach, terminate after a full round when any of the target coordinates have been found
        /// </summary>
        private void ExpandDistances(Dictionary<Coord, Room> rooms, Coord initCoords)
        {
            var dist = 0;
            var candidates = new List<Coord> { initCoords };
            while (candidates.Any())
            {
                var newCandidates = new List<Coord>();
                foreach (var c in candidates)
                {

                    rooms[c].Dist = dist;
                    foreach (var d in rooms[c].Doors.Values)
                    {

                        if (rooms[d.Coord].Dist <= dist)
                            continue; // already found a better or equal path

                        newCandidates.Add(d.Coord);
                    }
                }

                candidates = newCandidates.Distinct().ToList();

                dist++;
            }
        }


        private Node ParseRegex(string s)
        {
            var root = new ListNode(Mode.List);
            ParseList(s.Substring(1, s.Length - 2), 0, root);
            return root;
        }

        private int ParseList(string str, int pos, ListNode root)
        {
            root.Nodes = new List<Node>();
            while (pos < str.Length)
            {
                if (str[pos] == 'N' || str[pos] == 'W' || str[pos] == 'S' || str[pos] == 'E')
                {
                    root.Nodes.Add(new Leaf(str[pos]));
                    pos++;
                }
                else if (str[pos] == '(')
                {
                    pos++;
                    var n = new ListNode(Mode.Choice);
                    n.Nodes = new List<Node>();
                    do
                    {

                        var n2 = new ListNode(Mode.List);
                        pos = ParseList(str, pos, n2);
                        n.Nodes.Add(n2);
                    } while (str[pos - 1] == '|');
                    root.Nodes.Add(n);
                }
                else if (str[pos] == ')' || str[pos] == '|')
                {
                    pos++;
                    return pos;
                }
            }
            return pos;
        }



        [DebuggerDisplay("{Coord.Row}, {Coord.Col}")]
        internal class Room
        {
            public Coord Coord { get; }
            public int Dist { get; set; }

            public Dictionary<char, Room> Doors = new Dictionary<char, Room>();

            public Room(Coord coord)
            {
                Coord = coord;
                Dist = int.MaxValue;
            }
        }

        [DebuggerDisplay("{Mode} {Nodes.Count}")]
        internal class ListNode : Node
        {
            public Mode Mode { get; }
            public List<Node> Nodes { get; set; }

            public ListNode(Mode mode)
            {
                Mode = mode;
            }
        }

        internal enum Mode
        {
            List, Choice
        }

        [DebuggerDisplay("{C}")]

        internal class Leaf : Node
        {
            public char C { get; }

            public Leaf(char c)
            {
                C = c;
            }
        }

        internal class Node
        {
        }
    }
}