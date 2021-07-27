using System;

namespace LiveDocs.Server.Replacements
{
    public class Replacement
    {
        public string Match { get; init; }
        public string Instruction { get; init; }
        public string TimeToLive { get; init; } 
        public bool OnlyRunWhenClientsConnected { get; init; }
        public string LatestReplacedData { get; set; } = "waiting for data...";
        public string ParentResourceDocumentationName { get; set; }
        public DateTime Expired { get; set; }

        public bool HasExpired() => DateTime.UtcNow > Expired;
    }
}