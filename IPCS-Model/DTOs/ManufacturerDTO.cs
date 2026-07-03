using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class ManufacturerDTO
    {
        public int BrandId { get; set; }
        [Required(ErrorMessage = "Please input a BrandName")]
        public string BrandName { get; set; } = string.Empty;

        public string? Origin { get; set; }
        public string? ContactPerson { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
    }
}