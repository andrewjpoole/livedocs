using System;
using System.Text.Json.Serialization;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace LiveDocs.Server.Models
{
    public class ServiceBusQueueProperties
    {
        public string lockDuration { get; set; } = string.Empty;
        public int maxSizeInMegabytes { get; set; }
        public bool requiresDuplicateDetection { get; set; }
        public bool requiresSession { get; set; }
        public string defaultMessageTimeToLive { get; set; } = string.Empty;
        public bool deadLetteringOnMessageExpiration { get; set; }
        public bool enableBatchedOperations { get; set; }
        public string duplicateDetectionHistoryTimeWindow { get; set; } = string.Empty;
        public int maxDeliveryCount { get; set; }
        public int sizeInBytes { get; set; }
        public int messageCount { get; set; }
        public string status { get; set; } = string.Empty;
        public string autoDeleteOnIdle { get; set; } = string.Empty;
        public bool enablePartitioning { get; set; }
        public bool enableExpress { get; set; }

        [JsonPropertyName("countDetails")]
        public ServiceBusQueueCountDetails ServiceBusQueueCountDetails { get; set; } = new();
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public DateTime accessedAt { get; set; }
    }
}