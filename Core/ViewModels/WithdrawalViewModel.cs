using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using Core.Config;

namespace Core.ViewModels
{
    public class PendingWithdrawalsSearchResultViewModel
    {
		private readonly IGeneralConfiguration _generalConfiguration;

		public PendingWithdrawalsSearchResultViewModel(IGeneralConfiguration generalConfiguration)
		{
			_generalConfiguration = generalConfiguration;
		}
		public IPagedList<WithdrawalViewModel> WithdrawalRecords { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public DateTime SortTypeFrom { get; set; }
        public DateTime SortTypeTo { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public int PageCount { get; set; }
		public decimal DollarRate { get; set; }


		public PendingWithdrawalsSearchResultViewModel()
        {
			PageNumber = _generalConfiguration.PageNumber;
			PageSize = _generalConfiguration.PageSize;
			DollarRate = _generalConfiguration.DollarRate;
		}
    }
    public class WithdrawalViewModel
	{
		public Guid Id { get; set; }

		[Column(TypeName = "decimal(18,4)")]
		public decimal Amount { get; set; }

		[Display(Name = "Date Of Request")]
		public DateTime DateRequested { get; set; }

		[Display(Name = "Date Of Approval")]
		public DateTime DateApprovedAndSent { get; set; }

		public string UserId { get; set; }
		[Display(Name = "User")]
		[ForeignKey("UserId")]
		public virtual ApplicationUser User { get; set; }

		[Display(Name = "Withdrawal Status")]
		public Status WithdrawStatus { get; set; }

		[Display(Name = "Withdrawal Type")]
		public string WithdrawalType { get; set; }

		[Display(Name = "Name of Creditor")]
		public string CreditedBy { get; set; }

		[Display(Name = "Bank Name")]
		public string BankAccountName { get; set; }

		[Display(Name = "Account Number")]
		public string AccountNumber { get; set; }

		[Display(Name = "Account  Name")]
		public string AccountName { get; set; }

		[Display(Name = "Receiver's Full Name")]
		public string RequestedBy { get; set; }
		public decimal AmountInGGC { get; set; }
		public string FullName { get; set; }
		public string UserName { get; set; }
	}
}
