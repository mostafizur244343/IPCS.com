using IPCS_Model.DTOs;
using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface IBranchService
    {
        Task<IEnumerable<BranchResponseDTO>> GetAllAsync();
        Task<BranchResponseDTO?> GetByIdWithReportingAsync(int id);
        Task<Branch?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Branch branch);
        Task<bool> UpdateAsync(Branch branch);
        Task<bool> DeleteAsync(int id);
        Task<string> GenerateBranchCodeAsync(); 
    }
}