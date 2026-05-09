using Microsoft.AspNetCore.Http;
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
    public class ManufacturerController : ControllerBase
    {
        private readonly IManufacturerService _mfgService;

        public ManufacturerController(IManufacturerService mfgService)
        {
            _mfgService = mfgService;
        }

        [PermissionAuthorize(Permissions.Manufacturer.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try { return Ok(await _mfgService.GetAllAsync()); }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.Manufacturer.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ManufacturerDTO model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Please input a valid info");
                var mfg = new Manufacturer
                {
                    BrandName = model.BrandName,
                    Origin = model.Origin,
                    ContactPerson = model.ContactPerson,
                    PhoneNumber = model.PhoneNumber,
                    IsActive = model.IsActive,
                    CreatedBy = User.Identity?.Name
                };
                await _mfgService.CreateAsync(mfg);
                return Ok(new { Message = "Successfully Created" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.Manufacturer.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ManufacturerDTO model)
        {
            try
            {
                var brand = await _mfgService.GetByIdAsync(id);
                if (brand == null) return NotFound("No Info Found");

                brand.BrandName = model.BrandName;
                brand.Origin = model.Origin;
                brand.ContactPerson = model.ContactPerson;
                brand.PhoneNumber = model.PhoneNumber;
                brand.IsActive = model.IsActive;

                await _mfgService.UpdateAsync(brand);
                return Ok(new { Message = "Successfully Updated" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.Manufacturer.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _mfgService.DeleteAsync(id);
                return Ok(new { Message = "Delete Successfull" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }
    }
}