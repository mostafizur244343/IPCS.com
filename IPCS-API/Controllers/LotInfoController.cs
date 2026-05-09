using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

namespace IPCS_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LotInfoController : ControllerBase
    {
        private readonly ILotInfoService _lotService;

        public LotInfoController(ILotInfoService lotService)
        {
            _lotService = lotService;
        }

        
        [PermissionAuthorize(Permissions.LotInfo.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var results = await _lotService.GetAllAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Loading List Error", Detail = ex.Message });
            }
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var lot = await _lotService.GetByIdAsync(id);
                if (lot == null) return NotFound(new { Message = "Not found this lot" });
                return Ok(lot);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // (Create)
        [PermissionAuthorize(Permissions.LotInfo.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LotInfoDTO model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Information not Valid");

                var lot = new LotInfo
                {
                    ProductId = model.ProductId,
                    LotNumber = model.LotNumber,
                    ManufacturingDate = model.ManufacturingDate,
                    ExpiryDate = model.ExpiryDate,
                    PurchasePrice = model.PurchasePrice,
                    IsActive = model.IsActive,
                    CreatedBy = User.Identity?.Name //Who Created?
                };

                var success = await _lotService.CreateAsync(lot);
                if (success) return Ok(new { Message = "Create Successfully..." });

                return BadRequest("Create UnSuccessfull");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Save Error...", Detail = ex.Message });
            }
        }

        // (Update)
        [PermissionAuthorize(Permissions.LotInfo.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LotInfoDTO model)
        {
            try
            {
                var existingLot = await _lotService.GetByIdAsync(id);
                if (existingLot == null) return NotFound(new { Message = "Lot Not Found For Update" });

                // Update Fields
                existingLot.ProductId = model.ProductId;
                existingLot.LotNumber = model.LotNumber;
                existingLot.ManufacturingDate = model.ManufacturingDate;
                existingLot.ExpiryDate = model.ExpiryDate;
                existingLot.PurchasePrice = model.PurchasePrice;
                existingLot.IsActive = model.IsActive;

                var success = await _lotService.UpdateAsync(existingLot);
                if (success) return Ok(new { Message = "Lot Update Successfully..." });

                return BadRequest("Update Not Successfull");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Update Error...", Detail = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.LotInfo.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _lotService.DeleteAsync(id);
                if (!success) return NotFound(new { Message = "Not found for delete..." });

                return Ok(new { Message = "Delete Successfully..." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error Deleting", Detail = ex.Message });
            }
        }
    }
}