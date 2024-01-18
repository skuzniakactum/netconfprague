using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing.Drawing2D;

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

        private FaceDetectionService faceDetectionService;

        private int frameCounter = 0;

        public Form1()
        {
            InitializeComponent();

            faceDetectionService = new FaceDetectionService();
            faceDetectionService.newFaceDetected += FaceDetectionService_newFaceDetected;
        }

        private void FaceDetectionService_newFaceDetected(object sender, FaceDetectedEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, FaceDetectedEventArgs>(FaceDetectionService_newFaceDetected), sender, args);
            }
            else
            {
                tokenSource.Cancel();

                label1.Text = "Face detected";

                var newImage = OverlayImage(new Bitmap(pictureBox1.Image), args);
                pictureBox1.Image = newImage;
            }
        }

        private void CaptureCamera()
        {
            tokenSource = new CancellationTokenSource();

            camera = new Thread(new ThreadStart(async () => await CaptureCameraCallback(tokenSource.Token)));
            camera.Start();
        }

        private async Task CaptureCameraCallback(CancellationToken token)
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

                        Invoke(() =>
                        {
                            if (pictureBox1.Image != null)
                            {
                                pictureBox1.Image.Dispose();
                            }

                            pictureBox1.Image = image;
                        });

                        if (frameCounter == 100)
                        {
                            await faceDetectionService.DetectFace(frame.Clone().ToMemoryStream());
                            frameCounter = 0;
                        }
                        else { frameCounter++; }
                    }
                }
            }
        }

        private Bitmap OverlayImage(Bitmap frame, FaceDetectedEventArgs args)
        {
            Bitmap chicken = new Bitmap("C:\\projects\\netconfprague\\ChickenOnHead\\chick-mask.png");

            Bitmap chickenAdjusted = new Bitmap(chicken, new System.Drawing.Size(args.Width, args.Height));

            Bitmap bmp = new Bitmap(frame.Width, frame.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                chicken.MakeTransparent();
                g.DrawImage(frame, 0, 0);
                g.DrawImage(chickenAdjusted, args.Left, args.Top - (args.Height / 4));
            }

            return bmp;
        }

        private void StopCamera()
        {
            try
            {
                isCameraRunning = false;
                capture.Release();
            }
            catch (Exception ex)
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CaptureCamera();
            isCameraRunning = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            tokenSource.Cancel();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
        }
    }
}