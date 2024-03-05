using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContext;


        public UserHelper(AppDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContext)
        {
            _context = context;
            _userManager = userManager;
            _httpContext = httpContext;

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

    }
}
