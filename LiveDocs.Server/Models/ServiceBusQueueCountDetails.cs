// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace LiveDocs.Server.Models
{
    public class ServiceBusQueueCountDetails
    {
        public int activeMessageCount { get; set; }
        public int deadLetterMessageCount { get; set; }
        public int scheduledMessageCount { get; set; }
        public int transferMessageCount { get; set; }
        public int transferDeadLetterMessageCount { get; set; }
    }
}