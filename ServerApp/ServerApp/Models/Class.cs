using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models
{
    public class Class
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_Class { get; set; }

        public int PK_Discipline { get; set; }

        public DateTime StartDate { get; set; }

        public int Duration { get; set; }

        [MaxLength(40)]
        public string Topic { get; set; }

        public int PK_ClassType { get; set; }

        public int PK_Teacher { get; set; }

        public int PK_User { get; set; }

        public int PK_ClassRoom { get; set; }

        public int PK_Platoons { get; set; }
    }
}
