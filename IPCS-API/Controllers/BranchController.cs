using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPCS_API.Attributes;
using IPCS_Model.Constants;
using System.Linq;
using System;
using System.Threading.Tasks;

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
            var results = await _branchService.GetAllAsync();
            return Ok(results);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var branch = await _branchService.GetByIdWithReportingAsync(id);
            if (branch == null) return NotFound("Branch not found");
            return Ok(branch);
        }

        [PermissionAuthorize(Permissions.Branch.Create)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BranchDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var branch = new Branch
            {
                BranchName = model.BranchName,
                Address = model.Address,
                ContactNumber = model.ContactNumber,
                Email = model.Email,
                ManagerName = model.ManagerName,
                PicturePath = model.PicturePath,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name,
                CreatedDate = DateTime.Now
            };

            await _branchService.CreateAsync(branch);
            return Ok(new { Message = "Created successfully", BranchCode = branch.BranchCode });
        }

        [PermissionAuthorize(Permissions.Branch.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BranchDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var branch = await _branchService.GetByIdAsync(id);
            if (branch == null) return NotFound("Branch not found");

            branch.BranchName = model.BranchName;
            branch.Address = model.Address;
            branch.ContactNumber = model.ContactNumber;
            branch.Email = model.Email;
            branch.ManagerName = model.ManagerName;
            branch.PicturePath = model.PicturePath;
            branch.IsActive = model.IsActive;

            await _branchService.UpdateAsync(branch);
            return Ok(new { Message = "Updated successfully" });
        }

        [PermissionAuthorize(Permissions.Branch.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _branchService.DeleteAsync(id);
            return Ok(new { Message = "Deleted successfully" });
        }
    }
}