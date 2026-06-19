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
            var manufacturers = await _mfgService.GetAllAsync();
            var dtos = manufacturers.Select(m => new ManufacturerDTO
            {
                BrandId = m.BrandId,
                BrandName = m.BrandName,
                Origin = m.Origin,
                ContactPerson = m.ContactPerson,
                PhoneNumber = m.PhoneNumber,
                IsActive = m.IsActive
            });
            return Ok(dtos);
        }

        [PermissionAuthorize(Permissions.Manufacturer.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ManufacturerDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var mfg = new Manufacturer
            {
                BrandName = model.BrandName,
                Origin = model.Origin,
                ContactPerson = model.ContactPerson,
                PhoneNumber = model.PhoneNumber,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name,
                CreatedDate = DateTime.Now
            };

            await _mfgService.CreateAsync(mfg);
            model.BrandId = mfg.BrandId;
            return Ok(model);
        }

        [PermissionAuthorize(Permissions.Manufacturer.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ManufacturerDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var brand = await _mfgService.GetByIdAsync(id);
            if (brand == null) return NotFound("Manufacturer not found");

            brand.BrandName = model.BrandName;
            brand.Origin = model.Origin;
            brand.ContactPerson = model.ContactPerson;
            brand.PhoneNumber = model.PhoneNumber;
            brand.IsActive = model.IsActive;

            await _mfgService.UpdateAsync(brand);
            return Ok(new { Message = "Updated successfully" });
        }

        [PermissionAuthorize(Permissions.Manufacturer.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _mfgService.DeleteAsync(id);
            return Ok(new { Message = "Successfully deleted" });
        }
    }
}