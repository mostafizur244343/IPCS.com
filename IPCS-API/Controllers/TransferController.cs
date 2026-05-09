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
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;

        public TransferController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        // ================= Requisition APIs =================

        [PermissionAuthorize(Permissions.Transfer.View)]
        [HttpGet("requisition")]
        public async Task<IActionResult> GetAllRequisitions()
        {
            try { return Ok(await _transferService.GetAllRequisitionsAsync()); }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [HttpGet("requisition/{id}")]
        public async Task<IActionResult> GetRequisitionById(int id)
        {
            try
            {
                var req = await _transferService.GetRequisitionByIdAsync(id);
                if (req == null) return NotFound();
                return Ok(req);
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.Transfer.Create)]
        [HttpPost("requisition")]
        public async Task<IActionResult> CreateRequisition([FromBody] TransferRequisitionDTO dto)
        {
            try
            {
                var req = new TransferRequisition
                {
                    FromBranchId = dto.FromBranchId,
                    ToBranchId = dto.ToBranchId,
                    ExpectedDate = dto.ExpectedDate,
                    Remarks = dto.Remarks,
                    CreatedBy = User.Identity?.Name,
                    RequisitionDetails = dto.Details.Select(d => new TransferRequisitionDetails
                    {
                        ProductId = d.ProductId,
                        RequestQty = d.RequestQty,
                        UOMId = d.UOMId
                    }).ToList()
                };

                await _transferService.CreateRequisitionAsync(req);
                return Ok(new { Message = "Requisition Created Successfully" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }


        // ================= Transfer APIs =================

        [PermissionAuthorize(Permissions.Transfer.View)]
        [HttpGet]
        public async Task<IActionResult> GetAllTransfers()
        {
            try { return Ok(await _transferService.GetAllTransfersAsync()); }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransferById(int id)
        {
            try
            {
                var transfer = await _transferService.GetTransferByIdAsync(id);
                if (transfer == null) return NotFound(new { Message = "Transfer not found" });
                return Ok(transfer);
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.Transfer.Create)]
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateTransfer([FromBody] TransferMasterDTO dto)
        {
            try
            {
                var transfer = new TransferMaster
                {
                    RequisitionId = dto.RequisitionId == 0 ? null : dto.RequisitionId,
                    FromBranchId = dto.FromBranchId,
                    ToBranchId = dto.ToBranchId,
                    TransferDate = dto.TransferDate,
                    Remarks = dto.Remarks,
                    ShippingCharge = dto.ShippingCharge,
                    CreatedBy = User.Identity?.Name,
                    TransferDetails = dto.Details.Select(d => new TransferDetails
                    {
                        ProductId = d.ProductId,
                        UOMId = d.UOMId,
                        LotId = d.LotId,
                        TransferQty = d.TransferQty,
                        UnitPrice = d.UnitPrice
                    }).ToList()
                };

                await _transferService.InitiateTransferAsync(transfer);
                return Ok(new { Message = "Transfer Initiated Successfully. Status: Pending" });
            }
            catch (Exception ex) { return BadRequest(new { Message = "Error", Details = ex.Message }); }
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditTransfer(int id, [FromBody] TransferMasterDTO dto)
        {
            try
            {
                var transfer = new TransferMaster
                {
                    TransferId = id,
                    ToBranchId = dto.ToBranchId,
                    Remarks = dto.Remarks,
                    TransferDetails = dto.Details.Select(d => new TransferDetails
                    {
                        ProductId = d.ProductId,
                        UOMId = d.UOMId,
                        LotId = d.LotId,
                        TransferQty = d.TransferQty,
                        UnitPrice = d.UnitPrice
                    }).ToList()
                };

                var success = await _transferService.EditPendingTransferAsync(transfer);
                if (!success) return BadRequest(new { Message = "Cannot edit transfer. Ensure it exists and is Pending." });

                return Ok(new { Message = "Transfer Edited Successfully." });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.Transfer.Approve)]
        [HttpPost("confirm-receive")]
        public async Task<IActionResult> ConfirmReceive([FromBody] TransferReceiveDTO dto)
        {
            try
            {
                var success = await _transferService.ConfirmGoodsReceivedAsync(dto, User.Identity?.Name ?? "System");
                if (!success) return BadRequest(new { Message = "Cannot confirm. Ensure transfer exists and is In-Transit." });
                return Ok(new { Message = "Goods Received Confirmed. Stock Updated (including damage refund if any)." });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelTransfer(int id)
        {
            try
            {
                var success = await _transferService.CancelTransferAsync(id);
                if (!success) return BadRequest(new { Message = "Cannot cancel. Ensure transfer exists and is Pending." });
                return Ok(new { Message = "Transfer Canceled. Stock is not yet returned until Main branch confirms." });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.Transfer.Approve)]
        [HttpPost("confirm-cancel-return/{id}")]
        public async Task<IActionResult> ConfirmCancelReturn(int id)
        {
            try
            {
                var success = await _transferService.ConfirmCancelReturnAsync(id);
                if (!success) return BadRequest(new { Message = "Cannot confirm cancel return. Ensure status is Canceled." });
                return Ok(new { Message = "Cancel Return Confirmed. Stock restored to original branch." });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            try
            {
                var success = await _transferService.SoftDeleteTransferAsync(id);
                if (!success) return NotFound();
                return Ok(new { Message = "Transfer Deleted Softly." });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }
    }
}
