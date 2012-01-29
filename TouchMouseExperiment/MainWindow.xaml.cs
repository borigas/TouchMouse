using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.TouchMouseSensor;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace TouchMouseExperiment
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Ensure the image rendering does not interpolate
            RenderOptions.SetBitmapScalingMode(SensorImage, BitmapScalingMode.NearestNeighbor);

            // Initialise the mouse and register the callback function
            TouchMouseSensorEventManager.Handler += TouchMouseSensorHandler;

            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(TimerTick);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, SamplingRate);
            _timer.Start();
        }

        /// <summary>
        /// Handle callback from mouse.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        void TouchMouseSensorHandler(object sender, TouchMouseSensorEventArgs e)
        {
            // We're in a thread belonging to the mouse, not the user interface 
            // thread. Need to dispatch to the user interface thread.
            Dispatcher.Invoke((Action<TouchMouseSensorEventArgs>)SetSource, e);
        }

        private DispatcherTimer _timer = null;
        List<TouchImage> TouchImages = new List<TouchImage>();
        const int SamplingRate = 100;
        private TouchMouseSensorEventArgs _args = null;

        void SetSource(TouchMouseSensorEventArgs e)
        {
            _args = e;
        }

        private void TimerTick(object state, EventArgs e)
        {
            if (_args == null)
                return;

            TouchImage ti = new TouchImage()
            {
                Height = (byte)_args.Status.m_dwImageHeight,
                Width = (byte)_args.Status.m_dwImageWidth,
                Image = _args.Image,
            };

            ti.FindTouchPoints();

            if (ti.TouchPoints.Count > 0)
            {
                if (TouchImages.Count > 0)
                {
                    ti.FindGestures(TouchImages.Last());
                }
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

            foreach (var point in ti.TouchPoints)
            {
                Trace.WriteLine(string.Format("{0}, {1}", point.FocalPointX, point.FocalPointY));
                if (point.Gesture != null)
                {
                    Trace.WriteLine(string.Join<Movement>(", ", point.Gesture));
                }
            }
            System.Diagnostics.Trace.WriteLine("--------------------------");

            //SensorImage.Source = ti.GetSensorImage();
            //SensorImage.Source = ti.GetTouchPointImage();
            SensorImage.Source = ti.GetTouchPointImageColored();

            _args = null;
        }
    }
}
