using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;

namespace LiveTranslator
{
    public class TranslationService : IDisposable
    {
        static string speechKey = "";
        static string speechRegion = "";

        public delegate void NewTranslationEventHandler(object sender, TranslationEventArgs args);

        public NewTranslationEventHandler newTranslationHandler;

        public NewTranslationEventHandler finishedTranslation;

        private TranslationRecognizer _translationRecognizer;

        public TranslationService()
        {
        }

        public async Task StartRecognition()
        {
            var speechTranslationConfig = SpeechTranslationConfig.FromSubscription(speechKey, speechRegion);
            speechTranslationConfig.SpeechRecognitionLanguage = "pl-PL";
            speechTranslationConfig.AddTargetLanguage("en");

            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            _translationRecognizer = new TranslationRecognizer(speechTranslationConfig, audioConfig);

            _translationRecognizer.Recognizing += (sender, args) =>
            {
                OutputSpeechRecognitionResult(args.Result);
            };

            _translationRecognizer.Recognized += (sender, args) =>
            {
                finishedTranslation(this, new TranslationEventArgs());
            };

            await _translationRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            //var translationRecognitionResult = await translationRecognizer.RecognizeOnceAsync();
            //OutputSpeechRecognitionResult(translationRecognitionResult);
        }

        public void Dispose()
        {
            _translationRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

            Thread.Sleep(1000);

            _translationRecognizer.Dispose();
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

            newTranslationHandler(this, args);
        }
    }
}
