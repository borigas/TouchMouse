using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchMouseExperiment
{
    enum TouchPointType
    {
        Unknown,
        LeftButton,
        RightButton,
        LeftEdge,
        RightEdge,
    }

    public class TouchPoint
    {
        private const int LEFT_MARGIN = 2;
        private const int LEFT_THUMB_THRESHOLD = 5;
        private const int RIGHT_MARGIN = 2;
        private const int MIDDLE_DIVIDER = 8;

        internal int Top { get; set; }
        internal int Bottom { get; set; }
        internal int Left { get; set; }
        internal int Right { get; set; }

        public int FocalPointX { get; set; }
        public int FocalPointY { get; set; }
        public byte FocalPointValue { get; set; }

        internal bool HasBeenFollowed = false;
        internal bool HasBeenProcessed = false;

        internal Movement Movement { get; set; }

        internal TouchPointType TouchPointType { get; set; }

        internal List<Movement> Movements { get; set; }

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
            touchPoint.FindType(touchImage.Width);

            return touchPoint;
        }

        private void FindType(int width)
        {
            if (FocalPointY - FocalPointX > LEFT_THUMB_THRESHOLD)
            //if (FocalPointX < LEFT_MARGIN)
            {
                TouchPointType = TouchPointType.LeftEdge;
            }
            else if (FocalPointX >= width - RIGHT_MARGIN)
            {
                TouchPointType = TouchPointType.RightEdge;
            }
            else if (FocalPointX < MIDDLE_DIVIDER)
            {
                TouchPointType = TouchPointType.LeftButton;
            }
            else
            {
                TouchPointType = TouchPointType.RightButton;
            }
        }

        private void FindAdjacentPoints(TouchImage image, int x, int y)
        {
            // Look right
            if (x + 1 < image.Width && image[x + 1, y] > 0)
            {
                AddValidPoint(image, x + 1, y);
            }

            // Look left
            if (x == Left && x > 0 && image[x - 1, y] > 0)
            {
                AddValidPoint(image, x - 1, y);
            }

            // Look diagonal left-down if it hasn't been checked yet
            //if (x == Left && x > 0 && y + 1 < image.Height && image[x - 1, y + 1] > 0)
            //{
            //    AddValidPoint(image, x - 1, y + 1);
            //}

            // Look down
            if (y + 1 < image.Height && image[x, y + 1] > 0)
            {
                AddValidPoint(image, x, y + 1);
            }

            // Look down and right
            //if (x + 1 < image.Width && y + 1 < image.Height && image[x + 1, y + 1] > 0)
            //{
            //    AddValidPoint(image, x + 1, y + 1);
            //}
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
                System.Diagnostics.Trace.WriteLine("Y:" + y + "should not be less than Top:" + Top);
            }

            System.Diagnostics.Debug.Assert(y >= Top, "Y:" + y + "should be >= than Top:" + Top);
        }

        internal void CreateMovement(TouchImage previousImage)
        {
            TouchPoint previousPoint = null;
            if (previousImage != null)
            {
                //previousPoint = previousImage.TouchPoints.Where(x => x.TouchPointType == this.TouchPointType).OrderBy(x => this.DistanceTo(x)).FirstOrDefault();
                previousPoint = previousImage.TouchPoints.FirstOrDefault(x => x.TouchPointType == this.TouchPointType && !x.HasBeenFollowed);
                // Try checking points from button region if this point is a button
                if (previousPoint == null && (this.TouchPointType == TouchMouseExperiment.TouchPointType.LeftButton || this.TouchPointType == TouchMouseExperiment.TouchPointType.RightButton))
                {
                    //previousPoint = previousImage.TouchPoints.Where(x => x.TouchPointType == TouchMouseExperiment.TouchPointType.LeftButton || x.TouchPointType == TouchMouseExperiment.TouchPointType.RightButton).OrderBy(x => this.DistanceTo(x)).FirstOrDefault();
                    previousPoint = previousImage.TouchPoints.FirstOrDefault(x => !x.HasBeenFollowed && (x.TouchPointType == TouchMouseExperiment.TouchPointType.LeftButton || x.TouchPointType == TouchMouseExperiment.TouchPointType.RightButton));
                }
            }

            if (previousPoint != null)
            {
                previousPoint.HasBeenFollowed = true;
            }

            if (previousPoint != null && previousPoint.Movements != null)
            {
                Movements = previousPoint.Movements;
            }
            else
            {
                Movements = new List<Movement>();
            }

            Movement = Movement.Create(previousPoint, this);
            if (Movement.Direction != MovementDirection.None && Movement.Direction != MovementDirection.New)
            {
                Movements.Add(Movement);
            }
        }

        internal double DistanceTo(TouchPoint otherPoint)
        {
            return MathHelpers.PythagoreanDistance(FocalPointX - otherPoint.FocalPointX, FocalPointY - otherPoint.FocalPointY);
        }
    }
}
