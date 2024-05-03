using Core.Models;
using Logic.IHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Tradeify.Controllers
{
    [Authorize]
    public class BinaryController : Controller
	{
        private IBinaryHelper _binaryHelper;
        private IUserHelper _userHelper;
        private IDropdownHelper _dropdownHelper;
        private readonly UserManager<ApplicationUser> _userManager;
        public BinaryController(IBinaryHelper binaryHelper, IUserHelper userHelper, IDropdownHelper dropdownHelper, UserManager<ApplicationUser> userManager)
        {
            _binaryHelper = binaryHelper;
            _userHelper = userHelper;
            _dropdownHelper = dropdownHelper;
            _userManager = userManager;
        }

        [Route("/binary")]
		[HttpGet]
        public IActionResult Index()
		{
			var userName = User.Identity.Name;
            ViewBag.Gen = _dropdownHelper.GetMaxGenerationEnums();
			if (userName == null)
			{
				return Redirect("Home/Error");
			}
			var userGen = _binaryHelper.GetUserTree(userName, 1).Result;

			return View(userGen);
		}

		[HttpGet]
		public  IActionResult GetUserGeneration(string userId, int gen)
        {
            try
            {
                if (userId != null && gen > 0)
                {
                    var userGenerationLogs = _binaryHelper.GetGenUsers(userId, gen);
                    if (userGenerationLogs.Any())
                    {
                        return Json(new { isError = false, genDetails = userGenerationLogs });

                    }
                    return Json(new { isError = true, msg = "No users under this generation" });

                }
                return null;

                
            }
            catch (Exception ex) 
			{
                throw ex;
			}
			
		}
	}
}
