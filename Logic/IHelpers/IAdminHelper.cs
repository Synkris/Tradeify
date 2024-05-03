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
        bool CheckExistingCordinatorUserName(string cordinatorUserName);
        ApplicationUser GetNewCordinatorDetails(string cordinatorUserName);
        string CreateCordinator(ApplicationUser newCordinatorDetails, string loggedinAdmin);
        List<CordinatorViewModel> listOfCordinators();
        string RemoveCordinator(int id);
        //List<CommonDropdownViewModel> GetCommonDropdowns();
        bool UpdateDropDownService(string dropdown, int? id);
        string RemoveDropDown(int id);

		//List<ApplicationUserViewModel> ApprovedUsersDetails();
		bool DeactivateUser(string userId);
        ApplicationUser GetUserFullDetails(string userId);
        ApplicationUserViewModel UserDetailsToEdit(string userId);
        bool EditedDetails(ApplicationUserViewModel userDetails);
        IPagedList<ApplicationUserViewModel> RegisteredUsersDetails(ApplicationUserSearchResultViewModel applicationUserSearchResult, int pageNumber, int pageSize);
        IPagedList<CommonDropdownViewModel> GetCommonDropdowns(CommonDropdownSearchResultViewModel commonDropdownSearch, int pageNumber, int pageSize);
		WithdrawalViewModel GetWithdrawalDetails(Guid id);
        bool CheckExistingDropdownName(string name, int drpKey);
        Task<CompanySettingViewModel> GetCompanySettings();
		bool UpdateCompanySettings(CompanySettingViewModel companySettingViewModel);
        bool ReactivateUser(string userId);
    }
}
