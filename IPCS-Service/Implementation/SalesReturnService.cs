using IPCS_Model.Entities;
using IPCS_Repo.Data;
using IPCS_Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPCS_Service.Implementation
{
    public class SalesReturnService : ISalesReturnService
    {
        private readonly IPCSDBContext _context;

        public SalesReturnService(IPCSDBContext context)
        {
            _context = context;
        }

        private async Task<string> GenerateReturnNoAsync()
        {
            var last = await _context.SalesReturnMasters.IgnoreQueryFilters().OrderByDescending(r => r.ReturnId).FirstOrDefaultAsync();
            int nextId = (last == null) ? 1 : last.ReturnId + 1;
            return $"SRET-{nextId:D5}";
        }

        public async Task<IEnumerable<SalesReturnMaster>> GetAllActiveAsync()
        {
            return await _context.SalesReturnMasters
                .Include(r => r.Customer)
                .Include(r => r.Branch)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SalesReturnMaster?> GetByIdAsync(int id)
        {
            return await _context.SalesReturnMasters
                .Include(r => r.Customer)
                .Include(r => r.Branch)
                .Include(r => r.ReturnDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(r => r.ReturnId == id);
        }

        public async Task<bool> CreateReturnAsync(SalesReturnMaster returnMaster)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                returnMaster.ReturnNo = await GenerateReturnNoAsync();
                decimal totalCostSaved = 0;

                foreach (var detail in returnMaster.ReturnDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product == null) continue;

                    // 0. Validation against original invoice if SalesId is provided
                    if (returnMaster.SalesId.HasValue)
                    {
                        var originalDetail = await _context.SalesDetails
                            .FirstOrDefaultAsync(d => d.SalesId == returnMaster.SalesId && d.ProductId == detail.ProductId && d.LotId == detail.LotId);

                        if (originalDetail == null)
                            throw new Exception($"Product ID {detail.ProductId} with Lot {detail.LotId} was not part of original Invoice No.");

                        if (detail.Quantity > originalDetail.Quantity)
                            throw new Exception($"Return quantity ({detail.Quantity}) cannot exceed sold quantity ({originalDetail.Quantity}) for Product ID {detail.ProductId}.");

                        // Ensure refund unit price is not more than what was paid (Net Price after discount)
                        decimal paidPrice = originalDetail.UnitPrice - originalDetail.DiscountPerUnit;
                        if (detail.UnitPrice > paidPrice)
                            detail.UnitPrice = paidPrice; // Cap the return price to what was paid
                    }

                    // Skip stock replenishment for Services
                    if (!product.IsService)
                    {
                        // 1. Replenish Stock in specific Lot
                        var stock = await _context.BranchLotStocks
                            .FirstOrDefaultAsync(s => s.BranchId == returnMaster.BranchId && s.ProductId == detail.ProductId && s.LotId == detail.LotId);

                        if (stock == null)
                        {
                            stock = new BranchLotStock { BranchId = returnMaster.BranchId, ProductId = detail.ProductId, LotId = detail.LotId, CurrentStock = detail.Quantity };
                            await _context.BranchLotStocks.AddAsync(stock);
                        }
                        else
                        {
                            stock.CurrentStock += detail.Quantity;
                            _context.BranchLotStocks.Update(stock);
                        }

                        // 2. Global Product Stock Increase
                        product.CurrentStock += detail.Quantity;
                        _context.Products.Update(product);

                        // 4. Ledger Entry
                        var lastLedger = await _context.StockLedgers
                            .Where(l => l.BranchId == returnMaster.BranchId && l.ProductId == detail.ProductId && l.LotId == detail.LotId)
                            .OrderByDescending(l => l.LedgerId)
                            .FirstOrDefaultAsync();

                        var ledger = new StockLedger
                        {
                            BranchId = returnMaster.BranchId, ProductId = detail.ProductId, LotId = detail.LotId,
                            TransactionType = "SalesReturn", ReferenceNo = returnMaster.ReturnNo, TransactionDate = DateTime.Now,
                            PreviousBalance = lastLedger?.CurrentBalance ?? 0, QuantityIn = detail.Quantity, QuantityOut = 0
                        };
                        await _context.StockLedgers.AddAsync(ledger);

                        // 3. Profit Adjustment Calculation (using exact Lot cost) - ONLY FOR PRODUCTS
                        var lot = await _context.LotInfos.FindAsync(detail.LotId);
                        decimal costPrice = lot?.PurchasePrice ?? 0;
                        totalCostSaved += (costPrice * detail.Quantity);
                    }
                }

                // 5. Financial Adjustments
                var today = DateTime.Now.Date;
                var summary = await _context.DailyTransactionSummaries
                    .FirstOrDefaultAsync(d => d.TransactionDate == today && d.BranchId == returnMaster.BranchId);

                if (returnMaster.RefundType == "Adjustment")
                {
                    var customer = await _context.Customers.FindAsync(returnMaster.CustomerId);
                    if (customer != null)
                    {
                        customer.CurrentDue -= returnMaster.RefundAmount;
                        _context.Customers.Update(customer);
                    }
                    
                    if (summary != null) 
                    {
                        summary.TotalDueSales -= returnMaster.RefundAmount;
                        if (summary.TotalDueSales < 0) summary.TotalDueSales = 0;
                    }
                }
                else if (returnMaster.RefundType == "Cash")
                {
                    bool isDigital = false;
                    if (returnMaster.PaymentMethodId.HasValue)
                    {
                        var method = await _context.PaymentMethods.FindAsync(returnMaster.PaymentMethodId.Value);
                        if (method != null && method.IsDigital) isDigital = true;
                    }

                    if (summary != null)
                    {
                        // Revenue is lost regardless of method
                        summary.TotalCashSales -= returnMaster.RefundAmount;
                        if (summary.TotalCashSales < 0) summary.TotalCashSales = 0;

                        // ONLY decrease physical cash if it's not a digital refund
                        if (!isDigital)
                        {
                            summary.TotalCash -= returnMaster.RefundAmount;
                        }
                        
                        _context.DailyTransactionSummaries.Update(summary);
                    }

                    // Create Audit Trail in InvoicePayments
                    var payment = new InvoicePayment
                    {
                        SaleId = returnMaster.SalesId,
                        PaymentDate = DateTime.Now,
                        Amount = -returnMaster.RefundAmount, // Negative to represent refund
                        PaymentMethodId = returnMaster.PaymentMethodId ?? 1, // Default to Cash if null
                        TransactionType = "Refund",
                        TransactionNo = "REF-" + returnMaster.ReturnNo,
                        Remarks = "Refund for Return No: " + returnMaster.ReturnNo
                    };
                    await _context.InvoicePayments.AddAsync(payment);
                }
                else if (returnMaster.RefundType == "Credit" || returnMaster.RefundType == "Wallet")
                {
                    var customer = await _context.Customers.FindAsync(returnMaster.CustomerId);
                    if (customer != null)
                    {
                        customer.AdvanceBalance += returnMaster.RefundAmount;
                        _context.Customers.Update(customer);
                    }
                }

                // 6. Adjust Daily Profit & Sales
                if (summary != null)
                {
                    // Profit lost = Revenue Lost - Cost Saved
                    decimal profitLost = returnMaster.TotalAmount - totalCostSaved;
                    summary.NetProfit -= profitLost;
                    
                    // Note: We don't necessarily have a "Total Sales" column to decrease, 
                    // but we adjusted CashSales/DueSales above.
                    _context.DailyTransactionSummaries.Update(summary);
                }

                await _context.SalesReturnMasters.AddAsync(returnMaster);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Sales Return Failed: " + ex.Message);
            }
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var r = await _context.SalesReturnMasters.FindAsync(id);
            if (r == null) return false;
            r.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
