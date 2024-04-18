using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using System.Data;

namespace GGC.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public NewsController(IUserHelper userHelper, AppDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userHelper = userHelper;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult News(NewsViewModel newsViewModel)

        {
            ViewBag.Title = "News";

            int newsDay = newsViewModel.DateCreated.Day;


            var newsDisplay = _context.News.Where(x => x.Deleted == false && x.Active == true).OrderByDescending(x => x.DateCreated).Take(5);
            return View(newsDisplay);
        }


        public IActionResult SingleNews(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            ViewBag.Title = "News";
            var mainNewsView = _context.News.Where(x => x.Id == id && x.Deleted == false && x.Active == true).FirstOrDefault();
            return View(mainNewsView);
        }

        [HttpPost]
        public async Task<JsonResult> LogUserRedNews(string userName, int newsId)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user != null)
                {
                    var userId = user.Id;

                    var redNewsId = _userHelper.GetNewsById(newsId);
                    if (redNewsId != null)
                    {
                        string existingUserIds = redNewsId?.RedBy;

                        if (existingUserIds != null && existingUserIds.Split(',').Contains(userId))
                        {
                            return Json(new { isError = true });

                        }
                        existingUserIds = string.IsNullOrEmpty(existingUserIds)
                        ? userId
                        : existingUserIds + "," + userId;

                        redNewsId.RedBy = existingUserIds;
                        _context.Update(redNewsId);
                        await _context.SaveChangesAsync();
                        return Json(new { isError = false });

                    }

                }
                return Json(new { isError = true});

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

    }
}
