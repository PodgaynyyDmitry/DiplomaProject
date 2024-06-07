using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models
{
    public class Discipline
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_Discipline { get; set; }

        [MaxLength(20)]
        public string Module { get; set; }

        [MaxLength(20)]
        public string Chapter { get; set; }

        [MaxLength(20)]
        public string DisciplineNumber { get; set; }

        [MaxLength(100)]
        public string Title { get; set; }

        public int PK_Department { get; set; }
    }
}
