using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.IHelper;
using Logic.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;

namespace Logic.Helper
{
    public class RegistrationPackageHelper: IRegistrationPackageHelper
    {
        private readonly AppDbContext _context;
        public RegistrationPackageHelper(AppDbContext context)
        {
            _context = context;

        }

        public bool CheckExistingPackageName(string name)
        {
            if (name != null)
            {
                return _context.Packages.Where(x => x.Name == name && x.Active && !x.Deleted).Any();
            }
            return false;
        }

        public bool CreatePackage(RegistrationPackageViewModel createPackageData)
        {
            if (createPackageData != null)
            {
                var createpack = new Packages()
                {
                    Name = createPackageData.Name,
                    Price = createPackageData.Price,
                    BonusAmount = createPackageData.BonusAmount,
                    Description = createPackageData.Description,
                    MaxGeneration = (GenerationEnum)Enum.Parse(typeof(GenerationEnum), createPackageData.GenerationId.ToString()),

                Active = true,
                    Deleted = false,
                    DateCreated = DateTime.Now,
                };
                _context.Packages.Add(createpack);
                _context.SaveChanges();
                return true;
            }
            return false;

        }

        public List<RegistrationPackageViewModel> GetPackagesList()
        {
            var packages = _context.Packages
                .Where(p =>p.Id != 0 && !p.Deleted)
                .Select(p => new RegistrationPackageViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = (double?)p.Price ?? 0.0,
                    BonusAmount = (double?)p.BonusAmount ?? 0.0,
                    Description = p.Description,
                    MaxGeneration = p.MaxGeneration,
                    DateCreated = p.DateCreated,
                })
                .ToList();

            return packages; 
        }


        public Packages GetPackagesById(int id)
        {
            var package = new Packages();
            if (id > 0)
            {
                var packagesToBeEdited = _context.Packages.Where(x => x.Id == id && !x.Deleted).FirstOrDefault();
                if (packagesToBeEdited != null)
                {
                    package = packagesToBeEdited;

                    return package;
                }
            }
            return null;
        }



        public bool PackageEdited(RegistrationPackageViewModel registrationPackageDetails)
        {
            if (registrationPackageDetails != null)
            {
                var package = _context.Packages.Where(x => x.Id == registrationPackageDetails.Id && !x.Deleted).FirstOrDefault();
                if (package != null)
                {
                    package.Name = registrationPackageDetails.Name;
                    package.Price = registrationPackageDetails.Price;
                    package.MaxGeneration = (GenerationEnum)registrationPackageDetails.GenerationId;
                    package.Description = registrationPackageDetails.Description;
                    package.BonusAmount = registrationPackageDetails.BonusAmount;
                    _context.Packages.Update(package);
                    _context.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool DeletePackage(int id)
        {
            try
            {
                if (id != 0)
                {
                    var package = _context.Packages.Where(x => x.Id == id && x.Active && !x.Deleted).FirstOrDefault();
                    if (package != null)
                    {
                        package.Active = false;
                        package.Deleted = true;
                        _context.Packages.Update(package);
                        _context.SaveChanges();
                        return true;
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
            
            return false;
        }

      
    }
}






