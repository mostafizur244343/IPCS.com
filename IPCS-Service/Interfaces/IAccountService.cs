using IPCS_Model.Identity;
using Microsoft.AspNetCore.Identity;


namespace IPCS_Service.Interfaces
{
    public interface IAccountService
    {
        //Registration Method in time just simple after Completing i use DTO
        Task<IdentityResult> RegisterAsync(User user, string password);


        //Login Method Which Return Token
        Task<string?> LoginAsync(string emailOrMobile, string password);

        //Method of Creating Role
        Task<bool> CreateRoleAsync(string roleName);

        //Method of Assigning Role of Users
        Task<bool> AssignRoleAsync(string email, string roleName);

        // Remove Role
        Task<bool> RemoveRoleAsync(string email, string roleName);

        // Deactivate/Activate User
        Task<bool> ToggleUserStatusAsync(string email);

        // Helper Methods for Login Data
        Task<User?> GetUserByEmailOrMobileAsync(string emailOrMobile);
        Task<IList<string>> GetUserRolesAsync(string email);
        Task<IdentityResult> DeleteUserAsync(string userId, string currentUserId);
    }
}