using System;
using System.Security.Policy;

namespace LiveTranslator
{
    public class TranslationEventArgs : EventArgs
    {
        public string Recognized { get; set; } = string.Empty;

        public string Translation { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;
    }
}
