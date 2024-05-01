using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Logic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Logic.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IEmailService _emailService;


        public UserHelper(AppDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContext, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _httpContext = httpContext;
            _emailService = emailService;

        }

        public async Task<ApplicationUser> FindByUserNameAsync(string username)
        {
            return await _userManager.Users.Where(s => s.UserName == username)?.FirstOrDefaultAsync();
        }
        public int GetAllUser()
        {
            return _userManager.Users.Where(x => !x.Deactivated).Count();
        }
        public ApplicationUser FindByUserName(string username)
        {
            return _userManager.Users.Where(s => s.UserName == username)?.Include(s => s.Gender).FirstOrDefault();
        }
        public ApplicationUser FindById(string Id)
        {
            return _userManager.Users.Where(s => s.Id == Id)?.Include(s => s.Gender).FirstOrDefault();
        }

        public async Task<bool> RedirectToRolesDashboard(string userId)
        {
            try
            {
                if (userId == null)
                {
                    return false;
                }
                var currentUser = FindByUserNameAsync(userId);
                var userDetails = await _userManager.Users.Where(s => s.UserName == currentUser.Result.UserName)?.FirstOrDefaultAsync();
                if (userDetails != null)
                {
                    var goAdmin = await _userManager.IsInRoleAsync(userDetails, "Admin");
                    return goAdmin;
                }
                return false;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
		public async Task<ApplicationUser> FindByEmailAsync(string email)
		{
			return await _userManager.Users.Where(s => s.Email == email)?.FirstOrDefaultAsync();
		}

		public bool CheckRefRegPayment(string refferrerId)
		{
			if (refferrerId != null)
			{
				var refPay = _context.ApplicationUser.Where(s => s.Id == refferrerId && !s.Deactivated && s.RegFeePaid == true).FirstOrDefault();
				if (refPay != null)
				{
					return true;
				}
			}
			return false;
		}

		public async Task<bool> RegisterUser(ApplicationUserViewModel userDetails, string refferrerId, string parentId)
		{
			try
			{
				var user = new ApplicationUser();
				user.UserName = userDetails.UserName;
				user.Email = userDetails.Email;
				user.FirstName = userDetails.FirstName;
				user.LastName = userDetails.LastName;
				user.PhoneNumber = userDetails.Phonenumber;
				user.DateRegistered = DateTime.Now;
				user.Deactivated = false;
				user.RefferrerId = refferrerId;
				user.CordinatorId = userDetails.CordinatorId;
				user.GenderId = userDetails.GenderId;
				user.ParentId = parentId;

				var createUser = await _userManager.CreateAsync(user, userDetails.Password).ConfigureAwait(false);
				if (createUser.Succeeded)
				{
					await _userManager.AddToRoleAsync(user, "User").ConfigureAwait(false);
					AddUserPackageFromRegistration(userDetails.PackageId, user.Id);
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public bool AddUserPackageFromRegistration(int packageId, string userId)
		{
			if (packageId > 0 && userId != null)
			{
				var package = GetPackageforUser(packageId);
				if (package != null)
				{
					var userPackage = new UserPackages
					{
						Name = package.Name,
						Amount = (decimal)package.Price,
						UserId = userId,
						PackageId = packageId,
						DateCreated = DateTime.Now,
						Active = true,
						Deleted = false,
						MaxGeneration = (int)package.MaxGeneration,
						CryptoAmount = "0",
					};
					_context.Add(userPackage);
					_context.SaveChanges();
					return true;
				}
			}
			return false;
		}

		public Packages GetPackageforUser(int packageId)
		{
			if (packageId > 0)
			{
				var package = _context.Packages.Where(x => x.Id == packageId && x.Active && !x.Deleted).FirstOrDefault();
				if (package != null)
				{
					return package;
				}
			}
			return null;
		}

        public async Task<Impersonation> GetLatestImpersonateeRecord(string userId)
        {
            try
            {
                var user = FindById(userId);
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return null;
                }

                var lastTimeImpersonated = _context.Impersonations.Where(x => x.AdifMemberId == user.Id).OrderBy(x => x.DateImpersonated)?.LastOrDefault();
                if (lastTimeImpersonated != null)
                {
                    return lastTimeImpersonated;
                }
                return null;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public async Task<bool> UpdateLastLogin(ApplicationUser applicationUser)
        {
            try
            {
                applicationUser.CurrentLastLoginTime = DateTime.Now;
                _context.Update(applicationUser);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public async Task<UserVerification> CreateUserToken(string userEmail)
        {
            try
            {
                var user = await FindByEmailAsync(userEmail);
                if (user != null)
                {
                    UserVerification userVerification = new UserVerification()
                    {
                        UserId = user.Id,
                    };
                    await _context.AddAsync(userVerification);
                    await _context.SaveChangesAsync();
                    return userVerification;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<bool> ResetPasswordAsync(PasswordResetViewmodel viewmodel)
        {
            var userVerification = await GetUserToken(viewmodel.Token);
            if (userVerification != null && !userVerification.Used)
            {
                await _userManager.RemovePasswordAsync(userVerification.User);
                await _userManager.AddPasswordAsync(userVerification.User, viewmodel.Password);
                await _context.SaveChangesAsync();

                await MarkTokenAsUsed(userVerification);
                return true;
            }
            return false;
        }

        public async Task<UserVerification> GetUserToken(Guid token)
        {
            return await _context.UserVerifications.Where(t => t.Used != true && t.Token == token)?.Include(s => s.User).FirstOrDefaultAsync();

        }

        public async Task<bool> MarkTokenAsUsed(UserVerification userVerification)
        {
            try
            {
                var VerifiedUser = _context.UserVerifications.Where(s => s.UserId == userVerification.User.Id && s.Used != true).FirstOrDefault();
                if (VerifiedUser != null)
                {
                    userVerification.Used = true;
                    userVerification.DateUsed = DateTime.Now;
                    _context.Update(userVerification);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ApplicationUser> FindByIdAsync(string Id)
        {
            return await _userManager.Users.Where(s => s.Id == Id)?.FirstOrDefaultAsync();
        }

        public string GetCurrentUserId(string username)
        {
            try
            {
                if (username != null)
                {
                    return _userManager.Users.Where(s => s.UserName == username)?.FirstOrDefault()?.Id?.ToString();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<ImpersonationViewModel> CheckForImpersonation(string userName)
        {
            try
            {

                var session = _httpContext.HttpContext.Session;
                var loggedInUser = FindByUserName(userName);
                if (loggedInUser != null)
                {
                    if (await _userManager.IsInRoleAsync(loggedInUser, "Admin"))
                    {
                        return null;
                    }
                    var impersonation = new ImpersonationViewModel();
                    var impersonatee = FindById(loggedInUser.Id);
                    if (impersonatee != null)
                    {
                        var impersonationRecord = GetLatestImpersonateeRecord(loggedInUser.Id).Result;
                        if (impersonationRecord != null)
                        {

                            var SessionKeyName = impersonationRecord.AdminUserId;
                            var endSession = session.GetString(SessionKeyName);

                            impersonation.ImpersonationRecord = impersonationRecord;
                            impersonation.Impersonator = FindById(impersonationRecord.AdminUserId);
                            impersonation.AdminBeingImpersonated = loggedInUser;
                            impersonation.IsImpersonatorAdmin = CheckIfUserIsAdmin(impersonationRecord.AdminUserId).Result;
                            impersonation.Impersonatee = impersonatee;
                            impersonation.ShowEndSession = endSession;
                            return impersonation;


                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<bool> CheckIfUserIsAdmin(string userId)
        {
            try
            {
                if (userId == null)
                {
                    return false;
                }
                var currentUser = FindByIdAsync(userId).Result;
                if (currentUser != null)
                {
                    var goAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
                    if (goAdmin)
                    {
                        return goAdmin;

                    }

                }
                return false;
            }
            catch (Exception exp)
            {
                throw exp;
            }

        }

        public ApplicationUserViewModel ProfileDetailsToEdit(string userId)
        {
            var applicationViewModel = new ApplicationUserViewModel();
            applicationViewModel = _context.ApplicationUser.Where(x => x.Id == userId)
            .Select(b => new ApplicationUserViewModel
            {
                FirstName = b.FirstName,
                LastName = b.LastName,
                Email = b.Email,
                Phonenumber = b.PhoneNumber,
                UserName = b.UserName,
                Id = b.Id,
            }).FirstOrDefault();

            return applicationViewModel;
        }

        public bool EditedProfileDetails(ApplicationUserViewModel profileDetails)
        {
            if (profileDetails != null)
            {
                var editUser = _context.ApplicationUser.Where(a => a.Id == profileDetails.Id && a.RefferrerId != null && a.UserName != null).FirstOrDefault();
                if (editUser != null)
                {
                    editUser.FirstName = profileDetails.FirstName;
                    editUser.LastName = profileDetails.LastName;
                    editUser.Email = profileDetails.Email;
                    editUser.PhoneNumber = profileDetails.Phonenumber;

                    _context.Update(editUser);
                    _context.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public IPagedList<ApplicationUserViewModel> GetReferredUsers(ApplicationUserSearchResultViewModel applicationUserViewModel, string userId, int pageNumber, int pageSize)
        {

            var userContactsQuery = _userManager.Users
                .Where(u => u.RefferrerId == userId && u.Deactivated != true)
                .Include(u => u.Gender)
                .OrderByDescending(s => s.DateRegistered).AsQueryable();

            if (!string.IsNullOrEmpty(applicationUserViewModel.UserName))
            {
                userContactsQuery = userContactsQuery.Where(v =>
                    v.UserName.ToLower().Contains(applicationUserViewModel.UserName.ToLower())
                );
            }
            if (!string.IsNullOrEmpty(applicationUserViewModel.Name))
            {
                userContactsQuery = userContactsQuery.Where(v =>
                    (v.FirstName + " " + v.LastName).ToLower().Contains(applicationUserViewModel.Name.ToLower())
                );
            }
            if (!string.IsNullOrEmpty(applicationUserViewModel.PhoneNumber))
            {
                userContactsQuery = userContactsQuery.Where(v =>
                    v.PhoneNumber.ToLower().Contains(applicationUserViewModel.PhoneNumber.ToLower())
                );
            }
            if (!string.IsNullOrEmpty(applicationUserViewModel.Email))
            {
                userContactsQuery = userContactsQuery.Where(v =>
                    v.Email.ToLower().Contains(applicationUserViewModel.Email.ToLower())
                );
            }

            var totalItemCount = userContactsQuery.Count();
            var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

            var userContacts = userContactsQuery.Select(user => new ApplicationUserViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                Gender = user.Gender,
                Phonenumber = user.PhoneNumber,
                DateRegistered = user.DateRegistered,
            }).ToPagedList(pageNumber, pageSize, totalItemCount);
            applicationUserViewModel.PageCount = totalPages;
            applicationUserViewModel.UserRecords = userContacts;
            if (userContacts.Count() > 0)
            {
                return userContacts;
            }
            else
            {
                return null;
            }


        }

        public List<RolesViewModel> GetUsersInAdminRole()
        {
            var usersInAdminRole = _userManager.GetUsersInRoleAsync("Admin").Result
                .Select(x => new RolesViewModel { Name = x.Name, UserId = x.Id, RoleUserName = x.UserName }).ToList();
            return usersInAdminRole;
        }

        public async Task<string> GetRolesName(RolesViewModel rolesname)
        {

            var role = _context.Roles.Where(c => c.Id == rolesname.RoleSelected).FirstOrDefault();
            if (role != null)
            {
                return role.Name;
            }
            return null;
        }

        public News GetNewsById(int newsId)
        {
            try
            {
                if (newsId == null || newsId == 0)
                {
                    return null;
                }
                var mainNewsView = _context.News.Where(x => x.Id == newsId && x.Deleted == false && x.Active == true).FirstOrDefault();
                if (mainNewsView != null)
                {
                    return mainNewsView;

                }
                return null;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public List<PaymentFormViewModel> CoinDetails(string userId)
        {
            try
            {
                var paymentFormViewModel = new List<PaymentFormViewModel>();
                if (userId != null)
                {
                    paymentFormViewModel = _context.PaymentForms.Where(x => x.UserId == userId && x.PaymentTypeId == PaymentType.TokenFee)
                    .Select(x => new PaymentFormViewModel()
                    {
                        Amount = x.Amount,
                        Date = x.Date,
                        Details = x.Details,
                        NoOfTokensBought = x.NoOfTokensBought,
                        Status = x.Status,
                    }).ToList();
                }
                return paymentFormViewModel;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CompanySettings GetCompanySettingsDetails()
        {
            var companySettings = _context.CompanySettings.Where(x => x.Id != 0 && x.Active && !x.Deleted).FirstOrDefault();
            if (companySettings != null)
            {
                return companySettings;
            }
            return null;
        }

        public Packages GetPackageUgradeDetails(int packageId)
        {
            return _context.Packages.Where(x => x.Id == packageId && x.Active && !x.Deleted).FirstOrDefault();
        }

        public List<ApplicationUser> GetAllUserForBonus()
        {

            var users = _context.ApplicationUser
                .Where(a => !a.Deactivated && a.FirstName != "System")
                .Select(x => new ApplicationUser
                {
                    Id = x.Id,
                    FirstName = x.Name + " " + "(" + x.UserName + ")",
                });
            var userList = users.ToList();
            return userList;
        }

        public async Task<bool> SubmitAppreciationRequest(Appreciation appreciation, string adminId, string userId)
        {
            try
            {
                var admin = FindById(adminId);
                var user = FindById(userId);

                if (appreciation != null && adminId != null && userId != null)
                {
                    var newAppreciationRequest = new Appreciation
                    {
                        UserId = userId,
                        DateAppreciated = DateTime.Now,
                        AppreciationDetails = appreciation.AppreciationDetails,
                        AppreciatedBy = admin.UserName,
                        Amount = appreciation.Amount,

                    };

                    var latestRequest = await _context.AddAsync(newAppreciationRequest);
                    await _context.SaveChangesAsync();
                    if (latestRequest.Entity.Id != Guid.Empty)
                    {
                        string toEmail = user?.Email;
                        string subject = " GAP Appreciation Bonus";
                        string message = "Hello " + newAppreciationRequest?.User?.UserName + ", <br> An appreciation bonus  has been given to you." + ". <br/> Keep investing with GAP. <br/> Thanks!!! ";

                        _emailService.SendEmail(toEmail, subject, message);
                        return true;
                    }
                    return false;
                }

                return false;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public WithdrawFunds GetExistingBankWithdrawalDetails(string userId)
        {
            try
            {
                var existingRequest = _context.WithdrawFunds.Where(x => x.UserId == userId && x.WithdrawStatus == Status.Approved && x.WithdrawalType.Contains("Bank Account")).OrderByDescending(x => x.DateApprovedAndSent).FirstOrDefault();
                if (existingRequest != null)
                {
                    return existingRequest;
                }
                return null;
            }
            catch (Exception exp)
            {
                throw exp;
            }

        }

        public WithdrawFunds GetExistingCryptoDetails(string userId)
        {
            try
            {
                var existingRequest = _context.WithdrawFunds.Where(x => x.UserId == userId && x.WithdrawStatus == Status.Approved && x.WithdrawalType.Contains("Crypto Wallet")).OrderByDescending(x => x.DateApprovedAndSent).FirstOrDefault();
                if (existingRequest != null)
                {
                    return existingRequest;
                }
                return null;
            }
            catch (Exception exp)
            {
                throw exp;
            }

        }

        public IPagedList<UserPackagesViewModel> GetUserPackages(UserPackagesSearchResultViewModel userPackageViewModel, string userId, int pageNumber, int pageSize)
        {
            try
            {
                var userPackageQuery = _context.UserPackages
                    .Where(u => u.UserId == userId && u.Deleted != true && u.PaymentId != null)
                    .Include(u => u.Payment)
                    .Include(u => u.Package)
                    .OrderByDescending(s => s.DateOfPayment).AsQueryable();
                if (!string.IsNullOrEmpty(userPackageViewModel.Name))
                {
                    userPackageQuery = userPackageQuery.Where(v =>
                        v.Package.Name.ToLower().Contains(userPackageViewModel.Name.ToLower())
                    );
                }
                if (userPackageViewModel.SortTypeFrom != DateTime.MinValue)
                {
                    userPackageQuery = userPackageQuery.Where(v => v.DateOfPayment >= userPackageViewModel.SortTypeFrom);
                }
                if (userPackageViewModel.SortTypeTo != DateTime.MinValue)
                {
                    userPackageQuery = userPackageQuery.Where(v => v.DateOfPayment <= userPackageViewModel.SortTypeTo);
                }

                var totalItemCount = userPackageQuery.Count();
                var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

                var userPackages = userPackageQuery.Select(package => new UserPackagesViewModel
                {
                    Id = package.Id,
                    Name = package.Name,
                    UserId = package.UserId,
                    UserName = package.User.UserName,
                    Amount = package.Amount,
                    CryptoAmount = package.CryptoAmount,
                    DateOfPayment = package.DateOfPayment,
                    DateOfApproval = package.DateOfApproval,
                    DateBonusPaid = package.DateBonusPaid,
                    MaxGeneration = package.MaxGeneration,
                    PackageId = package.PackageId,
                    IsMatured = package.IsMatured,
                }).ToPagedList(pageNumber, pageSize, totalItemCount);
                userPackageViewModel.PageCount = totalPages;
                userPackageViewModel.UserPackagesRecords = userPackages;
                if (userPackages.Count() > 0)
                {
                    return userPackages;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<Packages> GetPackageDetails()
        {
            return _context.Packages.Where(x => x.Active && !x.Deleted).ToList();
        }

        public MiningLog UserLastMiningDetails(string userId)
        {
            try
            {
                return _context.miningLogs.Where(x => x.UserId == userId).OrderBy(x => x.DateCreated).LastOrDefault();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<bool> LogUserMiningHistory(string userId, decimal amount)
        {
            try
            {
                var miningSettings = _context.CompanySettings.Where(s => s.MiningQuantity == amount).FirstOrDefault();
                if (userId != null && amount >= 0)
                {
                    MiningLog newMiningLogHistory = new MiningLog()
                    {
                        UserId = userId,
                        MiningQuantity = amount,
                        MiningDuration = miningSettings.MiningDuration,
                        Active = true,
                        DateCreated = DateTime.Now,
                        Deleted = false,

                    };
                    var result = _context.Add(newMiningLogHistory);
                    _context.SaveChanges();
                    if (result.Entity.Id != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<ApplicationUser> GetUsers(string term)
        {
            return _userManager.Users
                .Where(x => x.UserName.ToLower().Contains(term.ToLower()))
                .Select(a => new ApplicationUser { Id = a.Id, UserName = a.UserName })
                .ToList();
        }

        public async Task<bool> SendTokensToMembers(Appreciation appreciation, string adminId, string userId)
        {
            try
            {
                var admin = FindById(adminId);
                var user = FindById(userId);

                if (appreciation != null && adminId != null && userId != null)
                {
                    var newAppreciationRequest = new Appreciation
                    {
                        UserId = userId,
                        DateAppreciated = DateTime.Now,
                        AppreciationDetails = appreciation.AppreciationDetails,
                        AppreciatedBy = admin?.UserName,
                        Amount = appreciation.Amount,
                    };
                    var latestRequest = await _context.AddAsync(newAppreciationRequest);
                    await _context.SaveChangesAsync();

                    if (latestRequest.Entity.Id != Guid.Empty)
                    {
                        string toEmail = user?.Email;
                        string subject = " GAP Gift Token";
                        string message = "Hello " + newAppreciationRequest?.User?.UserName + ", <br> A Gift Token has been awarded to you." + ". <br/> Keep investing with GAP. <br/> Thanks!!! ";

                        _emailService.SendEmail(toEmail, subject, message);
                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public IPagedList<ApplicationUserViewModel> GetUserWalletsDetails(ApplicationUserSearchResultViewModel applicationUserSearchResult, int pageNumber, int pageSize)
        {
            try
            {
                var userList = new List<ApplicationUserViewModel>();

                var userWallets = _context.Wallets.Where(x => x.Id != null).Include(x => x.User).AsQueryable();
                var userPVs = userWallets.Where(x => EF.Property<string>(x, "Discriminator") == "PvWallet" && x.Id != null).AsQueryable();
                var userAGCs = userWallets.Where(x => EF.Property<string>(x, "Discriminator") == "AGCWallet" && x.Id != null).AsQueryable();
                var wallets = userWallets.Where(x => EF.Property<string>(x, "Discriminator") == "Wallet" && x.Id != null && x.User.FirstName != "System").AsQueryable();

                if (!string.IsNullOrEmpty(applicationUserSearchResult.UserName))
                {
                    wallets = wallets.Where(v =>
                        v.User.UserName.ToLower().Contains(applicationUserSearchResult.UserName.ToLower())
                    );
                }
                if (!string.IsNullOrEmpty(applicationUserSearchResult.Name))
                {
                    wallets = wallets.Where(v =>
                        (v.User.FirstName + " " + v.User.LastName).ToLower().Contains(applicationUserSearchResult.Name.ToLower())
                    );
                }

                var totalItemCount = wallets.Count();
                var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

                var pageDetails = wallets.OrderBy(o => o.User.UserName)
                    .Select(x => new ApplicationUserViewModel()
                    {
                        Id = x.User.Id,
                        LastName = x.User.LastName,
                        FirstName = x.User.FirstName,
                        UserName = x.User.UserName,
                        WalletBalance = x.Balance,
                        PVBalance = userPVs.FirstOrDefault(p => p.UserId == x.User.Id) == null ? 0 : userPVs.FirstOrDefault(p => p.UserId == x.User.Id).Balance,
                        AGCWalletBalance = userAGCs.FirstOrDefault(p => p.UserId == x.User.Id) == null ? 0 : userAGCs.FirstOrDefault(p => p.UserId == x.User.Id).Balance,
                    }).ToPagedList(pageNumber, pageSize, totalItemCount);
                applicationUserSearchResult.PageCount = totalPages;

                return pageDetails;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public List<News> GetNews()
        {
            try
            {
                var newCreatedNews = new List<News>();
                newCreatedNews = _context.News.Where(x => x.Deleted == false && x.Active).OrderByDescending(x => x.DateCreated).ToList();
                if (newCreatedNews.Any())
                {
                    return newCreatedNews;
                }
                return newCreatedNews;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
