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
    public class GenericInfoController : ControllerBase
    {
        private readonly IGenericInfoService _genericService;

        public GenericInfoController(IGenericInfoService genericService)
        {
            _genericService = genericService;
        }

        [PermissionAuthorize(Permissions.GenericInfo.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = await _genericService.GetAllAsync();
            var dtos = results.Select(MapToDTO);
            return Ok(dtos);
        }

        [PermissionAuthorize(Permissions.GenericInfo.View)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _genericService.GetByIdAsync(id);
            if (result == null) return NotFound("Information not found");
            return Ok(MapToDTO(result));
        }

        [PermissionAuthorize(Permissions.GenericInfo.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GenericInfoDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var info = new GenericInfo
            {
                GenericName = model.GenericName,
                Indication = model.Indication,
                SideEffects = model.SideEffects,
                ContraIndication = model.ContraIndication,
                DosageFormAdvice = model.DosageFormAdvice,
                DrugClass = model.DrugClass,
                PregnancyCategory = model.PregnancyCategory,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name,
                CreatedDate = DateTime.Now
            };

            await _genericService.CreateAsync(info);
            return Ok(new { Message = "Created successfully", GenericId = info.GenericId });
        }

        [PermissionAuthorize(Permissions.GenericInfo.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GenericInfoDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var info = await _genericService.GetByIdAsync(id);
            if (info == null) return NotFound("Information not found");

            info.GenericName = model.GenericName;
            info.Indication = model.Indication;
            info.SideEffects = model.SideEffects;
            info.ContraIndication = model.ContraIndication;
            info.DosageFormAdvice = model.DosageFormAdvice;
            info.DrugClass = model.DrugClass;
            info.PregnancyCategory = model.PregnancyCategory;
            info.IsActive = model.IsActive;

            await _genericService.UpdateAsync(info);
            return Ok(new { Message = "Updated successfully" });
        }

        [PermissionAuthorize(Permissions.GenericInfo.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _genericService.DeleteAsync(id);
            return Ok(new { Message = "Deleted successfully" });
        }

        private static GenericInfoDTO MapToDTO(GenericInfo g) => new GenericInfoDTO
        {
            GenericId = g.GenericId,
            GenericName = g.GenericName,
            Indication = g.Indication,
            SideEffects = g.SideEffects,
            ContraIndication = g.ContraIndication,
            DosageFormAdvice = g.DosageFormAdvice,
            DrugClass = g.DrugClass,
            PregnancyCategory = g.PregnancyCategory,
            IsActive = g.IsActive
        };
    }
}