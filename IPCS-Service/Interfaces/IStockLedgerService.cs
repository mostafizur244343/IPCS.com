
using IPCS_Model.DTOs;
using IPCS_Model.Entities;


public interface IStockLedgerService
{

    Task<IEnumerable<StockLedger>> GetAllByBranchAsync(int branchId);

    Task<PagedResult<StockLedger>> GetPagedByBranchAsync(int branchId, int page, int pageSize, string? search = null);

    Task<bool> AddLedgerEntryAsync(StockLedgerDTO dto);

}