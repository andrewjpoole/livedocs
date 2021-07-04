using System.Collections.Generic;
using LiveDocs.Server.Replacements;
using LiveDocs.Server.Replacers;

namespace LiveDocs.Server
{
    public class ResourceDocumentation
    {
        public string Name { get; init; }
        public List<Replacement> Replacements { get; set; }
        public Dictionary<string, IReplacer> Replacers { get; set; }
        public string RawMarkdown { get; set; }
        public string RenderedMarkdown { get; set; }
    }
}