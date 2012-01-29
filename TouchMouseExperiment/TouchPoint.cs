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

    enum TouchPointMovement
    {
        Up = 0,
        UpRight = 1,
        Right = 2,
        DownRight = 3,
        Down = 4,
        DownLeft = 5,
        Left = 6,
        UpLeft = 7,
        None = 8,
        New = 9,
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

        internal TouchPointMovement Movement { get; set; }

        internal TouchPointType TouchPointType { get; set; }

        internal List<TouchPointMovement> Gesture { get; set; }

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

        internal TouchPointMovement FindMovement(TouchPoint previousPoint)
        {
            TouchPointMovement movement;
            if (previousPoint == null)
            {
                movement = TouchPointMovement.New;
            }
            else if (FocalPointX - previousPoint.FocalPointX == 0 && FocalPointY - previousPoint.FocalPointY == 0)
            {
                movement = TouchPointMovement.None;
            }
            else
            {
                movement = (TouchPointMovement)(Math.Atan2(previousPoint.FocalPointX - FocalPointX, FocalPointY - previousPoint.FocalPointY) * 4 / Math.PI + 4);
            }

            return movement;
        }

        internal void CreateGesture(TouchImage previousImage)
        {
            TouchPoint previousPoint = previousImage.TouchPoints.FirstOrDefault(x => x.TouchPointType == this.TouchPointType);
            // Try checking points from button region if this point is a button
            if (previousPoint == null && (this.TouchPointType == TouchMouseExperiment.TouchPointType.LeftButton || this.TouchPointType == TouchMouseExperiment.TouchPointType.RightButton))
            {
                previousPoint = previousImage.TouchPoints.FirstOrDefault(x => x.TouchPointType == TouchMouseExperiment.TouchPointType.LeftButton || x.TouchPointType == TouchMouseExperiment.TouchPointType.RightButton);
            }

            Movement = FindMovement(previousPoint);

            if(Movement != TouchPointMovement.New)
            {
                // If previousPoint.Gesture.Count > GESTURE_MAX_FRAME_COUNT
                //Gesture = previousPoint.Gesture.GetRange(1, previousPoint.Gesture.Count - 1);
                // else
                Gesture = previousPoint.Gesture == null ? new List<TouchPointMovement>() : previousPoint.Gesture;
                
                // Don't add consequtive duplicate
                if (Gesture != null && Movement != Gesture.LastOrDefault())
                {
                    // Remove near duplicates (left, left-up, left => left)
                    if (Gesture.Count >= 2 && Movement == Gesture[Gesture.Count - 2] && Math.Abs(Movement - Gesture.LastOrDefault()) <= 1)
                    {
                        Gesture.RemoveAt(Gesture.Count - 1);
                    }
                    else
                    {
                        Gesture.Add(Movement);
                    }
                }
            }

            CheckGesture();
        }

        //private const 

        private void CheckGesture()
        {
            throw new NotImplementedException();
        }
    }
}
