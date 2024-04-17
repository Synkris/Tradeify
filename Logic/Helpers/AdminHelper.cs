using Logic.IHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
