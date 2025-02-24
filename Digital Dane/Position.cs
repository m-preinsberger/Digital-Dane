using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital_Dane
{
    public record Position(int? Left, int? Top)
    {
        // Manhattan Distance Method
        public int ManhattanDistance(Position other)
        {
            if (Left is null || Top is null || other.Left is null || other.Top is null)
                throw new InvalidOperationException("Both positions must have non-null coordinates.");

            return Math.Abs(Left.Value - other.Left.Value) + Math.Abs(Top.Value - other.Top.Value);
        }
    }
}
