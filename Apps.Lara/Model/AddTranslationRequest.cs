using Blackbird.Applications.Sdk.Common;

namespace Apps.Lara.Model
{
    public class AddTranslationRequest
    {
        [Display("Sentence")]
        public string Setnence { get; set; }

        [Display("Translation")]
        public string Translation { get; set; }

        [Display("Translation uniqe ID")]
        public string? TranslationId { get; set; }

        [Display("Sentence before")]
        public string? SentenceBefore { get; set; }

        [Display("Sentence after")]
        public string? SentenceAfter { get; set; }
    }
}
