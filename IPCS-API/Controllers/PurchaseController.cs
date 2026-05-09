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
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [PermissionAuthorize(Permissions.Purchase.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _purchaseService.GetAllActiveAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Purchase.View)]
        [HttpGet("trash")]
        public async Task<IActionResult> GetTrash()
        {
            try
            {
                var list = await _purchaseService.GetDeletedListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Purchase.View)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var purchase = await _purchaseService.GetByIdAsync(id);
                if (purchase == null) return NotFound(new { Message = "Purchase Not Found" });
                return Ok(purchase);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Purchase.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PurchaseMasterDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var purchaseMaster = new PurchaseMaster
                {
                    SupplierInvoiceNo = dto.SupplierInvoiceNo,
                    PurchaseDate = dto.PurchaseDate,
                    BranchId = dto.BranchId,
                    SupplierId = dto.SupplierId,
                    MethodId = dto.MethodId,
                    TotalAmount = dto.TotalAmount,
                    DiscountAmount = dto.DiscountAmount,
                    NetAmount = dto.NetAmount,
                    PaidAmount = dto.PaidAmount,
                    DueAmount = dto.DueAmount,
                    PaymentStatus = dto.PaymentStatus,
                    IsShipment = dto.IsShipment,
                    Remarks = dto.Remarks,
                    CreatedBy = User.Identity?.Name,
                    PurchaseDetails = dto.PurchaseDetails.Select(d => new PurchaseDetails
                    {
                        ProductId = d.ProductId,
                        BatchNo = d.BatchNo,
                        ExpiryDate = d.ExpiryDate,
                        Qty = d.Qty,
                        FreeQty = d.FreeQty,
                        UOMId = d.UOMId,
                        IsInBox = d.IsInBox,
                        ConversionFactor = d.ConversionFactor,
                        PurchasePrice = d.PurchasePrice,
                        MRP = d.MRP,
                        LineTotal = d.LineTotal
                    }).ToList(),
                    Payments = dto.Payments.Select(p => new InvoicePayment
                    {
                        TransactionType = "Purchase",
                        PaymentMethodId = p.PaymentMethodId,
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        TransactionNo = p.TransactionNo,
                        Remarks = p.Remarks
                    }).ToList()
                };

                await _purchaseService.CreatePurchaseAsync(purchaseMaster);
                return Ok(new { Message = "Purchase Created Successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Purchase Creation Failed", Error = ex.Message });
            }
        }

        [HttpPost("receive-shipment/{id}")]
        public async Task<IActionResult> ReceiveShipment(int id)
        {
            try
            {
                var success = await _purchaseService.ReceiveShipmentAsync(id);
                if (!success) return BadRequest(new { Message = "Cannot receive shipment. It might be already received or not found." });

                return Ok(new { Message = "Shipment received and Good Received status updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Purchase.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            try
            {
                var result = await _purchaseService.SoftDeleteAsync(id);
                if (!result) return NotFound();
                return Ok(new { Message = "Purchase Deleted Successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var result = await _purchaseService.RestoreAsync(id);
                if (!result) return NotFound();
                return Ok(new { Message = "Purchase Restored Successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
