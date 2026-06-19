using IPCS_Model.DTOs;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

namespace IPCS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // In production, only Admin should manage roles
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [PermissionAuthorize(Permissions.Administration.ManageRoles)]
        [HttpGet("get-all-roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [PermissionAuthorize(Permissions.Administration.ManageRoles)]
        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] RoleDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(new { Message = "Invalid model state." });

            var result = await _roleService.CreateRoleAsync(model.RoleName);
            if (result.Succeeded)
            {
                return Ok(new { Message = $"Role '{model.RoleName}' created successfully." });
            }
            return BadRequest(result.Errors);
        }

        [PermissionAuthorize(Permissions.Administration.ManageRoles)]
        [HttpPost("update-role")]
        public async Task<IActionResult> UpdateRole([FromBody] RoleDTO model)
        {
            if (model == null) return BadRequest(new { Message = "Request body is empty." });
            
            if (!ModelState.IsValid) return BadRequest(new { Message = "Model validation failed." });
            
            if (string.IsNullOrEmpty(model.Id)) return BadRequest(new { Message = "Role ID is missing." });
            if (string.IsNullOrEmpty(model.RoleName)) return BadRequest(new { Message = "Role Name is missing." });

            var result = await _roleService.UpdateRoleAsync(model.Id, model.RoleName);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role updated successfully" });
            }
            
            return BadRequest(new { Message = result.Errors.FirstOrDefault()?.Description ?? "Unknown Identity error" });
        }

        [PermissionAuthorize(Permissions.Administration.ManageRoles)]
        [HttpDelete("delete-role/{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role deleted successfully." });
            }
            return BadRequest(result.Errors);
        }

        [PermissionAuthorize(Permissions.Administration.ManageRoles)]
        [HttpGet("users-with-roles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var data = await _roleService.GetUsersWithRolesAsync();
            return Ok(data);
        }
    }
}
