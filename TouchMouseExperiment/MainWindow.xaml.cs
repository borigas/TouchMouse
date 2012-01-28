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

        List<TouchImage> TouchImages = new List<TouchImage>();
        int ElapsedMillis = 0;
        const int ElapsedThreshold = 100;

        void SetSource(TouchMouseSensorEventArgs e)
        {
            if (ElapsedMillis > ElapsedThreshold)
            {
                // Convert bitmap from memory to graphic form.
                //BitmapSource source =
                //    BitmapSource.Create(e.Status.m_dwImageWidth, e.Status.m_dwImageHeight,
                //    96, 96,
                //    PixelFormats.Gray8, null, e.Image, e.Status.m_dwImageWidth);

                //Bitmap
                // Show bitmap in user interface.

                TouchImage ti = new TouchImage()
                {
                    Height = (byte)e.Status.m_dwImageHeight,
                    Width = (byte)e.Status.m_dwImageWidth,
                    Image = e.Image,
                };

                Stopwatch sw = new Stopwatch();
                sw.Start();
                ti.FindTouchPoints();
                sw.Stop();

                if (ti.TouchPoints.Count > 0)
                {
                    TouchImages.Add(ti);
                }
                else
                {
                    TouchImages.Clear();
                }

                System.Diagnostics.Trace.WriteLine("Elapsed: " + sw.ElapsedMilliseconds + ", Between Events: " + e.Status.m_dwTimeDelta);
                foreach (var point in ti.TouchPoints)
                {
                    System.Diagnostics.Trace.WriteLine(string.Format("{0}, {1}: {2}", point.FocalPointX, point.FocalPointY, point.FocalPointValue));
                }
                System.Diagnostics.Trace.WriteLine("--------------------------");

                //SensorImage.Source = ti.GetSensorImage();
                //SensorImage.Source = ti.GetTouchPointImage();
                SensorImage.Source = ti.GetTouchPointImageColored();

                ElapsedMillis = 0;
            }
            else
            {
                ElapsedMillis += e.Status.m_dwTimeDelta;
            }
        }

        private BitmapSource ProcessImage2(byte[] originalImage, int width, int height)
        {
            byte[] processedImage = new byte[originalImage.Length * 3];
            int[] currentTouchPoint = null;
            List<int[]> touchPoints = new List<int[]>();
            for (int i = 0; i < originalImage.Length; i++)
            {
                processedImage[3 * i] = originalImage[i];

                if (originalImage[i] > 0)
                {
                    if (!HasBeenChecked(originalImage, i % width, i / width, width))
                    {
                        currentTouchPoint = FindMainTouchPoint(originalImage, i % width, i / width, width, height);
                        touchPoints.Add(currentTouchPoint);
                        int touchPointIndex = GetByteIndex(currentTouchPoint[0], currentTouchPoint[1], width);
                        processedImage[3 * touchPointIndex + 1] = Convert.ToByte(currentTouchPoint[2]);
                    }
                }
            }
            foreach (var points in touchPoints)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("{0}, {1}: {2}", points[0], points[1], points[2]));
            }
            System.Diagnostics.Trace.WriteLine("--------------------------");

            //return BitmapSource.Create(width, height, 96, 96,
            //    PixelFormats.Indexed8, BitmapPalettes.Gray256, originalImage, width);

            return BitmapSource.Create(width, height, 96, 96,
                PixelFormats.Rgb24, null, processedImage, width * 3);
        }

        private int[] FindMainTouchPoint(byte[] image, int x, int y, int width, int height)
        {
            int left = x;
            int right = x;
            int top = y;
            int bottom = y;

            int currentX = x;
            int currentY = y;

            int maxX = x;
            int maxY = y;
            byte maxValue = image[GetByteIndex(currentX, currentY, width)];

            while (HasValueBelow(image, width, height, currentX, currentY))
            {
                while (HasValueRight(image, width, currentX, currentY))
                {
                    currentX++;
                    if (image[GetByteIndex(currentX, currentY, width)] > maxValue)
                    {
                        maxX = currentX;
                        maxY = currentY;
                        maxValue = image[GetByteIndex(currentX, currentY, width)];
                        if (maxValue == Byte.MaxValue)
                        {
                            return new int[] { maxX, maxY, maxValue };
                        }
                    }
                }
                currentY++;
            }
            return new int[] { maxX, maxY, maxValue };
        }

        // Checks all 3 adjacent lower squares
        private static bool HasValueBelow(byte[] image, int width, int height, int x, int y)
        {
            return y < height - 1 &&
                (image[GetByteIndex(x, y + 1, width)] > 0
                    || HasValueLeft(image, width, x, y + 1)
                    || HasValueRight(image, width, x, y + 1));
        }

        private static bool HasValueRight(byte[] image, int width, int x, int y)
        {
            return x < width - 1 && image[GetByteIndex(x + 1, y, width)] > 0;
        }

        private static bool HasValueLeft(byte[] image, int width, int x, int y)
        {
            return x > 0 && image[GetByteIndex(x - 1, y, width)] > 0;
        }

        private static int GetByteIndex(int x, int y, int width)
        {
            return width * y + x;
        }

        private static bool HasBeenChecked(byte[] image, int x, int y, int width)
        {
            // If left pixel or 3 above pixels were > 0, this was already checked

            // check left
            if (x > 0 && y >= 0 && image[(x - 1) + y * width] > 0)
            {
                return true;
            }
            // check diagonal up and left
            else if (x > 0 && y > 0 && image[(x - 1) + (y - 1) * width] > 0)
            {
                return true;
            }
            // check up
            else if (x >= 0 && y > 0 && image[x + (y - 1) * width] > 0)
            {
                return true;
            }
            // check diagonal up and right
            else if (x + 1 < width && y >= 0 && image[(x + 1) + (y - 1) * width] > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
