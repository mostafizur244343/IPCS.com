using IPCS_Model.Entities;
using IPCS_Model.DTOs;
using IPCS_Repo.Data;
using IPCS_Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class StockLedgerService : IStockLedgerService
    {
        private readonly IPCSDBContext _context;

        public StockLedgerService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StockLedger>> GetAllByBranchAsync(int branchId)
        {
            // AsNoTracking use
            return await _context.StockLedgers
                .AsNoTracking()
                .Include(l => l.Product)
                .Include(l => l.Lot)
                .Where(l => l.BranchId == branchId)
                .OrderByDescending(l => l.LedgerId)
                .ToListAsync();
        }

        // Main Transection Method: Stock and ledger update together
        public async Task<bool> AddLedgerEntryAsync(StockLedgerDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //Find Out Last Balance (Fast Searching by using AsNoTracking)
                var lastEntry = await _context.StockLedgers
                    .AsNoTracking()
                    .Where(l => l.BranchId == dto.BranchId && l.ProductId == dto.ProductId && l.LotId == dto.LotId)
                    .OrderByDescending(l => l.LedgerId)
                    .FirstOrDefaultAsync();

                decimal prevBalance = lastEntry?.CurrentBalance ?? 0;

                // Create Ledger Entry
                var ledger = new StockLedger
                {
                    BranchId = dto.BranchId,
                    ProductId = dto.ProductId,
                    LotId = dto.LotId,
                    UnitPrice = dto.UnitPrice,
                    PreviousBalance = prevBalance,
                    QuantityIn = dto.QuantityIn,
                    QuantityOut = dto.QuantityOut,
                    TransactionType = dto.TransactionType,
                    ReferenceNo = dto.ReferenceNo,
                    Remarks = dto.Remarks,
                    TransactionDate = DateTime.Now
                };

                await _context.StockLedgers.AddAsync(ledger);

                // BranchLotStock Update Logic
                var stock = await _context.BranchLotStocks
                    .FirstOrDefaultAsync(s => s.BranchId == dto.BranchId && s.ProductId == dto.ProductId && s.LotId == dto.LotId);

                if (stock == null)
                {
                    stock = new BranchLotStock
                    {
                        BranchId = dto.BranchId,
                        ProductId = dto.ProductId,
                        LotId = dto.LotId,
                        CurrentStock = dto.QuantityIn - dto.QuantityOut
                    };
                    await _context.BranchLotStocks.AddAsync(stock);
                }
                else
                {
                    stock.CurrentStock += (dto.QuantityIn - dto.QuantityOut);
                    stock.LastUpdated = DateTime.Now;
                    _context.Entry(stock).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // If ledger and stock successfully save
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}