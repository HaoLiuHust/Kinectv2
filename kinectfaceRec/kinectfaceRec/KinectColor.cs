using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace kinectfaceRec
{
    class KinectColor
    {
        public KinectSensor _kinectsensor;
        private ColorFrameReader _creader;
        public FrameDescription fd;
        private ColorFrame _cframe;

      
        public bool InitalKinect()
        {
            _kinectsensor = KinectSensor.GetDefault();
            if(_kinectsensor!=null)
            {
                _creader = _kinectsensor.ColorFrameSource.OpenReader();
                fd = _kinectsensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
                if(!_kinectsensor.IsOpen)
                  _kinectsensor.Open();
                return true;
            }
            return false;
        }

        public ColorFrame Update()
        {
            if (_kinectsensor.IsAvailable&&_creader!=null)
            {
                _cframe = _creader.AcquireLatestFrame();
                return _cframe;
            }
            else
                return null;
        }

        public void CloseKinect()
        {
            if(_creader!=null)
            {
                _creader.Dispose();
                _creader = null;
            }
            if(_kinectsensor!=null)
            {
                if(_kinectsensor.IsOpen)
                {
                    _kinectsensor.Close();
                }

                _kinectsensor = null;
            }
        }
    }
}
