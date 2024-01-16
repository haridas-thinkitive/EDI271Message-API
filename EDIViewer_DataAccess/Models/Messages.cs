using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDIViewer_DataAccess.Models
{
    public class Messages
    {
        [Key]
        public int EncounterId { get; set; }
        public string Response { get; set; }
    }
}
