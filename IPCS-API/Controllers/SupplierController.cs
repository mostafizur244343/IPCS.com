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
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supService;

        public SupplierController(ISupplierService supService)
        {
            _supService = supService;
        }

        [PermissionAuthorize(Permissions.Supplier.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _supService.GetAllActiveAsync();
            var dtos = suppliers.Select(MapToDTO);
            return Ok(dtos);
        }

        [PermissionAuthorize(Permissions.Supplier.View)]
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var suppliers = await _supService.GetAllActiveAsync();
            var dtos = suppliers.Select(MapToDTO);
            return Ok(dtos);
        }

        [HttpGet("deleted-list")]
        public async Task<IActionResult> GetDeleted()
        {
            var suppliers = await _supService.GetDeletedListAsync();
            var dtos = suppliers.Select(MapToDTO);
            return Ok(dtos);
        }

        [PermissionAuthorize(Permissions.Supplier.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SupplierDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var sup = new Supplier
            {
                SupplierName = model.SupplierName,
                Mobile = model.Mobile,
                PicturePath = model.PicturePath,
                OpeningBalance = model.OpeningBalance,
                IsDue = model.IsDue,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name,
                CreatedDate = DateTime.Now
            };

            await _supService.CreateAsync(sup);
            return Ok(new { Message = "Created successfully", SupplierCode = sup.SupplierCode });
        }

        [PermissionAuthorize(Permissions.Supplier.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SupplierDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _supService.GetByIdAsync(id);
            if (existing == null) return NotFound("Supplier not found");

            existing.SupplierName = model.SupplierName;
            existing.Mobile = model.Mobile;
            existing.PicturePath = model.PicturePath;
            existing.OpeningBalance = model.OpeningBalance;
            existing.IsDue = model.IsDue;
            existing.IsActive = model.IsActive;

            await _supService.UpdateAsync(existing);
            return Ok(new { Message = "Updated successfully" });
        }

        [PermissionAuthorize(Permissions.Supplier.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _supService.SoftDeleteAsync(id);
            return Ok(new { Message = "Deleted successfully" });
        }

        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            await _supService.RestoreAsync(id);
            return Ok(new { Message = "Restored successfully" });
        }

        private static SupplierResponseDTO MapToDTO(Supplier s) => new SupplierResponseDTO
        {
            SupplierId = s.SupplierId,
            SupplierCode = s.SupplierCode,
            SupplierName = s.SupplierName,
            Mobile = s.Mobile,
            PicturePath = s.PicturePath,
            OpeningBalance = s.OpeningBalance,
            IsDue = s.IsDue,
            IsActive = s.IsActive
        };
    }
}