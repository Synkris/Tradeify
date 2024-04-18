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

        [HttpPost]
        public async Task<IActionResult> CreateDropDown(string details)
        {
            ViewBag.Dropdownkeys = _dropdownHelper.GetDropDownEnumsList();
            try
            {
                var dropdownDetails = JsonConvert.DeserializeObject<CommonDropdowns>(details);
                var checkifNameExists = _adminHelper.CheckExistingDropdownName(dropdownDetails.Name, dropdownDetails.DropdownKey);
                if (checkifNameExists)
                {
                    return Json(new { isError = true, msg = "Dropdown Already Exists" });
                }
                var status = await _dropdownHelper.CreateDropdownsAsync(dropdownDetails);
                if (status)
                {
                    return Json(new { isError = false, msg = "Dropdown Details Created Successfully" });
                }
                return Json(new { isError = false, msg = "Sorry Unable to Create DropDowns" });
            }
            catch (Exception)
            {
                return Json(new { isError = true, msg = "Something Went Wrong" });
            }
        }

        [HttpGet]
        public IActionResult DropDownList(string name, DateTime sortTypeFrom, DateTime sortTypeTo, int pageNumber, int pageSize)
        {
            try
            {
                ViewBag.Dropdownkeys = _dropdownHelper.GetDropDownEnumsList();
                var commonDropdownViewModel = new CommonDropdownSearchResultViewModel(_generalConfiguration)
                {
                    PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
                    PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
                    SortTypeFrom = sortTypeFrom,
                    SortTypeTo = sortTypeTo,
                    Name = name,
                };
                pageNumber = (pageNumber == 0 ? commonDropdownViewModel.PageNumber : pageNumber);
                pageSize = pageSize == 0 ? commonDropdownViewModel.PageSize : pageSize;
                var dropdowns = _adminHelper.GetCommonDropdowns(commonDropdownViewModel, pageNumber, pageSize);
                commonDropdownViewModel.CommonDropdownRecords = dropdowns;
                if (dropdowns != null)
                {
                    return View(commonDropdownViewModel);
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        [HttpGet]
        public IActionResult EditDropDown(int? id)
        {
            ViewBag.Title = "Edit";
            if (id == null || id == 0)
            {
                return Json(new { isError = true, msg = "DropDown not found" });
            }
            var getDropDown = _context.CommonDropdowns.Find(id);

            if (getDropDown == null)
            {
                return Json(new { isError = true, msg = "Dropdwon not found" });
            }

            return Json(new { isError = false, dropdownDetails = getDropDown });
        }

        [HttpPost]
        public IActionResult SaveEditedDropDownName(string newDropdownName, int id)
        {

            if (newDropdownName != null)
            {
                var update = _adminHelper.UpdateDropDownService(newDropdownName, id);
                if (update == false)
                {
                    return Json(new { isError = true, msg = "Unable To Change Dropdown Name From Table" });
                }
                return Json(new { isError = false, msg = "DropDown Changed Successfully !" });
            }

            return View(newDropdownName);
        }
        [HttpGet]
        public IActionResult GetDropDown(int? id)
        {
            ViewBag.Title = "Remove";
            if (id == null || id == 0)
            {
                return Json(new { isError = true, msg = "Password and Confirm password do not match" });
            }
            var getDropDown = _context.CommonDropdowns.Find(id);

            if (getDropDown == null)
            {
                return Json(new { isError = true, msg = "Password and Confirm password do not match" });
            }

            return Json(new { isError = false, dropdownDetails = getDropDown });
        }

        [HttpPost]
        public JsonResult DeleteDropDown(int id)
        {
            if (id != 0)
            {
                var dropdownRemoved = _adminHelper.RemoveDropDown(id);
                if (dropdownRemoved != null)
                {
                    return Json(new { isError = false, msg = "Dropdown's Details Removed Successfully" });
                }
                return Json(new { isError = true, msg = "Unable To Remove Dropdown From Table" });
            }
            return Json(new { isError = true, msg = "Error Occurred" });
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetNameOfDropDownSelected(int dropdownId)
        {
            try
            {
                var selectedDropdown = _dropdownHelper.GetDropDownEnumsList().Where(x => x.Id == dropdownId).FirstOrDefault().Name;
                if (selectedDropdown.ToLower() == "networkkey")
                {
                    return Json(true);
                }
                return Json(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
