using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Tradeify.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserHelper _userHelper;
		private readonly IDropdownHelper _dropdownHelper;

		public AccountController(AppDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUserHelper userHelper, IDropdownHelper dropdownHelper)
        {
            _signInManager = signInManager;
            _context = context;
            _userManager = userManager;
            _userHelper = userHelper;
			_dropdownHelper = dropdownHelper;

		}
        public IActionResult Index()
        {
            return View();
        }

		[HttpGet]
		public IActionResult Register(string rf, string pr)
		{
			ViewBag.Cordinator = _dropdownHelper.DropdownOfCordinator();
			ViewBag.Package = _dropdownHelper.DropdownOfPackages();
			ViewBag.Gender = _dropdownHelper.GetDropdownByKey(DropdownEnums.GenderKey).Result;

			var checkExistingReffererId = _userManager.Users.Where(r => r.Id == rf);
			var checkExistingParentId = _userManager.Users.Where(r => r.Id == pr);
			if (checkExistingReffererId.Any() && checkExistingParentId.Any())
			{
				var model = new ApplicationUserViewModel()
				{
					RefferrerId = rf,
					ParentId = pr,
				};
				return View(model);
			}
			return View();
		}

		[HttpPost]
		public async Task<JsonResult> Registration(string userDetails, string refferrerId, string parentId)
		{
			if (userDetails != null)
			{
				var appUserViewModel = JsonConvert.DeserializeObject<ApplicationUserViewModel>(userDetails);
				if (appUserViewModel != null)
				{
					var checkForUserName = _context.ApplicationUser.Where(x => x.UserName == appUserViewModel.UserName && !x.Deactivated).FirstOrDefault();
					if (checkForUserName != null)
					{
						return Json(new { isError = true, msg = "UserName Already Exists" });
					}
					var checkForEmail = await _userHelper.FindByEmailAsync(appUserViewModel.Email).ConfigureAwait(false);
					if (checkForEmail != null)
					{
						return Json(new { isError = true, msg = "Email Already Exists" });
					}
					if (appUserViewModel.Password != appUserViewModel.ConfirmPassword)
					{
						return Json(new { isError = true, msg = "Password and Confirm password do not match" });
					}
					if (appUserViewModel.PackageId == 0)
					{
						return Json(new { isError = true, msg = " No Package Selected" });
					}
					if (appUserViewModel.CordinatorId == string.Empty)
					{
						return Json(new { isError = true, msg = " No Cordinator Selected" });
					}
					var checkRefRegPayment = _userHelper.CheckRefRegPayment(refferrerId);
					if (!checkRefRegPayment)
					{
						return Json(new { isError = true, msg = "Referrer has not paid registration fee" });
					}
					var createUser = await _userHelper.RegisterUser(appUserViewModel, refferrerId, parentId).ConfigureAwait(false);
					if (createUser)
					{
						return Json(new { isError = false, msg = "Registration Successful, Login to continue" });
					}
					return Json(new { isError = true, msg = "Unable to register" });
				}
			}
			return Json(new { isError = true, msg = "Unable to register" });
		}
	}
}
