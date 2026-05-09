using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllActiveAsync();
        Task<IEnumerable<Customer>> GetDeletedListAsync(); // For Audit List of Dashboard
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetWithSalesHistoryAsync(int id);
        Task<bool> CreateAsync(Customer customer);
        Task<bool> UpdateAsync(Customer customer);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<string> GenerateCustomerCodeAsync();
    }
}