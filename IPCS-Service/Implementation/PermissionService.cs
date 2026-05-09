using IPCS_Model.DTOs;
using IPCS_Model.Entities.Permissions;
using IPCS_Model.Identity;
using IPCS_Repo.Data;
using IPCS_Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IPCS_Service.Implementation
{
    public class PermissionService : IPermissionService
    {
        private readonly IPCSDBContext _context;
        private readonly UserManager<User> _userManager;

        public PermissionService(IPCSDBContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permissionKey)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // 1. Super Admin bypass (Ultimate authority)
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperAdmin")) return true;

            // 2. Check User Direct Permissions
            var userHasPermission = await _context.UserPermissions
                .Include(up => up.Permission)
                .AnyAsync(up => up.UserId == userId && up.Permission.PermissionKey == permissionKey);

            if (userHasPermission) return true;

            // 3. Check Role Permissions
            // Get all RoleIds for the user's roles
            var roleIds = await _context.Roles
                .Where(r => roles.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            var roleHasPermission = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .AnyAsync(rp => roleIds.Contains(rp.RoleId) && rp.Permission.PermissionKey == permissionKey);

            return roleHasPermission;
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            // Get direct permissions
            var directPermissions = await _context.UserPermissions
                .Include(up => up.Permission)
                .Where(up => up.UserId == userId)
                .Select(up => up.Permission.PermissionKey)
                .ToListAsync();

            // Get role permissions
            var roles = await _userManager.GetRolesAsync(user);
            var roleIds = await _context.Roles
                .Where(r => roles.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            var rolePermissions = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission.PermissionKey)
                .ToListAsync();

            return directPermissions.Union(rolePermissions).Distinct().ToList();
        }

        public async Task<bool> AssignPermissionToRoleAsync(string roleId, int permissionId)
        {
            if (await _context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId))
                return true;

            _context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AssignPermissionToUserAsync(string userId, int permissionId)
        {
            if (await _context.UserPermissions.AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId))
                return true;

            _context.UserPermissions.Add(new UserPermission { UserId = userId, PermissionId = permissionId });
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, int permissionId)
        {
            var entry = await _context.RolePermissions.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            if (entry == null) return true;

            _context.RolePermissions.Remove(entry);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemovePermissionFromUserAsync(string userId, int permissionId)
        {
            var entry = await _context.UserPermissions.FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);
            if (entry == null) return true;

            _context.UserPermissions.Remove(entry);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<PermissionModuleDTO>> GetAllModulesWithPermissionsAsync(string? roleId = null, string? userId = null)
        {
            var modules = await _context.AppModules.Include(m => m.Permissions).ToListAsync();
            
            var rolePermissionIds = roleId != null 
                ? await _context.RolePermissions.Where(rp => rp.RoleId == roleId).Select(rp => rp.PermissionId).ToListAsync() 
                : new List<int>();

            var userPermissionIds = userId != null
                ? await _context.UserPermissions.Where(up => up.UserId == userId).Select(up => up.PermissionId).ToListAsync()
                : new List<int>();

            return modules.Select(m => new PermissionModuleDTO
            {
                ModuleId = m.Id,
                ModuleName = m.DisplayName ?? m.Name,
                Permissions = m.Permissions.Select(p => new PermissionDTO
                {
                    Id = p.Id,
                    PermissionKey = p.PermissionKey,
                    DisplayName = p.DisplayName,
                    IsAssigned = rolePermissionIds.Contains(p.Id) || userPermissionIds.Contains(p.Id)
                }).ToList()
            }).ToList();
        }

        public async Task<bool> UpdateRolePermissionsAsync(string roleId, List<int> permissionIds)
        {
            var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            var newEntries = permissionIds.Select(id => new RolePermission { RoleId = roleId, PermissionId = id });
            await _context.RolePermissions.AddRangeAsync(newEntries);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserPermissionsAsync(string userId, List<int> permissionIds)
        {
            var existing = await _context.UserPermissions.Where(up => up.UserId == userId).ToListAsync();
            _context.UserPermissions.RemoveRange(existing);

            var newEntries = permissionIds.Select(id => new UserPermission { UserId = userId, PermissionId = id });
            await _context.UserPermissions.AddRangeAsync(newEntries);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SeedPermissionsAsync()
        {
            var permissionType = typeof(IPCS_Model.Constants.Permissions);
            var nestedTypes = permissionType.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

            foreach (var type in nestedTypes)
            {
                var moduleName = type.Name;
                var module = await _context.AppModules.FirstOrDefaultAsync(m => m.Name == moduleName);
                if (module == null)
                {
                    module = new AppModule { Name = moduleName, DisplayName = moduleName };
                    _context.AppModules.Add(module);
                    await _context.SaveChangesAsync();
                }

                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                foreach (var field in fields)
                {
                    var permissionKey = field.GetValue(null)?.ToString();
                    if (string.IsNullOrEmpty(permissionKey)) continue;

                    if (!await _context.AppPermissions.AnyAsync(p => p.PermissionKey == permissionKey))
                    {
                        _context.AppPermissions.Add(new AppPermission
                        {
                            PermissionKey = permissionKey,
                            DisplayName = field.Name,
                            ModuleId = module.Id
                        });
                    }
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
