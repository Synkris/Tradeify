using Core.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
	public class GrantHistorySearchResultViewModel
	{
		private readonly IGeneralConfiguration _generalConfiguration;

		public GrantHistorySearchResultViewModel(IGeneralConfiguration generalConfiguration)
		{
			_generalConfiguration = generalConfiguration;
		}
		public IPagedList<GrantHistoryViewModel> GrantHistoryRecords { get; set; }

		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public DateTime SortTypeFrom { get; set; }
		public DateTime SortTypeTo { get; set; }
		public string TransactionType { get; set; }
		public string UserName { get; set; }
		public int PageCount { get; set; }
		public decimal DollarRate { get; set; }


		public GrantHistorySearchResultViewModel()
		{
			PageNumber = _generalConfiguration.PageNumber;
			PageSize = _generalConfiguration.PageSize;
			DollarRate = _generalConfiguration.DollarRate;

		}


	}
	public class GrantHistoryViewModel
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
