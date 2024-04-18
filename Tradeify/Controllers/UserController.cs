using Core.Config;
using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Tradeify.Models;

namespace Tradeify.Controllers
{
	[Authorize]
    public class UserController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IDropdownHelper _dropdownHelper;
        private readonly IPaymentHelper _paymentHelper;
        private IGeneralConfiguration _generalConfiguration;
        private readonly IAdminHelper _adminHelper;


        public UserController(AppDbContext context, IUserHelper userHelper, IDropdownHelper dropdownHelper, IConfiguration configuration, IGeneralConfiguration generalConfiguration, IPaymentHelper paymentHelper, IAdminHelper adminHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _dropdownHelper = dropdownHelper;
            _generalConfiguration = generalConfiguration;
            _paymentHelper = paymentHelper;
            _adminHelper = adminHelper;
        }



        public IActionResult Index()
        {
            var user = _userHelper.FindByUserName(User?.Identity?.Name);
            var userWallet = _paymentHelper.GetUserWallet(user?.Id).Result;
            var userDetails = _userHelper.FindById(user.Id);
            //var userWalletBalance = userWallet.Balance;
            var rate = _generalConfiguration.DollarRate;
            var convertedBalance = userWallet.Balance / rate;
            var pv = _paymentHelper.GetUserPvWalletNonAsync(user?.Id);
            var userGrantWallet = _paymentHelper.GetUserGrantWalletNonAsync(user?.Id);
            var convertedGrant = userGrantWallet.Balance / rate;
            var convertedGrantToGGC = convertedGrant * 4;
            var convertedBalanceToGGC = convertedBalance * 4;
            var userGiftCardWallet = _paymentHelper.GetUserAGCWalletNonAsync(user.Id);

            var model = new ApplicationUserViewModel()
            {
                ConvertedGrant = convertedGrant,
                ConvertedGrantToGGC = convertedGrantToGGC,
                ConvertedBalanceToGGC = convertedBalanceToGGC,
                ConvertedBalance = convertedBalance,
                Pv = pv.Balance,
                ConvertedToken = userGiftCardWallet.Balance,
                DateRegistered = userDetails.DateRegistered,
                CurrentLastLoginTime = userDetails.CurrentLastLoginTime,
                Id = userDetails.Id,
                Deactivated = userDetails.Deactivated,
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult PaymentMethods()
        {
            try
            {
                return View();
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        [HttpGet]
        public async Task<IActionResult> RegistrationPayment()
        {
            var loggedInUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
            ViewBag.Bank = await _dropdownHelper.GetBankDropdownByKey(DropdownEnums.BankList);
            ViewBag.Package = _dropdownHelper.DropdownOfPackages();
            var userPackage = _paymentHelper?.GetUserPackage(loggedInUserId);
            var package = new PaymentFormViewModel()
            {
                PackageId = userPackage.PackageId,
            };
            return View(package);
        }

		[HttpPost]
		public JsonResult RegistrationFeePayment(string details)
		{
			var loggedInUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
			if (details != null)
			{
				var paymentFormViewModel = JsonConvert.DeserializeObject<PaymentFormViewModel>(details);
				if (paymentFormViewModel != null)
				{
					paymentFormViewModel.UserId = loggedInUserId;
					var checkPendingPayments = _paymentHelper.CheckIfUserhasPendingRegPayment(paymentFormViewModel.UserId);
					if (checkPendingPayments)
					{
						return Json(new { isError = true, msg = "You have a pending Registration Payment" });
					}
					if (paymentFormViewModel.BankAccountId == 0)
					{
						return Json(new { isError = true, msg = "Bank name not selected" });
					}
					var regFee = _paymentHelper.CreateRegFee(paymentFormViewModel);
					if (regFee)
					{
						return Json(new { isError = false, msg = "Registration Fee Submitted Successfully" });
					}
					return Json(new { isError = true, msg = "Unable to Submit" });
				}
			}
			return Json(new { isError = true, msg = "Network Failure" });
		}

		[HttpGet]
		public async Task<IActionResult> RegistrationCryptoPayment()
		{
			try
			{
				var loggedInUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
				ViewBag.CryptoWallets = await _dropdownHelper.GetCryptoDropdown(DropdownEnums.BankList);
				ViewBag.Package = _dropdownHelper.DropdownOfPackages();
				var userPackage = _paymentHelper.GetUserPackage(loggedInUserId);
				var package = new PaymentFormViewModel()
				{
					PackageId = userPackage.PackageId,
				};
				return View(package);
			}
			catch (Exception exp)
			{
				throw exp;
			}
		}

		public JsonResult RegistrationCryptoPayment(string details)
		{
			var loggedInUserId = _userHelper.GetCurrentUserId(User.Identity.Name);

			if (details != null)
			{
				var paymentFormViewModel = JsonConvert.DeserializeObject<PaymentFormViewModel>(details);
				if (paymentFormViewModel != null)
				{
					paymentFormViewModel.UserId = loggedInUserId;
					var checkPendingPayments = _paymentHelper.CheckIfUserhasPendingRegPayment(paymentFormViewModel.UserId);
					if (checkPendingPayments)
					{
						return Json(new { isError = true, msg = "You have a pending Registration Payment" });
					}
					if (paymentFormViewModel.BankAccountId == 0)
					{
						return Json(new { isError = true, msg = "Wallet name not selected" });
					}
					var regFee = _paymentHelper.CreateCryptoRegFeeAsync(paymentFormViewModel);
					if (regFee)
					{
						return Json(new { isError = false, msg = "Crypto Registration Fee Submitted Successfully" });
					}
					return Json(new { isError = true, msg = "Unable to Submit" });
				}
			}
			return Json(new { isError = true, msg = "Network Failure" });
		}

		[HttpGet]
		public JsonResult ReferralLink()
		{
			try
			{
				var url = "/User/PaymentMethods";
				var currentUser = _userHelper.FindByUserName(User.Identity.Name);
				if (currentUser != null)
				{
					var checkRegPayment = _paymentHelper.CheckIfUserHasPaidRegPayment(currentUser.Id);
					if (checkRegPayment)
					{
						var link = currentUser.Id;
						string referralLink = HttpContext.Request.Scheme.ToString()
						  + "://" + HttpContext.Request.Host.ToString() + "/Account/Register?pr=" + link + "&rf=" + link;
						return Json(referralLink);
					}
					return Json(new { isError = true, dashboard = url });
				}
				return Json(new { isError = true, msg = "Member Not Found" });
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[HttpGet]
		public JsonResult ChildReferralLink(string userId)
		{
			try
			{
				if (userId != null)
				{
					var url = "/User/PaymentMethods";
					var user = _userHelper.FindById(userId);
					if (user != null)
					{
						var checkRegPayment = _paymentHelper.CheckIfUserHasPaidRegPayment(user.Id);
						if (checkRegPayment)
						{
							var link = user.Id;
							string referralLink = HttpContext.Request.Scheme.ToString()
							+ "://" + HttpContext.Request.Host.ToString() + "/Account/Register?pr=" + link + "&rf=" + link;
							return Json(referralLink);
						}
						return Json(new { isError = true, dashboard = url });
					}
					return Json(new { isError = true, msg = "Member Not Found" });

				}
				return Json(new { isError = true, msg = "User Not Found" });

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[HttpGet]
		[Authorize]
		[Route("/Profile")]
		public IActionResult Profile()
		{
			ViewBag.Title = "View Profile";
			var user = _userHelper.FindByUserName(User.Identity.Name);
			var userBonus = _paymentHelper.GetUserWallet(user.Id).Result;
			var grantWallet = _paymentHelper.GetUserGrantWallet(user.Id).Result;
			var rate = _generalConfiguration.DollarRate;
			var Bonus = userBonus.Balance / rate;
			//var grantAmount = grantWallet.Balance / rate;
			var pv = _paymentHelper.GetUserPvWalletNonAsync(user.Id);
			var referrerId = _userHelper.FindById(user.ParentId);

			var newModel = new ApplicationUserViewModel()
			{
				BonusAmount = Bonus,
				GrantAmount = Bonus / _generalConfiguration.GGCConversionToDollar,
				Pv = pv.Balance,
				FirstName = user?.FirstName,
				LastName = user?.LastName,
				Email = user?.Email,
				Phonenumber = user?.PhoneNumber,
				DateRegistered = user.DateRegistered,
				CurrentLastLoginTime = user.CurrentLastLoginTime,
				RefferrerUserName = user?.Refferrer?.Name,
				GenderName = user?.Gender?.Name,
				Id = user?.Id,
				CordinatorId = user?.CordinatorId,
			};
			return View(newModel);

		}

		[HttpGet]
		public IActionResult EditProfileDetails(string userId)
		{
			try
			{
				if (userId != null)
				{
					var userProfile = _userHelper.ProfileDetailsToEdit(userId);
					return PartialView(userProfile);
				}
				return View();
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[HttpPost]
		public JsonResult EditedProfileDetails(string details)
		{
			if (details != null)
			{
				var profileDetailsToedit = JsonConvert.DeserializeObject<ApplicationUserViewModel>(details);
				if (profileDetailsToedit != null)
				{
					var editProfile = _userHelper.EditedProfileDetails(profileDetailsToedit);
					if (editProfile)
					{
						return Json(new { isError = false, msg = "Profile Edited Successfully" });
					}
				}
				return Json(new { isError = true, msg = "Could Not Edit" });
			}
			return Json(new { isError = true, msg = "Could Not find Member Profile" });
		}

		[Authorize]
		[HttpGet]
		public async Task<IActionResult> WalletTransactionHistory(string userName, DateTime sortTypeFrom, DateTime sortTypeTo, string transactionType, int pageNumber, int pageSize)
		{
			try
			{
				var currentUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
				var userwalletHistoryModel = new WalletHistorySearchResultViewModel(_generalConfiguration)
				{
					PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
					PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
					SortTypeFrom = sortTypeFrom,
					SortTypeTo = sortTypeTo,
					TransactionType = transactionType,
					UserName = userName,
					DollarRate = _generalConfiguration.DollarRate,

				};
				pageNumber = (pageNumber == 0 ? userwalletHistoryModel.PageNumber : pageNumber);
				pageSize = pageSize == 0 ? userwalletHistoryModel.PageSize : pageSize;
				TimeSpan datesSum = userwalletHistoryModel.SortTypeTo.Date - userwalletHistoryModel.SortTypeFrom.Date;
				if (datesSum.Days >= 31)
				{
					SetMessage("wallet Date Range Shouldn't be more than 30 days or 1 month", Message.Category.Error);
					return View("SortWalletTransactions");
				}
				var userHistory = _paymentHelper.UserWalletHistoryRange(userwalletHistoryModel, currentUserId, pageNumber, pageSize);
				userwalletHistoryModel.WalletHistoryRecords = userHistory;
				if (userHistory != null)
				{
					return View(userwalletHistoryModel);
				}
				return View();
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		[HttpGet]
		public IActionResult SortWalletTransactions()
		{
			return View();
		}
		[Authorize]
		[HttpGet]
		public async Task<IActionResult> GrantTransactionHistory(string userName, DateTime sortTypeFrom, DateTime sortTypeTo, string transactionType, int pageNumber, int pageSize)
		{
			try
			{
				var currentUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
				var userGrantwalletHistoryViewModel = new GrantHistorySearchResultViewModel(_generalConfiguration)
				{
					PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
					PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
					SortTypeFrom = sortTypeFrom,
					SortTypeTo = sortTypeTo,
					TransactionType = transactionType,
					UserName = userName,
					DollarRate = _generalConfiguration.DollarRate,

				};
				pageNumber = (pageNumber == 0 ? userGrantwalletHistoryViewModel.PageNumber : pageNumber);
				pageSize = pageSize == 0 ? userGrantwalletHistoryViewModel.PageSize : pageSize;
				TimeSpan datesSum = userGrantwalletHistoryViewModel.SortTypeTo.Date - userGrantwalletHistoryViewModel.SortTypeFrom.Date;
				if (datesSum.Days >= 31)
				{
					SetMessage("Grant Range Shouldn't be more than 30 days or 1 month", Message.Category.Error);
					return View("SortUserGrantsTransactions");
				}
				var grantHistory = _paymentHelper.ForUserGrantWalletHistory(userGrantwalletHistoryViewModel, currentUserId, pageNumber, pageSize);
				userGrantwalletHistoryViewModel.GrantHistoryRecords = grantHistory;
				if (grantHistory != null)
				{
					return View(userGrantwalletHistoryViewModel);
				}
				return View();
			}
			catch (Exception exp)
			{

				throw exp;
			}
		}
		[HttpGet]
		public IActionResult SortUserGrantsTransactions()
		{
			return View();
		}
		[Authorize]
		[HttpGet]
		public async Task<IActionResult> AGCTransactionHistory(string userName, DateTime sortTypeFrom, DateTime sortTypeTo, string transactionType, int pageNumber, int pageSize)
		{
			try
			{
				var currentUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
				var userAGCwalletHistoryViewModel = new AGCSearchResultViewModel(_generalConfiguration)
				{
					PageNumber = pageNumber == 0 ? 1 : pageNumber,
					PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
					SortTypeFrom = sortTypeFrom,
					SortTypeTo = sortTypeTo,
					TransactionType = transactionType,
					UserName = userName,

				};
				pageNumber = (pageNumber == 0 ? userAGCwalletHistoryViewModel.PageNumber : pageNumber);
				pageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize;
				TimeSpan datesSum = userAGCwalletHistoryViewModel.SortTypeTo.Date - userAGCwalletHistoryViewModel.SortTypeFrom.Date;
				if (datesSum.Days >= 31)
				{
					SetMessage("AGC Range Shouldn't be more than 30 days or 1 month", Message.Category.Error);
					return View("SortAGCTransactions");
				}
				var giftCardHistory = _paymentHelper.UserAGCWalletHistory(userAGCwalletHistoryViewModel, currentUserId, pageNumber, pageSize);
				userAGCwalletHistoryViewModel.AGCWalletHistoryRecords = giftCardHistory;
				if (giftCardHistory != null)
				{
					return View(userAGCwalletHistoryViewModel);
				}
				return View();
			}
			catch (Exception exp)
			{
				throw exp;
			}
		}
		[HttpGet]
		public IActionResult SortAGCTransactions()
		{
			return View();
		}

		public IActionResult MyContacts(string name, string phoneNumber, string email, string userName, int pageNumber, int pageSize)
		{
			try
			{
				var loggedInUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
				var userContactsViewModel = new ApplicationUserSearchResultViewModel(_generalConfiguration)
				{
					PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
					PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
					Name = name,
					PhoneNumber = phoneNumber,
					Email = email,
					UserName = userName,

				};
				pageNumber = (pageNumber == 0 ? userContactsViewModel.PageNumber : pageNumber);
				pageSize = pageSize == 0 ? userContactsViewModel.PageSize : pageSize;
				var referredUsers = _userHelper.GetReferredUsers(userContactsViewModel, loggedInUserId, pageNumber, pageSize);
				userContactsViewModel.UserRecords = referredUsers;
				if (referredUsers != null)
				{
					return View(userContactsViewModel);
				}
				return View();
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = "An error occurred while fetching referred users.";
				return View(new List<ApplicationUserViewModel>());
			}
		}

		[HttpGet]
		public IActionResult EditUserDetails(string userId)
		{
			try
			{
				if (userId != null)
				{
					ViewBag.Cordinator = _dropdownHelper.DropdownOfCordinator();
					ViewBag.Package = _dropdownHelper.DropdownOfPackages();
					var user = _adminHelper.UserDetailsToEdit(userId);
					return PartialView(user);
				}
				return View();
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[HttpPost]
		public JsonResult EditedUserDetails(string details)
		{
			if (details != null)
			{
				var detailsToedit = JsonConvert.DeserializeObject<ApplicationUserViewModel>(details);
				if (detailsToedit != null)
				{
					var edit = _adminHelper.EditedDetails(detailsToedit);
					if (edit)
					{
						return Json(new { isError = false, msg = "Member Edited Successfully" });
					}
				}
				return Json(new { isError = true, msg = "Could Not Edit Member" });
			}
			return Json(new { isError = true, msg = "Could Not find Member" });
		}

	}
}
