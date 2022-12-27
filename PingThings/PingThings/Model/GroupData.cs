using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PingThings.Model
{
    public class GroupData
    {
        [JsonPropertyName("groupname")]
        public string GroupName { get; set; }
        [JsonPropertyName("pings")]
        public List<HostData> Pings { get; set; }
    }
}
