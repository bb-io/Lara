using Apps.Lara.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Lara.Model
{
    public class LanguageRequest
    {
        [Display("Source language")]
        [DataSource(typeof(LanguageDataHandler))]
        public string? SourceLanguage { get; set; }

        [Display("Target language")]
        [DataSource(typeof(LanguageDataHandler))]
        public string TargetLanguage { get; set; }
    }
}
