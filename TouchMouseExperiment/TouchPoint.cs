using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchMouseExperiment
{
    class TouchPoint
    {
        internal int Top { get; set; }
        internal int Bottom { get; set; }
        internal int Left { get; set; }
        internal int Right { get; set; }

        internal int FocalPointX { get; set; }
        internal int FocalPointY { get; set; }
        internal byte FocalPointValue { get; set; }

        internal static TouchPoint Create(TouchImage touchImage, int i)
        {
            int x = i % touchImage.Width;
            int y = i / touchImage.Width;
            TouchPoint touchPoint = new TouchPoint()
            {
                Top = y,
                Bottom = y,
                Left = x,
                Right = x,
                FocalPointX = x,
                FocalPointY = y,
                FocalPointValue = touchImage[x, y],
            };

            touchPoint.FindAdjacentPoints(touchImage, x, y);

            return touchPoint;
        }

        private void FindAdjacentPoints(TouchImage image, int x, int y)
        {
            // Look right
            if (x + 1 < image.Width && image[x + 1, y] > 0)
            {
                AddValidPoint(image, x + 1, y);
            }

            // Look diagonal left-down if it hasn't been checked yet
            if (x == Left && x > 0 && y + 1 < image.Height && image[x - 1, y + 1] > 0)
            {
                AddValidPoint(image, x - 1, y + 1);
            }

            // Look down
            if (y + 1 < image.Height && image[x, y + 1] > 0)
            {
                AddValidPoint(image, x, y + 1);
            }

            // Look down and right
            if (x + 1 < image.Width && y + 1 < image.Height && image[x + 1, y + 1] > 0)
            {
                AddValidPoint(image, x + 1, y + 1);
            }
        }

        private void AddValidPoint(TouchImage image, int x, int y)
        {
            if (FocalPointValue < image[x, y])
            {
                FocalPointValue = image[x, y];
                FocalPointX = x;
                FocalPointY = y;
            }

            if (x > Right)
            {
                Right = x;
            }

            if (x < Left)
            {
                Left = x;
            }

            if (y > Bottom)
            {
                Bottom = y;
            }

            if (y < Top)
            {
                System.Diagnostics.Trace.WriteLine("Y=" + y + ", Top=" + Top);
            }

            //System.Diagnostics.Debug.Assert(y < Top, "y should not be less than Top");
            //System.Diagnostics.Debug.Assert(x < Left, "x should not be less than Left");
        }
    }
}
