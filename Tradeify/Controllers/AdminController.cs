using Core.Config;
using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Tradeify.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGeneralConfiguration _generalConfiguration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserHelper _userHelper;
        private readonly IDropdownHelper _dropdownHelper;
        private readonly IAdminHelper _adminHelper;
        private readonly IPaymentHelper _paymentHelper;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IGeneralConfiguration generalConfiguration, IUserHelper userHelper, IDropdownHelper dropdownHelper, IAdminHelper adminHelper, IPaymentHelper paymentHelper, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _userHelper = userHelper;
            _generalConfiguration = generalConfiguration;
            _dropdownHelper = dropdownHelper;
            _adminHelper = adminHelper;
            _paymentHelper = paymentHelper;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var user = _userHelper.FindByUserName(User?.Identity?.Name);
            var userWallet = _paymentHelper.GetUserWallet(user?.Id).Result;
            var userDetails = _userHelper.FindById(user.Id);
            var rate = _generalConfiguration.DollarRate;
            var convertedBalance = userWallet.Balance / rate;
            var pv = _paymentHelper.GetUserPvWalletNonAsync(user?.Id);
            var userGrantWallet = _paymentHelper.GetUserGrantWalletNonAsync(user?.Id);
            var convertedGrant = userGrantWallet.Balance / rate;
            //var userWalletBalance = userWallet.Balance;
            var convertedGrantToGGC = convertedGrant * 4;
            var convertedBalanceToGGC = convertedBalance * 4;
            var userGiftCardWallet = _paymentHelper.GetUserAGCWalletNonAsync(user.Id);
            var genButton = _generalConfiguration.MapGenButton;

            var model = new ApplicationUserViewModel()
            {
                ConvertedGrant = convertedGrant,
                ConvertedGrantToGGC = convertedGrantToGGC,
                ConvertedBalanceToGGC = convertedBalanceToGGC,
                ConvertedBalance = convertedBalance,
                ConvertedToken = userGiftCardWallet.Balance,
                DateRegistered = userDetails.DateRegistered,
                CurrentLastLoginTime = userDetails.CurrentLastLoginTime,
                Id = userDetails.Id,
                UserName = userDetails.UserName,
                MapGenButton = genButton,
                Pv = pv.Balance,
            };
            return View(model);
        }
    }
}
