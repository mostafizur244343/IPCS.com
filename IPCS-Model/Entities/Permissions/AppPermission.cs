using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities.Permissions
{
    public class AppPermission
    {
        public int Id { get; set; }
        public string PermissionKey { get; set; } = string.Empty; // e.g., Sales.Create
        public string DisplayName { get; set; } = string.Empty;
        
        public int ModuleId { get; set; }
        
        [ForeignKey("ModuleId")]
        public virtual AppModule Module { get; set; } = null!;
    }
}
