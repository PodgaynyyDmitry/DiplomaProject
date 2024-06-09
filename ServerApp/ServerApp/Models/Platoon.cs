namespace ServerApp.Models
{
    public class Platoon
    {
        public int PK_Platoons { get; set; } // Primary Key
        public string Name { get; set; }
        public bool Status { get; set; }
        public int PK_VisitDay { get; set; }
        public int PK_Department { get; set; }
    }

}
