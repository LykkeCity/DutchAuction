using System;

namespace DutchAuction.Core
{
    public static class DoubleExtensions
    {
        private const double Delta = 0.0000000000000001;

        public static bool IsApparentlyEquals(this double a, double b)
        {
            return Math.Abs(a - b) < Delta;
        }

        public static bool IsApparentlyLessOrEquals(this double a, double b)
        {
            return a < b || a.IsApparentlyEquals(b);
        }

        public static bool IsApparentlyGreateOrEquals(this double a, double b)
        {
            return a > b || a.IsApparentlyEquals(b);
        }
    }
}