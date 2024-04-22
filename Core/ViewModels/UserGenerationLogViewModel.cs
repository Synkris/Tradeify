using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Core.ViewModels
{
	public class UserGenerationLogSearchResultViewModel
	{
		public IPagedList<UserGenerationLogViewModel> UserGenerationLogRecords { get; set; }

		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public DateTime SortTypeFrom { get; set; }
		public DateTime SortTypeTo { get; set; }
		public string TransactionType { get; set; }
		public string UserName { get; set; }
		public int PageCount { get; set; }

		public UserGenerationLogSearchResultViewModel()
		{
			PageNumber = 1;
		}
	}
	public class UserGenerationLogViewModel
    {
        public int Id { get; set; }
        public Guid PaymentId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? ChildId { get; set; }
        public string? BonusFrom { get; set; }
        public GenerationEnun Generation { get; set; }
        public BonusStusEnum Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DatePaid { get; set; }
        public decimal BonusAmount { get; set; }
    }
}
