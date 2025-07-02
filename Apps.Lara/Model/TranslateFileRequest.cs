using Apps.Lara.Handlers;
using Apps.Lara.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;


namespace Apps.Lara.Model
{
    public class TranslateFileRequest : ITranslateFileInput
    {
        [Display("File for translation")]
        public FileReference File { get; set; }

        [Display("Instructions to customize")]
        public string? Instructions { get; set; }

        [Display("Priority")]
        [StaticDataSource(typeof(PriorityDataHandler))]
        public string? Priority { get; set; }

        [Display("Source language")]
        [DataSource(typeof(LanguageDataHandler))]
        public string? SourceLanguage { get; set; }

        [Display("Target language")]
        [DataSource(typeof(LanguageDataHandler))]
        public string TargetLanguage { get; set; }

        [Display("Translation memory ID")]
        [DataSource(typeof(MemoriesDataHandler))]
        public string? MemoryId { get; set; }


        //
        [DefinitionIgnore]
        public string? OutputFileHandling { get => string.Empty; set { /* no-op */ } }
    }
}
