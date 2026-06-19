using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IPCS_Model.DTOs
{
    /// <summary>
    /// DTO for creating or displaying a Role.
    /// </summary>
    public class RoleDTO
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Role Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters")]
        [JsonPropertyName("roleName")]
        public string RoleName { get; set; } = string.Empty;
    }


    /// <summary>
    /// DTO for displaying a list of users and their assigned roles.
    /// </summary>
    public class UserWithRolesDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
