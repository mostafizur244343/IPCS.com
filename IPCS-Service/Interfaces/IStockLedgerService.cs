
using IPCS_Model.DTOs;
using IPCS_Model.Entities;


public interface IStockLedgerService
{

    Task<IEnumerable<StockLedger>> GetAllByBranchAsync(int branchId);

    Task<bool> AddLedgerEntryAsync(StockLedgerDTO dto);

}