using Newtonsoft.Json;

namespace Apps.Lara.Model
{
    public class LanguageCodeResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("content")]
        public List<string> Content { get; set; }
    }
}
