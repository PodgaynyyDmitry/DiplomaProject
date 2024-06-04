namespace ServerApp.Models
{
    public class InformationUnit
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool AccessModifier { get; set; }
        public int ChapterId { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
