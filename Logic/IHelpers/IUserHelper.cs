using Core.Models;
using Core.ViewModels;
using X.PagedList;

namespace Logic.IHelpers
{
    public interface IUserHelper
    {
        Task<ApplicationUser> FindByEmailAsync(string email);
        int GetAllUser();
        string GetUserId(string username);
        ApplicationUser FindByUserName(string username);
        ApplicationUser FindById(string Id);
        string GetCurrentUserId(string username);
        Task<UserVerification> CreateUserToken(string userEmail);
        Task<UserVerification> GetUserToken(Guid token);
        Task<bool> MarkTokenAsUsed(UserVerification userVerification);
        Task<bool> ResetPasswordAsync(PasswordResetViewmodel viewmodel);
        Task<ApplicationUser> FindByUserNameAsync(string username);
        List<Packages> GetPackageDetails();
        Packages GetPackageforUser(int packageId);
        string GetUserRole(ApplicationUser user);
        Task<bool> RegisterUser(ApplicationUserViewModel userDetails, string refferrerId, string parentId);
        Task<bool> UpdateLastLogin(ApplicationUser applicationUser);
        Task<string> GetRolesName(RolesViewModel rolesname);
		List<RolesViewModel> GetUsersInAdminRole();
        List<RolesViewModel> GetRoles();
        List<ApplicationUser> GetUsers(string term);
        Task<bool> RedirectToRolesDashboard(string userId);
        List<ApplicationUser> GetAllUserForBonus();
        Task<ApplicationUser> FindByIdAsync(string Id);
        bool CheckRefRegPayment(string refferrerId);
        bool EditNews(News levl);
		bool DeleteNews(News news);
        Task<Impersonation> GetLatestImpersonateeRecord(string userId);
        bool AddUserPackageFromRegistration(int packageId, string userId);
        WithdrawFunds GetExistingCryptoDetails(string userId);
		WithdrawFunds GetExistingBankWithdrawalDetails(string userId);

        Task<bool> SubmitAppreciationRequest(Appreciation appreciation, string adminId, string userId);

		IPagedList<ApplicationUserViewModel> GetReferredUsers(ApplicationUserSearchResultViewModel applicationUserViewModel, string userId, int pageNumber, int pageSize);
        IPagedList<UserPackagesViewModel> GetUserPackages(UserPackagesSearchResultViewModel userPackageViewModel, string userId, int pageNumber, int pageSize);
        Task<ImpersonationViewModel> CheckForImpersonation(string userName);
        Task<bool> CheckIfUserIsAdmin(string userId);
        MiningLog UserLastMiningDetails(string userId);
		Task<bool> CreditWallet(string userId, decimal amount);
        Wallet GetUserWalletNonAsync(string userId);
        Wallet CreateWalletByUserIdNonAsync(string userId, decimal amount = 0);
        Task<bool> LogUserMiningHistory(string userId, decimal amount);
        List<PaymentFormViewModel> CoinDetails(string userId);
		CompanySettings GetCompanySettingsDetails();
        Task<bool> SendTokensToMembers(Appreciation appreciation, string adminId, string userId);
		Packages GetPackageUgradeDetails(int packageId);
        decimal GetUserWallet(string userId);
        decimal GetAGCWallet(string userId);
        decimal GetPvWallet(string userId);
        List<News> GetNews();
        News GetNewsById(int newsId);
        IPagedList<ApplicationUserViewModel> GetUserWalletsDetails(ApplicationUserSearchResultViewModel applicationUserSearchResult, int pageNumber, int pageSize);
		ApplicationUserViewModel ProfileDetailsToEdit(string userId);
		bool EditedProfileDetails(ApplicationUserViewModel userDetails);
	}
}
