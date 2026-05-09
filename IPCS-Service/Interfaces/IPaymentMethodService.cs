using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface IPaymentMethodService
    {
        Task<IEnumerable<PaymentMethod>> GetAllAsync();
        Task<PaymentMethod?> GetByIdAsync(int id);
        Task<bool> CreateAsync(PaymentMethod method);
        Task<bool> UpdateAsync(PaymentMethod method);
        Task<bool> DeleteAsync(int id);
    }
}