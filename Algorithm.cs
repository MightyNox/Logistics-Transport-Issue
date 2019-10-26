using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace Logistics_Transport_Issue
{
    internal class Algorithm
    {
        public static void Calculate(int[] supply, int[] demand, int[,] costs)
        {
            //TODO synchronize with minimal matrix method
            InsertFictionalSupplierOrReceiver(ref supply, ref demand);

            var distribution = MinimalElementMatrixMethod(supply, demand, costs);

            Console.WriteLine();

            var alphasAndBetas = CalculateAlphaAndBeta(distribution, costs);

            Console.WriteLine();

            var deltas = CalculateDeltas(distribution, costs, alphasAndBetas[0], alphasAndBetas[1]);

            Console.WriteLine();

            if (!IsAnyValueNegative(deltas))
            {
                Console.WriteLine(@"Algorithm completed!");

                Console.WriteLine(@"Total costs: " + CalculateTotalCosts(distribution, costs));
                return;
            }
        }

        private static void InsertFictionalSupplierOrReceiver(ref int[] supply, ref int[] demand)
        {
            var supplySum = supply.Length;
            var demandSum = demand.Length;

            if (supplySum > demandSum)
            {
                demand = demand.Concat(new[] {supplySum - demandSum}).ToArray();
            }
            else if (supplySum < demandSum)
            {
                supply = supply.Concat(new[] {demandSum - supplySum}).ToArray();
            }
        }

        private static int[,] MinimalElementMatrixMethod(IList<int> supply, IList<int> demand, int[,] costs)
        {
            dynamic distribution = new int[costs.GetLength(0), costs.GetLength(1)];
            dynamic tmpCosts = (int[,]) costs.Clone();

            while (true)
            {
                var index = GetIndicesOfMinimum(tmpCosts);
                var row = index[0];
                var column = index[1];

                if (tmpCosts[row, column] == int.MaxValue) break;

                var minimumDistribution = Math.Min(demand[column], supply[row]);
                distribution[row, column] = minimumDistribution;
                supply[row] -= minimumDistribution;
                demand[column] -= minimumDistribution;
                tmpCosts[row, column] = int.MaxValue;
            }

            //TODO Save to the file
            for (var i = 0; i < distribution.GetLength(0); i++)
            {
                for (var j = 0; j < distribution.GetLength(1); j++)
                {
                    Console.Write(distribution[i, j] + "\t");
                }

                Console.WriteLine();
            }

            return distribution;
        }

        private static int[][] CalculateAlphaAndBeta(int[,] distribution, int[,] costs)
        {
            var rows = distribution.GetLength(0);
            var columns = distribution.GetLength(1);

            var alphas = new int?[rows];
            var betas = new int?[columns];

            //TODO investigate edge-case - whether beta can be null
            alphas[0] = 0;
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    if (distribution[row, column] == 0) continue;

                    if (alphas[row] != null)
                    {
                        betas[column] = costs[row, column] - alphas[row];
                    }
                    else
                    {
                        alphas[row] = costs[row, column] - betas[column];
                    }
                }
            }

            //TODO Save to the file
            Console.Write(@"Alphas: ");
            for (var i = 0; i < rows; i++)
            {
                Console.Write(alphas[i] + @" ");
            }

            Console.WriteLine();

            Console.Write(@"Betas: ");
            for (var i = 0; i < columns; i++)
            {
                Console.Write(betas[i] + @" ");
            }

            Console.WriteLine();

            return new[]
            {
                Array.ConvertAll(alphas, x => x ?? 0),
                Array.ConvertAll(betas, x => x ?? 0)
            };
        }

        private static int[,] CalculateDeltas(int[,] distribution, int[,] costs, IReadOnlyList<int> alphas, IReadOnlyList<int> betas)
        {
            var rows = costs.GetLength(0);
            var columns = costs.GetLength(1);
            var deltas = new int[rows, columns];

            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    if (distribution[row, column] > 0)
                    {
                        deltas[row, column] = 0;
                    }
                    else
                    {
                        deltas[row, column] = costs[row, column] - alphas[row] - betas[column];
                    }
                }
            }


            //TODO Save to the file
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    if (deltas[i, j] == 0)
                    {
                        Console.Write(@"x");
                    }
                    else
                    {
                        Console.Write(deltas[i, j]);
                    }
                    
                    Console.Write(@" ");
                }

                Console.WriteLine();
            }

            return deltas;
        }

        private static bool IsAnyValueNegative(int[,] array)
        {
            var rows = array.GetLength(0);
            var columns = array.GetLength(1);

            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    if (array[row, column] < 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int CalculateTotalCosts(int[,] distribution, int[,] costs)
        {
            var rows = costs.GetLength(0);
            var columns = costs.GetLength(1);

            var totalCosts = 0;
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    totalCosts += distribution[row, column] * costs[row, column];
                }
            }

            return totalCosts;
        }

        private static int[] GetIndicesOfMinimum(int[,] array)
        {
            var minValue = int.MaxValue;
            var minFirstIndex = -1;
            var minSecondIndex = -1;

            for (var i = array.GetLength(0) - 1; i >= 0; --i)
            {
                for (var j = array.GetLength(1) - 1; j >= 0; --j)
                {
                    var value = array[i, j];

                    if (!(value <= minValue)) continue;

                    minFirstIndex = i;
                    minSecondIndex = j;

                    minValue = value;
                }
            }

            return new[] {minFirstIndex, minSecondIndex};
        }
    }
}