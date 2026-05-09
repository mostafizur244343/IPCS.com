
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using IPCS_Model.Entities;


namespace IPCS_Model.Identity
{
    /// <summary>
    /// User Entity representing the data stored in the database.
    /// Inherits from IdentityUser for authentication features.
    /// </summary>
    public class User : IdentityUser
    {
        [Required, PersonalData]
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string? PresentAddress { get; set; }
        public string? PermanentAddress { get; set; }
        
        [MaxLength(17)]
        public override string? PhoneNumber { get; set; } // Override default field
        
        public string? Description { get; set; }
        
        /// <summary>
        /// Admin can use this to deactivate a user without deleting them.
        /// Checked during login logic.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Custom Profile Fields
        public DateTime? DateOfBirth { get; set; }
        public DateTime JoiningDate { get; set; } = DateTime.Now;
        public string? Gender { get; set; }
        
        /// <summary>
        /// Foreign key for the Branch this user belongs to.
        /// </summary>
        public int? BranchId { get; set; }
        
        // Navigation Property
        public virtual Branch? Branch { get; set; } 
    }
}