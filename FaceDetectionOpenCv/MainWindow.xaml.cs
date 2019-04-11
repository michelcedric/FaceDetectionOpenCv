using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace FaceDetectionOpenCv
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer _captureTimer;
        private CascadeClassifier _cascadeClassifier;
        private CascadeClassifier _cascadeClassifier2;
        private VideoCapture _capture;
        private Bitmap _cameraCapture;

        public MainWindow()
        {
            InitializeComponent();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CascadeClassifier");
            _cascadeClassifier = new CascadeClassifier(Path.Combine(path, "haarcascade_frontalface_alt2.xml"));
            _cascadeClassifier2 = new CascadeClassifier(Path.Combine(path, "haarcascade_profileface.xml"));
            _capture = new VideoCapture();
            _capture.SetCaptureProperty(CapProp.Fps, 30);
            _capture.SetCaptureProperty(CapProp.FrameHeight, 450);
            _capture.SetCaptureProperty(CapProp.FrameWidth, 370);
            _captureTimer = new Timer(75);
            _captureTimer.Elapsed += _captureTimer_Elapsed;
            _captureTimer.Start();
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
            _captureTimer.Stop();
            using (var imageFrame = _capture.QueryFrame())
            {
                if (imageFrame != null)
                {
                    var grayframe = imageFrame.ToImage<Bgr, byte>();

                    var faces = _cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, System.Drawing.Size.Empty);
                    var faces2 = _cascadeClassifier2.DetectMultiScale(grayframe, 1.1, 10, System.Drawing.Size.Empty);

                    foreach (var face in faces.Concat(faces2))
                    {
                        var rectangle = new Rectangle(face.X - 20, face.Y - 20, face.Width + 40, face.Height + 40);

                        //uncommmented to hide face but is not yet stable

                        //Image<Bgr, Byte> temp = grayframe.Copy(rectangle); //copy the image data from the face
                        //temp = temp.PyrDown().PyrDown().PyrDown().PyrUp().PyrUp().PyrUp(); //very simple blurring effect
                        //grayframe.ROI = rectangle; //set the ROI to the same size as temp
                        /////if the program hangs here check to make sure the 
                        /////ImageFrame.ROI is the same size of temp.
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

                        grayframe.Draw(rectangle, new Bgr(Color.Transparent), 0);

                    }
                    CameraCapture = grayframe.ToBitmap();

                }
            }
            _captureTimer.Start();
        }



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
