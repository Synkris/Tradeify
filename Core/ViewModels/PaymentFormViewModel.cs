using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using X.PagedList;
using Core.Config;

namespace Core.ViewModels
{
    public class PendingPaymentsSearchResultViewModel
	{
	
		private readonly IGeneralConfiguration _generalConfiguration;

		public PendingPaymentsSearchResultViewModel(IGeneralConfiguration generalConfiguration)
		{
			_generalConfiguration = generalConfiguration;
		}
		public IPagedList<PaymentFormViewModel> PaymentRecords { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public DateTime SortTypeFrom { get; set; }
        public DateTime SortTypeTo { get; set; }
        public string Refferer { get; set; }
        public string Name { get; set; }
        public int PageCount { get; set; }
		public decimal DollarRate { get; set; }


		public PendingPaymentsSearchResultViewModel()
        {
			PageNumber = _generalConfiguration.PageNumber;
			PageSize = _generalConfiguration.PageSize;
			DollarRate = _generalConfiguration.DollarRate;

		}
	}
    public class PaymentFormViewModel
	{	
		[Key]
		public Guid Id { get; set; }

		public string Details { get; set; }

		public decimal Amount { get; set; }

		public string CryptoAmount { get; set; }
		public DateTime Date { get; set; }

		public string UserId { get; set; }
		public virtual ApplicationUser User { get; set; }
		public Status Status { get; set; }
		public string StatusBy { get; set; }
		public DateTime StatuseChangeDate { get; set; }
		public PaymentType PaymentTypeId { get; set; }
		public int? BankAccountId { get; set; }
		public virtual CommonDropdowns BankAccount { get; set; }
		public string  Bank { get; set; }
		public string PaidFrom { get; set; }
		public string BankNamePaidFrom { get; set; }
		public string AccountNumberPaidFrom { get; set; }
		public string PaymentMethod { get; set; }
		public string DistributorId { get; set; }
		public int PackageId { get; set; }
		public string OldPackage { get; set; }
		public string Name { get; set; }
		public string Refferer { get; set; }
		public string CordinatorId { get; set; }
		public string TokenMaximum { get; set; }
		public string TokenMinimum { get; set; }
		public decimal AmountPerToken { get; set; }
		public decimal? NoOfTokensBought { get; set; }
		public string? TokensBought { get; set; }
        public List<PaymentFormViewModel> UserTokenDetails { get; set; }
        public string NewPackage { get; set; }
    }
}
