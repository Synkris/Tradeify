using Core.Config;
using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.Helpers;
using Logic.IHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Tradeify.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BonusController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGeneralConfiguration _generalConfiguration;
        private readonly IUserHelper _userHelper;
        private readonly IBonusHelper _bonusHelper;

        public BonusController(AppDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IGeneralConfiguration generalConfiguration, IUserHelper userHelper, IBonusHelper bonusHelper)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _generalConfiguration = generalConfiguration;
            _userHelper = userHelper;
            _bonusHelper = bonusHelper;
        }

        public IActionResult Index()
        {
            var usersInBonus = _userHelper.GetAllUserForBonus();
            ViewBag.UsersInBonus = usersInBonus;
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> PayRegBonuses(List<string> userIds)
        //{

        //    if (userIds == null || userIds.Count == 0)
        //    {
        //        return Json(new { isError = true, msg = "No user selected" });
        //    }
        //    var paymentCheck = await _bonusHelper.CheckUserRegBonuses(userIds);
        //    if (paymentCheck)
        //    {
        //        return Json(new { isError = false, msg = "Regfee bonuses assigned successfully", });
        //    }
        //    return Json(new { isError = true, msg = $"No Regfee payment found for user" });
        //}

        [HttpGet]
        public IActionResult UserGenLog(string userName, DateTime sortTypeFrom, DateTime sortTypeTo, string transactionType, int pageNumber, int pageSize)
        {
			var userGenerationLogViewModel = new UserGenerationLogSearchResultViewModel
			{
				PageNumber = pageNumber == 0 ? 1 : pageNumber,
				PageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize,
				SortTypeFrom = sortTypeFrom,
				SortTypeTo = sortTypeTo,
				TransactionType = transactionType,
				UserName = userName,
			};
			pageNumber = (pageNumber == 0 ? userGenerationLogViewModel.PageNumber : pageNumber);
			pageSize = pageSize == 0 ? _generalConfiguration.PageSize : pageSize;
			var userGenLog = _bonusHelper.listOfUserGenLog(userGenerationLogViewModel,pageNumber,pageSize);
			userGenerationLogViewModel.UserGenerationLogRecords = userGenLog;
			if (userGenLog != null && userGenLog.Count() > 0)
            {
                return View(userGenerationLogViewModel);
            }
            return View();
        }
        [HttpPost]
		public IActionResult UpdateUserStatus(List<int> genLogIds, string action)
		{
			try
			{
                if (genLogIds == null || genLogIds.Count == 0)
                {
                    return Json(new { isError = true, msg = "No user selected" });
                }
                switch (action)
				{
					case "approve":
                       _bonusHelper.CheckUserRegBonuses(genLogIds);
                    return Json(new { isError = false, msg = "Status updated successfully", });
                      
					case "reject":
					_bonusHelper.RejectMatchingBonus(genLogIds);
                    return Json(new { isError = false, msg = "Matching Bonus Rejected ", });
                   
					default:
					return Json(new { isError = true, msg = "Invalid action." });
				}
			}
			catch (Exception ex)
			{
				return Json(new { isError = true, msg = $"An error occurred while updating user status:. {ex.InnerException}" });
			}
		}

        public IActionResult UpdateOneUserStatus(int genLogId, string action)
        {
            try
            {
                if (genLogId == 0)
                {
                    return Json(new { isError = true, msg = "User not found." });
                }
                switch (action)
                {
                    case "approve":
                        var result = _bonusHelper.AssignMatchingBonus(genLogId).Result;
                        if (result) { return Json(new { isError = false, msg = "Status updated successfully", }); }
						return Json(new { isError = true, msg = "Could not update user bonus at this moment", });
					case "reject":
						 result = _bonusHelper.RejectOneUserGenLog(genLogId);
                        if(result)
                        return Json(new { isError = false, msg = "Matching Bonus Rejected ", });

                        return Json(new { isError = true, msg = "Could not decline bonus request at this moment ", });

                    default:
                        return Json(new { isError = true, msg = "Invalid action." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { isError = true, msg = "An error occurred while updating user status." });
            }
        }
    }
}
