using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TouchMouseExperiment
{
    internal class TouchImage
    {
        internal const byte TOUCH_THRESHOLD = 0xC0;
        internal const int VERTICAL_DUPLICATE_POINT_THRESHOLD = 4;
        internal static readonly TouchPointType[] VERTICAL_NOISE_POTENTIAL_TYPES = new TouchPointType[] { TouchPointType.LeftButton, TouchPointType.RightButton };

        internal byte[] Image { get; set; }
        internal byte Width { get; set; }
        internal byte Height { get; set; }

        internal List<TouchPoint> TouchPoints = new List<TouchPoint>();

        internal byte this[int x, int y]
        {
            get { return Image[x + y * Width]; }
            set { Image[x + y * Width] = value; }
        }

        internal void FindTouchPoints()
        {
            int i = 0;
            while (i + 1 < Image.Length)
            {
                // If left and 3 above bytes are 0, this tp hasn't been added yet
                if (Image[i] > 0 &&
                    (i - 1 < 0 || Image[i - 1] == 0) &&
                    (i - Width + 1 < 0 || Image[i - Width + 1] == 0) &&
                    (i - Width < 0 || Image[i - Width] == 0) &&
                    (i - Width - 1 < 0 || Image[i - Width - 1] == 0))
                {
                    TouchPoints.Add(TouchPoint.Create(this, i));
                }
                i++;
            }

            // Remove points that are in the main button area and are well below the others
            foreach (var touchType in VERTICAL_NOISE_POTENTIAL_TYPES)
            {
                var pointsInArea = TouchPoints.Count(x => x.TouchPointType == touchType);
                if (pointsInArea > 1)
                {
                    // Y is inverted
                    var lowPoint = TouchPoints.Min(x => x.FocalPointY);
                    var highestPointAllowed = lowPoint + VERTICAL_DUPLICATE_POINT_THRESHOLD;
                    TouchPoints.RemoveAll(x => x.TouchPointType == touchType && x.FocalPointY > highestPointAllowed);
                }

            }
        }

        internal ImageSource GetSensorImage()
        {
            return BitmapSource.Create(Width, Height, 96, 96,
                PixelFormats.Gray8, null, Image, Width);
        }

        internal ImageSource GetTouchPointImage()
        {
            byte[] processedImage = new byte[Image.Length];

            for (int i = 0; i < Image.Length; i++)
            {
                if (TouchPoints.Count(x => i % Width == x.FocalPointX && i / Width == x.FocalPointY) > 0)
                {
                    //processedImage[i] = Image[i];
                    processedImage[i] = 0xff;
                }
            }
            return BitmapSource.Create(Width, Height, 96, 96,
                PixelFormats.Gray8, null, processedImage, Width);
        }

        internal ImageSource GetTouchPointImageColored()
        {
            byte[] processedImage = new byte[3 * Image.Length];
            TouchPoint tp = null;
            for (int i = 0; i < Image.Length; i++)
            {
                if ((tp = TouchPoints.FirstOrDefault(x => i % Width == x.FocalPointX && i / Width == x.FocalPointY)) != null)
                {
                    switch (tp.TouchPointType)
                    {
                        //LeftButton = Blue
                        case TouchPointType.LeftButton:
                            processedImage[3 * i + 2] = Image[i];
                            break;
                        //RightButton = Green
                        case TouchPointType.RightButton:
                            processedImage[3 * i + 1] = Image[i];
                            break;
                        //LeftEdge = Red+Green
                        case TouchPointType.LeftEdge:
                            processedImage[3 * i + 0] = Image[i];
                            processedImage[3 * i + 1] = Image[i];
                            break;
                        //RightEdge = Blue+Green
                        case TouchPointType.RightEdge:
                            processedImage[3 * i + 1] = Image[i];
                            processedImage[3 * i + 2] = Image[i];
                            break;
                    }
                }
                else
                {
                    processedImage[3 * i] = Image[i];
                }
            }
            return BitmapSource.Create(Width, Height, 96, 96,
                PixelFormats.Rgb24, null, processedImage, Width * 3);
        }

        internal void FindMovements(TouchImage previousImage)
        {
            foreach (var point in TouchPoints)
            {
                point.CreateMovement(previousImage);
            }
        }

        internal void CheckGesture()
        {
            // Check ended touch points
            //var endedPoints = TouchPoints.Where(x => x.HasBeenFollowed == false);
            //foreach (var endedPoint in endedPoints)
            //{
            //}

            CheckForTaps();
        }

        private void CheckForTaps()
        {
            var taps = TouchPoints.Where(x => (x.TouchPointType == TouchPointType.LeftButton || x.TouchPointType == TouchPointType.RightButton)
                && x.Movements.Count == 0 && x.Movement.InactiveFrameCount <= Movement.INACTIVITY_FRAME_THRESHOLD
                && x.HasBeenFollowed == false);
            
            if (taps.Count() == 1)
            {
                if (taps.First().TouchPointType == TouchPointType.LeftButton && TouchMouse.OnLeftTap != null)
                {
                    TouchMouse.OnLeftTap(this, new TouchMouseGestureEventArgs()
                    {
                        TouchPoints = TouchPoints,
                        TriggeringTouchPoint = taps.First(),
                    });
                }
                else if(TouchMouse.OnRightTap != null)
                {
                    TouchMouse.OnRightTap(this, new TouchMouseGestureEventArgs()
                    {
                        TouchPoints = TouchPoints,
                        TriggeringTouchPoint = taps.First(),
                    });
                }
            }
            else if (taps.Count() == 2)
            {

            }
            else if (taps.Count() == 3)
            {

            }
        }
    }
}
