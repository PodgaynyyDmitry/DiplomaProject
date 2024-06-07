using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models
{
    public class ClassFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_ClassFile { get; set; }

        public int PK_Discipline { get; set; }

        public int PK_Class { get; set; }

        public int SequenceNumber { get; set; }

        [MaxLength(200)]
        public string Path { get; set; }
    }
}
