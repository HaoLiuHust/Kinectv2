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
namespace KinectMario
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinectsensor;
        private byte[] colorframedata;
        private WriteableBitmap colormap;

        private MultiSourceFrameReader msfreader;
        private FrameDescription cfd;
        private Int32Rect crect;
        private int stride;

        private Body[] bodies;
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

            colorframedata = new byte[cfd.LengthInPixels * cfd.BytesPerPixel];
            colormap = new WriteableBitmap(cfd.Width, cfd.Height, 96, 96, PixelFormats.Bgra32, null);
            crect = new Int32Rect(0, 0, cfd.Width, cfd.Height);
            stride = (int)(cfd.Width * cfd.BytesPerPixel);

            colorview.Source = colormap;


            msfreader.MultiSourceFrameArrived += msfreader_MultiSourceFrameArrived;
            try
            {
                kinectsensor.Open();
            }
            catch (System.IO.IOException)
            {

            }
        }

        private void closeKinect()
        {
            if (msfreader != null)
            {
                msfreader.Dispose();
                msfreader = null;
            }
            if (kinectsensor != null)
            {
                if (kinectsensor.IsOpen)
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
                        cframe.CopyConvertedFrameDataToArray(colorframedata, ColorImageFormat.Bgra);

                        colormap.WritePixels(crect, colorframedata, stride, 0);


                    }

                    using(BodyFrame bframe=msframe.BodyFrameReference.AcquireFrame())
                    {
                        tracktip.Visibility = Visibility.Hidden;
                        if(bframe!=null)
                        {
                            bodies = new Body[bframe.BodyCount];
                            bframe.GetAndRefreshBodyData(bodies);

                            Body closestBody = selectClosetbody(bodies);
                            if(closestBody!=null&&closestBody.IsTracked)
                            {
                                tracktip.Visibility = Visibility.Visible;
                                mappingKinect2Keyboard(closestBody);

                                //presentPPT(closestBody);
                            }
                            

                        }
                       
                    }
                }
            }
        }

        private const double ArmStretchedThreadhold = 0.3;
        private const double ArmRaisedThreadhold = 0.2;
        private const double JumpDiffThreadhold = 0.05;
        private double headPreviousPosition = 2.0;
        private void mappingKinect2Keyboard(Body body)
        {
            Joint head = body.Joints[JointType.Head];
            Joint leftShoulder = body.Joints[JointType.ShoulderLeft];
            Joint rightShoulder = body.Joints[JointType.ShoulderRight];
            Joint leftHand = body.Joints[JointType.HandLeft];
            Joint rightHand = body.Joints[JointType.HandRight];

            bool isRightHandRaised = (rightHand.Position.Y - rightShoulder.Position.Y) > ArmRaisedThreadhold;
            bool isLeftHandRasied = (leftHand.Position.Y - leftShoulder.Position.Y) > ArmRaisedThreadhold;

            bool isRightHandStretched = (rightHand.Position.X - rightShoulder.Position.X) > ArmStretchedThreadhold;
            bool isLeftHandStretched = (leftShoulder.Position.X - leftHand.Position.X) > ArmStretchedThreadhold;

            bool isJump = ((head.Position.Y - headPreviousPosition) > JumpDiffThreadhold);
            headPreviousPosition = head.Position.Y;

            if(isRightHandStretched)
            {
                KeyboardToolkit.Keyboard.Type(Key.Right);
            }
            else if(isLeftHandStretched)
            {
                KeyboardToolkit.Keyboard.Type(Key.Left);
            }
            if(isJump)
            {
                KeyboardToolkit.Keyboard.Type(Key.Up);
            }
            else if(isRightHandRaised&&isLeftHandRasied)
            {
                KeyboardToolkit.Keyboard.Type(Key.Down);
            }
        }


        private bool isForwardGestureActive = false;
        private bool isBackGestureActive = false;
        private bool isBlackScreenActive = false;
        private const double ArmStretchedThreadhold4PPT = 0.4;
        private void presentPPT(Body body)
        {
            Joint head = body.Joints[JointType.Head];
            Joint leftShoulder = body.Joints[JointType.ShoulderLeft];
            Joint rightShoulder = body.Joints[JointType.ShoulderRight];
            Joint leftHand = body.Joints[JointType.HandLeft];
            Joint rightHand = body.Joints[JointType.HandRight];

            bool isRightHandRaised = (rightHand.Position.Y - rightShoulder.Position.Y) > ArmRaisedThreadhold;
            bool isLeftHandRasied = (leftHand.Position.Y - leftShoulder.Position.Y) > ArmRaisedThreadhold;

            bool isRightHandStretched = (rightHand.Position.X - rightShoulder.Position.X) > ArmStretchedThreadhold;
            bool isLeftHandStretched = (leftShoulder.Position.X - leftHand.Position.X) > ArmStretchedThreadhold;

            if(isRightHandStretched)
            {
                if(!isBackGestureActive&&!isForwardGestureActive)
                {
                    isForwardGestureActive = true;
                    System.Windows.Forms.SendKeys.SendWait("{Right}");
                }
                
            }
            else
            {
                isForwardGestureActive = false;
            }

            if(isLeftHandStretched)
            {
                if (!isBackGestureActive && !isForwardGestureActive)
                {
                    isBackGestureActive = true;
                    System.Windows.Forms.SendKeys.SendWait("{Left}");
                }
            }
            else 
            {
                isBackGestureActive = false;   
            }

            if(isLeftHandRasied&&isRightHandRaised)
            {
                if(!isBlackScreenActive)
                {
                    isBlackScreenActive = true;
                    System.Windows.Forms.SendKeys.SendWait("{B}");
                }
            }
            else
            {
                isBlackScreenActive = false;
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
