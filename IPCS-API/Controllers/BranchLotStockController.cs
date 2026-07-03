using IPCS_Model.DTOs;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BranchLotStockController : ControllerBase
{
    private readonly IBranchLotStockService _stockService;

    public BranchLotStockController(IBranchLotStockService stockService)
    {
        _stockService = stockService;
    }

    [PermissionAuthorize(Permissions.Stock.ViewBranchStock)]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll() => Ok(await _stockService.GetAllAsync());

    [HttpGet("low-stock/{branchId}")]
    public async Task<IActionResult> GetLowStock(int branchId) => Ok(await _stockService.GetLowStockAlertAsync(branchId));

    [HttpGet("product/{productId}/branch/{branchId}")]
    public async Task<IActionResult> GetLotsByProductAndBranch(int productId, int branchId) 
        => Ok(await _stockService.GetActiveLotsByProductAndBranchAsync(productId, branchId));

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] BranchLotStockDTO model)
    {
        var result = await _stockService.AdjustStockAsync(model);
        if (result) return Ok(new { Message = "Stock Adjustment Successfull..." });
        return BadRequest("Adjustment Error");
    }
}