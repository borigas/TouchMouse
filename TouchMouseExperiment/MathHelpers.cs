using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchMouseExperiment
{
    public static class MathHelpers
    {
        public static double FindDirection(int x, int y)
        {
            return Math.Atan2(-1 * x, -1 * y) * 180 / Math.PI + 180;
        }

        public static double PythagoreanDistance(int x, int y)
        {
            return Math.Sqrt(x * x + y * y);
        }
    }
}
