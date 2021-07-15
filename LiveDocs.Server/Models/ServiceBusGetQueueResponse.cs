using System.Text.Json.Serialization;

namespace LiveDocs.Server.Models
{
    public class ServiceBusGetQueueResponse
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string location { get; set; }
        [JsonPropertyName("properties")]
        public ServiceBusQueueProperties ServiceBusQueueProperties { get; set; }
    }
}