using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Microsoft.Research.TouchMouseSensor;
using System.Diagnostics;

namespace TouchMouseExperiment
{
    internal class TouchMouse
    {
        private static TouchMouse _instance = null;

        private DispatcherTimer _timer = null;
        private List<TouchImage> TouchImages = new List<TouchImage>();
        private const int SamplingRate = 100;
        private TouchMouseSensorEventArgs _args = null;

        private TouchMouse()
        {

            // Initialise the mouse and register the callback function
            TouchMouseSensorEventManager.Handler += this.TouchMouseSensorHandler;

            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(TimerTick);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, SamplingRate);
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
                _instance.TouchImages.Clear();
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
            if (_args == null)
                return;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            TouchImage ti = new TouchImage()
            {
                Height = (byte)_args.Status.m_dwImageHeight,
                Width = (byte)_args.Status.m_dwImageWidth,
                Image = _args.Image,
            };

            ti.FindTouchPoints();

            if (ti.TouchPoints.Count > 0)
            {
                ti.FindGestures(TouchImages.LastOrDefault());
                TouchImages.Add(ti);
            }
            else
            {
                // Touch Gesture over
                //if (TouchImages.Count > 0 && TouchImages.Count < 3)
                //{
                //    MouseHelper.LeftClick();
                //}

                TouchImages.Clear();
            }

            sw.Stop();
            Trace.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);

            bool hasPrinted = false;
            foreach (var point in ti.TouchPoints)
            {
                if (point.Gesture != null && point.Movement.Magnitude > Movement.MOVEMENT_THRESHOLD)//point.Gesture.LastOrDefault() == point.Movement)
                {
                Trace.WriteLine(string.Format("{0}, {1}: {2}, {3}: {4}: {5}", point.FocalPointX, point.FocalPointY, point.Movement.XMovement, point.Movement.YMovement, point.Movement.Magnitude, point.TouchPointType));
                    Trace.WriteLine(string.Join<Movement>(", ", point.Gesture));
                Trace.WriteLine("*********************");
                hasPrinted = true;
                }
            }
            if (hasPrinted)
            {
                System.Diagnostics.Trace.WriteLine("----------------------------------------------------");
            }

            //SensorImage.Source = ti.GetSensorImage();
            //SensorImage.Source = ti.GetTouchPointImage();
            MainWindow.SetSensorImage(ti.GetTouchPointImageColored());

            _args = null;
        }
    }
}
