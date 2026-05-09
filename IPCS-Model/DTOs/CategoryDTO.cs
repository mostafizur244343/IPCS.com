using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCS_Model.DTOs
{
    public class CategoryDTO
    {
        [Required(ErrorMessage = "Please Enter CategoryName")]
        [StringLength(100, ErrorMessage = "Name Can't be increase 100 Alphabet")]
        public string CategoryName { get; set; } = string.Empty;

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
