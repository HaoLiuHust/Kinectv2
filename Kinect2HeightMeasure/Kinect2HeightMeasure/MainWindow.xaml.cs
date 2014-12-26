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

using Microsoft.Kinect;

namespace Kinect2HeightMeasure
{
    
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    
    
    
    public partial class MainWindow : Window
    {
        private KinectSensor kinectsensor;
        
        private ushort[] depthData;
        private byte[] converteddepth;
       

        private MultiSourceFrameReader msframereader;
        private WriteableBitmap depthmap;

        private const int MapDepthToByte = 8000 / 256;
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
            msframereader = kinectsensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth|FrameSourceTypes.BodyIndex);
            FrameDescription fd = kinectsensor.DepthFrameSource.FrameDescription;

            depthData = new ushort[fd.LengthInPixels];
            converteddepth = new byte[fd.LengthInPixels];
            depthmap = new WriteableBitmap(fd.Width, fd.Height, 96, 96, PixelFormats.Gray8, null);
            depthimg.Source = depthmap;

            msframereader.MultiSourceFrameArrived += msframereader_MultiSourceFrameArrived;
            if(kinectsensor!=null&&!kinectsensor.IsOpen)
            {
                kinectsensor.Open();
            }
        }

        private void closeKinect()
        {
            if(msframereader!=null)
            {
                msframereader.Dispose();
                msframereader = null;
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
        void msframereader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame msframe = e.FrameReference.AcquireFrame();
            if (msframe != null)
            {
                using (DepthFrame dframe = msframe.DepthFrameReference.AcquireFrame())
               
                {
                    if (dframe != null)
                    {
                        dframe.CopyFrameDataToArray(depthData);
                        var fd = dframe.FrameDescription;
                        for(int pos=0;pos<fd.LengthInPixels;pos++)
                        {
                        converteddepth[pos] = (byte)((depthData[pos] >= dframe.DepthMinReliableDistance && depthData[pos] <= dframe.DepthMaxReliableDistance) ? depthData[pos] / MapDepthToByte : 0);

                        }    
                        depthmap.WritePixels(new Int32Rect(0, 0, fd.Width, fd.Height), converteddepth, fd.Width, 0);
                            using (BodyIndexFrame biframe = msframe.BodyIndexFrameReference.AcquireFrame())
                            {
                                if (biframe != null)
                                {
                                    Byte[] biframedata = new Byte[fd.LengthInPixels];
                                    biframe.CopyFrameDataToArray(biframedata);
                                    if (biframedata.Min(value => value) <= 5&&biframedata.Min(value=>value)>=0)
                                    {
                                        double[] depthavg = new double[6];
                                        for (int j = 0; j < 6; ++j)
                                        {
                                            depthavg[j] = 0;

                                        }
                                        int closestbodyindex=-1;
                                        int count = 0;
                                        for (int row = 0; row < fd.Height; ++row)
                                        {
                                            for (int col = 0; col < fd.Width; ++col)
                                            {
                                                int pos = row * fd.Width + col;
                                                int index = biframedata[pos];
                                                if (index >= 0 && index <= 5)
                                                {
                                                    depthavg[index] += depthData[pos];
                                                    count++;
                                                }

                                                
                                            }
                                        }


                                        //找到最近的Body
                                        for (int i = 0; i < 6; ++i)
                                        {
                                            double mindepth = double.MaxValue;
                                            if (depthavg[i] < mindepth&&depthavg[i]!=0)
                                            {
                                                mindepth = depthavg[i];
                                                closestbodyindex = i;
                                            }
                                        }
                                        depthavg[closestbodyindex] /= (double)count;
                                        int Bodyleft = fd.Width;
                                        int Bodyright = 0;
                                        int Bodyup = fd.Height;
                                        int Bodybottom = 0;
                                        for (int row = 0; row < fd.Height; row++)
                                        {
                                            for (int col = 0; col < fd.Width; col++)
                                            {
                                                int pos = row * fd.Width + col;

                                                if (depthData[pos] != 0 && biframedata[pos] == closestbodyindex)
                                                {
                                                    Bodyleft = Math.Min(Bodyleft, col);
                                                    Bodyright = Math.Max(Bodyright, col);
                                                    Bodyup = Math.Min(Bodyup, row);
                                                    Bodybottom = Math.Max(Bodybottom, row);
                                                }
                                            }
                                        }
                                        //float viewAngleV = fd.VerticalFieldOfView;
                                        double viewAngleH = fd.HorizontalFieldOfView;
                                        viewAngleH = Math.PI * viewAngleH / 180.0;
                                        double bodyheight = 0;
                                        double widthperpixel = 2 * depthavg[closestbodyindex] * Math.Tan(viewAngleH / 2.0) / (double)fd.Width;
                                        bodyheight = widthperpixel * (Bodybottom - Bodyup);
                                        bodyheight /= 10.0;
                                        tiplabel.FontSize = 32;
                                        tiplabel.Content = "Your height is:";
                                        heightlabel.FontSize = 48;

                                        heightlabel.Content = bodyheight.ToString();


                                    }
                                    else
                                    {
                                        string tipstr = "No body here!";
                                        tiplabel.Content = null;
                                        heightlabel.FontSize = 48;
                                        heightlabel.Content = tipstr;
                                    }
                                }

                            
                        }

                       
                    }

                }
            }
        }

        private Body selectClosetbody(Body[] candidates)
        {
            var tmpbody = (from s in candidates
                           where s.IsTracked && s.Joints[JointType.SpineMid].TrackingState == TrackingState.Tracked
                           orderby s.Joints[JointType.SpineMid].Position.Z
                           select s).FirstOrDefault();

            return tmpbody;
        }
    }
}
