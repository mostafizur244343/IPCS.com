using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class BranchDTO
    {
        [Required(ErrorMessage = "Must input Branch Name")]
        public string BranchName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? ManagerName { get; set; }
        public string? PicturePath { get; set; }
        public bool IsActive { get; set; } = true;
    }
}