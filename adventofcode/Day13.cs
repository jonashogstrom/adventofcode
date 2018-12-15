using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace adventofcode
{
    internal class Day13 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = false;
            Part1TestSolution = "7,3";
            Part2TestSolution = null;
            Part1Solution = "57,104";
            Part2Solution = "67,74";
        }

        protected override void DoRun(string[] input)
        {
            ParseMap(input);
            Log(MapToString());
            int gen = 0;
            while (Cars.Count > 1)
            {
                MoveCars();
                if (UseTestData)
                    Log(MapToString());
                gen++;

            }
            Log(MapToString());
            Log(gen.ToString());
            if (Cars.Any())
                Part2 = Cars.First().Col + "," + Cars.First().Row;
        }

        private void  ParseMap(string[] input)
        {
            Rows = input.Length;
            Cols = input[0].Length;
            Cars = new List<Car>();
            grid = new Pos[Rows][];
            for (int row = 0; row < Rows; row++)
                grid[row] = new Pos[Cols];

            for (int row = 0; row < input.Length; row++)
            {
                for (int col = 0; col < input[row].Length; col++)
                {
                    switch (input[row][col])
                    {
                        case '|': grid[row][col] = Pos.vertical; break;
                        case '-': grid[row][col] = Pos.horizontal; break;
                        case '>':
                            grid[row][col] = Pos.horizontal;
                            Cars.Add(new Car(row, col, Facing.right));
                            break;
                        case '<':
                            grid[row][col] = Pos.horizontal;
                            Cars.Add(new Car(row, col, Facing.left));
                            break;
                        case '^':
                            grid[row][col] = Pos.horizontal;
                            Cars.Add(new Car(row, col, Facing.up));
                            break;
                        case 'v':
                            grid[row][col] = Pos.horizontal;
                            Cars.Add(new Car(row, col, Facing.down));
                            break;
                        case '+':
                            grid[row][col] = Pos.intersection;
                            break;
                        case '/':
                            Pos x;
                            if (row == 0)
                                x = Pos.tl;
                            else
                            {
                                var above = grid[row - 1][col];
                                x = (above == Pos.empty) || above == Pos.horizontal || above == Pos.br || above == Pos.bl ? Pos.tl : Pos.br;
                            }

                            grid[row][col] = x;
                            break;
                        case '\\':
                            Pos x2;
                            if (row == 0)
                                x2 = Pos.tr;
                            else
                            {
                                var above = grid[row - 1][col];
                                x2 = (above == Pos.empty) || above == Pos.horizontal || above == Pos.br || above == Pos.bl ? Pos.tr : Pos.bl;
                            }

                            grid[row][col] = x2;
                            break;

                    }
                }
            }
        }

        public int Rows { get; set; }
        public int Cols { get; set; }
        public Pos[][] grid;

        public List<Car> Cars { get; set; }
        public List<Car> OrderedCars => Cars.OrderBy(c => c.Row).ThenBy(c => c.Col).ToList();

        public IEnumerable<Tuple<Car, Car>> Collisions()
        {
            var cars = OrderedCars;
            for (int i = 0; i < cars.Count - 1; i++)
                if (cars[i].Row == cars[i + 1].Row && cars[i].Col == cars[i + 1].Col)
                    yield return new Tuple<Car, Car>(cars[i], cars[i + 1]);
        }

        public void MoveCars()
        {
            foreach (var c in OrderedCars)
            {
                if (!Cars.Contains(c))
                    continue;
                
                var nextCol = c.Col;
                if (c.Facing == Facing.right)
                    nextCol += 01;
                if (c.Facing == Facing.left)
                    nextCol -= 01;
                var nextRow = c.Row;
                if (c.Facing == Facing.down)
                    nextRow += 01;
                if (c.Facing == Facing.up)
                    nextRow -= 01;
                var nextCell = grid[nextRow][nextCol];
                if (nextCell == Pos.intersection)
                {
                    if (c.NextTurn == nextTurn.left)
                        c.Facing = (Facing)((((int)c.Facing) + 4 - 1) % 4);
                    else if (c.NextTurn == nextTurn.right)
                        c.Facing = (Facing)((((int)c.Facing) + 4 + 1) % 4);

                    c.NextTurn = (nextTurn)(((int)c.NextTurn + 1) % 3);
                }
                else if (nextCell == Pos.tl)
                    c.Facing = c.Facing == Facing.left ? Facing.down : Facing.right;
                else if (nextCell == Pos.tr)
                    c.Facing = c.Facing == Facing.right ? Facing.down : Facing.left;
                else if (nextCell == Pos.br)
                    c.Facing = c.Facing == Facing.right ? Facing.up : Facing.left;
                else if (nextCell == Pos.bl)
                    c.Facing = c.Facing == Facing.left ? Facing.up : Facing.right;
                else if (nextCell == Pos.horizontal || nextCell == Pos.vertical)
                {
                    // nothing
                }
                else
                    throw new Exception();

                c.Row = nextRow;
                c.Col = nextCol;
                foreach (var collision in Collisions())
                {
                    if (Part1 == null)
                        Part1 = collision.Item1.Col + "," + collision.Item1.Row;
                    Cars.Remove(collision.Item1);
                    Cars.Remove(collision.Item2);
                }
            }
        }

        public string MapToString()
        {
            StringBuilder s = new StringBuilder();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    char c = ' ';
                    var cell = grid[row][col];
                    var hasCar = false;
                    if (cell == Pos.bl || cell == Pos.tr)
                        c = '\\';
                    else if (cell == Pos.br || cell == Pos.tl)
                        c = '/';
                    else if (cell == Pos.vertical)
                        c = '|';
                    else if (cell == Pos.horizontal)
                        c = '-';
                    else if (cell == Pos.intersection)
                        c = '+';
                    foreach(var car in Cars)
                    {
                        if (car.Row == row && car.Col == col)
                        {
                            if (hasCar)
                                c = 'X';
                            else if (car.Facing == Facing.down)
                                c = 'v';
                            else if(car.Facing == Facing.up)
                                c = '^';
                            else if(car.Facing == Facing.left)
                                c = '<';
                            else if(car.Facing == Facing.right)
                                c = '>';
                            hasCar = true;
                        }}

                    s.Append(c);
                }

                s.AppendLine();
            }

            return s.ToString();
        }
        
    }

    internal enum Pos
    {
        empty,
        vertical,
        horizontal,
        intersection,
        tl, tr, bl, br
    }
    internal enum Facing
    {
        left, up, right, down
    }

    internal enum nextTurn { left, straight, right }

    internal class Car
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public Facing Facing { get; set; }

        public Car(int row, int col, Facing facing)
        {
            Row = row;
            Col = col;
            Facing = facing;
            NextTurn = nextTurn.left;
        }

        public nextTurn NextTurn { get; set; }
    }
}