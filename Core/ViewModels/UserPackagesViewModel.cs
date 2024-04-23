using Core.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Core.ViewModels
{
    public class UserPackagesSearchResultViewModel
    {
		private readonly IGeneralConfiguration _generalConfiguration;

		public UserPackagesSearchResultViewModel(IGeneralConfiguration generalConfiguration)
		{
			_generalConfiguration = generalConfiguration;
		}
		public IPagedList<UserPackagesViewModel> UserPackagesRecords { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public DateTime SortTypeFrom { get; set; }
        public DateTime SortTypeTo { get; set; }
        public string TransactionType { get; set; }
        public string Name { get; set; }
        public int PageCount { get; set; }

        public UserPackagesSearchResultViewModel()
        {
			PageNumber = _generalConfiguration.PageNumber;
			PageSize = _generalConfiguration.PageSize;
		}
    }

    public class UserPackagesViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Package Name")]
        public string Name { get; set; }

        public string UserId { get; set; }

        [Display(Name = "User")]
        public string UserName { get; set; }

        [Display(Name = "Package Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Crypto Coin Paid")]
        public string CryptoAmount { get; set; }

        [Display(Name = "Date Of Payment")]
        public DateTime? DateOfPayment { get; set; }

        [Display(Name = "Date Of Admin-Approval")]
        public DateTime? DateOfApproval { get; set; }

        [Display(Name = "Date Bonus Paid & Sent")]
        public DateTime? DateBonusPaid { get; set; }

        [Display(Name = "Max Generation")]
        public int MaxGeneration { get; set; }

        [Display(Name = "PaymentId")]
        public Guid? PaymentId { get; set; }

        [Display(Name = "Payment Method")]
        public string PaymentMethodName { get; set; }

        [Display(Name = "PackageId")]
        public int PackageId { get; set; }

        [Display(Name = "Package")]
        public string PackageName { get; set; }

        [Display(Name = "Withdrawal Status")]
        public bool IsMatured { get; set; }
        public string Description { get; set; }
    }

}
