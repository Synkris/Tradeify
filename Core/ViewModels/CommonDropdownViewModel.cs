using Core.Config;
using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using X.PagedList;

namespace Core.ViewModels
{
	public class CommonDropdownSearchResultViewModel
	{
		private readonly IGeneralConfiguration _generalConfiguration;

		public CommonDropdownSearchResultViewModel(IGeneralConfiguration generalConfiguration)
		{
			_generalConfiguration = generalConfiguration;
		}
		public IPagedList<CommonDropdownViewModel> CommonDropdownRecords { get; set; }

		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public DateTime SortTypeFrom { get; set; }
		public DateTime SortTypeTo { get; set; }
		public string Name { get; set; }
		public int PageCount { get; set; }

		public CommonDropdownSearchResultViewModel()
		{
			PageNumber = _generalConfiguration.PageNumber;
			PageSize = _generalConfiguration.PageSize;
		}
	}
	public class CommonDropdownViewModel 
    {
        public int DropdownKey { get; set; }
        [Display(Name = " Code")]
        public int? Code { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
