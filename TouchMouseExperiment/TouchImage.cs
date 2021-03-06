﻿using System;
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
        internal const int TOUCH_MASS_THRESHOLD = 1000;
        internal const int VERTICAL_NOISE_POINT_THRESHOLD = 4;
        internal static readonly TouchPointType[] VERTICAL_NOISE_POTENTIAL_TYPES = new TouchPointType[] { TouchPointType.LeftButton, TouchPointType.RightButton };

        internal byte[] Image { get; set; }
        internal byte Width { get; set; }
        internal byte Height { get; set; }
        internal long FrameNumber { get; set; }

        internal TouchPoint CenterOfGravity { get; set; }

        internal List<TouchPoint> TouchPoints = new List<TouchPoint>();

        internal byte this[int x, int y]
        {
            get { return Image[x + y * Width]; }
            set { Image[x + y * Width] = value; }
        }

        internal void FindTouchPoints()
        {
            int xSum = 0;
            int ySum = 0;
            int cogPoints = 0;
            int i = 0;
            while (i < Image.Length)
            {
                // If left and 3 above bytes are 0, this tp hasn't been added yet
                if (Image[i] > 0 &&
                    (i - 1 < 0 || Image[i - 1] == 0) &&
                    (i - Width + 1 < 0 || Image[i - Width + 1] == 0) &&
                    (i - Width < 0 || Image[i - Width] == 0) &&
                    (i - Width - 1 < 0 || Image[i - Width - 1] == 0))
                {
                    var tp = TouchPoint.Create(this, i);
                    //System.Diagnostics.Trace.WriteLine("Mass: " + tp.Mass);
                    if (tp.Mass > TOUCH_MASS_THRESHOLD || !tp.IsButton())
                    {
                        TouchPoints.Add(TouchPoint.Create(this, i));
                    }
                }
                if (Image[i] != 0)
                {
                    ySum += i / Width;
                    xSum += i % Width;
                    cogPoints++;
                }
                i++;
            }

            if (cogPoints != 0)
            {
                CenterOfGravity = TouchPoint.Create(xSum / cogPoints, ySum / cogPoints);
                CenterOfGravity.TouchPointType = TouchPointType.CenterOfGravity;
            }

            // Remove points that are in the main button area and are well below the others
            foreach (var touchType in VERTICAL_NOISE_POTENTIAL_TYPES)
            {
                var pointsInArea = TouchPoints.Count(x => x.TouchPointType == touchType);
                if (pointsInArea > 1)
                {
                    // Y is inverted
                    var lowPoint = TouchPoints.Min(x => x.FocalPointY);
                    var highestPointAllowed = lowPoint + VERTICAL_NOISE_POINT_THRESHOLD;
                    TouchPoints.RemoveAll(x => x.TouchPointType == touchType && x.FocalPointY > highestPointAllowed);
                }

            }
        }

        internal static ImageSource GetEmptyImage()
        {
            int height = 15;
            int width = 13;
            
            return BitmapSource.Create(width, height, 96, 96,
                PixelFormats.Gray8, null, new byte[height * width], width);
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
            if (this.CenterOfGravity != null && previousImage != null)
            {
                this.CenterOfGravity.CreateMovement(previousImage.CenterOfGravity);
            }
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
            CheckForCenterOfGravityGesture();

            CheckForTaps();
        }

        private bool CheckForCenterOfGravityGesture()
        {
            // If there is multiple points
            bool eventOccurred = false;
            if (CenterOfGravity != null && CenterOfGravity.Movement != null)
            //if (TouchPoints.Count > 1 && CenterOfGravity != null && CenterOfGravity.Movement != null)
            {
                switch (CenterOfGravity.Movement.Direction)
                {
                    case MovementDirection.Up:
                        if (TouchMouse.OnCenterOfGravityRight != null)
                        {
                            eventOccurred = true;
                            TouchMouse.OnCenterOfGravityUp(this, new TouchMouseGestureEventArgs()
                                                                     {
                                                                         TouchPoints = TouchPoints,
                                                                         TriggeringTouchPoint = CenterOfGravity,
                                                                     });
                        }
                        break;
                    case MovementDirection.Down:
                        if (TouchMouse.OnCenterOfGravityRight != null)
                        {
                            eventOccurred = true;
                            TouchMouse.OnCenterOfGravityDown(this, new TouchMouseGestureEventArgs()
                                                                       {
                                                                           TouchPoints = TouchPoints,
                                                                           TriggeringTouchPoint = CenterOfGravity,
                                                                       });
                        }
                        break;
                    case MovementDirection.Right:
                        if (TouchMouse.OnCenterOfGravityRight != null)
                        {
                            eventOccurred = true;
                            TouchMouse.OnCenterOfGravityRight(this, new TouchMouseGestureEventArgs()
                                                                        {
                                                                            TouchPoints = TouchPoints,
                                                                            TriggeringTouchPoint = CenterOfGravity,
                                                                        });
                        }
                        break;
                    case MovementDirection.Left:
                        if (TouchMouse.OnCenterOfGravityRight != null)
                        {
                            eventOccurred = true;
                            TouchMouse.OnCenterOfGravityLeft(this, new TouchMouseGestureEventArgs()
                                                                       {
                                                                           TouchPoints = TouchPoints,
                                                                           TriggeringTouchPoint = CenterOfGravity,
                                                                       });
                        }
                        break;
                }
            }
            return eventOccurred;
        }

        private bool CheckForTaps()
        {
            bool tapped = false;
            var taps = TouchPoints.Where(x => x.IsButton() && x.HasBeenFollowed == false && x.Movements.Count == 0
                && x.Movement.InactiveMillis <= Movement.INACTIVITY_MILLIS_THRESHOLD);

            if (taps.Count() == 1)
            {
                if (taps.First().TouchPointType == TouchPointType.LeftButton && TouchMouse.OnLeftTap != null)
                {
                    tapped = true;
                    TouchMouse.OnLeftTap(this, new TouchMouseGestureEventArgs()
                    {
                        TouchPoints = TouchPoints,
                        TriggeringTouchPoint = taps.First(),
                    });
                }
                else if (TouchMouse.OnRightTap != null)
                {
                    tapped = true;
                    TouchMouse.OnRightTap(this, new TouchMouseGestureEventArgs()
                    {
                        TouchPoints = TouchPoints,
                        TriggeringTouchPoint = taps.First(),
                    });
                }
            }
            else if (taps.Count() == 2 && TouchMouse.OnTwoFingerTap != null)
            {
                tapped = true;
                TouchMouse.OnTwoFingerTap(this, new TouchMouseGestureEventArgs()
                {
                    TouchPoints = TouchPoints,
                    TriggeringTouchPoint = taps.First(),
                });
            }
            else if (taps.Count() == 3 && TouchMouse.OnThreeFingerTap != null)
            {
                tapped = true;
                TouchMouse.OnThreeFingerTap(this, new TouchMouseGestureEventArgs()
                {
                    TouchPoints = TouchPoints,
                    TriggeringTouchPoint = taps.First(),
                });
            }
            return tapped;
        }
    }
}
