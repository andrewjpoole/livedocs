namespace LiveDocs.Server.Models
{
    public class ResourceDocumentationFile
    {
        public string Name { get; set; } = string.Empty;
        public string MdPath { get; set; } = string.Empty;
        public string JsonPath { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string SubDomain { get; set; } = string.Empty;
    }
}