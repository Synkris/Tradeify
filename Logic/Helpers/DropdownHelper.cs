using Core.DB;
using Core.Models;
using Logic.IHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Helpers
{
	public class DropdownHelper : IDropdownHelper
	{
		private readonly AppDbContext _context;
		private readonly IUserHelper _userHelper;
		private UserManager<ApplicationUser> _userManager;

		public DropdownHelper(AppDbContext context, UserManager<ApplicationUser> userManager, IUserHelper userHelper)
		{
			_context = context;
			_userHelper = userHelper;
			_userManager = userManager;
		}

		public List<Cordinator> DropdownOfCordinator()
		{
			try
			{
				var common = new Cordinator()
				{
					CordinatorId = "",
					CordinatorUserName = "Select Cordinator"
				};
				var cordinatorList = _context.Cordinators.Where(x => x.Id > 0 && !x.RemovedAsCordinator).ToList();
				var drp = cordinatorList.Select(x => new Cordinator
				{
					CordinatorId = x.CordinatorId,
					CordinatorUserName = x.CordinatorUserName,
				}).ToList();
				drp.Insert(0, common);
				return drp;
			}
			catch (Exception exp)
			{
				throw exp;
			}
		}
		public List<Packages> DropdownOfPackages()
		{
			try
			{
				var common = new Packages()
				{
					Id = 0,
					Name = "Select Packages"
				};
				var packages = _context.Packages.Where(x => x.Id > 0 && x.Active && !x.Deleted)
				.Select(x => new Packages
				{
					Id = x.Id,
					Name = x.Name,
				}).ToList();
				packages.Insert(0, common);
				return packages;
			}
			catch (Exception exp)
			{
				throw exp;
			}
		}

		public async Task<List<CommonDropdowns>> GetDropdownByKey(DropdownEnums dropdownKey, bool deleteOption = false)
		{
			var common = new CommonDropdowns()
			{
				Id = 0,
				Name = "-- Select Gender--"
			};
			var dropdowns = await _context.CommonDropdowns.Where(s => s.Deleted == deleteOption && s.DropdownKey == (int)dropdownKey).OrderBy(s => s.Name).ToListAsync();
			dropdowns.Insert(0, common);
			return dropdowns;
		}

        public async Task<List<CommonDropdowns>> GetBankDropdownByKey(DropdownEnums dropdownKey, bool deleteOption = false)
        {
            var common = new CommonDropdowns()
            {
                Id = 0,
                Name = "-- Select Bank --"
            };
            var dropdowns = await _context.CommonDropdowns.Where(s => s.Deleted == deleteOption && s.DropdownKey == (int)dropdownKey && s.Name.Contains("BANK")).OrderBy(s => s.Name).ToListAsync();
            dropdowns.Insert(0, common);
            return dropdowns;
        }

        public async Task<List<CommonDropdowns>> GetCryptoDropdown(DropdownEnums dropdownKey, bool deleteOption = false)
        {
            var common = new CommonDropdowns()
            {
                Id = 0,
                Name = "-- Select Wallet --"
            };
            var dropdowns = await _context.CommonDropdowns.Where(s => s.Deleted == deleteOption && s.DropdownKey == (int)dropdownKey && !s.Name.Contains("BANK")).OrderBy(s => s.Name).ToListAsync();
            dropdowns.Insert(0, common);
            return dropdowns;
        }
    }
}
