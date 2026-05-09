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
            var result = await _paymentService.ProcessSplitPaymentAsync(payments);
            if (result) return Ok(new { Message = "Payments Processed Successfully" });
            return BadRequest("Error Processing Payments");
        }

        [PermissionAuthorize(Permissions.Payment.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _paymentService.DeletePaymentAsync(id);
            if (result) return Ok(new { Message = "Payment Deleted" });
            return BadRequest("Error Deleting Payment");
        }
    }
}
