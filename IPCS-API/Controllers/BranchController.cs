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
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        [PermissionAuthorize(Permissions.Branch.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try { return Ok(await _branchService.GetAllAsync()); }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try 
            { 
                var branch = await _branchService.GetByIdWithReportingAsync(id);
                if (branch == null) return NotFound();
                return Ok(branch); 
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.Branch.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BranchDTO model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Please input valid info");

                var branch = new Branch
                {
                    BranchName = model.BranchName,
                    Address = model.Address,
                    ContactNumber = model.ContactNumber,
                    Email = model.Email,
                    ManagerName = model.ManagerName,
                    IsActive = model.IsActive,
                    CreatedBy = User.Identity?.Name
                };

                await _branchService.CreateAsync(branch);
                return Ok(new { Message = "Successfully Created" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.Branch.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BranchDTO model)
        {
            try
            {
                var branch = await _branchService.GetByIdAsync(id);
                if (branch == null) return NotFound("Branch not Found");

                branch.BranchName = model.BranchName;
                branch.Address = model.Address;
                branch.ContactNumber = model.ContactNumber;
                branch.Email = model.Email;
                branch.ManagerName = model.ManagerName;
                branch.IsActive = model.IsActive;

                await _branchService.UpdateAsync(branch);
                return Ok(new { Message = "Update Successfully..." });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }

        [PermissionAuthorize(Permissions.Branch.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _branchService.DeleteAsync(id);
                return Ok(new { Message = "Successfully Deleted" });
            }
            catch (Exception ex) { return BadRequest(new { Message = ex.Message }); }
        }
    }
}