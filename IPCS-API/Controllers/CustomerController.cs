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
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _custService;
    public CustomerController(ICustomerService custService) { _custService = custService; }

    [PermissionAuthorize(Permissions.Customer.View)]
    [HttpGet("active")]
    public async Task<IActionResult> GetActive() => Ok(await _custService.GetAllActiveAsync());

    [HttpGet("trash")] // Deleted Customer LIst For Dashboard
    public async Task<IActionResult> GetDeleted() => Ok(await _custService.GetDeletedListAsync());

    [HttpGet("history/{id}")]
    public async Task<IActionResult> GetWithHistory(int id) => Ok(await _custService.GetWithSalesHistoryAsync(id));

    [PermissionAuthorize(Permissions.Customer.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(CustomerDTO model)
    {
        try
        {
            var cust = new Customer
            {
                CustomerName = model.CustomerName,
                Mobile = model.Mobile,
                Address = model.Address,
                OpeningBalance = model.OpeningBalance,
                AdvanceBalance = model.AdvanceBalance,
                IsDue = model.IsDue,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name
            };
            await _custService.CreateAsync(cust);
            return Ok(new { Message = "Create Successfully..." });
        }
        catch (Exception ex)
        {
            if (ex.InnerException?.Message.Contains("IX_Customer_Mobile") == true)
            {
                return BadRequest(new { Message = "Duplicate Mobile Number" });
            }
            return BadRequest(new { Message = ex.Message });
        }
    }

    [PermissionAuthorize(Permissions.Customer.Delete)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id) => Ok(await _custService.SoftDeleteAsync(id));

    [HttpPost("restore/{id}")]
    public async Task<IActionResult> Restore(int id) => Ok(await _custService.RestoreAsync(id));
}