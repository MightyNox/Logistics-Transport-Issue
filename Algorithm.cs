using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Logistics_Transport_Issue.Structures;

namespace Logistics_Transport_Issue
{
    internal class Algorithm
    {
        private static Document _reportFile;
        private static string _fileName;

        public static void Calculate(int[] supply, int[] demand, int[,] costs)
        {
            // Example 1
            //            demand = new[] {20, 40, 40};
            //            supply = new[] {32, 19, 27};
            //            costs = new[,] {{1, 4, 3}, {4, 5, 1}, {2, 6, 5}};

            // Example 2
            //            demand = new[] {90, 120, 150, 170};
            //            supply = new[] {100, 80, 50, 200};
            //            costs = new[,] {{2, 4, 5, 3}, {7, 1, 2, 5}, {4, 6, 7, 2}, {7, 8, 2, 1}};

            CreateReport();

            var isFictionalReceiver = false;
            var isFictionalProducer = false;
            InsertFictionalSupplierOrReceiver(ref supply, ref demand, ref costs, ref isFictionalReceiver,
                ref isFictionalProducer);

            var distribution =
                MinimalElementMatrixMethod(supply, demand, costs, isFictionalReceiver, isFictionalProducer);

            _reportFile.Add(new Paragraph("Total costs: " + CalculateTotalCosts(distribution, costs),
                    FontFactory.GetFont(FontFactory.DefaultEncoding, 15, BaseColor.RED))
                {Alignment = Element.ALIGN_CENTER});


            var iteration = 1;
            while (true)
            {
                _reportFile.Add(new Phrase(Chunk.NEWLINE));

                var iterationParagraph = new Paragraph("Iteration " + iteration,
                        FontFactory.GetFont(FontFactory.DefaultEncoding, 16))
                    {Alignment = Element.ALIGN_CENTER};
                _reportFile.Add(new Paragraph(iterationParagraph));
                _reportFile.Add(new Paragraph(" "));

                var alphasAndBetas = CalculateAlphaAndBeta(distribution, costs);

                var deltas = CalculateDeltas(distribution, costs, alphasAndBetas[0], alphasAndBetas[1]);


                if (!IsAnyValueNegative(deltas))
                {
                    _reportFile.Add(new Paragraph("Algorithm completed!",
                            FontFactory.GetFont(FontFactory.DefaultEncoding, 15))
                        {Alignment = Element.ALIGN_CENTER});
                    _reportFile.Add(new Paragraph(" "));
                    _reportFile.Add(new Paragraph("Total costs: " + CalculateTotalCosts(distribution, costs),
                            FontFactory.GetFont(FontFactory.DefaultEncoding, 15, BaseColor.GREEN))
                        {Alignment = Element.ALIGN_CENTER});
                    break;
                }

                var cycle = FindCycle(deltas);
                var minimum = GetDistributionCycleBasedMinimum(cycle, distribution);
                _reportFile.Add(new Paragraph("Minimum: " + minimum)
                    {Alignment = Element.ALIGN_CENTER});
                _reportFile.Add(new Paragraph(" "));


                RecalculateDistribution(distribution, cycle, (int) minimum);

                iteration++;
            }

            CloseReport();
        }

        private static void CloseReport()
        {
            _reportFile.Close();
            System.Diagnostics.Process.Start(_fileName);
        }

        private static void CreateReport()
        {
            _fileName = "report.pdf";
            File.Delete(_fileName);

            _reportFile = new Document();
            PdfWriter.GetInstance(_reportFile, new FileStream(_fileName, FileMode.Create));
            _reportFile.Open();

            var title = new Paragraph("Report", FontFactory.GetFont(FontFactory.DefaultEncoding, 30))
                {Alignment = Element.ALIGN_CENTER};

            _reportFile.Add(title);
            _reportFile.Add(Chunk.NEWLINE);
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

            //Save to the file
            var paragraph = new Paragraph("Minimal Matrix Method", FontFactory.GetFont(FontFactory.DefaultEncoding, 20))
                {Alignment = Element.ALIGN_CENTER};
            _reportFile.Add(paragraph);
            _reportFile.Add(new Paragraph(" "));

            var table = new PdfPTable(distribution.GetLength(1));

            var cell = new PdfPCell(new Phrase("Distribution"))
                {HorizontalAlignment = 1, Colspan = distribution.GetLength(1)};
            table.AddCell(cell);

            for (var i = 0; i < distribution.GetLength(0); i++)
            {
                for (var j = 0; j < distribution.GetLength(1); j++)
                {
                    cell = new PdfPCell(new Phrase(distribution[i, j].ToString())) {HorizontalAlignment = 1};
                    table.AddCell(cell);
                }
            }

            _reportFile.Add(table);
            _reportFile.Add(Chunk.NEWLINE);

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


            //Save to the file
            _reportFile.Add(new Paragraph(""));

            var table = new PdfPTable(alphas.Length + 1);
            var cell = new PdfPCell(new Phrase("Alphas")) {HorizontalAlignment = 1};
            table.AddCell(cell);
            for (var i = 0; i < rows; i++)
            {
                cell = new PdfPCell(new Phrase(alphas[i].ToString())) {HorizontalAlignment = 1};
                table.AddCell(cell);
            }

            _reportFile.Add(table);

            table = new PdfPTable(betas.Length + 1);
            cell = new PdfPCell(new Phrase("Betas")) {HorizontalAlignment = 1};
            table.AddCell(cell);
            for (var i = 0; i < columns; i++)
            {
                cell = new PdfPCell(new Phrase(betas[i].ToString())) {HorizontalAlignment = 1};
                table.AddCell(cell);
            }

            _reportFile.Add(table);
            _reportFile.Add(new Paragraph(" "));

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


            //Save to the file
            var table = new PdfPTable(columns);
            var cell = new PdfPCell(new Phrase("Deltas")) {HorizontalAlignment = 1, Colspan = columns};
            table.AddCell(cell);

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    if (deltas[i, j] == 0)
                    {
                        cell = new PdfPCell(new Phrase("X")) {HorizontalAlignment = 1};
                        table.AddCell(cell);
                    }
                    else
                    {
                        cell = new PdfPCell(new Phrase(deltas[i, j].ToString())) {HorizontalAlignment = 1};
                        table.AddCell(cell);
                    }
                }
            }

            _reportFile.Add(table);
            _reportFile.Add(new Paragraph(" "));

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
            Go(cycles, firstElementIndex, Direction.None, cycle, deltas, cycleUsedValues, Direction.None);

            cycles = cycles.OrderBy(tmp_cycle => tmp_cycle.Count).ToList();
            cycle = cycles.First();

            var tmp = new bool[deltas.GetLength(0), deltas.GetLength(1)];
            foreach (var x in cycle)
            {
                tmp[x.Row, x.Column] = true;
            }

            //Save to the file
            var table = new PdfPTable(deltas.GetLength(1));
            var cell = new PdfPCell(new Phrase("Cycle")) {HorizontalAlignment = 1, Colspan = deltas.GetLength(1)};

            var cycleArray = new string[deltas.GetLength(0), deltas.GetLength(1)];
            for (int m = 0; m < cycleArray.GetLength(0); m++)
            {
                for (int n = 0; n < cycleArray.GetLength(1); n++)
                {
                    cycleArray[m, n] = " ";
                }
            }

            var i = 0;
            foreach (var index in cycle)
            {
                i++;
                cycleArray[index.Row, index.Column] = i.ToString();
            }

            table.AddCell(cell);
            for (var row = 0; row < cycleArray.GetLength(0); row++)
            {
                for (var column = 0; column < cycleArray.GetLength(1); column++)
                {
                    cell = new PdfPCell(new Phrase(cycleArray[row, column])) {HorizontalAlignment = 1};
                    table.AddCell(cell);
                }
            }

            _reportFile.Add(table);
            _reportFile.Add(Chunk.NEWLINE);

            return cycle;
        }

        private static void Go(ICollection<List<Index>> cycles, Index firstElementIndex, Direction direction,
            List<Index> cycle, int[,] deltas, bool[,] cycleUsedValues, Direction previousDirection)
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
            if (AddIndexToCycle(direction, cycle, deltas, ref cycleUsedValues, ref previousDirection))
            {
                return;
            }

            Go(cycles, firstElementIndex, Direction.Left, cycle, deltas, cycleUsedValues, previousDirection);
            Go(cycles, firstElementIndex, Direction.Up, cycle, deltas, cycleUsedValues, previousDirection);
            Go(cycles, firstElementIndex, Direction.Right, cycle, deltas, cycleUsedValues, previousDirection);
            Go(cycles, firstElementIndex, Direction.Down, cycle, deltas, cycleUsedValues, previousDirection);
        }

        //TODO deltas rows and columns size has to be greater than 1
        private static bool AddIndexToCycle(Direction direction, ICollection<Index> cycle, int[,] deltas,
            ref bool[,] cycleUsedValues, ref Direction previousDirection)
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

                    if (previousDirection == Direction.Left) cycle.Remove(cycleLastElement);

                    cycleUsedValues[cycleLastElement.Row, i] = true;
                    cycle.Add(new Index(cycleLastElement.Row, i));
                    previousDirection = Direction.Left;
                    return false;
                }
            }

            if (direction == Direction.Down)
            {
                for (var i = cycleLastElement.Row + 1; i < deltas.GetLength(0); i++)
                {
                    if (deltas[i, cycleLastElement.Column] != 0) continue;


                    if (cycleUsedValues[i, cycleLastElement.Column]) continue;

                    if (previousDirection == Direction.Down) cycle.Remove(cycleLastElement);

                    cycleUsedValues[i, cycleLastElement.Column] = true;
                    cycle.Add(new Index(i, cycleLastElement.Column));
                    previousDirection = Direction.Down;
                    return false;
                }
            }

            if (direction == Direction.Right)
            {
                for (var i = cycleLastElement.Column + 1; i < deltas.GetLength(1); i++)
                {
                    if (deltas[cycleLastElement.Row, i] != 0) continue;

                    if (cycleUsedValues[cycleLastElement.Row, i]) continue;

                    if (previousDirection == Direction.Right) cycle.Remove(cycleLastElement);

                    cycleUsedValues[cycleLastElement.Row, i] = true;
                    cycle.Add(new Index(cycleLastElement.Row, i));
                    previousDirection = Direction.Right;
                    return false;
                }
            }

            if (direction == Direction.Up)
            {
                for (uint i = 0; i < cycleLastElement.Row; i++)
                {
                    if (deltas[i, cycleLastElement.Column] != 0) continue;

                    if (cycleUsedValues[i, cycleLastElement.Column]) continue;

                    if (previousDirection == Direction.Up) cycle.Remove(cycleLastElement);

                    cycleUsedValues[i, cycleLastElement.Column] = true;
                    cycle.Add(new Index(i, cycleLastElement.Column));
                    previousDirection = Direction.Up;
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

            //Save to the file
            var table = new PdfPTable(distribution.GetLength(1));

            var cell = new PdfPCell(new Phrase("Distribution v1"))
                {HorizontalAlignment = 1, Colspan = distribution.GetLength(1)};
            table.AddCell(cell);

            for (var row = 0; row < distribution.GetLength(0); row++)
            {
                for (var column = 0; column < distribution.GetLength(1); column++)
                {
                    var printed = false;
                    foreach (var index in zipped)
                    {
                        if (row == index.Odd.Row && column == index.Odd.Column)
                        {
                            printed = true;
                            cell = new PdfPCell(new Phrase(distribution[row, column] + " - " + minDistribution,
                                    FontFactory.GetFont(FontFactory.DefaultEncoding, Font.DEFAULTSIZE, BaseColor.BLUE)))
                                {HorizontalAlignment = 1};
                        }
                        else if (row == index.Even.Row && column == index.Even.Column)
                        {
                            printed = true;
                            cell = new PdfPCell(new Phrase(distribution[row, column] + " + " + minDistribution,
                                    FontFactory.GetFont(FontFactory.DefaultEncoding, Font.DEFAULTSIZE, BaseColor.RED)))
                                {HorizontalAlignment = 1};
                        }
                    }

                    if (!printed)
                    {
                        cell = new PdfPCell(new Phrase(distribution[row, column].ToString())) {HorizontalAlignment = 1};
                    }

                    table.AddCell(cell);
                }
            }

            _reportFile.Add(table);
            _reportFile.Add(Chunk.NEWLINE);

            //CALCULATE
            foreach (var index in zipped)
            {
                distribution[index.Odd.Row, index.Odd.Column] -= minDistribution;
                distribution[index.Even.Row, index.Even.Column] += minDistribution;
            }


            // Save v2
            table = new PdfPTable(distribution.GetLength(1));

            cell = new PdfPCell(new Phrase("Distribution v2"))
                {HorizontalAlignment = 1, Colspan = distribution.GetLength(1)};
            table.AddCell(cell);

            for (var row = 0; row < distribution.GetLength(0); row++)
            {
                for (var column = 0; column < distribution.GetLength(1); column++)
                {
                    cell = new PdfPCell(new Phrase(distribution[row, column].ToString())) {HorizontalAlignment = 1};
                    table.AddCell(cell);
                }
            }

            _reportFile.Add(table);
            _reportFile.Add(Chunk.NEWLINE);
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