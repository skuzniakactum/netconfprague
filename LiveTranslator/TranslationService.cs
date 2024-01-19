using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.Extensions.Configuration;

namespace LiveTranslator
{
    public class TranslationService : IDisposable
    {
        public delegate void NewTranslationEventHandler(object sender, TranslationEventArgs args);

        public NewTranslationEventHandler NewTranslationHandler;

        public NewTranslationEventHandler FinishedTranslation;

        private TranslationRecognizer translationRecognizer;

        private readonly IConfiguration configuration;

        public TranslationService()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.local.json")
                .Build();
        }

        public async Task StartRecognition()
        {
            var settings = configuration.GetRequiredSection("Settings").Get<Settings>();
            if (settings == null)
            {
                throw new ArgumentException("Settings not found");
            }

            var speechTranslationConfig = SpeechTranslationConfig.FromSubscription(settings.SubscriptionKey, settings.Region);
            speechTranslationConfig.SpeechRecognitionLanguage = "pl-PL";
            speechTranslationConfig.AddTargetLanguage("en");

            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();

            translationRecognizer = new TranslationRecognizer(speechTranslationConfig, audioConfig);

            translationRecognizer.Recognizing += (sender, args) =>
            {
                OutputSpeechRecognitionResult(args.Result);
            };

            translationRecognizer.Recognized += (sender, args) =>
            {
                FinishedTranslation(this, new TranslationEventArgs());
            };

            await translationRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            translationRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

            Thread.Sleep(5000);

            translationRecognizer.Dispose();
        }

        void OutputSpeechRecognitionResult(TranslationRecognitionResult translationRecognitionResult)
        {
            var args = new TranslationEventArgs();

            switch (translationRecognitionResult.Reason)
            {
                case ResultReason.TranslatingSpeech:
                    args.Recognized = translationRecognitionResult.Text;
                    if (translationRecognitionResult.Translations.ContainsKey("en"))
                    {
                        args.Translation += translationRecognitionResult.Translations["en"];
                    }
                    break;
                case ResultReason.NoMatch:
                    args.Error = $"NOMATCH: Speech could not be recognized.";
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(translationRecognitionResult);
                    args.Error = $"CANCELED: Reason={cancellation.Reason}";
                    break;
                default:
                    args.Error = $"Default: {translationRecognitionResult.Reason}";
                    break;
            }

            NewTranslationHandler(this, args);
        }
    }
}
