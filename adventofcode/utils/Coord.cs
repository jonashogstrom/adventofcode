using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AdventofCode
{
    [DebuggerDisplay("({Row},{Col})")]
    public class Coord
    {
        public static Coord N = new Coord(-1, 0);
        public static Coord E = new Coord(0, 1);
        public static Coord S = new Coord(1, 0);
        public static Coord W = new Coord(0, -1);
        public static Coord Origin = new Coord(0, 0);
        public static Coord[] NSWE = new[] { N, S, W, E };

        public static Coord NW = N.Move(W);
        public static Coord NE = N.Move(E);
        public static Coord SE = S.Move(E);
        public static Coord SW = S.Move(W);

        public static readonly Coord[] Directions4 = { N, E, S, W };
        public static readonly Coord[] Directions8 = { N, NE, E, SE, S, SW, W, NW };
        public static readonly HexDirection[] HexNeighbors =
        {
            HexDirection.sw, HexDirection.w, HexDirection.nw,
            HexDirection.ne, HexDirection.e, HexDirection.se,
        };

        public static readonly Dictionary<Coord, char> trans2NESW = new Dictionary<Coord, char>()
        {
            {N, 'N'}, {S, 'S'}, {E, 'E'}, {W, 'W'},
        };

        public static readonly Dictionary<Coord, string> trans28Dir = new Dictionary<Coord, string>()
        {
            {N, "N"}, {S, "S"}, {E, "E"}, {W, "W"},
            {NE, "NE"}, {SE, "SE"}, {NW, "NW"}, {SW, "SW"},
        };

        public static readonly Dictionary<Coord, char> trans2Arrow = new Dictionary<Coord, char>()
        {
            {N, '^'}, {S, 'v'}, {E, '>'}, {W, '<'},
        };
        public static readonly Dictionary<char, Coord> trans2Coord = new Dictionary<char, Coord>()
        {
            {'N', N}, {'S', S}, {'E', E}, {'W', W},
            {'^', N}, {'v', S}, {'>', E}, {'<', W},
            {'U', N}, {'D', S}, {'R', E}, {'L', W},
        };

        public int Row { get; set; }
        public int Col { get; set; }

        public int X => Col;
        public int Y => Row;

        public Coord(int row, int col)
        {
            CoordCounter++;
            Row = row;
            Col = col;
        }

        public static int CoordCounter;

        public static Coord FromXY(int x, int y)
        {
            return new Coord(y, x);
        }

        public bool IsNorthOf(Coord c) => this.Y < c.Y;
        public bool IsWestOf(Coord c) => this.X < c.X;


        private int GetDir(int v1, int v2)
        {
            return v1.CompareTo(v2);
        }
        /// <summary>
        /// Return a list of coordinates from current pos to target pos (c2) as long as the path is either NSWE or a 45 degree angle
        /// </summary>
        /// <param name="c2"></param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IEnumerable<Coord> PathTo(Coord c2, bool inclusive = true)
        {
            var rDir = c2.Row.CompareTo(Row);
            var cDir = c2.Col.CompareTo(Col);
            //var res = new List<Coord>();

            var rDist = Math.Abs(Row - c2.Row);
            var cDist = Math.Abs(Col - c2.Col);
            if (rDist != 0 && cDist != 0 && rDist != cDist)
                throw new Exception($"Path is not a multiple of 45 degrees! {this} => {c2} (rDist: {rDist}, cDist: {cDist})");
            var steps = Math.Max(rDist, cDist) + 1;
            for (int s = 0; s < steps; s++)
            {
                if (inclusive || (s > 1 && s < steps - 1))
                {
                    var c = Col + s * cDir;
                    var r = Row + s * rDir;
                    yield return new Coord(r, c);
                }
            }
        }

        /// <summary>
        /// Sample Coord strings supported: "x, y", "(x y)", "x;y"
        /// Supported caracters: (, ;)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Coord Parse(string s)
        {
            var parts = s.Split(new[] { '(', ',', ' ', ';', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new Exception("Invalid coord string: " + s);
            return new Coord(int.Parse(parts[1]), int.Parse(parts[0]));
        }

        private bool Equals(Coord other)
        {
            return Row == other.Row && Col == other.Col;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Coord)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Row * 397) ^ Col;
            }
        }

        public static Coord CharToDir(char c)
        {
            return trans2Coord[c];
        }
        public static bool TryCharToDir(char c, out Coord dir)
        {
            return trans2Coord.TryGetValue(c, out dir);
        }

        public char DirToNESW()
        {
            return trans2NESW[this];
        }

        public Coord RotateCCW90()
        {
            return new Coord(-Col, Row);
        }
        public Coord RotateCCWDegrees(int degrees)
        {
            if (degrees % 90 != 0)
                throw new Exception("can only rotate even 90 degrees");
            var res = this;
            for (var i = 0; i < degrees / 90; i++)
                res = res.RotateCCW90();
            return res;
        }

        public Coord RotateCWDegrees(int degrees)
        {
            if (degrees % 90 != 0)
                throw new Exception("can only rotate even 90 degrees");
            var res = this;
            for (var i = 0; i < degrees / 90; i++)
                res = res.RotateCW90();
            return res;
        }

        public Coord RotateCW90()
        {
            return new Coord(Col, -Row);
        }

        public IEnumerable<Coord> GenAdjacent4()
        {
            foreach (var d in Directions4)
                yield return Move(d);
        }

        public IEnumerable<Coord> GenAdjacent8()
        {
            foreach (var d in Directions8)
                yield return Move(d);
        }

        public Coord Move(Coord coord, int count = 1)
        {
            return new Coord(Row + coord.Row * count, Col + coord.Col * count);
        }

        public Coord HexMove(HexDirection hexDirection)
        {
            if (hexDirection == HexDirection.e)
                return Move(E);
            if (hexDirection == HexDirection.w)
                return Move(W);

            if (Y % 2 == 0)
            {
                switch (hexDirection)
                {
                    case HexDirection.nw:
                        return Move(N);
                    case HexDirection.ne:
                        return Move(NE);
                    case HexDirection.sw:
                        return Move(S);
                    case HexDirection.se:
                        return Move(SE);
                    default:
                        throw new Exception();
                }

            }
            else
            {
                switch (hexDirection)
                {
                    case HexDirection.nw:
                        return Move(NW);
                    case HexDirection.ne:
                        return Move(N);
                    case HexDirection.sw:
                        return Move(SW);
                    case HexDirection.se:
                        return Move(S);
                    default:
                        throw new Exception();
                }
            }
        }

        public int Dist(Coord pos)
        {
            return Math.Abs(Row - pos.Row) + Math.Abs(Col - pos.Col);
        }

        public override string ToString()
        {
            return $"{nameof(Row)}: {Row}, {nameof(Col)}: {Col}";
        }

        public Coord Move1Toward(Coord target)
        {
            var xDir = Clamp(target.X - X, -1, 1);
            var yDir = Clamp(target.Y - Y, -1, 1);
            return new Coord(Y + yDir, X + xDir);
        }

        private int Clamp(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        public bool OnSameLine(Coord other)
        {
            return X == other.X || Y == other.Y;
        }

        public Coord Multiply(int factor)
        {
            return new Coord(Row * factor, Col * factor);
        }

        public Coord Subtract(Coord other)
        {
            return Coord.FromXY(X - other.X, Y - other.Y);
        }
    }

    [DebuggerDisplay("{x}, {y}, {z}")]
    public class Coord3d
    {
#if dotnet4x
        private static List<AxisAngleRotation3D> _rotations = new List<AxisAngleRotation3D>();

        static Coord3d()
        {
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 90));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0, 1, 0), -90));

            var a = 0.5774;
            var b = 0.7071;
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(a, a, a), 120));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(b, b, 0), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-a, -a, a), 120));

            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0, 0, -1), 90));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-a, a, -a), 120));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-b, b, 0), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(a, -a, -a), 120));

            _rotations.Add(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(a, a, -a), 120));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0, b, -b), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(a, -a, a), 120));

            _rotations.Add(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(b, 0, -b), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(b, 0, b), 180));

            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-1, 0, 0), 90));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-a, a, a), 120));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(0, b, b), 180));
            _rotations.Add(new AxisAngleRotation3D(new Vector3D(-a, -a, -a), 120));
        }


        public static IEnumerable<AxisAngleRotation3D> AllRotations => _rotations;
#endif
        public int Max => new[] { x, y, z }.Max();
        public int Min => new[] { x, y, z }.Min();

        public Coord3d(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        protected bool Equals(Coord3d other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Coord3d)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x;
                hashCode = (hashCode * 397) ^ y;
                hashCode = (hashCode * 397) ^ z;
                return hashCode;
            }
        }

        public int x;
        public int y;
        public int z;

        public Coord3d Clone()
        {
            return new Coord3d(x, y, z);
        }

        public Coord3d Move(int x, int y, int z)
        {
            return new Coord3d(this.x + x, this.y + y, this.z + z);
        }

        public Coord3d Move(Coord3d v)
        {
            return Move(v.x, v.y, v.z);
        }

        public int absValue()
        {
            return Math.Abs(x) + Math.Abs(y) + Math.Abs(z);
        }

        public static Coord3d Parse(string s)
        {
            var p = s.Split(',');
            var c = new Coord3d(int.Parse(p[0]), int.Parse(p[1]), int.Parse(p[2]));
            return c;
        }
#if dotnet4x
        public Coord3d Rotate(AxisAngleRotation3D rotation)
        {
            var rt3d = new RotateTransform3D
            {
                Rotation = rotation
            };

            var newP = rt3d.Value.Transform(new Point3D(x, y, z));

            var res = new Coord3d((int)Math.Round(newP.X), (int)Math.Round(newP.Y), (int)Math.Round(newP.Z));
            if (Math.Abs(res.x) + Math.Abs(res.y) + Math.Abs(res.z) != Math.Abs(x) + Math.Abs(y) + Math.Abs(z))
                throw new Exception("Bad math!");
            return res;
        }
#endif

        public IEnumerable<Coord3d> Neighbors6()
        {
            yield return Move(1, 0, 0);
            yield return Move(-1, 0, 0);
            yield return Move(0, 1, 0);
            yield return Move(0, -1, 0);
            yield return Move(0, 0, 1);
            yield return Move(0, 0, -1);
        }

        public bool IsInsideCube(int min, int max)
        {
            return x <= max && x >= min &&
                   y <= max && y >= min &&
                   z <= max && z >= min;
        }

        public long Distance(Coord3d c)
        {
            return Math.Abs(x - c.x) +
                   Math.Abs(y - c.y) +
                   Math.Abs(z - c.z);
        }
    }

    public enum HexDirection
    {
        nw, ne, e, se, sw, w
    }
}