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
        private static MainWindow _instance = null;

        public MainWindow()
        {
            InitializeComponent();
            _instance = this;

            // Ensure the image rendering does not interpolate
            RenderOptions.SetBitmapScalingMode(SensorImage, BitmapScalingMode.NearestNeighbor);

            TouchMouse.OnLeftTap += (object sender, TouchMouseGestureEventArgs e) =>
            {
                SetMessage("Left Click for " + e.TriggeringTouchPoint.Movement.InactiveMillis / TouchMouse.SAMPLING_RATE + " Frames");
                InputHelper.LeftClick();
            };
            TouchMouse.OnRightTap += (object sender, TouchMouseGestureEventArgs e) =>
            {
                SetMessage("Right Click for " + e.TriggeringTouchPoint.Movement.InactiveMillis / TouchMouse.SAMPLING_RATE + " Frames");
                InputHelper.RightClick();
            };
            TouchMouse.OnTwoFingerTap += (object sender, TouchMouseGestureEventArgs e) =>
            {
                SetMessage("Left/Right Click for " + e.TouchPoints.Max(x => x.Movement.InactiveMillis + ((x.Movement.InactiveMillis <= Movement.INACTIVITY_MILLIS_THRESHOLD) ? 0 : int.MinValue)) / TouchMouse.SAMPLING_RATE + " Frames");
                InputHelper.MiddleClick();
            };
            TouchMouse.OnThreeFingerTap += (object sender, TouchMouseGestureEventArgs e) =>
            {
                SetMessage("3 Finger Click for " + e.TouchPoints.Max(x => x.Movement.InactiveMillis + ((x.Movement.InactiveMillis <= Movement.INACTIVITY_MILLIS_THRESHOLD) ? 0 : int.MinValue)) / TouchMouse.SAMPLING_RATE + " Frames");
                InputHelper.PlayPause();
            };


            TouchMouse.Start();
            SetMessage("Starting...");
        }

        internal static void SetSensorImage(ImageSource image)
        {
            _instance.SensorImage.Source = image;
        }

        internal static void SetMessage(string text)
        {
            _instance.Message.Text = text;
        }

        internal static void ClearMessage()
        {
            _instance.Message.Text = string.Empty;
        }
    }
}
