using Core.Config;
using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IPackageHelper _packageHelper;


        public UserController(AppDbContext context, IUserHelper userHelper, IDropdownHelper dropdownHelper, IConfiguration configuration, IGeneralConfiguration generalConfiguration, IPaymentHelper paymentHelper, IAdminHelper adminHelper, IPackageHelper packageHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _dropdownHelper = dropdownHelper;
            _generalConfiguration = generalConfiguration;
            _paymentHelper = paymentHelper;
            _adminHelper = adminHelper;
            _packageHelper = packageHelper;
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
            var checkApprovedRegFee = _paymentHelper.CheckIfUserHasPaidRegPayment(loggedInUserId);
            if (checkApprovedRegFee)
            {
                return RedirectToAction("Index", "User");
            }
            ViewBag.Bank = await _dropdownHelper.GetBankDropdownByKey(DropdownEnums.BankList);
            ViewBag.Package = _dropdownHelper.DropdownOfPackages();
            var userPackage = _paymentHelper.GetUserPackage(loggedInUserId);
            if (userPackage != null)
            {
                var package = new PaymentFormViewModel()
                {
                    PackageId = userPackage.PackageId,
                };
                return View(package);
            }
            return View();
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
                var checkApprovedRegFee = _paymentHelper.CheckIfUserHasPaidRegPayment(loggedInUserId);
                if (checkApprovedRegFee)
                {
                    return RedirectToAction("Index", "User");
                }
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

        public IActionResult RegistrationSuccessPage()
        {
            return View();
        }

        public IActionResult Coin()
        {
            var userId = _userHelper.GetCurrentUserId(User.Identity.Name);
            var userTokens = _userHelper.CoinDetails(userId);
            var companySettings = _userHelper.GetCompanySettingsDetails();
            var coinDetails = new PaymentFormViewModel()
            {
                TokenMaximum = companySettings.MaximumToken,
                TokenMinimum = companySettings.MinimumToken,
                AmountPerToken = companySettings.Tokenamount,
                UserTokenDetails = userTokens,
            };
            return View(coinDetails);
        }
        public IActionResult Payment()
        {
            return View();
        }

        public IActionResult PaymentDetails()
        {
            ViewBag.PaymentType = _dropdownHelper.GetPaymentTypeDropDownEnumsList();
            ViewBag.Bank = _dropdownHelper.GetBankDropdownByKey(DropdownEnums.BankList).Result;
            ViewBag.Package = _dropdownHelper.DropdownOfPackages();

            return View();
        }

        [HttpPost]
        public JsonResult CoinPayment(string details)
        {
            var loggedInUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
            if (details != null)
            {
                var paymentFormViewModel = JsonConvert.DeserializeObject<PaymentFormViewModel>(details);
                if (paymentFormViewModel != null)
                {
                    paymentFormViewModel.UserId = loggedInUserId;
                    var checkRegPayment = _paymentHelper.CheckIfUserHasPaidRegPayment(paymentFormViewModel.UserId);
                    if (!checkRegPayment)
                    {
                        return Json(new { isError = true, msg = "You have not paid for Registration" });
                    }
                    if (paymentFormViewModel.PaymentTypeId == PaymentType.TokenFee || paymentFormViewModel.PaymentTypeId == PaymentType.PackageFee)
                    {
                        var checkIfDeactivated = _paymentHelper.CheckifDeactivated(paymentFormViewModel.UserId);
                        if (checkIfDeactivated)
                        {
                            return Json(new { isError = true, msg = "Sorry, You need to pay activation fee to continue." });
                        }
                    }
                    if (paymentFormViewModel.PaymentTypeId == PaymentType.ReActivationFee)
                    {
                        var checkPendingPayment = _paymentHelper.CheckIfUserhasPendingActPayment(paymentFormViewModel.UserId);
                        if (checkPendingPayment)
                        {
                            return Json(new { isError = true, msg = "You have a pending Activation Payment" });
                        }
                    }
                    if (paymentFormViewModel.PaymentTypeId == PaymentType.PackageFee)
                    {
                        var checkPackageUpgradeAmount = CheckPackageUpgradeAmount(paymentFormViewModel.Amount, paymentFormViewModel.UserId, paymentFormViewModel.PackageId);
                        if (!checkPackageUpgradeAmount)
                        {
                            return Json(new { isError = true, msg = "This is not the correct amount to pay" });
                        }
                        else if (paymentFormViewModel.Amount < 0)
                        {
                            return Json(new { isError = true, msg = "This is not the correct package for upgrade" });
                        }
                    }

                    if (paymentFormViewModel.PaymentTypeId != PaymentType.ReActivationFee && paymentFormViewModel.PaymentTypeId != PaymentType.PackageFee)
                    {
                        var checkNoOfTokens = _paymentHelper.CheckForNoOfTokens(paymentFormViewModel?.TokensBought);
                        if (!checkNoOfTokens)
                        {
                            return Json(new { isError = true, msg = "You can not buy this number of tokens" });
                        }
                    }

                    if (paymentFormViewModel.BankAccountId == 0)
                    {
                        return Json(new { isError = true, msg = "Bank name not selected" });
                    }
                    var coinPayment = _paymentHelper.CreateCoinPayment(paymentFormViewModel);
                    if (coinPayment)
                    {
                        if (paymentFormViewModel.PaymentTypeId == PaymentType.TokenFee)
                        {
                            return Json(new { isError = false, msg = "Coin payment fee submitted successfully." });
                        }
                        else if (paymentFormViewModel.PaymentTypeId == PaymentType.ReActivationFee)
                        {
                            return Json(new { isError = false, data = paymentFormViewModel.PaymentTypeId, msg = "Re-Activation fee payment submitted successfully." });
                        }
                        else if (paymentFormViewModel.PaymentTypeId == PaymentType.PackageFee)
                        {
                            return Json(new { isError = false, data = paymentFormViewModel.PaymentTypeId, msg = "Package fee payment submitted successfully." });
                        }

                    }
                    return Json(new { isError = true, msg = "Unable to Submit" });
                }
            }
            return Json(new { isError = true, msg = "Network Failure" });
        }
        [HttpGet]
        public IActionResult CryptoPaymentDetails()
        {
            ViewBag.PaymentType = _dropdownHelper.GetPaymentTypeDropDownEnumsList();
            ViewBag.CryptoWallets = _dropdownHelper.GetCryptoDropdown(DropdownEnums.BankList).GetAwaiter().GetResult();
            ViewBag.Package = _dropdownHelper.DropdownOfPackages();

            return View();
        }
        [HttpPost]
        public JsonResult CryptoCoinPayment(string details)
        {
            var loggedInUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
            if (details != null)
            {
                var paymentFormViewModel = JsonConvert.DeserializeObject<PaymentFormViewModel>(details);
                if (paymentFormViewModel != null)
                {
                    paymentFormViewModel.UserId = loggedInUserId;
                    var checkRegPayment = _paymentHelper.CheckIfUserHasPaidRegPayment(paymentFormViewModel.UserId);
                    if (!checkRegPayment)
                    {
                        return Json(new { isError = true, msg = "You have not paid for Registration" });
                    }
                    if (paymentFormViewModel.PaymentTypeId == PaymentType.TokenFee || paymentFormViewModel.PaymentTypeId == PaymentType.PackageFee)
                    {
                        var checkIfDeactivated = _paymentHelper.CheckifDeactivated(paymentFormViewModel.UserId);
                        if (checkIfDeactivated)
                        {
                            return Json(new { isError = true, msg = "Sorry, You need to pay activation fee to continue." });
                        }
                    }
                    if (paymentFormViewModel.PaymentTypeId == PaymentType.ReActivationFee)
                    {
                        var checkPendingPayment = _paymentHelper.CheckIfUserhasPendingActPayment(paymentFormViewModel.UserId);
                        if (checkPendingPayment)
                        {
                            return Json(new { isError = true, msg = "You have a pending Activation Payment" });
                        }
                    }
                    if (paymentFormViewModel.PaymentTypeId == PaymentType.ReActivationFee)
                    {
                        var checkActivationAmount = _paymentHelper.CheckActivationAmount(paymentFormViewModel.Amount);
                        if (!checkActivationAmount)
                        {
                            return Json(new { isError = true, msg = "This is not the correct amount to pay" });
                        }
                    }
                    if (paymentFormViewModel.PaymentTypeId == PaymentType.PackageFee)
                    {
                        var checkPackageUpgradeAmount = CheckPackageUpgradeAmount(paymentFormViewModel.Amount, paymentFormViewModel.UserId, paymentFormViewModel.PackageId);
                        if (!checkPackageUpgradeAmount)
                        {
                            return Json(new { isError = true, msg = "This is not the correct amount to pay" });
                        }
                        else if (paymentFormViewModel.Amount < 0)
                        {
                            return Json(new { isError = true, msg = "This is not the correct package for upgrade" });
                        }
                    }
                    if (paymentFormViewModel.PaymentTypeId != PaymentType.ReActivationFee && paymentFormViewModel.PaymentTypeId != PaymentType.PackageFee)
                    {
                        var checkNoOfTokens = _paymentHelper.CheckForNoOfTokens(paymentFormViewModel.TokensBought);
                        if (!checkNoOfTokens)
                        {
                            return Json(new { isError = true, msg = "You can not buy this number of tokens" });
                        }
                    }
                    if (paymentFormViewModel.BankAccountId == 0)
                    {
                        return Json(new { isError = true, msg = "Wallet name not selected" });
                    }
                    var coinPayment = _paymentHelper.CreateCryptoTokenPayment(paymentFormViewModel);
                    if (coinPayment)
                    {
                        if (paymentFormViewModel.PaymentTypeId == PaymentType.TokenFee)
                        {
                            return Json(new { isError = false, msg = "Crypto Fee Submitted Successfully." });
                        }
                        else if (paymentFormViewModel.PaymentTypeId == PaymentType.ReActivationFee)
                        {
                            return Json(new { isError = false, data = paymentFormViewModel.PaymentTypeId, msg = "Crypto fee for Re-Activation payment submitted successfully." });
                        }
                        else if (paymentFormViewModel.PaymentTypeId == PaymentType.PackageFee)
                        {
                            return Json(new { isError = false, data = paymentFormViewModel.PaymentTypeId, msg = "Crypto fee for Package payment submitted successfully." });
                        }

                    }
                    return Json(new { isError = true, msg = "Unable to Submit" });
                }
            }
            return Json(new { isError = true, msg = "Network Failure" });
        }

        public bool CheckPackageUpgradeAmount(decimal amount, string userId, int packageId)
        {

            var userPackage = _paymentHelper.GetUserPackage(userId);
            var newPackageDetails = _userHelper.GetPackageUgradeDetails(packageId);
            if (newPackageDetails.Price.HasValue)
            {
                var newPackagePrice = (decimal)newPackageDetails.Price;
                var userPackageAmountToPay = newPackagePrice - userPackage.Amount;

                if (userPackageAmountToPay == amount)
                {
                    return true;
                }
            }
            return false;
        }

        [Authorize]
        [Route("/WithdrawFunds")]
        [HttpGet]
        public IActionResult WithdrawFunds()
        {
            try
            {
                var currentUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
                var existingDetails = _userHelper.GetExistingBankWithdrawalDetails(currentUserId);
                var model = new WithdrawalViewModel()
                {
                    BankAccountName = existingDetails?.BankAccountName,
                    AccountName = existingDetails?.AccountName,
                    AccountNumber = existingDetails?.AccountNumber,
                };
                return View(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize]
        [Route("/WithdrawFunds")]
        [HttpPost]
        public async Task<IActionResult> WithdrawFunds(WithdrawalViewModel withdrawFunds)
        {
            try
            {
                var convertGGCTorealAmount = withdrawFunds.Amount * _generalConfiguration.GGCWithdrawalConversionToNaira;

                var currentUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
                var userWallet = _paymentHelper.GetUserWalletNonAsync(currentUserId);

                if (userWallet.Balance >= Convert.ToDecimal(convertGGCTorealAmount.ToString("F4")))
                {
                    var existingRequest = _context.WithdrawFunds.Where(x => x.UserId == currentUserId && x.WithdrawStatus == Status.Pending);
                    if (existingRequest.Any())
                    {
                        SetMessage("Your have a pending withdrawal request, try after your first request has been approved ", Message.Category.Warning);
                        ModelState.Clear();
                        return View();
                    }
                    withdrawFunds.Amount = convertGGCTorealAmount;
                    var createdrequest = await _paymentHelper.CreateWithdrawalRequest(withdrawFunds, currentUserId);
                    if (createdrequest)
                    {
                        SetMessage("Your withdrawal request was successful, account will be verified and credited shortly ", Message.Category.Information);
                        ModelState.Clear();
                        return View();
                    }

                }
                SetMessage("Withdrawal Denied, Insufficient Wallet Balance ", Message.Category.Error);
                ModelState.Clear();
                return View();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize]
        [Route("/WithdrawByCrypto")]
        [HttpGet]
        public IActionResult WithdrawByCrypto()
        {
            try
            {
                var currentUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
                var existingDetails = _userHelper.GetExistingCryptoDetails(currentUserId);
                var model = new WithdrawalViewModel()
                {
                    AccountNumber = existingDetails?.AccountNumber,
                };
                return View(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize]
        [Route("/WithdrawByCrypto")]
        [HttpPost]
        public async Task<IActionResult> WithdrawByCrypto(WithdrawalViewModel withdrawFunds)
        {
            try
            {
                var convertGGCTorealAmount = withdrawFunds.Amount * _generalConfiguration.GGCWithdrawalConversionToNaira;
                var currentUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
                var userWallet = _paymentHelper.GetUserWalletNonAsync(currentUserId);
                if (userWallet.Balance >= Convert.ToDecimal(convertGGCTorealAmount.ToString("F4")))
                {

                    var existingRequest = _context.WithdrawFunds.Where(x => x.UserId == currentUserId && x.WithdrawStatus == Status.Pending);
                    if (existingRequest.Any())
                    {
                        SetMessage("Your have a pending withdrawal request, try after your first request has been approved ", Message.Category.Warning);
                        ModelState.Clear();
                        return View();
                    }
                    withdrawFunds.Amount = convertGGCTorealAmount;
                    var createdrequest = await _paymentHelper.CreateCryptoWithdrawalRequest(withdrawFunds, currentUserId);
                    if (createdrequest)
                    {
                        SetMessage(" Your withdrawal request was successful & crypto wallet will be verified and credited shortly ", Message.Category.Information);
                        ModelState.Clear();
                        return View();
                    }

                }
                SetMessage("Withdrawal Denied, Insufficient Wallet Balance ", Message.Category.Error);
                ModelState.Clear();
                return View();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult ViewUserPackages(string name, DateTime sortTypeFrom, DateTime sortTypeTo, int pageNumber, int pageSize)
        {
            try
            {
                var loggedInUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
                var userPackageViewModel = new UserPackagesSearchResultViewModel(_generalConfiguration)
                {
                    PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
                    PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
                    SortTypeFrom = sortTypeFrom,
                    SortTypeTo = sortTypeTo,
                    Name = name,

                };
                pageNumber = (pageNumber == 0 ? userPackageViewModel.PageNumber : pageNumber);
                pageSize = pageSize == 0 ? userPackageViewModel.PageSize : pageSize;
                // Retrieve user packages using the helper method
                var userPackages = _userHelper.GetUserPackages(userPackageViewModel, loggedInUserId, pageNumber, pageSize);
                userPackageViewModel.UserPackagesRecords = userPackages;
                if (userPackages != null)
                {
                    return View(userPackageViewModel);
                }
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while fetching user packages.";
                return View(new List<UserPackagesViewModel>());
            }


        }
        public JsonResult GetUserPackageDetails(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Json(new { isError = true, msg = "Package not found" });
            }
            var details = _context.UserPackages.Where(x => x.Id == id && x.Active && !x.Deleted).Include(x => x.Package).FirstOrDefault();
            if (details != null)
            {
                return Json(new { isError = false, data = details });
            }
            return Json(new { isError = true, msg = " Details not found" });
        }

        [Authorize]
        public IActionResult Packages()
        {
            try
            {
                var userId = _userHelper.GetCurrentUserId(User.Identity.Name);
                var userPackage = _paymentHelper.GetUserPackage(userId);
                ViewBag.getUserPackage = userPackage.PackageId;
                var packages = _packageHelper.GetPackages();
                if (packages != null)
                {
                    return View(packages);
                }
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public JsonResult GetPackageUpgradeDetailsForPaymentForm()
        {
            var userId = _userHelper.GetCurrentUserId(User.Identity.Name);

            var newPackageDetails = _userHelper.GetPackageDetails();
            var oldPackageDetails = _paymentHelper.GetUserPackage(userId);
            var oldPackageAmount = oldPackageDetails?.Amount;

            return Json(new { isError = false, data = JsonConvert.SerializeObject(newPackageDetails), results = oldPackageAmount });
        }

        public JsonResult GetPackageUpgradeAlertDetails(int packageId)
        {
            var userId = _userHelper.GetCurrentUserId(User.Identity.Name);

            var newPackageDetails = _userHelper.GetPackageUgradeDetails(packageId);
            var oldPackageDetails = _paymentHelper.GetUserPackage(userId);
            var oldPackageAmount = oldPackageDetails.Amount;

            return Json(new { isError = false, data = JsonConvert.SerializeObject(newPackageDetails), results = oldPackageAmount });
        }

        public async Task<JsonResult> UserMiningLog(string userId)
        {
            try
            {
                var getUserMiningAdminSetup = await _adminHelper.GetCompanySettings();
                var miningLog = _userHelper.UserLastMiningDetails(userId);

                var checkIfUserHasPaidRegistrationFee = _paymentHelper.CheckIfUserHasPaidRegPayment(userId);
                if (!checkIfUserHasPaidRegistrationFee)
                {
                    return Json(new { isError = true, msg = "You have not made Registration Payment" });
                }

                if (getUserMiningAdminSetup != null)
                {
                    var amount = getUserMiningAdminSetup.MiningQuantity;
                    if (miningLog != null)
                    {
                        var userLastMining = (DateTime.Now - miningLog.DateCreated).TotalHours;
                        var isMiningDue = userLastMining >= getUserMiningAdminSetup.MiningDuration;
                        if (isMiningDue)
                        {
                            var creditedUser = await _paymentHelper.CreditGGCToken(userId, amount).ConfigureAwait(false);
                            if (creditedUser)
                            {
                                var date = DateTime.Now.AddHours(getUserMiningAdminSetup.MiningDuration);
                                return Json(new { isError = false, msg = "Token mined successfully.", data = date, token = amount });
                            }
                        }
                        return Json(new { isError = true, msg = "Not due for another mining, try again later." });
                    }
                    else
                    {
                        var creditedUser = await _paymentHelper.CreditGGCToken(userId, amount).ConfigureAwait(false);

                        if (creditedUser)
                        {
                            var date = DateTime.Now.AddHours(getUserMiningAdminSetup.MiningDuration);
                            return Json(new { isError = false, data = date, msg = "Token mined successfully.", token = amount });
                        }
                    }

                    return Json(new { isError = true, msg = "Error occurred while mining, try again later." });

                }
                else
                {
                    return Json(new { isError = true, msg = "Error occurred, please contact admin" });
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [Authorize]
        public IActionResult GetUserLastMining()
        {
            var userName = User.Identity?.Name;
            if (userName != null)
            {
                var userId = _userHelper.GetCurrentUserId(userName);
                var getAdminSetup = _adminHelper.GetCompanySettings().Result;
                var userLastMiningDetails = _userHelper.UserLastMiningDetails(userId);
                var lastMiningDate = userLastMiningDetails?.DateCreated;
                if (lastMiningDate != null)
                {
                    lastMiningDate = lastMiningDate.Value.AddHours(getAdminSetup.MiningDuration);
                    return Json(new { isError = false, user = userId, data = lastMiningDate });
                }
                return Json(new { isError = true, msg = "There is no mining history" });
            }
            return Json(new { isError = true, msg = "Could Not get the last mining" });
        }

        public JsonResult GetNews()
        {
            try
            {
                var user = User.Identity.Name;
                var userId = _userHelper.GetCurrentUserId(user);
                var newCreatedNews = _userHelper.GetNews();
                if (newCreatedNews.Any())
                {
                    foreach (var news in newCreatedNews.ToList())
                    {
                        string existingUserIds = news?.RedBy;
                        if (existingUserIds != null && existingUserIds.Split(',').Contains(userId))
                        {
                            newCreatedNews.Remove(news);
                        }
                    }
                    return Json(new { isError = false, data = newCreatedNews });
                }
                return Json(new { isError = true, msg = "News Not Found" });
            }
            catch (Exception)
            {
                throw;
            }
        }


	}
}
