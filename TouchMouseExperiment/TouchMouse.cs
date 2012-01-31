using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Microsoft.Research.TouchMouseSensor;
using System.Diagnostics;

namespace TouchMouseExperiment
{
    public delegate void TouchMouseGestureHandler(object sender, TouchMouseGestureEventArgs e);

    internal class TouchMouse
    {
        private const int SAMPLING_RATE = 100;

        private static TouchMouse _instance = null;
        public static TouchMouseGestureHandler OnLeftTap;
        public static TouchMouseGestureHandler OnRightTap;
        public static TouchMouseGestureHandler OnTwoFingerTap;
        public static TouchMouseGestureHandler OnThreeFingerTap;

        private DispatcherTimer _timer = null;
        private TouchImage _previousImage = null;
        private TouchMouseSensorEventArgs _args = null;
        private int _frameNumber = 0;

        private TouchMouse()
        {

            // Initialise the mouse and register the callback function
            TouchMouseSensorEventManager.Handler += this.TouchMouseSensorHandler;

            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(TimerTick);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, SAMPLING_RATE);
        }

        public static void Start()
        {
            _instance = new TouchMouse();
            _instance._timer.Start();
        }

        public static void Stop()
        {
            if (_instance != null)
            {
                _instance._timer.Stop();
                _instance._timer.Tick -= new EventHandler(_instance.TimerTick);
                TouchMouseSensorEventManager.Handler -= _instance.TouchMouseSensorHandler;
                _instance = null;
            }
        }

        void TouchMouseSensorHandler(object sender, TouchMouseSensorEventArgs e)
        {
            // We're in a thread belonging to the mouse, not the user interface 
            // thread. Need to dispatch to the user interface thread.
            _args = e;
        }


        private void TimerTick(object state, EventArgs e)
        {
            _frameNumber++;
            MainWindow.ClearMessage();

            if (_args == null)
                return;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            TouchImage currentImage = new TouchImage()
            {
                Height = (byte)_args.Status.m_dwImageHeight,
                Width = (byte)_args.Status.m_dwImageWidth,
                Image = _args.Image,
            };

            currentImage.FindTouchPoints();

            if (currentImage.TouchPoints.Count > 0)
            {
                currentImage.FindMovements(_previousImage);
            }

            // Check touchpoints that were not continued for gestures
            if (_previousImage != null)
            {
                _previousImage.CheckGesture();
            }

            _previousImage = currentImage;

            sw.Stop();
            Trace.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
            MainWindow.SetMessage("Elapsed: " + sw.ElapsedMilliseconds);

            bool hasPrinted = false;
            if (currentImage.TouchPoints.Count(x => x.Movement.Magnitude > Movement.MOVEMENT_THRESHOLD) > 0)
            {
                int movementCount = 0;
                foreach (var point in currentImage.TouchPoints)
                {
                    movementCount += point.Movements.Count;
                    //if (point.Gesture != null && point.Movement.Magnitude > Movement.MOVEMENT_THRESHOLD)//point.Gesture.LastOrDefault() == point.Movement)
                    //{
                    Trace.WriteLine(string.Format("{0}, {1}: {2}, {3}: {4}: {5}", point.FocalPointX, point.FocalPointY, point.Movement.XMovement, point.Movement.YMovement, point.Movement.Magnitude, point.TouchPointType));
                    Trace.WriteLine(string.Join<Movement>(", ", point.Movements));
                    Trace.WriteLine("*********************");
                    hasPrinted = true;
                }
            }
            if (hasPrinted)
            {
                System.Diagnostics.Trace.WriteLine("------------------- End Frame " + _frameNumber + " --------------------");
            }

            //SensorImage.Source = ti.GetSensorImage();
            //SensorImage.Source = ti.GetTouchPointImage();
            MainWindow.SetSensorImage(currentImage.GetTouchPointImageColored());

            _args = null;
        }
    }
}
