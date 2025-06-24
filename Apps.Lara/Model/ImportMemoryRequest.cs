using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Lara.Model
{
    public class ImportMemoryRequest
    {
        [Display("TMX file")]
        public FileReference File { get; set; }

        public string Compression { get; set; }
    }
}
