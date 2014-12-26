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
using System.IO;

using Microsoft.Kinect;
namespace KinectChangeFace
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinectsensor;

        private MultiSourceFrameReader msfreader;
        private byte[] colordata;
        private Body[] bodies;

        private WriteableBitmap colorimage;
        private FrameDescription cfd;
        private Int32Rect crect;
        private int stride;
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
            msfreader = kinectsensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
            cfd = kinectsensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            colordata = new byte[cfd.LengthInPixels * cfd.BytesPerPixel];
            colorimage = new WriteableBitmap(cfd.Width, cfd.Height, 96, 96, PixelFormats.Bgra32, null);
            crect = new Int32Rect(0, 0, cfd.Width, cfd.Height);
            stride = (int)(cfd.Width * cfd.BytesPerPixel);

            colormap.Source = colorimage;
            
            
            msfreader.MultiSourceFrameArrived += msfreader_MultiSourceFrameArrived;
            try
            {
                kinectsensor.Open();
            }
            catch(System.IO.IOException)
            {
                
            }
        }

        private void closeKinect()
        {
            if(msfreader!=null)
            {
                msfreader.Dispose();
                msfreader = null;
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
        void msfreader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame msframe = e.FrameReference.AcquireFrame();
            if(msframe!=null)
            {
                using(ColorFrame cframe=msframe.ColorFrameReference.AcquireFrame())
                {
                    if(cframe!=null)
                    {
                        cframe.CopyConvertedFrameDataToArray(colordata, ColorImageFormat.Bgra);
                        colorimage.WritePixels(crect, colordata, stride, 0);

                        using(BodyFrame bframe=msframe.BodyFrameReference.AcquireFrame())
                        {
                            if(bframe!=null)
                            {
                                bodies = new Body[bframe.BodyCount];
                                bframe.GetAndRefreshBodyData(bodies);
                                
                                Body selectedBody = selectClosetbody(bodies);

                                changeFace(ref selectedBody);
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

        private int faceIndex=0;
        private void changeFace(ref Body body)
        {
            if (body != null && body.IsTracked)
            {
                Joint headJoint = body.Joints[JointType.Head];
                Joint lefthand = body.Joints[JointType.HandLeft];
                Joint righthand = body.Joints[JointType.HandRight];
                ColorSpacePoint csphead;
                ColorSpacePoint csphandleft;
                ColorSpacePoint csphandright;
                //headcanvas.Children.Clear();
                lefthandcanvas.Children.Clear();
                righthandcanvas.Children.Clear();


                

                if(headJoint.TrackingState == TrackingState.Tracked&&(righthand.TrackingState==TrackingState.Tracked||lefthand.TrackingState==TrackingState.Tracked))
                {
                    csphead = kinectsensor.CoordinateMapper.MapCameraPointToColorSpace(headJoint.Position);
                    if (lefthand.TrackingState == TrackingState.Tracked)
                    {
                        csphandleft = kinectsensor.CoordinateMapper.MapCameraPointToColorSpace(lefthand.Position);
                        Ellipse leftellipse = new Ellipse() { Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0)) };

                        lefthandcanvas.Children.Add(leftellipse);

                        Canvas.SetLeft(leftellipse, csphandleft.X / 2 - 10);
                        Canvas.SetTop(leftellipse, csphandleft.Y / 2 - 10);
                                                
                        if(Math.Abs(csphandleft.X-csphead.X)<50&&Math.Abs(csphead.Y-csphandleft.Y)<50)
                        {
                            headcanvas.Children.Clear();
                            faceIndex++;
                            faceIndex %= 4;
                            if (faceIndex == 0)
                            {

                                headimg.Source = null;
                                headcanvas.Visibility = Visibility.Hidden;
                            }
                            else
                            {

                                headcanvas.Visibility = Visibility.Visible;
                                string imagename = "faceimages/face001.png";
                                char tmp = faceIndex.ToString().ToCharArray().ElementAt(0);
                                imagename = imagename.Replace('1', tmp);

                                BitmapImage headpng = new BitmapImage(new Uri(imagename, UriKind.Relative));
                                headimg.Source = headpng;
                                headcanvas.Children.Add(headimg);


                            }
                        }
                        
                    }
                    
                    if (righthand.TrackingState == TrackingState.Tracked)
                    {
                        
                        csphandright = kinectsensor.CoordinateMapper.MapCameraPointToColorSpace(righthand.Position);
                        Ellipse rightellipse = new Ellipse() { Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0)) };

                        righthandcanvas.Children.Add(rightellipse);
                       
                        Canvas.SetLeft(rightellipse, csphandright.X / 2 - 10);
                        Canvas.SetTop(rightellipse, csphandright.Y / 2 - 10);
                        if (Math.Abs(csphandright.X - csphead.X) < 50 && Math.Abs(csphead.Y - csphandright.Y) < 50)
                        {
                            headcanvas.Children.Clear();
                            

                            faceIndex++;
                            faceIndex %= 4;
                            if (faceIndex == 0)
                            {
                                
                                headimg.Source = null;
                                headcanvas.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                
                                headcanvas.Visibility = Visibility.Visible;
                                string imagename = "faceimages/face001.png";
                                char tmp = faceIndex.ToString().ToCharArray().ElementAt(0);
                                imagename=imagename.Replace('1', tmp);

                                BitmapImage headpng = new BitmapImage(new Uri(imagename, UriKind.Relative));
                                headimg.Source = headpng;
                                headcanvas.Children.Add(headimg);
                                
                               
                            }
                        }
                        
                    }

                    Canvas.SetLeft(headimg, csphead.X / 2 - 40);
                    Canvas.SetTop(headimg, csphead.Y / 2 - 40);
                }
                
                else if(headJoint.TrackingState==TrackingState.NotTracked)
                {
                    headcanvas.Children.Clear();
                    headimg.Source = null;
                    headcanvas.Visibility = Visibility.Hidden;
                    lefthandcanvas.Children.Clear();
                    righthandcanvas.Children.Clear();
                }
            }
            else
            {
                headcanvas.Children.Clear();
                headimg.Source = null;
                headcanvas.Visibility = Visibility.Hidden;
                lefthandcanvas.Children.Clear();
                righthandcanvas.Children.Clear();
            }
        }
    }
}
