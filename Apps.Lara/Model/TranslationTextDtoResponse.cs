using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Lara.Model
{
    public class TranslationTextResponse
    {
        public TranslationContent Translation { get; set; }
    }

    public class TranslationTextDtoResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("content")]
        public TranslationContent Content { get; set; }
    }
    public class TranslationContent
    {
        [JsonProperty("content_type")]
        [Display("Content type")]
        public string ContentType { get; set; }

        [JsonProperty("source_language")]
        [Display("Source language")]
        public string SourceLanguage { get; set; }

        [JsonProperty("translation")]
        public string Translation { get; set; }
    }
}
