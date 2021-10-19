using System;

namespace LiveDocs.Server.Replacements
{
    public class Replacement
    {
        public string Match { get; init; } = string.Empty;
        public string Instruction { get; init; } = string.Empty;
        public string TimeToLive { get; init; } = string.Empty;
        public bool OnlyRunWhenClientsConnected { get; init; }
        public string LatestReplacedData { get; set; } = "waiting for data...";
        public string ParentResourceDocumentationName { get; set; } = string.Empty;
        public DateTime Expired { get; set; } = DateTime.MinValue;

        public bool HasExpired() => DateTime.UtcNow > Expired;
        public bool IsScheduled { get; set; }
    }
}