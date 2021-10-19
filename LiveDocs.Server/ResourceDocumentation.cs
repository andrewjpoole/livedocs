using System.Collections.Generic;
using LiveDocs.Server.Replacements;
using LiveDocs.Server.Replacers;

namespace LiveDocs.Server
{
    public class ResourceDocumentation
    {
        public string Name { get; init; } = string.Empty;
        public List<Replacement> Replacements { get; set; } = new();
        public Dictionary<string, IReplacer> Replacers { get; set; } = new();
        public string RawMarkdown { get; set; } = string.Empty;
        public string RenderedMarkdown { get; set; } = string.Empty;
    }
}