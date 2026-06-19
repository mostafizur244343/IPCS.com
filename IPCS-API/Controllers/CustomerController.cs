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
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Please Enter Valid Info...", Errors = ModelState });
            }

            var cust = new Customer
            {
                CustomerName = model.CustomerName,
                Mobile = model.Mobile,
                Address = model.Address,
                PicturePath = model.PicturePath,
                OpeningBalance = model.OpeningBalance,
                AdvanceBalance = model.AdvanceBalance,
                IsDue = model.IsDue,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name
            };
            await _custService.CreateAsync(cust);
            return Ok(new { Message = "Create Successfully..." });
        }
        catch (InvalidOperationException ex)
        {
            Serilog.Log.Error(ex, "Error creating customer");
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error creating customer");
            var errorMessage = ex.Message;
            var fullMessage = ex.ToString();

            if (ex.InnerException != null)
            {
                errorMessage = ex.InnerException.Message;
                if (ex.InnerException.Message.Contains("IX_Customer_Mobile", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { Message = "Duplicate Mobile Number" });
                }
            }
            return BadRequest(new { Message = errorMessage, FullError = fullMessage });
        }
    }

    [PermissionAuthorize(Permissions.Customer.Delete)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id) => Ok(await _custService.SoftDeleteAsync(id));

    [HttpPost("restore/{id}")]
    public async Task<IActionResult> Restore(int id) => Ok(await _custService.RestoreAsync(id));

    [PermissionAuthorize(Permissions.Customer.Edit)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CustomerDTO model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Please Enter Valid Info...", Errors = ModelState });
            }

            var cust = await _custService.GetByIdAsync(id);
            if (cust == null) return NotFound(new { Message = "Customer Not Found" });

            cust.CustomerName = model.CustomerName;
            cust.Mobile = model.Mobile;
            cust.Address = model.Address;
            cust.PicturePath = model.PicturePath;
            cust.OpeningBalance = model.OpeningBalance;
            cust.AdvanceBalance = model.AdvanceBalance;
            cust.IsDue = model.IsDue;
            cust.IsActive = model.IsActive;

            if (cust.IsDue)
            {
                cust.CurrentDue = cust.OpeningBalance;
                cust.AdvanceBalance = 0;
            }
            else
            {
                cust.AdvanceBalance = cust.OpeningBalance;
                cust.CurrentDue = 0;
            }

            await _custService.UpdateAsync(cust);
            return Ok(new { Message = "Update Successfully..." });
        }
        catch (InvalidOperationException ex)
        {
            Serilog.Log.Error(ex, "Error updating customer");
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error updating customer");
            var errorMessage = ex.Message;
            var fullMessage = ex.ToString();

            if (ex.InnerException != null)
            {
                errorMessage = ex.InnerException.Message;
                if (ex.InnerException.Message.Contains("IX_Customer_Mobile", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { Message = "Duplicate Mobile Number" });
                }
            }
            return BadRequest(new { Message = errorMessage, FullError = fullMessage });
        }
    }
}