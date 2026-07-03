using Microsoft.AspNetCore.Mvc;
using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using IPCS_API.Attributes;
using IPCS_Model.Constants;
using System.Linq;

namespace IPCS_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UOMController : ControllerBase
    {
        private readonly IUOMService _uomService;

        public UOMController(IUOMService uomService)
        {
            _uomService = uomService;
        }

        [PermissionAuthorize(Permissions.UOM.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var uoms = await _uomService.GetAllAsync();
            var dtos = uoms.Select(u => new UOMResponseDTO
            {
                UOMId = u.UOMId,
                UOMName = u.UOMName,
                Description = u.Description,
                IsActive = u.IsActive
            });
            return Ok(dtos);
        }

        [PermissionAuthorize(Permissions.UOM.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UOMDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var uom = new UOM
            {
                UOMName = model.UOMName,
                Description = model.Description,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name,
                CreatedDate = DateTime.Now
            };

            await _uomService.CreateAsync(uom);
            var response = new UOMResponseDTO
            {
                UOMId = uom.UOMId,
                UOMName = uom.UOMName,
                Description = uom.Description,
                IsActive = uom.IsActive
            };
            return Ok(response);
        }

        [PermissionAuthorize(Permissions.UOM.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UOMDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var uom = await _uomService.GetByIdAsync(id);
            if (uom == null) return NotFound("UOM not found");

            uom.UOMName = model.UOMName;
            uom.Description = model.Description;
            uom.IsActive = model.IsActive;

            await _uomService.UpdateAsync(uom);
            return Ok(new { Message = "Updated successfully" });
        }

        [PermissionAuthorize(Permissions.UOM.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _uomService.DeleteAsync(id);
            return Ok(new { Message = "Successfully deleted" });
        }
    }
}
