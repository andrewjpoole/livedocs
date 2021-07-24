namespace LiveDocs.Server.Models
{
    public class ResourceDocumentationFile
    {
        public string Name { get; set; }
        public string MdPath { get; set; }
        public string JsonPath { get; set; }
        public string Domain { get; set; }
        public string SubDomain { get; set; }
    }
}