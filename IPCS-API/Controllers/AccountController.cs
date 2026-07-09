using IPCS_Model.DTOs;
using IPCS_Model.Identity;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using IPCS_API.Attributes;
using IPCS_Model.Constants;

namespace IPCS_API.Controllers
{
    /// <summary>
    /// API Controller for handling user accounts, login, and registration.
    /// Admin management endpoints (roles, status) are also included.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /* 
         * FRONTEND TIP: Persisting Token in Local Storage
         * When Login is successful (Ok response), save the token like this:
         * 
         * // JavaScript (Web):
         * localStorage.setItem("authToken", response.token);
         * 
         * // C# MAUI (Mobile):
         * await SecureStorage.Default.SetAsync("auth_token", response.token);
         * 
         * If RememberMe is true, you can set a longer expiration logic on the client.
         */

        [PermissionAuthorize(Permissions.Administration.ManageUsers)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Wrong Information, Please Check ");
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.MobileNumber,
                    PresentAddress = model.PresentAddress,
                    PermanentAddress = model.PermanentAddress,
                    Description = model.Description,
                    Gender = model.Gender,
                    DateOfBirth = model.DateOfBirth,
                    BranchId = model.BranchId,
                    JoiningDate = DateTime.Now,
                    IsActive = true
                };

                var result = await _accountService.RegisterAsync(user, model.Password, model.RoleName);

                if (result.Succeeded)
                {
                    // [Waiting System]: No default role assigned here. 
                    // Admin must assign a role from the Admin Panel.
                    return Ok(new { Message = "User Registered successfully. Waiting for Admin approval/Role assignment." });
                }

                return BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Registration Error..." + ex.Message });
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                // Now supports Email or Mobile lookup via the Service
                var token = await _accountService.LoginAsync(model.EmailOrMobile, model.Password);

                if (token == null)
                {
                    return Unauthorized(new { message = "Email/Mobile or Password is incorrect, Please try again" });
                }

                // In a real app, 'RememberMe' would be handled by setting cookie expiration or refresh tokens
                var user = await _accountService.GetUserByEmailOrMobileAsync(model.EmailOrMobile);
                if (user == null) return Unauthorized(new { message = "User record not found" });

                var roles = await _accountService.GetUserRolesAsync(user.Email);

                return Ok(new
                {
                    Token = token,
                    User = new {
                        Name = user.FullName,
                        Email = user.Email,
                        Role = roles.FirstOrDefault() ?? "Staff"
                    },
                    Message = "Successfully Login",
                    RememberMe = model.RememberMe
                });
            }

            catch (Exception ex)
            {
                // This will capture "Pending approval" or "Incorrect Password" messages from Service
                return BadRequest(new { Message = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Administration.ManageRoles)]
        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            try
            {
                var result = await _accountService.CreateRoleAsync(roleName);
                if (result)
                {
                    return Ok(new { Message = "Create Role Successsfully..." });
                }
                return BadRequest(new { Message = "Error Role or Already Exist" });
            }

            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error Create Role..." + ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Administration.ManageRoles)]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleModel model)
        {
            try
            {
                var result = await _accountService.AssignRoleAsync(model.Email, model.RoleName);
                if (result)
                {
                    return Ok(new { Message = "User Added to Role Successfully" });
                }
                return BadRequest(new { Message = "Role assignment failed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error to assign role..." + ex.Message });
            }
        }

        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRole([FromBody] UserRoleModel model)
        {
            try
            {
                var result = await _accountService.RemoveRoleAsync(model.Email, model.RoleName);
                if (result)
                {
                    return Ok(new { Message = "Role Removed Successfully" });
                }
                return BadRequest(new { Message = "Role removal failed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error removing role: " + ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Administration.ManageUsers)]
        [HttpPost("toggle-status")]
        public async Task<IActionResult> ToggleStatus([FromBody] string email)
        {
            try
            {
                var result = await _accountService.ToggleUserStatusAsync(email);
                if (result)
                {
                    return Ok(new { Message = "User status updated successfully" });
                }
                return BadRequest(new { Message = "Status update failed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error toggling status: " + ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Administration.ManageUsers)]
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

                var result = await _accountService.DeleteUserAsync(id, currentUserId);
                if (result.Succeeded)
                {
                    return Ok(new { Message = "User deleted successfully." });
                }
                return BadRequest(new { Message = result.Errors.FirstOrDefault()?.Description ?? "Delete failed" });
            }

            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error deleting user: " + ex.Message });
            }
        }
    }
}

