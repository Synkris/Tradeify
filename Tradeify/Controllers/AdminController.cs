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
using System.Data;

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
        private readonly IBonusHelper _bonusHelper;
        private readonly IBinaryHelper _binaryHelper;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IGeneralConfiguration generalConfiguration, IUserHelper userHelper, IDropdownHelper dropdownHelper, IAdminHelper adminHelper, IPaymentHelper paymentHelper, IWebHostEnvironment webHostEnvironment, IBonusHelper bonusHelper, IBinaryHelper binaryHelper)
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
            _binaryHelper = binaryHelper;
            _bonusHelper = bonusHelper;
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

        public IActionResult News()
        {
            ViewBag.Title = "Create News";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> News(NewsViewModel newsViewModel)
        {
            try
            {
                if (newsViewModel.Details != null)
                {
                    string uniqueFileName = string.Empty;
                    if (newsViewModel.MasterImage != null)
                    {
                        uniqueFileName = UploadedFile(newsViewModel);
                    }

                    News news = new News
                    {
                        Name = newsViewModel.Name,
                        Details = newsViewModel.Details,
                        CreatedBy = User.Identity.Name,
                        DateCreated = DateTime.Now,
                        MasterImageUrl = string.IsNullOrEmpty(uniqueFileName) ? null : uniqueFileName,
                        Active = true,
                        Deleted = false
                    };
                    _context.News.Add(news);
                    await _context.SaveChangesAsync();
                    ModelState.Clear();
                    return View();
                }
                return View();


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string UploadedFile(NewsViewModel newsViewModel)
        {
            string uniqueFileName = string.Empty;

            if (newsViewModel.MasterImage != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                string pathString = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(pathString))
                {
                    Directory.CreateDirectory(pathString);
                }
                uniqueFileName = Guid.NewGuid().ToString() + "_" + newsViewModel.MasterImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    newsViewModel.MasterImage.CopyTo(fileStream);
                }
            }
            return "/uploads/" + uniqueFileName;
        }

        public IActionResult GetNews()
        {
            var allNews = _context.News.Where(x => x.Deleted == false);
            ViewBag.Title = "News List";
            return View(allNews);
        }

        [HttpGet]
        public IActionResult EditNews(int? id)
        {
            ViewBag.Title = "Edit News";
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var getNews = _context.News.Find(id);

            if (getNews == null)
            {
                return NotFound();
            }

            return View(getNews);
        }

        [HttpPost]
        public async Task<IActionResult> EditNews(int? id, News news, IFormFile file)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }
                var user = await _context.News.Where(x => x.Id == id && x.Deleted == false).FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound();
                }
                if (file != null)
                {
                    string filename = System.Guid.NewGuid().ToString() + ".jpg";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", filename);
                    using (var steam = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(steam);
                    }
                    user.MasterImageUrl = "/uploads/" + filename;
                }

                user.Name = news.Name;
                user.Details = news.Details;
                user.CreatedBy = User.Identity.Name;
                user.DateCreated = DateTime.Now;
                user.Active = true;
                user.Deleted = false;


                await _context.SaveChangesAsync();
                return RedirectToAction("GetNews");


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IActionResult DeleteNews(int? id)
        {
            var deleteNews = _context.News.Find(id);
            if (deleteNews != null)
            {
                deleteNews.Deleted = true;
                deleteNews.Active = false;
                _context.Update(deleteNews);
                _context.SaveChanges();
                return RedirectToAction("GetNews");
            }
            return View();
        }

        [HttpGet]
        public JsonResult GetNewsById(int newsId)
        {
            if (newsId == 0)
            {
                return Json(new { isError = true, msg = "Error Occurred" });
            }
            var news = _context.News
                .FirstOrDefault(x => x.Id == newsId && x.Active);
            return Json(new { isError = false, data = news });
        }

        [HttpPost]
        public async Task<JsonResult> ImpersonateAUser(string username)
        {
            try
            {
                var session = HttpContext.Session;
                var responseMsg = string.Empty;
                var adminId = _userHelper.GetCurrentUserId(User.Identity.Name);
                var getUser = _userManager.Users.Where(x => x.UserName == username).FirstOrDefault();
                if (getUser == null)
                {
                    return Json(new { isError = true, msg = "This username does not exist, try again." });
                }
                if (getUser.UserName == User.Identity.Name)
                {
                    return Json(new { isError = true, msg = "You can't impersonate yourself, you're logged In already." });
                }
                var impersonationRecord = new Impersonation
                {
                    AdifMemberId = getUser.Id,
                    AdminUserId = adminId,
                    AmTheRealUser = false,
                    DateImpersonated = DateTime.Now
                };
                if (impersonationRecord != null)
                {
                    await _context.Impersonations.AddAsync(impersonationRecord);
                    _context.SaveChanges();
                    await _signInManager.SignInAsync(getUser, isPersistent: false);
                    var sessionKeyName = impersonationRecord.AdminUserId;
                    if (string.IsNullOrEmpty(session.GetString(sessionKeyName)))
                    {
                        session.SetString(sessionKeyName, "true");
                    }
                    else { session.Clear(); }
                    return Json(new { isError = false, msg = "User Impersonated  successfully." });
                }
                return Json(new { isError = true, msg = "Error Occured while impersonating." });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LogAdminOut()
        {
            try
            {
                var session = HttpContext.Session;

                var currentUserId = _userHelper.FindByUserName(User.Identity.Name);
                var lastTimeImpersonated = _context.Impersonations.Where(x => x.AdifMemberId == currentUserId.Id).Max(x => x.DateImpersonated);
                if (lastTimeImpersonated != null)
                {
                    var impersonationRecords = _context.Impersonations.Where(x => x.DateImpersonated == lastTimeImpersonated && x.EndSession == false).FirstOrDefault();
                    impersonationRecords.EndSession = true;
                    impersonationRecords.DateSessionEnded = DateTime.Now;
                    _context.Update(impersonationRecords);
                    _context.SaveChanges();
                    var admin = _userHelper.FindById(impersonationRecords.AdminUserId);
                    session.Clear();
                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(admin, isPersistent: true);
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public IActionResult PaymentApproval(string refferer, DateTime sortTypeFrom, DateTime sortTypeTo, string name, int pageNumber, int pageSize)
        {
            try
            {
                var paymentViewModel = new PendingPaymentsSearchResultViewModel(_generalConfiguration)
                {
                    PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
                    PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
                    SortTypeFrom = sortTypeFrom,
                    SortTypeTo = sortTypeTo,
                    Name = name,
                    Refferer = refferer,
                    DollarRate = _generalConfiguration.DollarRate,
                };
                pageNumber = (pageNumber == 0 ? paymentViewModel.PageNumber : pageNumber);
                pageSize = pageSize == 0 ? paymentViewModel.PageSize : pageSize;
                var pendindRegPayments = _paymentHelper.PendingRegFeeDetails(paymentViewModel, pageNumber, pageSize);
                paymentViewModel.PaymentRecords = pendindRegPayments;
                if (pendindRegPayments != null)
                {
                    return View(paymentViewModel);
                }
                return View();
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
        public async Task<JsonResult> ApproveRegFee(Guid paymentId)
        {
            var responseMsg = string.Empty;
            try
            {
                if (paymentId != Guid.Empty)
                {
                    var user = _userHelper.GetCurrentUserId(User.Identity.Name);
                    var payment = _context.PaymentForms.Where(x => x.Id == paymentId).Include(x => x.User).FirstOrDefault();
                    var checkifApprovedBefore = _paymentHelper.CheckIfApproved(paymentId);
                    if (checkifApprovedBefore)
                    {
                        return Json(new { isError = true, msg = "This registration has  been approved before" });
                    }
                    var checkIfUserHasPaid = _paymentHelper.CheckUserRegPayment(payment.User.Id);
                    if (!checkIfUserHasPaid)
                    {
                        return Json(new { isError = true, msg = "This user has already paid registration fee" });
                    }
                    var approve = _paymentHelper.ApproveRegFee(paymentId, user);
                    if (approve)
                    {
                        //Sending out matching bonuses to referrals
                        _binaryHelper.SendUserGenerationBonuses(payment.UserId);

                        //Sending out bonuses to users and cordinators
                        await _bonusHelper.EnqueueApproveRegFeePaymentProcess(payment);
                        return Json(new { isError = false, msg = "Registration Payment has been approved, all bonuses are now processing" });
                    }
                }
                return Json(new { isError = true, msg = "Could not find registration to approve" });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult DeclineRegFee(Guid paymentId)
        {
            try
            {
                if (paymentId != Guid.Empty)
                {
                    var loggedInUser = _userHelper.GetCurrentUserId(User.Identity.Name);
                    var regFeePayments = _context.PaymentForms.Where(s => s.Id == paymentId && s.Status == Status.Pending).Include(s => s.User).FirstOrDefault();
                    if (regFeePayments != null)
                    {
                        if (regFeePayments.Status == Status.Approved)
                        {
                            return Json(new { isError = true, msg = " This Registration payment request has been approved" });
                        }
                        if (regFeePayments.Status == Status.Rejected)
                        {
                            return Json(new { isError = true, msg = "This Registration payment request has been rejected" });
                        }
                        var rejectPayment = _paymentHelper.RejectPayment(paymentId, loggedInUser);
                        if (rejectPayment)
                        {
                            return Json(new { isError = false, msg = " Registration Fee Rejected" });
                        }
                        return Json(new { isError = true, msg = "Error occured while rejecting, try again." });
                    }
                }
                return Json(new { isError = true, msg = " No Payment Request Found" });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }

        public JsonResult ApproveToken(Guid paymentId)
        {
            try
            {
                if (paymentId != Guid.Empty)
                {
                    var user = _userHelper.GetCurrentUserId(User.Identity.Name);
                    var checkifApprovedBefore = _paymentHelper.CheckIfApproved(paymentId);
                    if (checkifApprovedBefore)
                    {
                        return Json(new { isError = true, msg = "This token has  been approved before" });
                    }
                    var approve = _paymentHelper.ApproveTokenFee(paymentId, user);
                    if (approve)
                    {
                        return Json(new { isError = false, msg = "Token Payment has been approved successfully" });
                    }
                    return Json(new { isError = true, msg = "Could not approve" });
                }
                return Json(new { isError = true, msg = "Network Failure" });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult DeclineTokenPayment(Guid paymentId)
        {
            try
            {
                if (paymentId != Guid.Empty)
                {
                    var loggedInUser = _userHelper.GetCurrentUserId(User.Identity.Name);
                    var checkifApprovedBefore = _paymentHelper.CheckIfApproved(paymentId);
                    if (checkifApprovedBefore)
                    {
                        return Json(new { isError = true, msg = "This token payment has been approved before" });
                    }
                    var checkifRejectedBefore = _paymentHelper.CheckIfDeclined(paymentId);
                    if (checkifRejectedBefore)
                    {
                        return Json(new { isError = true, msg = "This token payment has been declined before" });
                    }
                    var rejectPayment = _paymentHelper.RejectTokenPayment(paymentId, loggedInUser);
                    if (rejectPayment)
                    {
                        return Json(new { isError = false, msg = " Token payment fee declined" });
                    }
                    return Json(new { isError = true, msg = "Error occured while rejecting, try again." });

                }
                return Json(new { isError = true, msg = " No Payment Request Found" });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult AppreciateMember()
        {
            var usersInBonus = _userHelper.GetAllUserForBonus();
            ViewBag.UsersInBonus = usersInBonus;
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> AppreciateMember(List<string> userIds, string details)
        {
            try
            {
                if (userIds != null || userIds.Count > 0 && details != null)
                {
                    var appreciationDetails = JsonConvert.DeserializeObject<Appreciation>(details);
                    if (appreciationDetails != null)
                    {
                        var paymentId = Guid.Empty;
                        var currentAdminId = _userHelper.GetCurrentUserId(User.Identity.Name);
                        var appreciatedAmountinNaira = _generalConfiguration.GGCWithdrawalConversionToNaira * appreciationDetails.Amount;
                        var amountinGGC = appreciatedAmountinNaira / _generalConfiguration.GGCWithdrawalConversionToNaira;

                        appreciationDetails.Amount = appreciatedAmountinNaira;
                        foreach (var userId in userIds)
                        {
                            if (!string.IsNullOrEmpty(userId))
                            {
                                var submitRequest = await _userHelper.SubmitAppreciationRequest(appreciationDetails, currentAdminId, userId);

                                if (submitRequest)
                                {
                                    var creaditdetails = "Credited " + appreciationDetails.Amount.ToString("F2") + " GAP, " + appreciationDetails.AppreciationDetails;
                                    await _paymentHelper.CreditWallet(userId, appreciationDetails.Amount, paymentId, creaditdetails);
                                }
                            }
                        }
                        return Json(new { isError = false, msg = $"You have successfully appreciated members with {amountinGGC.ToString("F0")} GAP." });
                    }
                    return Json(new { isError = true, msg = "An error occurred." });
                }
                return Json(new { isError = true, msg = "No user selected" });
            }
            catch (Exception exp)
            {
                return Json(new { isError = true, msg = "An unexpected error occurred." + exp });
            }
        }

        [Route("/Pending Withdrawals")]
        public IActionResult PendingWithdrawals(string accountNumber, string accountName, DateTime sortTypeFrom, DateTime sortTypeTo, int pageNumber, int pageSize)
        {
            var PendingWithdrawalsViewModel = new PendingWithdrawalsSearchResultViewModel(_generalConfiguration)
            {
                PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
                PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
                SortTypeFrom = sortTypeFrom,
                SortTypeTo = sortTypeTo,
                AccountName = accountName,
                AccountNumber = accountNumber,
            };
            pageNumber = (pageNumber == 0 ? PendingWithdrawalsViewModel.PageNumber : pageNumber);
            pageSize = pageSize == 0 ? PendingWithdrawalsViewModel.PageSize : pageSize;
            var pendings = _paymentHelper.GetPendingWithdrawals(PendingWithdrawalsViewModel, pageNumber, pageSize);
            PendingWithdrawalsViewModel.WithdrawalRecords = pendings;

            if (pendings != null)
            {
                return View(PendingWithdrawalsViewModel);
            }
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ApproveWithdrawals(Guid withdrawalRequestId)
        {
            var paymentId = Guid.Empty;
            var responseMsg = string.Empty;
            try
            {
                var currentUserId = _userHelper.GetCurrentUserId(User.Identity.Name);
                var requestForm = await _context.WithdrawFunds.Where(s => s.Id == withdrawalRequestId).Include(s => s.User).FirstOrDefaultAsync();
                if (requestForm != null && requestForm.WithdrawStatus == Status.Approved)
                    return Json(new { isError = true, msg = "This transaction has already been approved" });

                var responseStatus = _paymentHelper.ApproveWithdrawalRequest(requestForm, currentUserId, withdrawalRequestId);
                if (responseStatus)
                {
                    var details = requestForm.Amount + " Debit from Withdrawal Request ";
                    await _paymentHelper.DebitWallet(requestForm.UserId, requestForm.Amount, paymentId, details);
                    return Json(new { isError = false, msg = "Approved successfully" });
                }
                return Json(new { isError = !responseStatus, msg = responseMsg });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult RejectWithdrawalRequest(Guid withdrawalId)
        {
            try
            {
                if (withdrawalId != Guid.Empty)
                {
                    var withdrawals = _paymentHelper.RejectWithdrawalRequest(withdrawalId);
                    if (withdrawals != null)
                    {
                        return Json(new { isError = false, msg = " Withdrawal Request Rejected Successfully " });
                    }
                    else
                    {
                        return Json(new { isError = true, msg = "This withdrawal request has been approved, check other requests." });
                    }
                }
                return Json(new { isError = true, msg = " Error occured while rejecting request, try again." });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult CompanySettings()
        {
            try
            {
                var companySetting = _adminHelper.GetCompanySettings().Result;
                if (companySetting != null)
                {
                    return View(companySetting);
                }
                return View();
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult AddCompanySetting(string companySettingViewModel)
        {
            try
            {
                if (companySettingViewModel != null)
                {
                    var companySettingsDetails = JsonConvert.DeserializeObject<CompanySettingViewModel>(companySettingViewModel);
                    if (companySettingsDetails != null)
                    {
                        var saveCompanySettings = _adminHelper.UpdateCompanySettings(companySettingsDetails);
                        if (saveCompanySettings)
                        {
                            return Json(new { isError = false, msg = "Company settings added successfully" });
                        }
                        return Json(new { isError = true, msg = "Failed to Update Company Settings" });
                    }
                }
                return Json(new { isError = true, msg = "Unable to Update Company Settings" });
            }
            catch (Exception)
            {

                throw;
            }
        }

        public JsonResult ReactivateUser(string userId)
        {
            if (userId != null)
            {
                var Reactivate = _adminHelper.ReactivateUser(userId);
                if (Reactivate)
                {
                    return Json(new { isError = false, msg = " You have Reactivated this member" });
                }
            }
            return Json(new { isError = true, msg = " Member Not Found" });
        }

        public async Task<JsonResult> ApproveReActivationFee(Guid paymentId)
        {
            var responseMsg = string.Empty;
            try
            {
                if (paymentId != Guid.Empty)
                {
                    var userId = _userHelper.GetCurrentUserId(User.Identity.Name);
                    //var payment = _context.PaymentForms.Where(x => x.Id == paymentId).Include(x => x.User).FirstOrDefault();
                    var checkifApprovedBefore = _paymentHelper.CheckIfApproved(paymentId);
                    if (checkifApprovedBefore)
                    {
                        return Json(new { isError = true, msg = "This Re-Activation has  been approved before" });
                    }
                    var approve = _paymentHelper.ApproveReActivationFee(paymentId, userId);
                    if (approve)
                    {
                        return Json(new { isError = false, msg = "Re-Activation Payment has been approved" });
                    }

                }
                return Json(new { isError = true, msg = "Could not find Re-Activation to approve" });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeclineReActivationFee(Guid paymentId)
        {
            try
            {
                if (paymentId != Guid.Empty)
                {
                    var loggedInUser = _userHelper.GetCurrentUserId(User.Identity.Name);
                    var regFeePayments = _context.PaymentForms.Where(s => s.Id == paymentId && s.Status == Status.Pending).Include(s => s.User).FirstOrDefault();
                    if (regFeePayments != null)
                    {
                        if (regFeePayments.Status == Status.Approved)
                        {
                            return Json(new { isError = true, msg = " This Re-Activation payment request has been approved" });
                        }
                        if (regFeePayments.Status == Status.Rejected)
                        {
                            return Json(new { isError = true, msg = "This Re-Activation payment request has been rejected" });
                        }
                        var rejectPayment = _paymentHelper.RejectReActivationPayment(paymentId, loggedInUser);
                        if (rejectPayment)
                        {
                            return Json(new { isError = false, msg = " Re-Activation Fee Rejected" });
                        }
                        return Json(new { isError = true, msg = "Error occurred while rejecting payment, try again." });
                    }
                }
                return Json(new { isError = true, msg = " No Re-Activation Request Found" });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }

        public JsonResult GetAllUsers(string term)
        {
            if (term != null)
            {
                var user = _userHelper.GetUsers(term);
                if (user != null)
                {
                    return Json(user);
                }
                return Json("Error Occurred");
            }
            return Json("Please add name");
        }


        [HttpPost]
        public JsonResult DeleteCordinator(int id)
        {
            if (id != 0)
            {
                var distributor = _adminHelper.RemoveCordinator(id);
                if (distributor != null)
                {
                    return Json(new { isError = false, msg = "Cordinator's Details Removed Successfully" });
                }
                return Json(new { isError = true, msg = "Unable To Remove Cordinator From Role" });
            }
            return Json(new { isError = true, msg = "Error Occurred" });
        }

        public IActionResult Users(string userName, string email, string phoneNumber, string name, int pageNumber, int pageSize)
        {
            try
            {
                var userDetailsViewModel = new ApplicationUserSearchResultViewModel(_generalConfiguration)
                {
                    PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
                    PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
                    Email = email,
                    Name = name,
                    PhoneNumber = phoneNumber,
                    UserName = userName,
                };
                pageNumber = (pageNumber == 0 ? userDetailsViewModel.PageNumber : pageNumber);
                pageSize = pageSize == 0 ? userDetailsViewModel.PageSize : pageSize;
                var registeredUsers = _adminHelper.RegisteredUsersDetails(userDetailsViewModel, pageNumber, pageSize);
                userDetailsViewModel.UserRecords = registeredUsers;
                if (registeredUsers != null)
                {
                    return View(userDetailsViewModel);
                }
                return View();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult DeactivateUser(string userId)
        {
            if (userId != null)
            {
                var deactivate = _adminHelper.DeactivateUser(userId);
                if (deactivate)
                {
                    return Json(new { isError = false, msg = " You have Deactivated this member" });
                }
            }
            return Json(new { isError = true, msg = " Member Not Found" });
        }
        public IActionResult UserFullDetail(string id)
        {
            try
            {
                var user = _adminHelper.GetUserFullDetails(id);
                return PartialView(user);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpGet]
        public IActionResult WithdrawalFullDetails(Guid id)
        {
            try
            {
                if (id != Guid.Empty)
                {
                    var withdrawalDetails = _adminHelper.GetWithdrawalDetails(id);
                    return PartialView(withdrawalDetails);
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpGet]
        [Route("GetOldMembers")]
        public async Task<JsonResult> GetOldMembers()
        {
            var users = await _bonusHelper.GetOldMembers();
            return Json($"{users}User found");
        }

        public async Task<JsonResult> ApprovePackageFee(Guid paymentId)
        {
            var responseMsg = string.Empty;
            try
            {
                if (paymentId != Guid.Empty)
                {
                    var user = _userHelper.GetCurrentUserId(User.Identity.Name);
                    var checkifApprovedBefore = _paymentHelper.CheckIfApproved(paymentId);
                    if (checkifApprovedBefore)
                    {
                        return Json(new { isError = true, msg = "This Package Payment Fee has  been approved before" });
                    }
                    var approve = _paymentHelper.ApprovePackageFee(paymentId, user);
                    if (approve)
                    {
                        return Json(new { isError = false, msg = "package fee Payment has been approved" });
                    }

                }
                return Json(new { isError = true, msg = "Could not find package payment to approve" });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeclinePackagePaymentFee(Guid paymentId)
        {
            try
            {
                if (paymentId != Guid.Empty)
                {
                    var loggedInUser = _userHelper.GetCurrentUserId(User.Identity.Name);
                    var checkifRejectedBefore = _paymentHelper.CheckIfDeclined(paymentId);
                    if (checkifRejectedBefore)
                    {
                        return Json(new { isError = true, msg = "This token payment has been declined before" });
                    }
                    var rejectPayment = _paymentHelper.RejectPackagePaymentFee(paymentId, loggedInUser);
                    if (rejectPayment)
                    {
                        return Json(new { isError = false, msg = " Package Fee Rejected" });
                    }
                    return Json(new { isError = true, msg = "Error occurred while rejecting payment, try again." });

                }
                return Json(new { isError = true, msg = " No package fee payment Request Found" });
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> SendMemberTokens(List<string> peopleIds, string details)
        {
            try
            {
                if (peopleIds != null || peopleIds.Count > 0 && details != null)
                {
                    var appreciationDetails = JsonConvert.DeserializeObject<Appreciation>(details);
                    if (appreciationDetails != null)
                    {
                        var paymentId = Guid.Empty;
                        var currentAdminId = _userHelper.GetCurrentUserId(User.Identity.Name);

                        foreach (var memberId in peopleIds)
                        {
                            if (!string.IsNullOrEmpty(memberId))
                            {
                                var submitToken = await _userHelper.SendTokensToMembers(appreciationDetails, currentAdminId, memberId);
                                if (submitToken)
                                {
                                    await _bonusHelper.CreditAGCWallet(memberId, appreciationDetails.Amount, paymentId);
                                }
                            }
                        }
                        return Json(new { isError = false, msg = $"You have successfully appreciated members with {appreciationDetails.Amount} GAP Tokens." });
                    }
                    return Json(new { isError = true, msg = "An error occurred." });
                }
                return Json(new { isError = true, msg = "No user selected" });
            }
            catch (Exception exp)
            {
                return Json(new { isError = true, msg = "An unexpected error occurred." + exp });
            }
        }

        [HttpGet]
        public IActionResult MemberWalletBalance(string userName, string name, int pageNumber, int pageSize)
        {
            try
            {
                var loggInedUser = _userHelper.FindByUserName(User.Identity.Name);
                var userWalleetsDetailsViewModel = new ApplicationUserSearchResultViewModel(_generalConfiguration)
                {
                    PageNumber = pageNumber == 0 ? _generalConfiguration.PageNumber : pageNumber,
                    PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
                    Name = name,
                    UserName = userName,
                };
                pageNumber = (pageNumber == 0 ? userWalleetsDetailsViewModel.PageNumber : pageNumber);
                pageSize = pageSize == 0 ? userWalleetsDetailsViewModel.PageSize : pageSize;
                var getUsersWalletDetails = _userHelper.GetUserWalletsDetails(userWalleetsDetailsViewModel, pageNumber, pageSize);
                userWalleetsDetailsViewModel.UserRecords = getUsersWalletDetails;
                if (getUsersWalletDetails != null)
                {
                    return View(userWalleetsDetailsViewModel);
                }
                return View();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



    }
}
