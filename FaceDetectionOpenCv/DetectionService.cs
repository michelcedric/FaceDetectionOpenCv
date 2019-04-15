using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace FaceDetectionOpenCv
{
    public class DetectionService
    {
        private const int Frame = 2;
        private CascadeClassifier _frontFaceCascadeClassifier;
        private CascadeClassifier _profileFaceCascadeClassifier;
        private VideoCapture _capture;
        private int _counter = Frame;
        private List<Rectangle> _rectangles;

        public event EventHandler<Bitmap> NewImage;

        public DetectionService()
        {
            _rectangles = new List<Rectangle>();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CascadeClassifier");
            _frontFaceCascadeClassifier = new CascadeClassifier(Path.Combine(path, "haarcascade_frontalface_alt2.xml"));
            _profileFaceCascadeClassifier = new CascadeClassifier(Path.Combine(path, "haarcascade_profileface.xml"));
            _capture = new VideoCapture();
            _capture.ImageGrabbed += _capture_ImageGrabbed;
            _capture.Start();
        }

        protected virtual void OnNewImage(Bitmap bitmap)
        {
            NewImage?.Invoke(this, bitmap);
        }


        private void _capture_ImageGrabbed(object sender, EventArgs e)
        {
            _counter++;
            Mat imageFrame = new Mat();
            _capture.Retrieve(imageFrame);

            var grayframe = imageFrame.ToImage<Bgr, byte>();

            if (_counter % Frame == 0)
            {
                var frontFacesDetection = _frontFaceCascadeClassifier.DetectMultiScale(grayframe);
                var profileFaceDetection = _profileFaceCascadeClassifier.DetectMultiScale(grayframe);

                foreach (var faceDetection in frontFacesDetection.Concat(profileFaceDetection))
                {
                    var rectangle = new Rectangle(faceDetection.X - 25, faceDetection.Y - 25, faceDetection.Width + 50, faceDetection.Height + 50);
                    _rectangles.Add(rectangle);

                    grayframe.Draw(rectangle, new Bgr(Color.Transparent), 0);
                    _counter = 0;
                }
                OnNewImage(grayframe.ToBitmap());
                //CameraCapture = grayframe.ToBitmap();
            }
            else
            {
                if (_rectangles.Any())
                {
                    foreach (var item in _rectangles)
                    {
                        grayframe.Draw(item, new Bgr(Color.Transparent), 0);
                    }
                    OnNewImage(grayframe.ToBitmap());
                    _rectangles = new List<Rectangle>();
                }
            }
            //little break to reduce cpu usage
            Thread.Sleep(5);
        }

    }
}
