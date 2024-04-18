using Core.Models;
using Core.ViewModels;
using Logic.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.IHelper
{
    public interface IRegistrationPackageHelper
    {
       
        bool CreatePackage(RegistrationPackageViewModel createPackageData);
        List<RegistrationPackageViewModel> GetPackagesList();
        Packages GetPackagesById(int id);
        bool PackageEdited(RegistrationPackageViewModel packageDetails);
        bool DeletePackage(int packageId);
        bool CheckExistingPackageName(string name);
    }
}
