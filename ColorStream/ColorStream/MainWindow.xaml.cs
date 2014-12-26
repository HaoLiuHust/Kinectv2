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
namespace ColorStream
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    class Notifiert:Notifier
    {
        private string statustext = null;
        public string StatusText
        {
            get { return this.statustext; }
            set
            {
                if (this.statustext != value)
                {
                    this.statustext = value;

                    RaisePropertyChanged<string>(() => StatusText);
                }
            }
        }
    }
    public partial class MainWindow : Window
    {
        private KinectSensor kinectsensor;
        private ColorFrameReader colorFrameReader;
        public MainWindow()
        {
            StartKinect();
            //propertyChanged = new Notifiert();
            //colorManager = new ColorManger();
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if(colorManager!=null)
            this.kinectDisplay.DataContext = colorManager;
           
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseKinect();
        }

        
        Notifiert propertyChanged =new Notifiert();
        ColorManger colorManager=new ColorManger();
        
        private void StartKinect()
        {
                kinectsensor = KinectSensor.GetDefault();
                colorFrameReader = kinectsensor.ColorFrameSource.OpenReader();
                this.kinectsensor.IsAvailableChanged += kinectsensor_IsAvailableChanged;
                colorFrameReader.FrameArrived += colorFrameReader_FrameArrived;
                kinectsensor.Open();
                propertyChanged.StatusText = this.kinectsensor.IsAvailable ? Properties.Resources.RunningStatusText : Properties.Resources.NoSensorStatusText;
                if (propertyChanged.StatusText != null)
                    this.DataContext = propertyChanged;
                
        }

        private void CloseKinect()
        {
            if(colorFrameReader!=null)
            {
                colorFrameReader.Dispose();
                colorFrameReader = null;
            }

            if(kinectsensor!=null)
            {
                if(kinectsensor.IsOpen)
                {
                    kinectsensor.Close();
                    kinectsensor = null;
                }
            }
        }

        
        void colorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using(ColorFrame cframe=e.FrameReference.AcquireFrame())
            {
                if(cframe==null)
                {
                    return;
                }

                colorManager.Updata(cframe);
                
            }
        }

        void kinectsensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            propertyChanged.StatusText = this.kinectsensor.IsAvailable ? Properties.Resources.RunningStatusText : Properties.Resources.SensorNotAvaliableStatusText;
        }
    }
}
