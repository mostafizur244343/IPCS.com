using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities.Permissions
{
    public class RolePermission
    {
        public int Id { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public int PermissionId { get; set; }

        [ForeignKey("PermissionId")]
        public virtual AppPermission Permission { get; set; } = null!;
    }
}
