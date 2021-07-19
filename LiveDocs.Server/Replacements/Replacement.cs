namespace LiveDocs.Server.Replacements
{
    public class Replacement
    {
        public string Match { get; init; }
        public string Instruction { get; init; }
        public string Schedule { get; init; } public bool OnlyRunWhenClientsConnected { get; init; }
        public string LatestReplacedData { get; set; }
    }
}