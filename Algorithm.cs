using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logistics_Transport_Issue
{
    internal class Algorithm
    {
        public static void Calculate(int[] supply, int[] demand, int[,] costs)
        {
            InsertFictionalSupplierOrReceiver(ref supply, ref demand);

            dynamic distribution = MinimalElementMatrixMethod(supply, demand, costs);

            Console.WriteLine();

            CalculateAlphaAndBeta(distribution, costs);
        }

        private static void InsertFictionalSupplierOrReceiver(ref int[] supply, ref int[] demand)
        {
            var supplySum = supply.Length;
            var demandSum = demand.Length;

            if (supplySum > demandSum)
            {
                demand = demand.Concat(new[] { supplySum - demandSum }).ToArray();
            }
            else if (supplySum < demandSum)
            {
                supply = supply.Concat(new[] { demandSum - supplySum }).ToArray();
            }
        }

        public static int[,] MinimalElementMatrixMethod(int[] supply, int[] demand, int[,] costs)
        {
            dynamic distribution = new int[costs.GetLength(0), costs.GetLength(1)];
            dynamic tmpCosts = (int[,])costs.Clone();

            while (true)
            {
                var index = GetIndicesOfMinimum(tmpCosts);
                var row = index[0];
                var column = index[1];

                if (tmpCosts[row, column] == int.MaxValue) break;

                dynamic minimumDistribution = Math.Min(demand[column], supply[row]);
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

        public static void CalculateAlphaAndBeta(int[,] distribution, int[,] costs)
        {
            dynamic alphas = new int[costs.GetLength(0)];
            dynamic betas = new int[costs.GetLength(1)];

            alphas[0] = 0;
            for (var row = 0; row < distribution.GetLength(0); row++)
            {
                for (var column = 0; column < distribution.GetLength(1); column++)
                {
                    if (distribution[row, column] == 0)
                    {
                        continue;
                    }

                    if (alphas[row] != int.MaxValue)
                    {
                        betas[column] = costs[row, column] - alphas[row];
                    }
                    else
                    {
                        alphas[row] = costs[row, column] - betas[column];
                    }
                }
            }

            for (var i = 0; i < alphas.GetLength(0); i++)
            {
                Console.Write(alphas[i] + "\t");
            }

            Console.WriteLine();
            for (var i = 0; i < betas.GetLength(0); i++)
            {
                Console.Write(betas[i] + "\t");
            }

            Console.WriteLine();
        }

        public static int[] GetIndicesOfMinimum(int[,] array)
        {
            var minValue = double.PositiveInfinity;
            var minFirstIndex = -1;
            var minSecondIndex = -1;

            for (var i = array.GetLength(0) - 1; i >= 0; --i)
            {
                for (var j = array.GetLength(1) - 1; j >= 0; --j)
                {
                    double value = array[i, j];

                    if (!(value <= minValue)) continue;

                    minFirstIndex = i;
                    minSecondIndex = j;

                    minValue = value;
                }
            }

            return new[] { minFirstIndex, minSecondIndex };
        }
    }
}