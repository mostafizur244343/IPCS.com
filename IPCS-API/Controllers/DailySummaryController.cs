using IPCS_Model.DTOs;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DailySummaryController : ControllerBase
{
    private readonly IDailySummaryService _summaryService;
    public DailySummaryController(IDailySummaryService summaryService) { _summaryService = summaryService; }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert(DailySummaryDTO model) => Ok(await _summaryService.UpsertDailySummaryAsync(model));

    [PermissionAuthorize(Permissions.Administration.ViewDailySummary)]
    [HttpGet("monthly/{branchId}/{year}/{month}")]
    public async Task<IActionResult> GetMonthly(int branchId, int year, int month) => Ok(await _summaryService.GetMonthlyReportAsync(branchId, year, month));

    [HttpGet("by-date/{branchId}/{date}")]
    public async Task<IActionResult> GetByDate(int branchId, DateTime date) => Ok(await _summaryService.GetByDateAsync(branchId, date));

    [HttpGet("inventory-stats")]
    public async Task<IActionResult> GetInventoryStats() => Ok(await _summaryService.GetInventoryStatsAsync());
}