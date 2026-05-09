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
    public class SalesService : ISalesService
    {
        private readonly IPCSDBContext _context;

        public SalesService(IPCSDBContext context)
        {
            _context = context;
        }

        private async Task<string> GenerateInvoiceNoAsync()
        {
            var last = await _context.SalesMasters.IgnoreQueryFilters().AsNoTracking().OrderByDescending(s => s.SalesId).FirstOrDefaultAsync();
            int nextId = (last == null) ? 1 : last.SalesId + 1;
            return $"INV-{nextId:D6}";
        }

        public async Task<IEnumerable<SalesMaster>> GetAllActiveAsync()
        {
            return await _context.SalesMasters
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.Branch)
                .OrderByDescending(s => s.SalesDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SalesMaster>> GetDeletedListAsync()
        {
            return await _context.SalesMasters
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.Branch)
                .Where(s => s.IsDeleted)
                .ToListAsync();
        }

        public async Task<SalesMaster?> GetByIdAsync(int id)
        {
            return await _context.SalesMasters
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.Branch)
                .Include(s => s.SalesDetails)
                    .ThenInclude(d => d.Product)
                .Include(s => s.Payments)
                    .ThenInclude(p => p.PaymentMethod)
                .FirstOrDefaultAsync(s => s.SalesId == id);
        }

        public async Task<bool> CreateSalesAsync(SalesMaster salesMaster)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                salesMaster.InvoiceNo = await GenerateInvoiceNoAsync();
                decimal totalProfit = 0;

                // 0. Auto-calculate Paid, Due and Change Amount
                decimal totalPaidFromInput = salesMaster.Payments.Sum(p => p.Amount);
                if (totalPaidFromInput > salesMaster.NetAmount)
                {
                    salesMaster.ChangeAmount = totalPaidFromInput - salesMaster.NetAmount;
                    salesMaster.PaidAmount = salesMaster.NetAmount;
                    salesMaster.DueAmount = 0;
                    salesMaster.PaymentStatus = "Paid";

                    // Note: We don't subtract ChangeAmount from payments anymore 
                    // because we want to track the actual cash collected.
                }
                else
                {
                    salesMaster.ChangeAmount = 0;
                    salesMaster.PaidAmount = totalPaidFromInput;
                    salesMaster.DueAmount = salesMaster.NetAmount - totalPaidFromInput;
                    salesMaster.PaymentStatus = salesMaster.DueAmount <= 0 ? "Paid" : (salesMaster.PaidAmount > 0 ? "Partial" : "Due");
                }

                // 1. Process Stock & Details
                foreach (var detail in salesMaster.SalesDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product == null) continue;

                    // Skip stock logic for Services
                    if (!product.IsService)
                    {
                        // Validate Stock in Branch/Lot
                        var branchStock = await _context.BranchLotStocks
                            .FirstOrDefaultAsync(s => s.BranchId == salesMaster.BranchId && s.ProductId == detail.ProductId && s.LotId == detail.LotId);

                        if (branchStock == null || branchStock.CurrentStock < detail.Quantity)
                        {
                            throw new Exception($"Insufficient stock for {product.ProductName}. Available: {branchStock?.CurrentStock ?? 0}");
                        }

                        // Deduct Stock
                        branchStock.CurrentStock -= detail.Quantity;
                        _context.BranchLotStocks.Update(branchStock);

                        // Update Global Product Stock
                        product.CurrentStock -= detail.Quantity;
                        _context.Products.Update(product);

                        // Stock Ledger Entry
                        var lastLedger = await _context.StockLedgers
                            .AsNoTracking()
                            .Where(l => l.BranchId == salesMaster.BranchId && l.ProductId == detail.ProductId && l.LotId == detail.LotId)
                            .OrderByDescending(l => l.LedgerId)
                            .FirstOrDefaultAsync();

                        var ledger = new StockLedger
                        {
                            BranchId = salesMaster.BranchId,
                            ProductId = detail.ProductId,
                            LotId = detail.LotId,
                            TransactionType = "Sale",
                            ReferenceNo = salesMaster.InvoiceNo,
                            TransactionDate = DateTime.Now,
                            PreviousBalance = lastLedger?.CurrentBalance ?? 0,
                            QuantityIn = 0,
                            QuantityOut = detail.Quantity
                        };
                        await _context.StockLedgers.AddAsync(ledger);
                    }

                    // Get Cost Price from Lot for Profit Calculation (Services usually have 0 cost)
                    var lot = await _context.LotInfos.AsNoTracking().FirstOrDefaultAsync(l => l.LotId == detail.LotId);
                    detail.CostPriceAtSale = lot?.PurchasePrice ?? 0;
                    
                    // Simple Profit: Revenue - (Cost * Qty)
                    totalProfit += (detail.LineTotal - (detail.CostPriceAtSale * detail.Quantity));
                }

                // 2. Update Customer Due or Advance Credit
                var customer = await _context.Customers.FindAsync(salesMaster.CustomerId);
                if (customer != null)
                {
                    if (salesMaster.DueAmount > 0)
                    {
                        customer.CurrentDue += salesMaster.DueAmount;
                    }

                    if (salesMaster.ChangeAmount > 0 && salesMaster.IsChangeConvertedToCredit)
                    {
                        customer.AdvanceBalance += salesMaster.ChangeAmount;
                    }
                    
                    _context.Customers.Update(customer);
                }

                // 3. Update Daily Transaction Summary
                var today = DateTime.Now.Date;
                var summary = await _context.DailyTransactionSummaries.FirstOrDefaultAsync(d => d.TransactionDate == today && d.BranchId == salesMaster.BranchId);
                if (summary == null)
                {
                    summary = new DailyTransactionSummary 
                    { 
                        TransactionDate = today, 
                        BranchId = salesMaster.BranchId,
                        OpeningBalance = 0 // Should ideally fetch from yesterday
                    };
                    await _context.DailyTransactionSummaries.AddAsync(summary);
                }

                summary.TotalCashSales += salesMaster.PaidAmount; 
                
                // If Change is taken as income, add it to Cash Sales and Profit
                if (salesMaster.ChangeAmount > 0 && salesMaster.IsChangeTakenAsIncome)
                {
                    summary.TotalCashSales += salesMaster.ChangeAmount;
                    totalProfit += salesMaster.ChangeAmount; // Extra income is pure profit
                }

                summary.TotalDueSales += salesMaster.DueAmount;
                summary.NetProfit += totalProfit;

                // Update specific method buckets
                foreach(var payment in salesMaster.Payments)
                {
                    var method = await _context.PaymentMethods.AsNoTracking().FirstOrDefaultAsync(m => m.MethodId == payment.PaymentMethodId);
                    if (method != null)
                    {
                        string name = method.MethodName.ToLower();
                        if (name.Contains("cash")) summary.TotalCash += payment.Amount;
                        else if (name.Contains("bkash")) summary.TotalBkash += payment.Amount;
                        else if (name.Contains("nagad")) summary.TotalNagad += payment.Amount;
                        else if (name.Contains("card") || name.Contains("bank")) summary.TotalCard += payment.Amount;
                    }
                }

                // 4. Final Save
                await _context.SalesMasters.AddAsync(salesMaster);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Sales Error: " + ex.Message);
            }
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var sale = await _context.SalesMasters.FindAsync(id);
            if (sale == null) return false;
            sale.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var sale = await _context.SalesMasters.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SalesId == id);
            if (sale == null) return false;
            sale.IsDeleted = false;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
