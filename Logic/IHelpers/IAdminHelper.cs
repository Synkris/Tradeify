﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.IHelpers
{
    public interface IAdminHelper
    {
        ApplicationUserViewModel UserDetailsToEdit(string userId);
        bool EditedDetails(ApplicationUserViewModel userDetails);
        IPagedList<CommonDropdownViewModel> GetCommonDropdowns(CommonDropdownSearchResultViewModel commonDropdownSearch, int pageNumber, int pageSize);
        bool UpdateDropDownService(string dropdown, int? id);
        string RemoveDropDown(int id);
    }
}
