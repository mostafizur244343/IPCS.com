
using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.Entities
{
    public class UOM
    {
        [Key]
        public int UOMId { get; set; }

        [Required(ErrorMessage = "Please Give an UOM Name...")]
        [StringLength(50)]
        public string UOMName { get; set; } = string.Empty; //Example : PCs, Strips or etc

        [StringLength(200)]
        public string? Description { get; set; } 

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}
