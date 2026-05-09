using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Model.Identity;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supService;
    public SupplierController(ISupplierService supService) { _supService = supService; }

    [PermissionAuthorize(Permissions.Supplier.View)]
    [HttpGet("active")]
    public async Task<IActionResult> GetActive() => Ok(await _supService.GetAllActiveAsync());

    [HttpGet("deleted-list")] 
    public async Task<IActionResult> GetDeleted() => Ok(await _supService.GetDeletedListAsync());

    [PermissionAuthorize(Permissions.Supplier.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(SupplierDTO model)
    {
        try
        {
            var sup = new Supplier
            {
                SupplierName = model.SupplierName,
                Mobile = model.Mobile,
                OpeningBalance = model.OpeningBalance,
                IsDue = model.IsDue,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name
            };
            await _supService.CreateAsync(sup);
            return Ok(new { Message = "Create Successfully" });
        }
        catch (Exception ex) { return BadRequest(new { Error = "Duplicate Number or Others Problem", Msg = ex.Message }); }
    }

    [PermissionAuthorize(Permissions.Supplier.Delete)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) => Ok(await _supService.SoftDeleteAsync(id));

    [HttpPost("restore/{id}")]
    public async Task<IActionResult> Restore(int id) => Ok(await _supService.RestoreAsync(id));
}