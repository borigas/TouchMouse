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

    class TouchPoint
    {
        private const int LEFT_MARGIN = 2;
        private const int LEFT_THRESHOLD = 5;
        private const int RIGHT_MARGIN = 2;
        private const int MIDDLE_DIVIDER = 8;
        private const int INACTIVITY_FRAME_COUNT = 3;

        internal int Top { get; set; }
        internal int Bottom { get; set; }
        internal int Left { get; set; }
        internal int Right { get; set; }

        internal int FocalPointX { get; set; }
        internal int FocalPointY { get; set; }
        internal byte FocalPointValue { get; set; }

        internal TouchPointType TouchPointType { get; set; }

        internal List<Movement> Gesture { get; set; }

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
            if (FocalPointY - FocalPointX > LEFT_THRESHOLD)
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
                System.Diagnostics.Trace.WriteLine("Y:" + y + "should not be less than Top:" + Top);
            }

            System.Diagnostics.Debug.Assert(y >= Top, "Y:" + y + "should be >= than Top:" + Top);
        }

        internal MovementDirection FindMovementDirection(TouchPoint previousPoint)
        {
            MovementDirection direction;
            if (previousPoint == null)
            {
                direction = MovementDirection.New;
            }
            else if (FocalPointX - previousPoint.FocalPointX == 0 && FocalPointY - previousPoint.FocalPointY == 0)
            {
                direction = MovementDirection.None;
            }
            else
            {
                direction = (MovementDirection)(Math.Atan2(previousPoint.FocalPointX - FocalPointX, FocalPointY - previousPoint.FocalPointY) * 4 / Math.PI + 4);
            }

            return direction;
        }

        internal void CreateGesture(TouchImage previousImage)
        {
            TouchPoint previousPoint = previousImage.TouchPoints.FirstOrDefault(x => x.TouchPointType == this.TouchPointType);
            // Try checking points from button region if this point is a button
            if (previousPoint == null && (this.TouchPointType == TouchMouseExperiment.TouchPointType.LeftButton || this.TouchPointType == TouchMouseExperiment.TouchPointType.RightButton))
            {
                previousPoint = previousImage.TouchPoints.FirstOrDefault(x => x.TouchPointType == TouchMouseExperiment.TouchPointType.LeftButton || x.TouchPointType == TouchMouseExperiment.TouchPointType.RightButton);
            }


            if (previousPoint != null)
            {
                Movement movement = new Movement()
                {
                    Direction = FindMovementDirection(previousPoint),
                    Magnitude = Math.Sqrt(Math.Pow(previousPoint.FocalPointX - FocalPointX, 2) + Math.Pow(previousPoint.FocalPointY - FocalPointY, 2)),
                };

                Gesture = previousPoint.Gesture == null ? new List<Movement>() : previousPoint.Gesture;
                Gesture.Add(movement);

                CheckGesture();
            }
        }

        private void CheckGesture()
        {
            //throw new NotImplementedException();
        }
    }
}
