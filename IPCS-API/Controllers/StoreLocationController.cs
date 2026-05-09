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
    public class StoreLocationController : ControllerBase
    {
        private readonly IStoreLocationService _locationService;

        public StoreLocationController(IStoreLocationService locationService)
        {
            _locationService = locationService;
        }

        [PermissionAuthorize(Permissions.StoreLocation.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try { return Ok(await _locationService.GetAllAsync()); }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.StoreLocation.View)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _locationService.GetByIdAsync(id)); }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.StoreLocation.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StoreLocationDTO model)
        {
            try
            {
                var loc = new StoreLocation
                {
                    ShelfName = model.ShelfName,
                    RowNumber = model.RowNumber,
                    ColumnNumber = model.ColumnNumber,
                    FloorNumber = model.FloorNumber,
                    RoomNumber = model.RoomNumber,
                    Capacity = model.Capacity,
                    Notes = model.Notes,
                    BranchId = model.BranchId, 
                    IsActive = model.IsActive,
                    CreatedBy = User.Identity?.Name
                };
                await _locationService.CreateAsync(loc);
                return Ok(new { Message = "Saved Location info Successfully" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.StoreLocation.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StoreLocationDTO model)
        {
            try
            {
                var loc = await _locationService.GetByIdAsync(id);
                if (loc == null) return NotFound("Not found any Info");

                loc.ShelfName = model.ShelfName;
                loc.RowNumber = model.RowNumber;
                loc.ColumnNumber = model.ColumnNumber;
                loc.FloorNumber = model.FloorNumber;
                loc.RoomNumber = model.RoomNumber;
                loc.Capacity = model.Capacity;
                loc.Notes = model.Notes;
                loc.BranchId = model.BranchId;
                loc.IsActive = model.IsActive;

                await _locationService.UpdateAsync(loc);
                return Ok(new { Message = "Updated Successfully" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.StoreLocation.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _locationService.DeleteAsync(id);
                return Ok(new { Message = "Successfully Deleted." });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }
    }
}