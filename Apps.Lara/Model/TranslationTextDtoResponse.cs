using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;
using Newtonsoft.Json;

namespace Apps.Lara.Model
{   
    public class TranslationTextDtoResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("content")]
        public TranslationContent Content { get; set; }
    }
    public class TranslationContent : ITranslateTextOutput
    {
        [JsonProperty("content_type")]
        [Display("Content type")]
        public string ContentType { get; set; }

        [JsonProperty("source_language")]
        [Display("Source language")]
        public string SourceLanguage { get; set; }

        [Display("Translated text")]
        [JsonProperty("translation")]
        public string TranslatedText { get; set; }
    }


    public class TranslationTextsResponse
    {
        [JsonProperty("content")]
        public TranslationContents Translation { get; set; }
    }

    public class TranslationContents
    {
        [JsonProperty("content_type")]
        [Display("Content type")]
        public string ContentType { get; set; }

        [JsonProperty("source_language")]
        [Display("Source language")]
        public string SourceLanguage { get; set; }

        [JsonProperty("translation")]
        public List<TranslationSegment> Translation { get; set; }
    }

    public class TranslationSegment
    {

        [JsonProperty("text")]
        public string Text { get; set; }


        [JsonProperty("translatable")]
        public bool Translatable { get; set; }
    }
}
