using IPCS_Model.DTOs;
using IPCS_Model.Identity;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Fetches all roles using AsNoTracking for high performance.
        /// </summary>
        public async Task<List<RoleDTO>> GetAllRolesAsync()
        {
            return await _roleManager.Roles
                .AsNoTracking()
                .Select(r => new RoleDTO
                {
                    Id = r.Id,
                    RoleName = r.Name!
                })
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new role. Identity handles the internal transaction.
        /// </summary>
        public async Task<IdentityResult> CreateRoleAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role already exists." });
            }
            return await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        /// <summary>
        /// Deletes a role by its ID.
        /// </summary>
        public async Task<IdentityResult> DeleteRoleAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });
            }

            if (role.Name == "SuperAdmin")
            {
                return IdentityResult.Failed(new IdentityError { Description = "The 'SuperAdmin' role is a core system role and cannot be deleted." });
            }

            return await _roleManager.DeleteAsync(role);
        }

        public async Task<IdentityResult> UpdateRoleAsync(string roleId, string newRoleName)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });
            }

            if (role.Name == "SuperAdmin")
            {
                return IdentityResult.Failed(new IdentityError { Description = "The 'SuperAdmin' role cannot be renamed." });
            }

            // Only check existence if the name is actually changing
            if (!string.Equals(role.Name, newRoleName, StringComparison.OrdinalIgnoreCase))
            {
                if (await _roleManager.RoleExistsAsync(newRoleName))
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Role name already exists." });
                }
            }

            role.Name = newRoleName;
            role.NormalizedName = newRoleName.ToUpper(); // Ensure normalized name is also updated
            return await _roleManager.UpdateAsync(role);
        }




        /// <summary>
        /// Gets a complex list of Users and their Roles. Optimized with projection.
        /// </summary>
        public async Task<List<UserWithRolesDTO>> GetUsersWithRolesAsync()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var userWithRoles = new List<UserWithRolesDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userWithRoles.Add(new UserWithRolesDTO
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    Roles = roles.ToList()
                });
            }

            return userWithRoles;
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }
    }
}
