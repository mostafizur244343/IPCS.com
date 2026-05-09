using Microsoft.AspNetCore.Mvc;
using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

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
            try
            {
                var uoms = await _uomService.GetAllAsync();
                return Ok(uoms);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.UOM.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UOMDTO model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Invalid Input Format");

                var uom = new UOM
                {
                    UOMName = model.UOMName,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedBy = User.Identity?.Name
                };

                await _uomService.CreateAsync(uom);
                return Ok(new { Message = "UOM Created Successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [PermissionAuthorize(Permissions.UOM.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UOMDTO model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Invalid Input Format");

                var existingUom = await _uomService.GetByIdAsync(id);
                if (existingUom == null)
                {
                    return NotFound(new { Message = "Don't find this UOM" });
                }

                // Property For Update Table
                existingUom.UOMName = model.UOMName;
                existingUom.Description = model.Description;
                existingUom.IsActive = model.IsActive;
                // CreatedBy Don;t keep now

                await _uomService.UpdateAsync(existingUom);
                return Ok(new { Message = "UOM Updated Successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.UOM.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _uomService.DeleteAsync(id);
                if (result)
                {
                    return Ok(new { Message = "UOM Deleted Successfully" });
                }
                return BadRequest(new { Message = "UOM Delete Unsuccessful" });
            }
            catch (Exception ex)
            {
                // Now Show the Custom exception messsege in a service
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
