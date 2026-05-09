using System.Collections.Generic;

namespace IPCS_Model.DTOs
{
    public class PermissionModuleDTO
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public List<PermissionDTO> Permissions { get; set; } = new List<PermissionDTO>();
    }

    public class PermissionDTO
    {
        public int Id { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }

    public class UpdateRolePermissionDTO
    {
        public string RoleId { get; set; } = string.Empty;
        public List<int> PermissionIds { get; set; } = new List<int>();
    }

    public class UpdateUserPermissionDTO
    {
        public string UserId { get; set; } = string.Empty;
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}
