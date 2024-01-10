using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.CognitiveServices.Speech;
using System.ComponentModel;
using static LiveTranslator.TranslationService;

namespace LiveTranslator
{
    public partial class Form1 : Form
    {
        private TranslationService _translationService;

        private BackgroundWorker _backgroundWorker = new BackgroundWorker();

        public Form1()
        {
            InitializeComponent();

            _backgroundWorker.DoWork += _backgroundWorker_DoWork;

            //_backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            //_backgroundWorker.WorkerReportsProgress = true;
        }

        private void _backgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            //textBox1.Lines = new[] { args.Recognized, args.Translation, args.Error };
        }

        private void _backgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            _translationService = new TranslationService();
            _translationService.newTranslationHandler += NewTranslationArrived;
            _translationService.finishedTranslation += PreserveResult;
            _translationService.StartRecognition().ConfigureAwait(false);
        }

        private void NewTranslationArrived(object sender, TranslationEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, TranslationEventArgs>(NewTranslationArrived), sender, args);
            }
            else
            {
                label1.Text = args.Translation;
                label2.Text = string.IsNullOrEmpty(args.Recognized) ? args.Error : args.Recognized;
            }
        }

        private void PreserveResult(object sender, TranslationEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, TranslationEventArgs>(PreserveResult), sender, args);
            }
            else
            {
                label3.Text = label1.Text;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _backgroundWorker.RunWorkerAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_translationService != null)
            {
                _translationService.Dispose();
            }
        }
    }
}