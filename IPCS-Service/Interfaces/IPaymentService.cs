using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPCS_Service.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<InvoicePayment>> GetPaymentsByInvoiceAsync(string type, int invoiceId);
        Task<bool> ProcessSplitPaymentAsync(List<InvoicePaymentDTO> payments);
        Task<bool> DeletePaymentAsync(int paymentId);
        
        // Reporting
        Task<decimal> GetTotalCollectionByMethodAsync(int methodId, DateTime? date = null);
    }
}
