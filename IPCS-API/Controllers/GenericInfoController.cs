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
            try { return Ok(await _genericService.GetAllAsync()); }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.GenericInfo.View)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _genericService.GetByIdAsync(id)); }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.GenericInfo.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GenericInfoDTO model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Wrong Information");

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
                    CreatedBy = User.Identity?.Name
                };

                await _genericService.CreateAsync(info);
                return Ok(new { Message = "Successfully Created" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.GenericInfo.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GenericInfoDTO model)
        {
            try
            {
                var info = await _genericService.GetByIdAsync(id);
                if (info == null) return NotFound("Not Found this Information");

                info.GenericName = model.GenericName;
                info.Indication = model.Indication;
                info.SideEffects = model.SideEffects;
                info.ContraIndication = model.ContraIndication;
                info.DosageFormAdvice = model.DosageFormAdvice;
                info.DrugClass = model.DrugClass;
                info.PregnancyCategory = model.PregnancyCategory;
                info.IsActive = model.IsActive;

                await _genericService.UpdateAsync(info);
                return Ok(new { Message = "Successfully Updated" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.GenericInfo.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _genericService.DeleteAsync(id);
                return Ok(new { Message = "Delete Successfully" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }
    }
}