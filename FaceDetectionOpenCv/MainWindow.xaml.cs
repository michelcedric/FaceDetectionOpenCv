using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FaceDetectionOpenCv
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Frame = 2;
        private CascadeClassifier _frontFaceCascadeClassifier;
        private CascadeClassifier _profileFaceCascadeClassifier;
        private VideoCapture _capture;
        private Bitmap _cameraCapture;
        private int _counter = Frame;
        List<Rectangle> _rectangles = new List<Rectangle>();

        public MainWindow()
        {
            InitializeComponent();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CascadeClassifier");
            _frontFaceCascadeClassifier = new CascadeClassifier(Path.Combine(path, "haarcascade_frontalface_alt2.xml"));
            _profileFaceCascadeClassifier = new CascadeClassifier(Path.Combine(path, "haarcascade_profileface.xml"));

            _capture = new VideoCapture();
            _capture.ImageGrabbed += _capture_ImageGrabbed;
            _capture.Start();
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
                CameraCapture = grayframe.ToBitmap();
            }
            else
            {
                if (_rectangles.Any())
                {
                    foreach (var item in _rectangles)
                    {
                        grayframe.Draw(item, new Bgr(Color.Transparent), 0);
                    }
                    CameraCapture = grayframe.ToBitmap();
                    _rectangles = new List<Rectangle>();
                }
            }
            //little break to reduce cpu usage
            Thread.Sleep(5);
        }

        public Bitmap CameraCapture
        {
            get { return _cameraCapture; }
            set
            {
                _cameraCapture = value;
                imgCamUser.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { imgCamUser.Source = BitmapToImageSource(_cameraCapture); }));
            }
        }

        private void _captureTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //_captureTimer.Stop();
            using (var imageFrame = _capture.QueryFrame())
            {
                if (imageFrame != null)
                {
                    var grayframe = imageFrame.ToImage<Bgr, byte>();

                    var faces = _frontFaceCascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, System.Drawing.Size.Empty);
                    var faces2 = _profileFaceCascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, System.Drawing.Size.Empty);

                    foreach (var face in faces.Concat(faces2))
                    {
                        var rectangle = new Rectangle(face.X - 20, face.Y - 20, face.Width + 40, face.Height + 40);

                        //uncommmented to hide face but is not yet stable

                        Image<Bgr, Byte> temp = grayframe.Copy(rectangle); //copy the image data from the face
                        temp = temp.PyrDown().PyrDown().PyrDown().PyrUp().PyrUp().PyrUp(); //very simple blurring effect
                        grayframe.ROI = rectangle; //set the ROI to the same size as temp
                                                   /////if the program hangs here check to make sure the 
                                                   /////ImageFrame.ROI is the same size of temp.
                                                   //if (grayframe.ROI.Size.Equals(temp.Size))
                                                   //{
                                                   //CvInvoke.cvCopy(temp, grayframe, new IntPtr(0)); //copy the temp to the frame 
                                                   //}
                                                   //else
                                                   //{
                        temp = temp.Resize(grayframe.ROI.Size.Width, grayframe.ROI.Size.Height, Inter.Area);
                        CvInvoke.cvCopy(temp, grayframe, new IntPtr(0)); //copy the temp to the frame 
                        //}
                        grayframe.ROI = Rectangle.Empty;

                        grayframe.Draw(rectangle, new Bgr(Color.Transparent), 0);

                    }
                    CameraCapture = grayframe.ToBitmap();

                }
            }
            //_captureTimer.Start();
        }


        //Image<Bgr, Byte> temp = grayframe.Copy(rectangle); //copy the image data from the face
        //temp = temp.PyrDown().PyrUp(); //very simple blurring effect
        //grayframe.ROI = rectangle; //set the ROI to the same size as temp
        /////if the program hangs here check to make sure the 
        ///////ImageFrame.ROI is the same size of temp.
        //if (grayframe.ROI.Size.Equals(temp.Size))
        //{
        //    CvInvoke.cvCopy(temp, grayframe, new IntPtr(0)); //copy the temp to the frame 
        //}
        //else
        //{
        //    temp = temp.Resize(grayframe.ROI.Size.Width, grayframe.ROI.Size.Height, Inter.Area);
        //    CvInvoke.cvCopy(temp, grayframe, new IntPtr(0)); //copy the temp to the frame 
        //}
        //grayframe.ROI = Rectangle.Empty;

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
