using IPCS_Model.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPCS_API.Attributes;
using IPCS_Model.Constants;
using IPCS_Service.Interfaces;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class StockLedgerController : ControllerBase
{
    private readonly IStockLedgerService _ledgerService;

    public StockLedgerController(IStockLedgerService ledgerService)
    {
        _ledgerService = ledgerService;
    }

    [PermissionAuthorize(Permissions.Stock.ViewLedger)]
    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetByBranch(int branchId)
    {
        return Ok(await _ledgerService.GetAllByBranchAsync(branchId));
    }

    [HttpPost("entry")]
    public async Task<IActionResult> CreateEntry([FromBody] StockLedgerDTO model)
    {
        // How much purchase actual entry
        var result = await _ledgerService.AddLedgerEntryAsync(model);
        if (result) return Ok(new { Message = "Stock and ledger update Successfull..." });
        return BadRequest("Transection Error...");
    }
}