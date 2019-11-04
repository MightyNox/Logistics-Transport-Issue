using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Logistics_Transport_Issue.Structures;

namespace Logistics_Transport_Issue
{
    internal class Algorithm
    {
        public static void Calculate(int[] supply, int[] demand, int[,] costs)
        {
            //TODO synchronize with minimal matrix method
            var isFictionalReceiver = false;
            var isFictionalProducer = false;
            InsertFictionalSupplierOrReceiver(ref supply, ref demand, ref costs, ref isFictionalReceiver,
                ref isFictionalProducer);

            var distribution =
                MinimalElementMatrixMethod(supply, demand, costs, isFictionalReceiver, isFictionalProducer);

            Console.WriteLine();

            while (true)
            {
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
                else
                {
                    var cycle = FindCycle(deltas);
                    Console.WriteLine();
                    var minimum = GetDistributionCycleBasedMinimum(cycle, distribution);
                    Console.WriteLine("Minimum: " + minimum);


                    Console.WriteLine();
                    RecalculateDistribution(distribution, cycle, (int)minimum);
                }
            }
        }

        private static int? GetDistributionCycleBasedMinimum(List<Index> cycle, int[,] distribution)
        {
            int? min = null;
            foreach (var currentDistribution in cycle
                .Select(index => distribution[index.Row, index.Column])
                .Where((c, i) => i % 2 != 0))
            {
                if (min == null)
                    min = currentDistribution;
                else if (currentDistribution < min)
                    min = currentDistribution;
            }

            return min;
        }

        private static void InsertFictionalSupplierOrReceiver(ref int[] supply, ref int[] demand, ref int[,] costs,
            ref bool isFictionalReceiver, ref bool isFictionalProducer)
        {
            var supplySum = supply.Sum();
            var demandSum = demand.Sum();

            if (supplySum > demandSum)
            {
                demand = demand.Concat(new[] {supplySum - demandSum}).ToArray();
                var newCosts = new int[costs.GetLength(0), costs.GetLength(1) + 1];
                Array.Copy(costs, newCosts, costs.Length);
                costs = newCosts;
                isFictionalReceiver = true;
            }
            else if (supplySum < demandSum)
            {
                supply = supply.Concat(new[] {demandSum - supplySum}).ToArray();
                var newCosts = new int[costs.GetLength(0) + 1, costs.GetLength(1)];
                Array.Copy(costs, newCosts, costs.Length);
                costs = newCosts;
                isFictionalProducer = true;
            }
        }

        private static int[,] MinimalElementMatrixMethod(IList<int> supply, IList<int> demand, int[,] costs,
            bool isFictionalReceiver, bool isFictionalProducer)
        {
            var distribution = new int[costs.GetLength(0), costs.GetLength(1)];
            var tmpCosts = (int[,]) costs.Clone();
            if (isFictionalReceiver)
            {
                var tmpCostsLength = costs.GetLength(0);
                for (var i = 0; i < tmpCostsLength; i++)
                {
                    tmpCosts[i, tmpCostsLength - 1] = int.MaxValue - 1;
                }
            }
            else if (isFictionalProducer)
            {
                var tmpCostsLength = costs.GetLength(1);
                for (var i = 0; i < tmpCostsLength; i++)
                {
                    tmpCosts[tmpCostsLength - 1, i] = int.MaxValue - 1;
                }
            }

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

            alphas[0] = 0;
            while (alphas.Contains(null) || betas.Contains(null))
            {
                for (var row = 0; row < rows; row++)
                {
                    for (var column = 0; column < columns; column++)
                    {
                        if (distribution[row, column] == 0) continue;

                        if (alphas[row] != null && betas[column] == null)
                        {
                            betas[column] = costs[row, column] - alphas[row];
                        }
                        else if (betas[column] != null && alphas[row] == null)
                        {
                            alphas[row] = costs[row, column] - betas[column];
                        }
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

        private static int[,] CalculateDeltas(int[,] distribution, int[,] costs, IReadOnlyList<int> alphas,
            IReadOnlyList<int> betas)
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

        private static List<Index> FindCycle(int[,] deltas)
        {
            var indices = GetIndicesOfMinimum(deltas);
            var firstElementIndex = new Index((uint) indices[0], (uint) indices[1]);

            var cycles = new List<List<Index>>();

            var cycle = new List<Index> {firstElementIndex};
            var cycleUsedValues = new bool[deltas.GetLength(0), deltas.GetLength(1)];
            cycleUsedValues[firstElementIndex.Row, firstElementIndex.Column] = true;
            Go(cycles, firstElementIndex, Direction.None, cycle, deltas, cycleUsedValues);

            cycles = cycles.OrderBy(tmp_cycle => tmp_cycle.Count).ToList();
            cycle = cycles.First();

            Console.WriteLine("Cycle");
            var tmp = new bool[deltas.GetLength(0), deltas.GetLength(1)];
            foreach (var x in cycle)
            {
                tmp[x.Row, x.Column] = true;
            }

            for (var row = 0; row < deltas.GetLength(0); row++)
            {
                for (var column = 0; column < deltas.GetLength(1); column++)
                {
                    if (tmp[row, column] == true)
                    {
                        Console.Write("X ");
                    }
                    else
                    {
                        Console.Write(". ");
                    }
                }

                Console.WriteLine();
            }

            return cycle;
        }

        private static void Go(List<List<Index>> cycles, Index firstElementIndex, Direction direction,
            List<Index> cycle, int[,] deltas, bool[,] cycleUsedValues)
        {
            if (cycle.Count > deltas.Length)
                return;

            if (IsCycleFinished(cycle, firstElementIndex))
            {
                cycles.Add(cycle);
                return;
            }

            cycle = new List<Index>(cycle);
            cycleUsedValues = cycleUsedValues.Clone() as bool[,];
            if (AddIndexToCycle(direction, cycle, deltas, ref cycleUsedValues))
            {
                return;
            }

            Go(cycles, firstElementIndex, Direction.Left, cycle, deltas, cycleUsedValues);
            Go(cycles, firstElementIndex, Direction.Up, cycle, deltas, cycleUsedValues);
            Go(cycles, firstElementIndex, Direction.Right, cycle, deltas, cycleUsedValues);
            Go(cycles, firstElementIndex, Direction.Down, cycle, deltas, cycleUsedValues);
        }

        //TODO deltas rows and columns size has to be greater than 1
        //TODO When we cant move in the same direction 2 times
        private static bool AddIndexToCycle(Direction direction, List<Index> cycle, int[,] deltas,
            ref bool[,] cycleUsedValues)
        {
            if (direction == Direction.None)
                return false;

            var cycleLastElement = cycle.Last();
            if (direction == Direction.Left)
            {
                for (uint i = 0; i < cycleLastElement.Column; i++)
                {
                    if (deltas[cycleLastElement.Row, i] != 0) continue;

                    if (cycleUsedValues[cycleLastElement.Row, i]) continue;

                    cycleUsedValues[cycleLastElement.Row, i] = true;
                    cycle.Add(new Index(cycleLastElement.Row, i));
                    return false;
                }
            }

            if (direction == Direction.Down)
            {
                for (var i = cycleLastElement.Row + 1; i < deltas.GetLength(0); i++)
                {
                    if (deltas[i, cycleLastElement.Column] != 0) continue;


                    if (cycleUsedValues[i, cycleLastElement.Column]) continue;

                    cycleUsedValues[i, cycleLastElement.Column] = true;
                    cycle.Add(new Index(i, cycleLastElement.Column));
                    return false;
                }
            }

            if (direction == Direction.Right)
            {
                for (var i = cycleLastElement.Column + 1; i < deltas.GetLength(1); i++)
                {
                    if (deltas[cycleLastElement.Row, i] != 0) continue;

                    if (cycleUsedValues[cycleLastElement.Row, i]) continue;

                    cycleUsedValues[cycleLastElement.Row, i] = true;
                    cycle.Add(new Index(cycleLastElement.Row, i));
                    return false;
                }
            }

            if (direction == Direction.Up)
            {
                for (uint i = 0; i < cycleLastElement.Row; i++)
                {
                    if (deltas[i, cycleLastElement.Column] != 0) continue;

                    if (cycleUsedValues[i, cycleLastElement.Column]) continue;

                    cycleUsedValues[i, cycleLastElement.Column] = true;
                    cycle.Add(new Index(i, cycleLastElement.Column));
                    return false;
                }
            }

            return true;
        }

        private static bool IsCycleFinished(IReadOnlyCollection<Index> cycle, Index firstElementIndex)
        {
            if (cycle.Count < 4)
                return false;
            if (cycle.Count % 2 != 0)
                return false;
            if (cycle.Last().Row != firstElementIndex.Row && cycle.Last().Column != firstElementIndex.Column)
                return false;

            return true;
        }

        private static void RecalculateDistribution(int[,] distribution, List<Index> cycle, int minDistribution)
        {
            var oddIndexes = cycle.Where((c, i) => i % 2 != 0);
            var evenIndexes = cycle.Where((c, i) => i % 2 == 0);

            var zipped = oddIndexes.Zip(evenIndexes, (o, e) => new {Odd = o, Even = e});
            foreach (var index in zipped)
            {
                distribution[index.Odd.Row, index.Odd.Column] -= minDistribution;
                distribution[index.Even.Row, index.Even.Column] += minDistribution;
            }

            Console.WriteLine("New distribution");
            for (var row = 0; row < distribution.GetLength(0); row++)
            {
                for (var column = 0; column < distribution.GetLength(1); column++)
                {
                    Console.Write(distribution[row, column] + " ");
                }

                Console.WriteLine();
            }
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