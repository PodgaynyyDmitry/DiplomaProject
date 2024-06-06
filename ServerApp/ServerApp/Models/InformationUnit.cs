using ServerApp.Models;
using System;
using System.Collections.Generic;

namespace ServerApp.Models
{
    public class InformationUnit
    {
        public int PK_InformationUnit { get; set; }
        public string Title { get; set; }
        public bool AccessModifier { get; set; }
        public int PK_Chapter { get; set; }
        public DateTime CreationDate { get; set; }

        public ICollection<ContentItem> ContentItems { get; set; }
        public ICollection<File> Files { get; set; }
    }
}
