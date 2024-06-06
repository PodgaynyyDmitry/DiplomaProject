namespace ServerApp.Models
{
    public class ContentItem
    {
        public int PK_ContentItem { get; set; }
        public int InformationUnitId { get; set; }
        public int SequenceNumber { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
    }
}
