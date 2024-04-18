using Core.DB;
using Core.Models;
using Logic.IHelpers;
using Logic.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Logic.Helpers
{
    public class PackageHelper : IPackageHelper
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PackageHelper(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<News> GetNews()
        {
            return _context.News.Where(x => x.Active).OrderByDescending(x => x.DateCreated).Take(3).ToList();
        }

        public List<News> GetAllNews()
        {
            return _context.News.Where(x => x.Active).OrderByDescending(x => x.DateCreated).Take(15).ToList();
        }
		public List<Packages> GetPackages()
		{
			try
			{
				return _context.Packages.Where(x => x.Active && !x.Deleted).ToList();
			}
			catch (Exception ex)
			{

				throw ex;
			}

		}

        public async Task<Packages> GetPackageById(int packageId)
        {
            try
            {

                var getpackageId = await _context.Packages.Where(x => !x.Deleted && x.Active && x.Id == packageId).FirstOrDefaultAsync();
                if (getpackageId != null)
                {
                    return getpackageId;
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
