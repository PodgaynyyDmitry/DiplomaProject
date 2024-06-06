namespace ServerApp.Models
{
    public class File
    {
        public int PK_File { get; set; }
        public int PK_InformationUnit { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public int SequenceNumber { get; set; }

        public InformationUnit InformationUnit { get; set; }

    }
}
