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
    public class PaymentMethodController : ControllerBase
    {
        private readonly IPaymentMethodService _methodService;

        public PaymentMethodController(IPaymentMethodService methodService)
        {
            _methodService = methodService;
        }

        [PermissionAuthorize(Permissions.PaymentMethod.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try { return Ok(await _methodService.GetAllAsync()); }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _methodService.GetByIdAsync(id)); }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.PaymentMethod.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentMethodDTO model)
        {
            try
            {
                var method = new PaymentMethod
                {
                    MethodName = model.MethodName,
                    Description = model.Description,
                    IsDigital = model.IsDigital,
                    ExtraChargePercentage = model.ExtraChargePercentage,
                    AccountNumber = model.AccountNumber,
                    IconPath = model.IconPath,
                    QRCodePath = model.QRCodePath,
                    MinimumAmount = model.MinimumAmount,
                    IsActive = model.IsActive,
                    CreatedBy = User.Identity?.Name
                };
                await _methodService.CreateAsync(method);
                return Ok(new { Message = "Create Successfully..." });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.PaymentMethod.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentMethodDTO model)
        {
            try
            {
                var method = await _methodService.GetByIdAsync(id);
                if (method == null) return NotFound("Informetion Not Found...");

                method.MethodName = model.MethodName;
                method.Description = model.Description;
                method.IsDigital = model.IsDigital;
                method.ExtraChargePercentage = model.ExtraChargePercentage;
                method.AccountNumber = model.AccountNumber;
                method.IconPath = model.IconPath;
                method.QRCodePath = model.QRCodePath;
                method.MinimumAmount = model.MinimumAmount;
                method.IsActive = model.IsActive;

                await _methodService.UpdateAsync(method);
                return Ok(new { Message = "Update Successfully..." });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.PaymentMethod.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _methodService.DeleteAsync(id);
                return Ok(new { Message = " Delete Successfully..." });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }
    }
}