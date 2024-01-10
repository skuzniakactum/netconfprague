using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics.Eventing.Reader;

namespace ChickenOnHead
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource tokenSource;

        private VideoCapture capture;

        private Mat frame;

        private Bitmap image;

        private Thread camera;

        private bool isCameraRunning = false;

        public Form1()
        {
            InitializeComponent();
            CaptureCamera();
            isCameraRunning = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            tokenSource.Cancel();
        }

        private void CaptureCamera()
        {
            tokenSource = new CancellationTokenSource();

            camera = new Thread(new ThreadStart(() => CaptureCameraCallback(tokenSource.Token)));
            camera.Start();
        }

        private void CaptureCameraCallback(CancellationToken token)
        {
            token.Register(StopCamera);

            int cameraId = 2;

            using (frame = new Mat())
            {
                capture = new VideoCapture(cameraId, VideoCaptureAPIs.ANY);
                capture.Open(cameraId);

                if (capture.IsOpened())
                {
                    while (isCameraRunning)
                    {
                        capture.Read(frame);
                        image = BitmapConverter.ToBitmap(frame);
                        if (pictureBox1.Image != null)
                        {
                            pictureBox1.Image.Dispose();
                        }

                        pictureBox1.Image = image;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
        }

        private void StopCamera()
        {
            try
            {
                isCameraRunning = false;
                capture.Release();
                camera.Abort();
            }
            catch (Exception ex)
            {
            }
        }
    }
}