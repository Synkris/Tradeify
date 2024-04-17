using Core.Config;
using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Core.ViewModels
{
    public class PvSearchResultViewModel
    {
		private readonly IGeneralConfiguration _generalConfiguration;

		public PvSearchResultViewModel(IGeneralConfiguration generalConfiguration)
		{
			_generalConfiguration = generalConfiguration;
		}
		public IPagedList<PvWalletHistoryViewModel> PvWalletHistoryRecords { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public DateTime SortTypeFrom { get; set; }
        public DateTime SortTypeTo { get; set; }
        public string TransactionType { get; set; }
        public string UserName { get; set; }
        public int PageCount { get; set; }
		public decimal DollarRate { get; set; }

		public PvSearchResultViewModel()
		{
			PageNumber = _generalConfiguration.PageNumber;
			PageSize = _generalConfiguration.PageSize;
			DollarRate = _generalConfiguration.DollarRate;

		}
	}
	public class PvWalletHistoryViewModel
    {

        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; }
        public decimal Amount { get; set; }

        public DateTime DateOfTransaction { get; set; }

        public TransactionType TransactionType { get; set; }

        public Guid? PaymentId { get; set; }
        public PaymentForm Payment { get; set; }

        public decimal NewBalance { get; set; }

        [NotMapped]
        public decimal PreviousBalance
        {
            get
            {

                if (TransactionType == TransactionType.Credit || TransactionType == TransactionType.Refund)
                {
                    return NewBalance - Amount;

                }
                else if (TransactionType == TransactionType.Debit)
                {
                    return NewBalance + Amount;
                }

                return decimal.Zero;
            }
        }



        public string Details { get; set; }
    }
}
