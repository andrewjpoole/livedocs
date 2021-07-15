using System;
using System.Text.Json.Serialization;

namespace LiveDocs.Server.Models
{
    public class ServiceBusQueueProperties
    {
        public string lockDuration { get; set; }
        public int maxSizeInMegabytes { get; set; }
        public bool requiresDuplicateDetection { get; set; }
        public bool requiresSession { get; set; }
        public string defaultMessageTimeToLive { get; set; }
        public bool deadLetteringOnMessageExpiration { get; set; }
        public bool enableBatchedOperations { get; set; }
        public string duplicateDetectionHistoryTimeWindow { get; set; }
        public int maxDeliveryCount { get; set; }
        public int sizeInBytes { get; set; }
        public int messageCount { get; set; }
        public string status { get; set; }
        public string autoDeleteOnIdle { get; set; }
        public bool enablePartitioning { get; set; }
        public bool enableExpress { get; set; }
        [JsonPropertyName("countDetails")]
        public ServiceBusQueueCountDetails ServiceBusQueueCountDetails { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public DateTime accessedAt { get; set; }
    }
}