using Apps.Lara.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Lara.Model
{
    public class TranslateTextRequest
    {
        [Display("Text to translate")]
        public string Text { get; set; }

        [Display("Content type")]
        [StaticDataSource(typeof(ContentTypeDataHandler))]
        public string? ContentType { get; set; }

        [Display("Instructions to customize")]
        public IEnumerable<string?> Instructions { get; set; }

        [Display("Priority")]
        [StaticDataSource(typeof(PriorityDataHandler))]
        public string? Priority { get; set; }
    }
}
