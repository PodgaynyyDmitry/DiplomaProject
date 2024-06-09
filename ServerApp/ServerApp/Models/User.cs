using System.Data;

namespace ServerApp.Models
{
    public class User
    {
        public int PK_Users { get; set; } // Primary Key
        public string Login { get; set; }
        public string Password { get; set; }
        public bool SessionStatus { get; set; }
        public int PK_Role { get; set; }
    }

}
