using Apps.Lara.Handlers;
using Apps.Lara.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

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
        public string? Instructions { get; set; }

        [Display("Priority")]
        [StaticDataSource(typeof(PriorityDataHandler))]
        public string? Priority { get; set; }

        [Display("Translation memory ID")]
        [DataSource(typeof(MemoriesDataHandler))]
        public string? MemoryId { get; set; }
    }
}
