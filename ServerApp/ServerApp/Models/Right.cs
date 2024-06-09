namespace ServerApp.Models
{
    public class Right
    {
        public int PK_Rights { get; set; }
        public string Name { get; set; }
        public ICollection<CurrentRight> CurrentRights { get; set; }
    }

}
