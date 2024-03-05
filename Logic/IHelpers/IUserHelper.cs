using Core.Models;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.IHelpers
{
    public interface IUserHelper 
    {
        int GetAllUser();
        Task<bool> RedirectToRolesDashboard(string userId);
        Task<ApplicationUser> FindByUserNameAsync(string username);
        Task<ApplicationUser> FindByEmailAsync(string email);
        bool CheckRefRegPayment(string refferrerId);
        Task<bool> RegisterUser(ApplicationUserViewModel userDetails, string refferrerId, string parentId);
        bool AddUserPackageFromRegistration(int packageId, string userId);
        Packages GetPackageforUser(int packageId);
        ApplicationUser FindByUserName(string username);
        ApplicationUser FindById(string Id);
        Task<Impersonation> GetLatestImpersonateeRecord(string userId);
        Task<bool> UpdateLastLogin(ApplicationUser applicationUser);
        Task<UserVerification> CreateUserToken(string userEmail);
        Task<bool> ResetPasswordAsync(PasswordResetViewmodel viewmodel);
        Task<UserVerification> GetUserToken(Guid token);
        Task<bool> MarkTokenAsUsed(UserVerification userVerification);
        Task<ApplicationUser> FindByIdAsync(string Id);
        string GetCurrentUserId(string username);
        Task<ImpersonationViewModel> CheckForImpersonation(string userName);

    }
}
