using Core.Models;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Logic.IHelpers
{
    public interface IAdminHelper
    {
        ApplicationUserViewModel UserDetailsToEdit(string userId);
        bool EditedDetails(ApplicationUserViewModel userDetails);
        IPagedList<CommonDropdownViewModel> GetCommonDropdowns(CommonDropdownSearchResultViewModel commonDropdownSearch, int pageNumber, int pageSize);
        bool UpdateDropDownService(string dropdown, int? id);
        string RemoveDropDown(int id);
        bool CheckExistingDropdownName(string name, int drpKey);
        List<CordinatorViewModel> listOfCordinators();
        bool CheckExistingCordinatorUserName(string cordinatorUserName);
        ApplicationUser GetNewCordinatorDetails(string cordinatorUserName);
        string CreateCordinator(ApplicationUser newCordinatorDetails, string loggedinAdmin);
        string RemoveCordinator(int id);
        Task<CompanySettingViewModel> GetCompanySettings();
        bool UpdateCompanySettings(CompanySettingViewModel companySettingViewModel);
        bool ReactivateUser(string userId);
    }
}
