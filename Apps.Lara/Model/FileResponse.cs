using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Lara.Model
{
    public class FileResponse
    {
        [Display("Translated file")]
        public FileReference File { get; set; }
    }
}
