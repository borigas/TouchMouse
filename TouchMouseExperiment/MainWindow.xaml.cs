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

            TouchMouse.Start();
        }

        internal static void SetSensorImage(ImageSource image)
        {
            _instance.SensorImage.Source = image;
        }
    }
}
