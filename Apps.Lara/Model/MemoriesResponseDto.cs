using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;


namespace Apps.Lara.Model
{
    public class MemoriesResponseDto
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("content")]
        public List<MemoryItem> Content { get; set; }
    }

    public class MemoryItem
    {
        [JsonProperty("id")]
        [Display("Memory ID")]
        public string Id { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("owner_id")]
        [Display("Owner ID")]
        public string OwnerId { get; set; }

        [JsonProperty("collaborators_count")]
        [Display("Collaborators count")]
        public int CollaboratorsCount { get; set; }

        [JsonProperty("created_at")]
        [Display("Created at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [Display("Updated at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("shared_at")]
        [Display("Shared at")]
        public DateTime SharedAt { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class MemoryResponseDto
    {
        [JsonProperty("content")]
        public MemoryItem Content { get; set; }
    }
}
