using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;


using Microsoft.Kinect;
namespace Kinectlocker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        extern static void LockWorkStation();

        private KinectSensor kinectsensor;
        private DepthFrameReader dfreader;

        private ushort[] depthData;
        private Byte[] converteddepth;
        private FrameDescription fd;

        private WriteableBitmap depthmap;

        private const int MapDepthToByte = 8000 / 256;
        private const int AverNum = 100;
        private const double LockThread = 300.0;

        readonly List<double> AverList=new List<double>();
    

       
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            closeKinect();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            startKinect();
            
        }



        private void startKinect()
        {
            kinectsensor = KinectSensor.GetDefault();
            
            dfreader = kinectsensor.DepthFrameSource.OpenReader();
             dfreader.FrameArrived += dfreader_FrameArrived;
             fd = dfreader.DepthFrameSource.FrameDescription;
             depthmap = new WriteableBitmap(fd.Width, fd.Height, 96, 96, PixelFormats.Gray8, null);
             depthData = new ushort[fd.LengthInPixels];
             converteddepth = new Byte[fd.LengthInPixels];
             depth.Source = depthmap;
            if (!kinectsensor.IsOpen)
                kinectsensor.Open();

           
               
        }

        private void closeKinect()
        {
            
            if(dfreader!=null)
            {
                dfreader.Dispose();
                dfreader = null;
            }
            if(kinectsensor!=null)
            {
                if(kinectsensor.IsOpen)
                {
                    kinectsensor.Close();
                }

                kinectsensor = null;
            }
        }
        void dfreader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            using(DepthFrame dframe=e.FrameReference.AcquireFrame())
            {
                if(dframe!=null)
                {
                    dframe.CopyFrameDataToArray(depthData);

                    for(int i=0;i<fd.LengthInPixels;i++)
                    {
                        converteddepth[i] = (byte) ((depthData[i] >= dframe.DepthMinReliableDistance && depthData[i] <= dframe.DepthMaxReliableDistance) ? depthData[i] / MapDepthToByte : 0);

                        
                    }
                    var avgvalue = depthData.Average(value => value);

                    AverList.Add(avgvalue);
                    var avglist = AverList.Average(value => value);

                    predepth.Content = avglist.ToString();
                    currdepth.Content = avgvalue.ToString();
                    if (AverList.Count > AverNum)
                    {
                        AverList.RemoveAt(0);
                    }

                    if (Math.Abs(avgvalue - avglist) > LockThread)
                    {
                        LockWorkStation();
                        AverList.Clear();
                    }
                    var stride=fd.Width;
                    depthmap.WritePixels(new Int32Rect(0, 0, fd.Width, fd.Height), converteddepth, stride, 0);
                }
            }
        }
    }
}
