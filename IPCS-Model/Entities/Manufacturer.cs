using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.Entities
{
    public class Manufacturer
    {
        [Key]
        public int BrandId { get; set; } 

        [Required(ErrorMessage = "Must input a BrandName")]
        [StringLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Origin { get; set; } // Example: Local Or Imported

        public string? ContactPerson { get; set; }

        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}
