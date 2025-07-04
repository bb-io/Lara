using Apps.Lara.Handlers;
using Apps.Lara.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;

namespace Apps.Lara.Model
{
    public class TranslateTextRequest : ITranslateTextInput
    {
        [Display("Text")]
        public string Text { get; set; }

        [Display("Source language")]
        [DataSource(typeof(LanguageDataHandler))]
        public string? SourceLanguage { get; set; }

        [Display("Target language")]
        [DataSource(typeof(LanguageDataHandler))]
        public string TargetLanguage { get; set; }

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
