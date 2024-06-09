namespace ServerApp.Models
{
    public class CurrentRight
    {
        public int PK_CurrentRights { get; set; } // Primary Key
        public int PK_Rights { get; set; }
        public int PK_User { get; set; }
        public bool Writing { get; set; }
        public bool Reading { get; set; }
    }

}
