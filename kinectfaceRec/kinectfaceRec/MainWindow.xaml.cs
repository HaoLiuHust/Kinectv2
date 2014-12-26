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

using System.Threading;
using System.IO;
using Microsoft.Kinect;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.UI;
namespace kinectfaceRec
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Byte[] cframedata;
        ColorFrame cframe;
        FrameDescription fd;
        int width;
        int height;
        kinectfaceRec.KinectColor kinectcolor;
        bool isrun = false;

        Image<Bgra, byte> cimage;
        ImageViewer imageview;

        WriteableBitmap colormap;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.KeyDown += MainWindow_KeyDown;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Tab: StartTrack(); 
                    Thread thread = new Thread(new ThreadStart(FaceRec)); thread.Start(); 
                    break;
                default:break;
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            width = 0;
            height = 0;
            
        }
        [DllImport("facedetector.dll",CallingConvention=CallingConvention.Cdecl)]
        public static extern void facedetector( Byte[] img,int scalewidth,int width,int height);
        
        void StartTrack()
        {
            kinectcolor = new KinectColor();
            
            if (!kinectcolor.InitalKinect())
            {
                MessageBox.Show("Kinect启动失败");
                Application.Current.Shutdown();
            }

            isrun = true;
            fd = kinectcolor.fd;
            cframedata = new byte[fd.LengthInPixels * fd.BytesPerPixel];
            width = fd.Width;
            height = fd.Height;

            cimage = new Image<Bgra, byte>(fd.Width, fd.Height,new Bgra(0,0,0,0));
            imageview = new ImageViewer();
            imageview.Size = new System.Drawing.Size(width, height);
            imageview.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            imageview.WindowState = System.Windows.Forms.FormWindowState.Normal;
            imageview.ImageBox.FunctionalMode = ImageBox.FunctionalModeOption.Minimum;
            colormap = new WriteableBitmap(fd.Width, fd.Height, 96, 96, PixelFormats.Bgr32, null);
            image.Source = colormap;
            imageview.KeyDown += imageview_KeyDown;

            //imageview.Image = cimage;
            imageview.Show();
            //FaceRec();
            
        }

        void imageview_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case System.Windows.Forms.Keys.Escape: isrun = !isrun; break;
                default: break;
            }
        }
        void FaceRec()
        {            
            while(isrun)
            {                
               using(cframe=kinectcolor.Update())
               {
                   if (cframe != null)
                   {
                       
                       cframe.CopyConvertedFrameDataToArray(cframedata, ColorImageFormat.Bgra);
                       cimage.Bytes = cframedata;
                       Image<Bgr, byte> bgrimage = cimage.Convert<Bgr, byte>();
                       Byte[] imageptr = bgrimage.Bytes;
                       facedetector(imageptr, 480, width, height);

                       bgrimage.Bytes = imageptr;
                       //CvInvoke.cvShowImage("test",bgrimage);
                       imageview.Image = bgrimage;
                       //imageview.Show();
                       //Dispatcher.Invoke(new Action(() => Updatadisplay(cframedata)));
                   }
               }                                
            }
            imageview.Dispose();
            imageview.Close();
            kinectcolor.CloseKinect();
        }

        void Updatadisplay(Byte[] data)
        {
            colormap.WritePixels(new Int32Rect(0, 0, 1920, 1080), cframedata, fd.Width * 4, 0);
            image.Source = colormap;
            //CvInvoke.cvShowImage("test", cimage);
            
        }
    }
}
