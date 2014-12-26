using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
namespace ColorStream
{
    public class ColorManger:Notifier
    {
        public WriteableBitmap Bitmap { get; private set; }
        public void Updata(ColorFrame frame)
        {
            var fd = frame.CreateFrameDescription(ColorImageFormat.Bgra);
            var imagedata = new byte[fd.BytesPerPixel * fd.LengthInPixels];
            frame.CopyConvertedFrameDataToArray(imagedata, ColorImageFormat.Bgra);
            if(Bitmap==null)
            {
                Bitmap = new WriteableBitmap(fd.Width, fd.Height, 96, 96, PixelFormats.Bgra32, null);
            }
            int stride = (int)(Bitmap.Width * fd.BytesPerPixel);
            Int32Rect rect = new Int32Rect(0, 0, fd.Width, fd.Height);
            Bitmap.WritePixels(rect, imagedata, stride, 0);

            RaisePropertyChanged<WriteableBitmap>(() => Bitmap);//监听Bitmap,用于数据绑定
        }
    }
}
