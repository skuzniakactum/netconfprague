using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Extensions.Configuration;

namespace ChickenOnHead
{
    public class FaceDetectionService
    {
        private readonly IFaceClient faceClient;

        private readonly IConfiguration configuration;

        public delegate void NewFaceDetectedEventHandler(object sender, FaceDetectedEventArgs args);

        public event NewFaceDetectedEventHandler newFaceDetected;

        public FaceDetectionService()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.local.json")
                .Build();

            faceClient = Authenticate();
        }

        public async Task DetectFace(Stream stream)
        {
            var faces = await faceClient.Face.DetectWithStreamAsync(stream);

            if (faces.Count > 0)
            {
                var args = new FaceDetectedEventArgs
                {
                    Top = faces[0].FaceRectangle.Top,
                    Left = faces[0].FaceRectangle.Left,
                    Width = faces[0].FaceRectangle.Width,
                    Height = faces[0].FaceRectangle.Height
                };

                newFaceDetected(this, args);
            }
        }

        private IFaceClient Authenticate()
        {
            var settings = configuration.GetRequiredSection("Settings").Get<Settings>();

            if (settings == null)
            {
                throw new ArgumentException("Settings not found");
            }

            return new FaceClient(new ApiKeyServiceClientCredentials(settings.SubscriptionKey))
            {
                Endpoint = settings.Endpoint
            };
        }
    }
}
