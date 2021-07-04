using System.Collections.Generic;
using LiveDocs.Server.Replacements;

namespace LiveDocs.Server
{
    public class ResourceDocumentation
    {
        public string Name { get; init; }
        public List<Replacement> Replacements { get; set; }
        public string RawMarkdown { get; set; }
        public string RenderedMarkdown { get; set; }
    }
}