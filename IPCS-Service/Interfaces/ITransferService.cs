using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPCS_Service.Interfaces
{
    public interface ITransferService
    {
        // Requisition
        Task<IEnumerable<TransferRequisition>> GetAllRequisitionsAsync();
        Task<TransferRequisition?> GetRequisitionByIdAsync(int id);
        Task<bool> CreateRequisitionAsync(TransferRequisition requisition);
        Task<bool> UpdateRequisitionStatusAsync(int id, string status);

        // Transfer Setup
        Task<IEnumerable<TransferMaster>> GetAllTransfersAsync();
        Task<TransferMaster?> GetTransferByIdAsync(int id);
        
        // Actions
        Task<bool> InitiateTransferAsync(TransferMaster transferMaster);
        Task<bool> EditPendingTransferAsync(TransferMaster updatedTransfer);
        Task<bool> ConfirmGoodsReceivedAsync(TransferReceiveDTO receiveDto, string receivedBy);
        Task<bool> CancelTransferAsync(int transferId); // Rejects/Cancels it
        Task<bool> ConfirmCancelReturnAsync(int transferId); // Adds stock back to FromBranch

        // Soft Delete
        Task<bool> SoftDeleteTransferAsync(int id);
    }
}
