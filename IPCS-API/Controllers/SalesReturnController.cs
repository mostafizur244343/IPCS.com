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

    public class SalesReturnController : ControllerBase
    {
        private readonly ISalesReturnService _returnService;

        public SalesReturnController(ISalesReturnService returnService)
        {
            _returnService = returnService;
        }

        [PermissionAuthorize(Permissions.SalesReturn.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _returnService.GetAllActiveAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _returnService.GetByIdAsync(id);
            if (r == null) return NotFound();
            return Ok(r);
        }

        [PermissionAuthorize(Permissions.SalesReturn.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SalesReturnDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var returnMaster = new SalesReturnMaster
                {
                    ReturnDate = dto.ReturnDate,
                    SalesId = dto.SalesId,
                    CustomerId = dto.CustomerId,
                    BranchId = dto.BranchId,
                    TotalAmount = dto.TotalAmount,
                    RefundAmount = dto.RefundAmount,
                    RefundType = dto.RefundType,
                    PaymentMethodId = dto.PaymentMethodId,
                    Reason = dto.Reason,
                    CreatedBy = User.Identity?.Name,
                    ReturnDetails = dto.ReturnDetails.Select(d => new SalesReturnDetails
                    {
                        ProductId = d.ProductId,
                        LotId = d.LotId,
                        Quantity = d.Quantity,
                        UOMId = d.UOMId,
                        UnitPrice = d.UnitPrice,
                        LineTotal = d.LineTotal
                    }).ToList()
                };

                var result = await _returnService.CreateReturnAsync(returnMaster);
                if (result) return Ok(new { Message = "Sales Return Processed", ReturnNo = returnMaster.ReturnNo });

                return BadRequest("Failed to process sales return.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _returnService.SoftDeleteAsync(id);
            if (result) return Ok(new { Message = "Return record moved to trash" });
            return BadRequest("Error deleting record");
        }
    }
}
