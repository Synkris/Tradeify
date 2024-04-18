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

        [HttpGet]
        public IActionResult Cordinator()
        {
            var cordinators = _adminHelper.listOfCordinators();
            if (cordinators != null && cordinators.Count() > 0)
            {
                return View(cordinators);
            }
            return View(cordinators);
        }

        [HttpPost]
        public JsonResult AddCordinator(string cordinatorUserName)
        {
            if (cordinatorUserName != null)
            {

                var loggedInUser = _userHelper.FindByUserName(User.Identity.Name);
                var checkExistingCordinatorUserName = _adminHelper.CheckExistingCordinatorUserName(cordinatorUserName);
                if (checkExistingCordinatorUserName == true)
                {
                    return Json(new { isError = true, msg = "Cordinator Name Already Exist" });
                }
                var getNewCordinatorDetails = _adminHelper.GetNewCordinatorDetails(cordinatorUserName);
                if (getNewCordinatorDetails != null)
                {
                    var distributors = _adminHelper.CreateCordinator(getNewCordinatorDetails, loggedInUser.UserName);
                    if (distributors != null)
                    {
                        return Json(new { isError = false, msg = "New Cordinator Added Successfully" });
                    }
                    return Json(new { isError = true, msg = "Unable To create New Cordinator" });
                }
                return Json(new { isError = true, msg = "Unable To Fetch User" });

            }
            return Json(new { isError = true, msg = "Please enter Valid Cordinator UserName" });
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

        [HttpGet]
        public IActionResult Permission()
        {
            ViewBag.roles = _dropdownHelper.DropdownOfRoles();
            try
            {
                var userWithAdminRole = _userHelper.GetUsersInAdminRole();
                if (userWithAdminRole != null)
                {
                    return View(userWithAdminRole);
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToRoles(String roleData)
        {
            ViewBag.roles = _dropdownHelper.DropdownOfRoles();
            try
            {
                var userRoleDatas = JsonConvert.DeserializeObject<RolesViewModel>(roleData);
                if (userRoleDatas != null)
                {
                    var checkExistingUsername = _userHelper.FindByUserName(userRoleDatas.RoleUserName);
                    if (checkExistingUsername != null)
                    {

                        if (checkExistingUsername.RegFeePaid != true)
                        {
                            return Json(new { isError = true, msg = "This user has not paid registeration fee, please inform the user", roleData });

                        }
                        var getRoleName = _userHelper.GetRolesName(userRoleDatas);

                        if (getRoleName != null)
                        {
                            var checkIfIsInRole = _userManager.IsInRoleAsync(checkExistingUsername, getRoleName.Result).Result;
                            if (checkIfIsInRole == true)
                            {
                                return Json(new { isError = true, msg = "This user already belong to this role", roleData });

                            }
                            else
                            {
                                await _userManager.AddToRoleAsync(checkExistingUsername, getRoleName.Result);
                                return Json(new { isError = false, msg = "User added to role successfully" });

                            }

                        }
                        return Json(new { isError = true, msg = "This role name not found, add a valid role name", roleData });


                    }
                    return Json(new { isError = true, msg = "This user does not exist, add a valid Username", roleData });

                }
                return View(roleData);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromRoles(string userRole)
        {
            ViewBag.roles = _dropdownHelper.DropdownOfRoles();

            try
            {
                var checkExistingUsername = _userHelper.FindByUserName(userRole);
                if (checkExistingUsername != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(checkExistingUsername);

                    if (currentRoles.Any())
                    {
                        var roleNameToRemove = currentRoles.Last();
                        await _userManager.RemoveFromRoleAsync(checkExistingUsername, roleNameToRemove);
                        var checkIfinAnyRole = await _userManager.GetRolesAsync(checkExistingUsername);
                        if (checkIfinAnyRole != null)
                        {
                            await _userManager.AddToRoleAsync(checkExistingUsername, "User");
                        }
                        return Json(new { isError = false, msg = $"User removed from role '{roleNameToRemove}' successfully!" });
                    }
                    else
                    {
                        return Json(new { isError = true, msg = "User is not in any roles" });
                    }
                }
                return Json(new { isError = true, msg = "This user does not exist, add a valid Username" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AllWalletHistories(string userName, DateTime sortTypeFrom, DateTime sortTypeTo, string transactionType, int pageNumber, int pageSize)
        {
            try
            {
                var walletHistoryModel = new WalletHistorySearchResultViewModel(_generalConfiguration)
                {
                    PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
                    PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
                    SortTypeFrom = sortTypeFrom,
                    SortTypeTo = sortTypeTo,
                    TransactionType = transactionType,
                    UserName = userName,
                    DollarRate = _generalConfiguration.DollarRate,
                };
                pageNumber = (pageNumber == 0 ? walletHistoryModel.PageNumber : pageNumber);
                pageSize = pageSize == 0 ? walletHistoryModel.PageSize : pageSize;
                TimeSpan datesSum = walletHistoryModel.SortTypeTo.Date - walletHistoryModel.SortTypeFrom.Date;
                if (datesSum.Days >= 31)
                {
                    SetMessage("Date Range Should Not be more than 30 days or 1 month", Message.Category.Error);
                    return View("AllWalletHistories");
                }

                var userWalletHistory = _paymentHelper.SortUsersWalletHistory(walletHistoryModel, pageNumber, pageSize);
                walletHistoryModel.WalletHistoryRecords = userWalletHistory;

                if (userWalletHistory != null)
                {
                    return View(walletHistoryModel); ;
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet]
        public IActionResult SortAllWalletTransactions()
        {
            return View();

        }
        [HttpGet]
        public async Task<IActionResult> AllAGCHistories(string userName, DateTime sortTypeFrom, DateTime sortTypeTo, string transactionType, int pageNumber, int pageSize)
        {
            try
            {
                var agcViewModel = new AGCSearchResultViewModel(_generalConfiguration)
                {
                    PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
                    PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
                    SortTypeFrom = sortTypeFrom,
                    SortTypeTo = sortTypeTo,
                    TransactionType = transactionType,
                    UserName = userName,
                };
                pageNumber = (pageNumber == 0 ? agcViewModel.PageNumber : pageNumber);
                pageSize = pageSize == 0 ? agcViewModel.PageSize : pageSize;
                TimeSpan datesSum = agcViewModel.SortTypeTo.Date - agcViewModel.SortTypeFrom.Date;
                if (datesSum.Days >= 31)
                {
                    SetMessage("Date Range Should Not be more than 30 days or 1 month", Message.Category.Error);
                    return View("SortAllAGCTransactions");
                }
                var agcHistories = _paymentHelper.SortAGCWalletHistories(agcViewModel, pageNumber, pageSize);
                agcViewModel.AGCWalletHistoryRecords = agcHistories;
                if (agcHistories != null)
                {
                    return View(agcViewModel);
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public IActionResult SortAllAGCTransactions()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AllGrantHistories(string userName, DateTime sortTypeFrom, DateTime sortTypeTo, string transactionType, int pageNumber, int pageSize)
        {
            try
            {
                var viewModel = new GrantHistorySearchResultViewModel(_generalConfiguration)
                {
                    PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
                    PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
                    SortTypeFrom = sortTypeFrom,
                    SortTypeTo = sortTypeTo,
                    TransactionType = transactionType,
                    UserName = userName,
                    DollarRate = _generalConfiguration.DollarRate,

                };
                pageNumber = (pageNumber == 0 ? viewModel.PageNumber : pageNumber);
                pageSize = pageSize == 0 ? viewModel.PageSize : pageSize;
                TimeSpan datesSum = viewModel.SortTypeTo.Date - viewModel.SortTypeFrom.Date;
                if (datesSum.Days >= 31)
                {
                    SetMessage(" Grant Date Range Shouldn't be more than 30 days or 1 month", Message.Category.Error);
                    return View("SortAllGrantsTransactions");
                }
                var grantHistory = _paymentHelper.SortUserGrantWalletHistory(viewModel, pageNumber, pageSize);
                viewModel.GrantHistoryRecords = grantHistory;
                if (grantHistory != null)
                {
                    return View(viewModel);
                }
                return View();
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        [HttpGet]
        public IActionResult SortAllGrantsTransactions()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AllPvHistories(string userName, DateTime sortTypeFrom, DateTime sortTypeTo, string transactionType, int pageNumber, int pageSize)
        {
            try
            {
                var pvViewModel = new PvSearchResultViewModel(_generalConfiguration)
                {
                    PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
                    PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
                    SortTypeFrom = sortTypeFrom,
                    SortTypeTo = sortTypeTo,
                    TransactionType = transactionType,
                    UserName = userName,
                };
                pageNumber = (pageNumber == 0 ? pvViewModel.PageNumber : pageNumber);
                pageSize = pageSize == 0 ? pvViewModel.PageSize : pageSize;
                TimeSpan datesSum = pvViewModel.SortTypeTo.Date - pvViewModel.SortTypeFrom.Date;
                if (datesSum.Days >= 31)
                {
                    SetMessage("Pv Range Shouldn't be more than 30 days or 1 month", Message.Category.Error);
                    return View("SortAllPvTransactions");
                }
                var PvHistory = _paymentHelper.SortPvWalletHistories(pvViewModel, pageNumber, pageSize);
                pvViewModel.PvWalletHistoryRecords = PvHistory;
                if (PvHistory != null)
                {
                    return View(pvViewModel);
                }
                return View();
            }
            catch (Exception exp)
            {
                throw exp;
            }

        }

        [HttpGet]
        public IActionResult SortAllPvTransactions()
        {
            return View();
        }
    }
}
