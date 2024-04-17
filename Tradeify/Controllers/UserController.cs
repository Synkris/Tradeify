using Core.Config;
using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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


        public UserController(AppDbContext context, IUserHelper userHelper, IDropdownHelper dropdownHelper, IConfiguration configuration, IGeneralConfiguration generalConfiguration, IPaymentHelper paymentHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _dropdownHelper = dropdownHelper;
            _generalConfiguration = generalConfiguration;
            _paymentHelper = paymentHelper;
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

	}
}
