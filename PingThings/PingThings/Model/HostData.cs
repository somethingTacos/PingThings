using System.Text.Json.Serialization;

namespace PingThings.Model
{
    public class HostData
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }
        [JsonPropertyName("host")]
        public string Host { get; set; }
    }
}
