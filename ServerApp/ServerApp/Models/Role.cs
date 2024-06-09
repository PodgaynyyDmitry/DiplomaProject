namespace ServerApp.Models
{
    public class Role
    {
        public int PK_Role { get; set; }
        public string RoleName { get; set; }
        public ICollection<User> Users { get; set; }
    }

}
