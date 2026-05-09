using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class UOMDTO
    {
        [Required(ErrorMessage = "Must be input an UOM Name")]
        [StringLength(50)]
        public string UOMName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}