using System;
using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    /// <summary>
    /// User registration input model (DTO).
    /// Used to receive data from the client during account creation.
    /// </summary>
    public class RegisterModel
    {
        [Required(ErrorMessage = "Full Name is Required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is Required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is Required")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is Required")]
        public string RoleName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Mobile Number is Required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string MobileNumber { get; set; } = string.Empty;
        public string? PresentAddress { get; set; }
        public string? PermanentAddress { get; set; }
        public string? Description { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        
        /// <summary>
        /// ID of the branch the user will be assigned to.
        /// </summary>
        public int? BranchId { get; set; }
    }
}
