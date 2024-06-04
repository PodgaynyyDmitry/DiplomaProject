namespace ServerApp.Models
{
    public class ContentItem
    {
        public int Id { get; set; }
        public int InformationUnitId { get; set; }
        public int SequenceNumber { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
    }
}
