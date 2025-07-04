using Apps.Lara.Handlers;
using Apps.Lara.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Handlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;


namespace Apps.Lara.Model
{
    public class TranslateFileRequest : ITranslateFileInput
    {
        [Display("File")]
        public FileReference File { get; set; }

        [Display("Glossary")]
        public FileReference? GlossaryFile { get; set; }

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

        [Display("Output file handling", Description = "Determine the format of the output file. The default Blackbird behavior is to convert to XLIFF for future steps."), StaticDataSource(typeof(ProcessFileFormatHandler))]
        public string? OutputFileHandling { get; set; }

        [Display("File translation strategy", Description = "Select whether to use Lara's own file processing capabilities or use Blackbird interoperability mode"), StaticDataSource(typeof(FileTranslationStrategyHandler))]
        public string? FileTranslationStrategy { get; set; }
    }
}
