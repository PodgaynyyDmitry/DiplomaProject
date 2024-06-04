namespace ServerApp.Models
{
    public class File
    {
        public int Id { get; set; }
        public int InformationUnitId { get; set; }
        public string Path { get; set; }
        public int SequenceNumber { get; set; }
    }
}
