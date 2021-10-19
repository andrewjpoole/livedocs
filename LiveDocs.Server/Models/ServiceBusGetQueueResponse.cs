using System.Text.Json.Serialization;
// ReSharper disable InconsistentNaming

namespace LiveDocs.Server.Models
{
    public class ServiceBusGetQueueResponse
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string location { get; set; } = string.Empty;

        [JsonPropertyName("properties")]
        public ServiceBusQueueProperties ServiceBusQueueProperties { get; set; } = new();
    }
}