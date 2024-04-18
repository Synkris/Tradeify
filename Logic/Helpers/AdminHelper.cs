using Core.Config;
using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Logic.Helpers
{
    public class AdminHelper : IAdminHelper
    {
        private readonly AppDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IPaymentHelper _paymentHelper;
        private readonly UserManager<ApplicationUser> _userManager;
        private IGeneralConfiguration _generalConfiguration;

        public AdminHelper(AppDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IGeneralConfiguration generalConfiguration, IUserHelper userHelper, IPaymentHelper paymentHelper)
        {
            _context = context;
            _userManager = userManager;
            _generalConfiguration = generalConfiguration;
            _userHelper = userHelper;
            _paymentHelper = paymentHelper;
        }

        public ApplicationUserViewModel UserDetailsToEdit(string userId)
        {
            var userPackage = GetUserPackage(userId);
            var applicationViewModel = new ApplicationUserViewModel();
            var userDetails = _context.ApplicationUser.Where(x => x.Id == userId).Include(x => x.Gender).FirstOrDefault();
            var cordinator = GetCordinatorUserName(userDetails?.CordinatorId);
            if (userDetails != null)
            {
                var result = new ApplicationUserViewModel
                {
                    FirstName = userDetails.FirstName,
                    LastName = userDetails.LastName,
                    Email = userDetails.Email,
                    Phonenumber = userDetails.PhoneNumber,
                    UserName = userDetails.UserName,
                    Id = userDetails.Id,
                    CordinatorId = userDetails?.CordinatorId,
                    CordinatorUsername = cordinator?.CordinatorUserName,
                    GenderId = userDetails.GenderId,
                    PackageName = userPackage?.Name,
                    PackageId = userPackage?.PackageId == null ? 0 : (int)userPackage?.PackageId,
                };
                return result;
            }
            return null;
        }

        public bool EditedDetails(ApplicationUserViewModel userDetails)
        {
            if (userDetails != null)
            {
                var editUser = _context.ApplicationUser.Where(a => a.Id == userDetails.Id && a.RefferrerId != null && a.UserName != null).FirstOrDefault();
                if (editUser != null)
                {
                    editUser.FirstName = userDetails.FirstName;
                    editUser.LastName = userDetails.LastName;
                    editUser.Email = userDetails.Email;
                    editUser.PhoneNumber = userDetails.Phonenumber;
                    editUser.CordinatorId = userDetails.CordinatorId;
                    _context.Update(editUser);

                    var updateUserPackage = _context.UserPackages.Where(x => x.UserId == userDetails.Id && x.Active && !x.Deleted).FirstOrDefault();
                    if (updateUserPackage != null)
                    {
                        if (updateUserPackage.PackageId != userDetails.PackageId)
                        {
                            var packageDetails = _userHelper.GetPackageforUser(userDetails.PackageId);
                            if (packageDetails != null)
                            {
                                updateUserPackage.Name = packageDetails?.Name;
                                updateUserPackage.Amount = (decimal)(packageDetails?.Price);
                                updateUserPackage.UserId = userDetails.Id;
                                updateUserPackage.PackageId = (int)(packageDetails?.Id);
                                updateUserPackage.MaxGeneration = (int)packageDetails?.MaxGeneration;
                                updateUserPackage.Active = true;
                            }
                            _context.Update(updateUserPackage);
                            UpdatePaymentForm(userDetails.PackageId, editUser.Id);
                        }
                    }
                    _context.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public IPagedList<CommonDropdownViewModel> GetCommonDropdowns(CommonDropdownSearchResultViewModel commonDropdownSearch, int pageNumber, int pageSize)
        {

            var dropdownQuery = _context.CommonDropdowns.Where(s => s.Deleted != true).OrderByDescending(s => s.DateCreated).AsQueryable();

            if (!string.IsNullOrEmpty(commonDropdownSearch.Name))
            {
                dropdownQuery = dropdownQuery.Where(v =>
                    v.Name.ToLower().Contains(commonDropdownSearch.Name.ToLower())
                );
            }


            if (commonDropdownSearch.SortTypeFrom != DateTime.MinValue)
            {
                dropdownQuery = dropdownQuery.Where(v => v.DateCreated >= commonDropdownSearch.SortTypeFrom);
            }
            if (commonDropdownSearch.SortTypeTo != DateTime.MinValue)
            {
                dropdownQuery = dropdownQuery.Where(v => v.DateCreated <= commonDropdownSearch.SortTypeTo);
            }

            var totalItemCount = dropdownQuery.Count();
            var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

            var dropDownViewModelList = dropdownQuery.Select(dropdown => new CommonDropdownViewModel
            {
                Id = dropdown.Id,
                DropdownKey = dropdown.DropdownKey,
                Code = dropdown.Code,
                Name = dropdown.Name,
                DateCreated = dropdown.DateCreated,
            })
            .ToPagedList(pageNumber, pageSize, totalItemCount);
            commonDropdownSearch.PageCount = totalPages;
            commonDropdownSearch.CommonDropdownRecords = dropDownViewModelList;
            if (dropDownViewModelList.Count() > 0)
            {
                return dropDownViewModelList;
            }
            else
            {
                return null;
            }

        }

        public bool UpdateDropDownService(string dropdown, int? id)
        {
            try
            {
                if (dropdown != null)
                {
                    var oldDropdown = _context.CommonDropdowns.Where(x => x.Id == id).FirstOrDefault();
                    if (oldDropdown != null)
                    {
                        oldDropdown.Name = dropdown;
                        _context.Update(oldDropdown);
                        _context.SaveChanges();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string RemoveDropDown(int id)
        {
            if (id != 0)
            {
                var dropDownToBeRemoved = _context.CommonDropdowns.Where(x => x.Id == id && !x.Deleted).FirstOrDefault();
                if (dropDownToBeRemoved != null)
                {
                    dropDownToBeRemoved.Deleted = true;
                    _context.CommonDropdowns.Update(dropDownToBeRemoved);
                    _context.SaveChanges();
                    return "Dropdown Successfully Removed";
                }
            }
            return "Dropdown failed To Removed";
        }

        public UserPackages GetUserPackage(string userId)
        {
            if (userId != null)
            {
                var userPackage = _context.UserPackages.Where(x => x.UserId == userId && x.Active && !x.Deleted).OrderByDescending(o => o.DateCreated).FirstOrDefault();
                return userPackage;
            }
            return null;
        }

        public Cordinator GetCordinatorUserName(string cordinatorId)
        {
            if (cordinatorId != null)
            {
                var cordinator = _context.Cordinators.Where(a => a.CordinatorId == cordinatorId && !a.RemovedAsCordinator).FirstOrDefault();
                if (cordinator != null)
                {
                    return cordinator;
                }
            }
            return null;
        }

        public bool UpdatePaymentForm(int packageId, string userId)
        {
            if (packageId != 0 && userId != null)
            {
                var updatePaymentForm = _context.PaymentForms.Where(x => x.UserId == userId).FirstOrDefault();
                if (updatePaymentForm != null)
                {
                    updatePaymentForm.PackageId = packageId;
                    _context.Update(updatePaymentForm);
                    _context.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool CheckExistingDropdownName(string name, int drpKey)
        {
            if (name != null && drpKey != 0)
            {
                var checkName = _context.CommonDropdowns.Where(x => x.Name == name && x.DropdownKey == drpKey && x.Active && !x.Deleted).FirstOrDefault();
                if (checkName != null)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
