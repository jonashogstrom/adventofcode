namespace AdventofCode.AoC_2018
{
    internal class Day11 : BaseDay
    {
        protected override void Setup()
        {
            UseTestData = false;
            Part1TestSolution = "33,45";
            Part1TestSolution = "21,61";
            Part2TestSolution = "19,269,16";
            Part1Solution = null;
            Part2Solution = null;
        }

        protected override void DoRun(string[] input)
        {
            //            var power = calcPowerlevel(122, 79, 57);
            //            power = calcPowerlevel(217, 196, 39);
            //            power = calcPowerlevel(101, 153, 71);
            var serial = UseTestData ? 18 : 7400;

            Part1 = FindLargest(serial);
        }

        private string FindLargest(int serial)
        {
            var gridSize = 300;
            var grid = new int[gridSize][];
            for (int x = 0; x < gridSize; x++)
            {
                grid[x] = new int[gridSize];
                for (int y = 0; y < gridSize; y++)
                    grid[x][y] = calcPowerlevel(x, y, serial);
            }

            var maxPower = int.MinValue;
            var top = -1;
            var left = -1;
            var selectedSize = -1;
            for (int size = 1; size < 300; size++)
            {
                Log(size.ToString());
                for (int x = 0; x < gridSize - size; x++)
                {
                    for (int y = 0; y < gridSize - size; y++)
                    {
                        var pow = 0;
                        for (var x1 = x; x1 < x + size; x1++)
                            for (var y1 = y; y1 < y + size; y1++)
                            {
                                pow += grid[x1][y1];
                            }

                        if (pow > maxPower)
                        {
                            maxPower = pow;
                            top = y;
                            left = x;
                            selectedSize = size;
                            Log(left + "," + top + "," + selectedSize);
                        }
                    }
                }}

            return left + "," + top + "," + selectedSize;
        }

        private int calcPowerlevel(int x, int y, int serial)
        {
            var rackid = x + 10;
            var power = rackid * y;
            power += serial;
            power = power * rackid;
            power = (power / 100) % 10;
            power = power - 5;
            return power;
        }
    }
}