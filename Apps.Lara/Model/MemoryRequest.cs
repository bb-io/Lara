using Apps.Lara.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Lara.Model
{
    public class MemoryRequest
    {
        [Display("Memory ID")]
        [DataSource(typeof(MemoriesDataHandler))]
        public string MemoryId { get; set; }

    }
}
