using System.Collections.Generic;

namespace IPCS_Model.Entities.Permissions
{
    public class AppModule
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<AppPermission> Permissions { get; set; } = new List<AppPermission>();
    }
}
