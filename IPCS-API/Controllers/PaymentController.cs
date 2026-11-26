using IPCS_Model.DTOs;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

namespace IPCS_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [PermissionAuthorize(Permissions.Payment.View)]
        [HttpGet("{type}/{invoiceId}")]
        public async Task<IActionResult> GetByInvoice(string type, int invoiceId)
        {
            return Ok(await _paymentService.GetPaymentsByInvoiceAsync(type, invoiceId));
        }

        [PermissionAuthorize(Permissions.Payment.Create)]
        [HttpPost("process-split")]
        public async Task<IActionResult> ProcessSplit([FromBody] List<InvoicePaymentDTO> payments)
        {
            try
            {
                var result = await _paymentService.ProcessSplitPaymentAsync(payments);
                if (result) return Ok(new { Message = "Payments Processed Successfully" });
                return BadRequest("Error Processing Payments");
            }
            catch (Exception ex)
            {
                // Guard rejections (e.g. a split against a deleted invoice)
                // surface as a clear 400, not a 500.
                return BadRequest(new { Message = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Payment.Create)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHeader(int id, [FromBody] PaymentHeaderUpdateDTO dto)
        {
            // Header-only: Remarks / TransactionNo. Amount, payment method and
            // invoice links are not editable through this endpoint.
            // (No Payment.Edit permission exists, so Create guards this edit.)
            var result = await _paymentService.UpdateHeaderAsync(id, dto);
            if (!result) return NotFound(new { Message = "Payment not found" });
            return Ok(new { Message = "Payment header updated successfully" });
        }

        [PermissionAuthorize(Permissions.Payment.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _paymentService.DeletePaymentAsync(id);
            if (result) return Ok(new { Message = "Payment Deleted" });
            return BadRequest("Error Deleting Payment");
        }

        [PermissionAuthorize(Permissions.Payment.View)]
        [HttpGet("trash")]
        public async Task<IActionResult> GetTrash() => Ok(await _paymentService.GetDeletedPaymentsAsync());

        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var result = await _paymentService.RestorePaymentAsync(id);
                if (result) return Ok(new { Message = "Payment restored successfully" });
                return NotFound(new { Message = "Payment not found or not deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
