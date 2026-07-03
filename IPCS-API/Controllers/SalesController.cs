using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

namespace IPCS_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class SalesController : ControllerBase
    {
        private readonly ISalesService _salesService;

        public SalesController(ISalesService salesService)
        {
            _salesService = salesService;
        }

        [PermissionAuthorize(Permissions.Sales.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var sales = await _salesService.GetAllActiveAsync();
                var data = sales.Select(s => new SalesListDTO
                {
                    SalesId = s.SalesId,
                    InvoiceNo = s.InvoiceNo,
                    SalesDate = s.SalesDate,
                    CustomerName = s.Customer?.CustomerName ?? "Walk-in Customer",
                    TotalAmount = s.TotalAmount,
                    NetAmount = s.NetAmount,
                    PaidAmount = s.PaidAmount,
                    DueAmount = s.DueAmount,
                    PaymentStatus = s.PaymentStatus
                }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Sales List Error: " + ex.Message, Trace = ex.StackTrace });
            }
        }

        [HttpGet("trash")]
        public async Task<IActionResult> GetTrash()
        {
            var sales = await _salesService.GetDeletedListAsync();
            return Ok(sales.Select(s => new SalesListDTO
            {
                SalesId = s.SalesId,
                InvoiceNo = s.InvoiceNo,
                SalesDate = s.SalesDate,
                CustomerName = s.Customer?.CustomerName ?? "Walk-in Customer",
                TotalAmount = s.TotalAmount,
                NetAmount = s.NetAmount,
                PaidAmount = s.PaidAmount,
                DueAmount = s.DueAmount,
                PaymentStatus = s.PaymentStatus
            }).ToList());
        }

        [PermissionAuthorize(Permissions.Sales.View)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _salesService.GetByIdAsync(id);
            if (s == null) return NotFound("Sale not found");

            var dto = new SalesMasterDTO
            {
                SalesId = s.SalesId,
                InvoiceNo = s.InvoiceNo,
                SalesDate = s.SalesDate,
                BranchId = s.BranchId,
                CustomerId = s.CustomerId,
                TotalAmount = s.TotalAmount,
                DiscountAmount = s.DiscountAmount,
                NetAmount = s.NetAmount,
                PaidAmount = s.PaidAmount,
                DueAmount = s.DueAmount,
                ChangeAmount = s.ChangeAmount,
                IsChangeConvertedToCredit = s.IsChangeConvertedToCredit,
                IsChangeTakenAsIncome = s.IsChangeTakenAsIncome,
                PaymentStatus = s.PaymentStatus,
                Remarks = s.Remarks,

                SalesDetails = s.SalesDetails.Select(d => new SalesDetailsDTO
                {
                    ProductId = d.ProductId,
                    ProductName = d.Product?.ProductName,
                    LotId = d.LotId,
                    Quantity = d.Quantity,
                    UOMId = d.UOMId,
                    UomName = d.UOM?.UOMName,
                    UnitPrice = d.UnitPrice,
                    DiscountPerUnit = d.DiscountPerUnit,
                    LineTotal = d.LineTotal
                }).ToList(),

                Payments = s.Payments.Select(p => new InvoicePaymentDTO
                {
                    PaymentId = p.PaymentId,
                    PaymentMethodId = p.PaymentMethodId,
                    PaymentMethodName = p.PaymentMethod?.MethodName,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    TransactionNo = p.TransactionNo,
                    Remarks = p.Remarks
                }).ToList()
            };

            return Ok(dto);
        }

        [PermissionAuthorize(Permissions.Sales.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SalesMasterDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // Mapping DTO to Entity
                var salesMaster = new SalesMaster
                {
                    SalesDate = dto.SalesDate,
                    BranchId = dto.BranchId,
                    CustomerId = dto.CustomerId ?? 1, // Defaulting to 1 (usually Walk-in) if nothing selected, but UI should provide it
                    TotalAmount = dto.TotalAmount,
                    DiscountAmount = dto.DiscountAmount,
                    NetAmount = dto.NetAmount,
                    PaidAmount = dto.PaidAmount,
                    DueAmount = dto.DueAmount,
                    ChangeAmount = dto.ChangeAmount,
                    IsChangeConvertedToCredit = dto.IsChangeConvertedToCredit,
                    IsChangeTakenAsIncome = dto.IsChangeTakenAsIncome,
                    PaymentStatus = dto.PaymentStatus,
                    Remarks = dto.Remarks,
                    CreatedBy = User.Identity?.Name,
                    
                    SalesDetails = dto.SalesDetails.Select(d => new SalesDetails
                    {
                        ProductId = d.ProductId,
                        LotId = d.LotId,
                        Quantity = d.Quantity,
                        UOMId = d.UOMId ?? 0,
                        UnitPrice = d.UnitPrice,
                        DiscountPerUnit = d.DiscountPerUnit,
                        LineTotal = d.LineTotal
                    }).ToList(),

                    Payments = dto.Payments.Select(p => new InvoicePayment
                    {
                        TransactionType = "Sale",
                        PaymentMethodId = p.PaymentMethodId,
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        TransactionNo = p.TransactionNo,
                        Remarks = p.Remarks,
                        CreatedBy = User.Identity?.Name
                    }).ToList()
                };

                var result = await _salesService.CreateSalesAsync(salesMaster);
                if (result) return Ok(new { Message = "Sale Completed Successfully", InvoiceNo = salesMaster.InvoiceNo });
                
                return BadRequest("Error completing sale");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Sales.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _salesService.SoftDeleteAsync(id);
            if (result) return Ok(new { Message = "Sale put in trash" });
            return BadRequest("Error deleting sale");
        }

        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _salesService.RestoreAsync(id);
            if (result) return Ok(new { Message = "Sale restored" });
            return BadRequest("Error restoring sale");
        }
    }
}
