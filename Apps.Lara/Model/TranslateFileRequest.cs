using Apps.Lara.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;


namespace Apps.Lara.Model
{
    public class TranslateFileRequest 
    {
        [Display("File for translation")]
        public FileReference File { get; set; }

        [Display("Instructions to customize")]
        public string? Instructions { get; set; }

        [Display("Priority")]
        [StaticDataSource(typeof(PriorityDataHandler))]
        public string? Priority { get; set; }
    }
}
