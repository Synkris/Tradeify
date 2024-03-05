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


	}
}
