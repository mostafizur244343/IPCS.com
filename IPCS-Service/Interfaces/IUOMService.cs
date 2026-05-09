using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface IUOMService
    {
        Task<IEnumerable<UOM>> GetAllAsync();
        Task<UOM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(UOM uom);
        Task<bool> UpdateAsync(UOM uom);
        Task<bool> DeleteAsync(int id);
    }
}