﻿using Core.Models;
using Logic.Helper;
using Logic.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.IHelpers
{
    public interface IDropdownHelper
    {
        List<Cordinator> DropdownOfCordinator();
        List<Packages> DropdownOfPackages();
		Task<List<CommonDropdowns>> GetBankDropdownByKey(DropdownEnums dropdownKey, bool deleteOption = false);
        Task<List<CommonDropdowns>> GetCryptoDropdown(DropdownEnums dropdownKey, bool deleteOption = false);
        Task<List<CommonDropdowns>> GetDropdownByKey(DropdownEnums dropdownKey, bool deleteOption = false);
        List<DropDown> DropdownOfRoles();
        List<DropdownEnumModel> GetDropDownEnumsList();
        Task<bool> CreateDropdownsAsync(CommonDropdowns commonDropdown);
        List<EnumDropdownModalViewModel> GetMaxGenerationEnums();
        List<DropdownEnumModel> GetPaymentTypeDropDownEnumsList();


	}

}
