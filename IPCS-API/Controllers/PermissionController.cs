using IPCS_Model.Constants;
using IPCS_Model.DTOs;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using IPCS_API.Attributes;

namespace IPCS_API.Controllers
{
    [PermissionAuthorize(Permissions.Administration.ManagePermissions)]
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet("all-modules")]
        public async Task<IActionResult> GetAllModules([FromQuery] string? roleId, [FromQuery] string? userId)
        {
            var result = await _permissionService.GetAllModulesWithPermissionsAsync(roleId, userId);
            return Ok(result);
        }

        [HttpPost("update-role-permissions")]
        public async Task<IActionResult> UpdateRolePermissions([FromBody] UpdateRolePermissionDTO model)
        {
            var result = await _permissionService.UpdateRolePermissionsAsync(model.RoleId, model.PermissionIds);
            if (result) return Ok(new { Message = "Role Permissions updated successfully" });
            return BadRequest("Update failed");
        }

        [HttpPost("update-user-permissions")]
        public async Task<IActionResult> UpdateUserPermissions([FromBody] UpdateUserPermissionDTO model)
        {
            var result = await _permissionService.UpdateUserPermissionsAsync(model.UserId, model.PermissionIds);
            if (result) return Ok(new { Message = "User Permissions updated successfully" });
            return BadRequest("Update failed");
        }

        [HttpPost("seed-permissions")]
        public async Task<IActionResult> SeedPermissions()
        {
            var result = await _permissionService.SeedPermissionsAsync();
            if (result) return Ok(new { Message = "Permissions seeded successfully from Constants" });
            return Ok(new { Message = "No new permissions found to seed or Error occurred" });
        }
        
        [HttpGet("my-permissions")]
        [Authorize] // Any logged in user can see their own permissions
        public async Task<IActionResult> GetMyPermissions()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(permissions);
        }
    }
}
