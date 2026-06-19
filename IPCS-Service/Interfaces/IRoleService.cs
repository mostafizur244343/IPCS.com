using IPCS_Model.DTOs;
using Microsoft.AspNetCore.Identity;

namespace IPCS_Service.Interfaces
{
    public interface IRoleService
    {
        // Get all roles from database
        Task<List<RoleDTO>> GetAllRolesAsync();

        // Create a new role
        Task<IdentityResult> CreateRoleAsync(string roleName);

        // Delete a role
        Task<IdentityResult> DeleteRoleAsync(string roleId);

        // Update a role name
        Task<IdentityResult> UpdateRoleAsync(string roleId, string newRoleName);

        // Get all users with their assigned roles
        Task<List<UserWithRolesDTO>> GetUsersWithRolesAsync();

        // Check if a role exists
        Task<bool> RoleExistsAsync(string roleName);
    }
}
