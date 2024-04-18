using Core.DB;
using Core.ViewModels;
using Logic.IHelper;
using Logic.IHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace GGC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RegistrationPackageController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IRegistrationPackageHelper _iregistrationPackageHelper;
        private readonly IDropdownHelper _idropdownHelper;


        public RegistrationPackageController(AppDbContext context, IRegistrationPackageHelper iregistrationPackageHelper, IDropdownHelper idropdownHelper)
        {
            _context = context;
            _iregistrationPackageHelper = iregistrationPackageHelper;
            _idropdownHelper = idropdownHelper;

        }
        [HttpGet]
        public IActionResult Index()
        
        {
            ViewBag.MaxEnum = _idropdownHelper.GetMaxGenerationEnums();
            var packageList = _iregistrationPackageHelper.GetPackagesList();
             return View(packageList);
        }
        [HttpPost]
        public IActionResult CreateRegPacks(string CreatePackageData)
        {
            
            if (CreatePackageData != null)
            {
                var regPackageViewModel = JsonConvert.DeserializeObject<RegistrationPackageViewModel>(CreatePackageData);
                if (regPackageViewModel != null)
                {
                    var checkIfPackageNameExists = _iregistrationPackageHelper.CheckExistingPackageName(regPackageViewModel.Name);
                    if (checkIfPackageNameExists)
                    {
                        return Json(new { isError = true, msg = "Package Name Already Exists" });
                    }
                    var regpackage = _iregistrationPackageHelper.CreatePackage(regPackageViewModel);
                    if (regpackage)
                    {
                        return Json(new { isError = false, msg = "Registration Package Successfully Added" });
                    }
                    return Json(new { isError = true, msg = "Unable To Add Registration Package" });
                }
                return Json(new { isError = true, msg = "Error Occurred Creating Registration Package" });
            }
            return Json(new { isError = true, msg = "Error Occurred" });
            
        }

        [HttpGet]
        public async Task<IActionResult> PackageToEdit(int packageId)
        {
            if (packageId > 0)
            {
                var packageDetails = _iregistrationPackageHelper.GetPackagesById(packageId);
                if (packageDetails != null)
                {
                    //ViewBag.DetailId = packageDetails.Id;

                    return Json(new { isError = false, data = packageDetails });
                }
            }
            return Json(new { isError = true, msg = "Network failure, please try again." });
        }

        [HttpPost]
        public JsonResult EditedPackage(string registrationPackageDetails)
        {
            if (registrationPackageDetails != null)
            {
                var packagesViewModel = JsonConvert.DeserializeObject<RegistrationPackageViewModel>(registrationPackageDetails);
                if (packagesViewModel != null)
                {
                    var editedPackages = _iregistrationPackageHelper.PackageEdited(packagesViewModel);
                    if (editedPackages)
                    {
                        return Json(new { isError = false, msg = " Package Updated Successfully" });
                    }
                    return Json(new { isError = true, msg = "Unable To Updated Package" });
                }
            }
            return Json(new { isError = true, msg = "Network failure, please try again." });
        }

        [HttpPost]
        public JsonResult PackageDeleted(int id)
        {
            try
            {
                if (id > 0)
                {
                    var success = _iregistrationPackageHelper.DeletePackage(id);
                    if (success)
                    {
                        return Json(new { IsError = false, msg = "Registered Package deleted successfully." });
                    }
                }
				return Json(new { IsError = true, msg = "Package not found." });
			}
            catch (Exception ex)
            {
                // Log the exception or handle it as appropriate for your application
                return Json(new { IsError = true, msg = "Error deleting package: " + ex.Message });
            }
        }
    }

}



