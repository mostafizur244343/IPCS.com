

using IPCS_Model.Identity;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace IPCS_Service.Implementation
{
    /// <summary>
    /// Service responsible for managing user accounts, authentication, and roles.
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;


        public AccountService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }


        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        public async Task<IdentityResult> RegisterAsync (User user, string password)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException("Please Enter User Information");
                }
                return await _userManager.CreateAsync(user, password);
            }
            catch (Exception ex)
            {
                
                throw new Exception(" Error Registration..!" + ex.Message);
            }
           
        }

        /// <summary>
        /// Authenticates a user using Email or Mobile Number.
        /// If login is successful, returns a JWT token.
        /// </summary>
        public async Task<string?> LoginAsync(string emailOrMobile, string password)
        {
            try
            {
                User? user = null;

                // 1. Try to find user by Email
                if (emailOrMobile.Contains("@"))
                {
                    user = await _userManager.FindByEmailAsync(emailOrMobile);
                }

                // 2. If not found by email or no '@', try searching by Phone Number
                if (user == null)
                {
                    user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == emailOrMobile);
                }

                if (user == null)
                {
                    throw new Exception("User Not Found with this Email or Mobile Number");
                }

                // 3. Check if User is Active (Admin deactivation check)
                if (!user.IsActive)
                {
                    throw new Exception("Your account is deactivated. Please contact Admin.");
                }

                // 4. Check if User is Locked Out (5 wrong attempts)
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var timeLeft = lockoutEnd.Value - DateTimeOffset.UtcNow;
                    throw new Exception($"Account is locked. Please try again after {Math.Ceiling(timeLeft.TotalMinutes)} minutes.");
                }

                // 5. Validate Password
                if (!await _userManager.CheckPasswordAsync(user, password))
                {
                    // Increment failed attempt counter
                    await _userManager.AccessFailedAsync(user);
                    
                    var failedCount = await _userManager.GetAccessFailedCountAsync(user);
                    int attemptsLeft = 5 - failedCount;

                    if (attemptsLeft <= 0)
                        throw new Exception("Account is locked due to 5 failed attempts. Please try again after 5 minutes.");
                    
                    throw new Exception($"Incorrect Password! You have {attemptsLeft} attempts left.");
                }

                // 6. Reset access fail count on successful login
                await _userManager.ResetAccessFailedCountAsync(user);

                // 5. Check if User has any Role (Waiting System)
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles == null || userRoles.Count == 0)
                {
                    // User is registered but Admin hasn't assigned a role yet
                    throw new Exception("Your account is pending approval. Please wait for Admin to assign a role.");
                }

                // 6. Generate JWT Claims
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim("BranchId", user.BranchId?.ToString() ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecurityKey"]!));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JwtSettings:ValidIssuer"],
                    audience: _configuration["JwtSettings:ValidAudience"],
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"])),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new role in the system if it doesn't exist.
        /// </summary>
        public async Task<bool> CreateRoleAsync (string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                {
                    throw new Exception("Please Enter RoleName");
                }

                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                    return true;
                }
                // if role already exist
                return false;
            

            }

            catch (Exception ex)
            {
                throw new Exception("Error Creating Role...");
            }
        }

        /// <summary>
        /// Assigns a specific role to a user.
        /// </summary>
        public async Task<bool> AssignRoleAsync(string email, string roleName)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    throw new Exception("User Not Found");
                }

                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    throw new Exception("Role Not Found");
                }

                await _userManager.AddToRoleAsync(user, roleName);
                return true;
            }

            catch (Exception ex)
            {
                throw new Exception("Role Assigning Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Removes a specific role from a user.
        /// </summary>
        public async Task<bool> RemoveRoleAsync(string email, string roleName)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null) throw new Exception("User Not Found");

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                throw new Exception("Role Removing Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Toggles the IsActive status of a user (Deactivate/Activate).
        /// </summary>
        public async Task<bool> ToggleUserStatusAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null) throw new Exception("User Not Found");

                user.IsActive = !user.IsActive;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Toggling Status: " + ex.Message);
            }
        }
    }
}
