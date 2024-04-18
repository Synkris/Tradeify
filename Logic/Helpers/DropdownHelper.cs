using Core.DB;
using Core.Models;
using Logic.Helper;
using Logic.IHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

        public List<DropDown> DropdownOfRoles()
        {
            try
            {
                var common = new DropDown()
                {
                    Id = "0",
                    Name = "Select Role"
                };
                var roles = _context.Roles.Where(r => !r.Name.ToLower().Contains("SuperAdmin")).Select(r => new DropDown
                {
                    Id = r.Id,
                    Name = r.Name,
                }).ToList();

                roles.Insert(0, common);
                return roles;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
        public List<EnumDropdownModalViewModel> GetMaxGenerationEnums()
        {
            var data = new List<EnumDropdownModalViewModel>();
            var modules = ((GenerationEnum[])Enum.GetValues(typeof(GenerationEnum)));

            foreach (var item in modules)
            {
                var enumId = (GenerationEnum)item;
                var surgestedEnum = GetEnumDescription(enumId);
                var mydata = new EnumDropdownModalViewModel()
                {
                    Name = surgestedEnum,
                    Id = (int)item,
                };
                data.Add(mydata);
            }
            return data;
        }


        public string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                var des = attributes.First().Description;
                return des;
            }

            return value.ToString();
        }
        public List<DropdownEnumModel> GetDropDownEnumsList()
        {
            return ((DropdownEnums[])Enum.GetValues(typeof(DropdownEnums))).Select(c => new DropdownEnumModel() { Id = (int)c, Name = c.ToString() }).Where(x => x.Id != (int)DropdownEnums.AdminNotice).ToList();
        }

        public async Task<bool> CreateDropdownsAsync(CommonDropdowns commonDropdown)
        {
            try
            {
                if (commonDropdown != null && commonDropdown.DropdownKey > 0 && commonDropdown.Name != null)
                {
                    CommonDropdowns newCommonDropdowns = new CommonDropdowns
                    {
                        Name = commonDropdown.Name,
                        DropdownKey = commonDropdown.DropdownKey,
                        Code = commonDropdown.Code,
                        Deleted = false,
                        Active = true,
                        DateCreated = DateTime.Now,
                    };

                    var createdDropdowns = await _context.CommonDropdowns.AddAsync(newCommonDropdowns);

                    await _context.SaveChangesAsync();

                    if (createdDropdowns.Entity.Id > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<DropdownEnumModel> GetPaymentTypeDropDownEnumsList()
        {
            var data = new List<DropdownEnumModel>();
            var enumsList = ((PaymentType[])Enum.GetValues(typeof(PaymentType))).Where(e => e != PaymentType.RegistrationFee && e != PaymentType.VtuActivationFee);

            data.Add(new DropdownEnumModel { Id = 0, Name = "-- Select Payment Type --" });

            foreach (var item in enumsList)
            {
                var enumId = (PaymentType)item;
                var descriptionEnum = GetEnumDescription(enumId);
                var mydata = new DropdownEnumModel()
                {
                    Name = descriptionEnum,
                    Id = (int)item,
                };
                data.Add(mydata);
            }
            return data;

        }


        //      public List<Cordinator> DropdownOfCordinator()
        //{
        //	try
        //	{
        //		var common = new Cordinator()
        //		{
        //			CordinatorId = "",
        //			CordinatorUserName = "Select Cordinator"
        //		};
        //		var cordinatorList = _context.Cordinators.Where(x => x.Id > 0 && !x.RemovedAsCordinator).ToList();
        //		var drp = cordinatorList.Select(x => new Cordinator
        //		{
        //			CordinatorId = x.CordinatorId,
        //			CordinatorUserName = x.CordinatorUserName,
        //		}).ToList();
        //		drp.Insert(0, common);
        //		return drp;
        //	}
        //	catch (Exception exp)
        //	{
        //		throw exp;
        //	}
        //}
        //public List<Packages> DropdownOfPackages()
        //{
        //	try
        //	{
        //		var common = new Packages()
        //		{
        //			Id = 0,
        //			Name = "Select Packages"
        //		};
        //		var packages = _context.Packages.Where(x => x.Id > 0 && x.Active && !x.Deleted)
        //		.Select(x => new Packages
        //		{
        //			Id = x.Id,
        //			Name = x.Name,
        //		}).ToList();
        //		packages.Insert(0, common);
        //		return packages;
        //	}
        //	catch (Exception exp)
        //	{
        //		throw exp;
        //	}
        //}

        //public async Task<List<CommonDropdowns>> GetDropdownByKey(DropdownEnums dropdownKey, bool deleteOption = false)
        //{
        //	var common = new CommonDropdowns()
        //	{
        //		Id = 0,
        //		Name = "-- Select Gender--"
        //	};
        //	var dropdowns = await _context.CommonDropdowns.Where(s => s.Deleted == deleteOption && s.DropdownKey == (int)dropdownKey).OrderBy(s => s.Name).ToListAsync();
        //	dropdowns.Insert(0, common);
        //	return dropdowns;
        //}

        //      public async Task<List<CommonDropdowns>> GetBankDropdownByKey(DropdownEnums dropdownKey, bool deleteOption = false)
        //      {
        //          var common = new CommonDropdowns()
        //          {
        //              Id = 0,
        //              Name = "-- Select Bank --"
        //          };
        //          var dropdowns = await _context.CommonDropdowns.Where(s => s.Deleted == deleteOption && s.DropdownKey == (int)dropdownKey && s.Name.Contains("BANK")).OrderBy(s => s.Name).ToListAsync();
        //          dropdowns.Insert(0, common);
        //          return dropdowns;
        //      }

        //      public async Task<List<CommonDropdowns>> GetCryptoDropdown(DropdownEnums dropdownKey, bool deleteOption = false)
        //      {
        //          var common = new CommonDropdowns()
        //          {
        //              Id = 0,
        //              Name = "-- Select Wallet --"
        //          };
        //          var dropdowns = await _context.CommonDropdowns.Where(s => s.Deleted == deleteOption && s.DropdownKey == (int)dropdownKey && !s.Name.Contains("BANK")).OrderBy(s => s.Name).ToListAsync();
        //          dropdowns.Insert(0, common);
        //          return dropdowns;
        //      }
        //public List<DropdownEnumModel> GetDropDownEnumsList()
        //{
        //	return ((DropdownEnums[])Enum.GetValues(typeof(DropdownEnums))).Select(c => new DropdownEnumModel() { Id = (int)c, Name = c.ToString() }).Where(x => x.Id != (int)DropdownEnums.AdminNotice).ToList();
        //}

        //public async Task<bool> CreateDropdownsAsync(CommonDropdowns commonDropdown)
        //{
        //	try
        //	{
        //		if (commonDropdown != null && commonDropdown.DropdownKey > 0 && commonDropdown.Name != null)
        //		{
        //			CommonDropdowns newCommonDropdowns = new CommonDropdowns
        //			{
        //				Name = commonDropdown.Name,
        //				DropdownKey = commonDropdown.DropdownKey,
        //				Code = commonDropdown.Code,
        //				Deleted = false,
        //				Active = true,
        //				DateCreated = DateTime.Now,
        //			};

        //			var createdDropdowns = await _context.CommonDropdowns.AddAsync(newCommonDropdowns);

        //			await _context.SaveChangesAsync();

        //			if (createdDropdowns.Entity.Id > 0)
        //			{
        //				return true;
        //			}
        //			else
        //			{
        //				return false;
        //			}

        //		}
        //		else
        //		{
        //			return false;
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		throw ex;
        //	}
        //}
    }

    public class DropDown
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
    public class DropdownEnumModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }


}
