using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Lara.Model
{
    public class MemoryTranslationResponse
    {
        [JsonProperty("content")]
        public OperationContent Content { get; set; }
    }

    public class OperationContent
    {
        [JsonProperty("id")]
        [Display("Import process ID")]
        public string Id { get; set; }

        [JsonProperty("begin")]
        public long Begin { get; set; }

        [JsonProperty("end")]
        public long End { get; set; }

        [JsonProperty("channel")]
        public int Channel { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("progress")]
        public int Progress { get; set; }
    }
}
