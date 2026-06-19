using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPCS_API.Attributes;
using IPCS_Model.Constants;
using System.Linq;

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
            var results = await _locationService.GetAllAsync();
            var dtos = results.Select(MapToDTO);
            return Ok(dtos);
        }

        [PermissionAuthorize(Permissions.StoreLocation.View)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _locationService.GetByIdAsync(id);
            if (result == null) return NotFound("Location not found");
            return Ok(MapToDTO(result));
        }

        [PermissionAuthorize(Permissions.StoreLocation.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StoreLocationDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

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
                CreatedBy = User.Identity?.Name,
                CreatedDate = DateTime.Now
            };

            await _locationService.CreateAsync(loc);
            return Ok(new { Message = "Created successfully", LocationId = loc.LocationId });
        }

        [PermissionAuthorize(Permissions.StoreLocation.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StoreLocationDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var loc = await _locationService.GetByIdAsync(id);
            if (loc == null) return NotFound("Location not found");

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
            return Ok(new { Message = "Updated successfully" });
        }

        [PermissionAuthorize(Permissions.StoreLocation.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _locationService.DeleteAsync(id);
            return Ok(new { Message = "Deleted successfully" });
        }

        private static StoreLocationDTO MapToDTO(StoreLocation l) => new StoreLocationDTO
        {
            LocationId = l.LocationId,
            ShelfName = l.ShelfName,
            RowNumber = l.RowNumber,
            ColumnNumber = l.ColumnNumber,
            FloorNumber = l.FloorNumber,
            RoomNumber = l.RoomNumber,
            Capacity = l.Capacity,
            Notes = l.Notes,
            BranchId = l.BranchId,
            IsActive = l.IsActive
        };
    }
}