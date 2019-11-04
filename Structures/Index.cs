using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logistics_Transport_Issue.Structures
{
    internal class Index
    {
        public uint Row { get; }

        public uint Column { get; }

        public Index(uint row, uint column)
        {
            Row = row;
            Column = column;
        }
    }
}