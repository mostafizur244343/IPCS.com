using IPCS_Model.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPCS_Service.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string userId, string permissionKey);
        Task<List<string>> GetUserPermissionsAsync(string userId);
        
        // Admin Management Methods
        Task<bool> AssignPermissionToRoleAsync(string roleId, int permissionId);
        Task<bool> AssignPermissionToUserAsync(string userId, int permissionId);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, int permissionId);
        Task<bool> RemovePermissionFromUserAsync(string userId, int permissionId);

        // UI Support
        Task<List<PermissionModuleDTO>> GetAllModulesWithPermissionsAsync(string? roleId = null, string? userId = null);
        Task<bool> UpdateRolePermissionsAsync(string roleId, List<int> permissionIds);
        Task<bool> UpdateUserPermissionsAsync(string userId, List<int> permissionIds);
        Task<bool> SeedPermissionsAsync(); // To populate the database with constants
    }
}
