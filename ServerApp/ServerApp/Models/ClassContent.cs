using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models
{
    public class ClassContent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_ClassParagraph { get; set; }

        public int PK_Discipline { get; set; }

        public int PK_Class { get; set; }

        public int SequenceNumber { get; set; }

        [MaxLength(100)]
        public string ContentType { get; set; }

        public string Content { get; set; }

        [MaxLength(200)]
        public string FilePath { get; set; }
    }
}
