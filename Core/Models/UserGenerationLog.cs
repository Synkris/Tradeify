using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Core.Models
{
	public class UserGenerationLog
	{
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
		public string ChildId { get; set; }
        public virtual ApplicationUser Child { get; set; }

        public GenerationEnun Generation { get; set; }
		public BonusStusEnum Status { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime? DatePaid { get; set; }
		public decimal BonusAmount { get; set; }
	}

}

public enum GenerationEnun
{
	[Description("For generation 1")]
	Gen1 = 1,
	[Description("For generation 2")]
	Gen2 = 2,
	[Description("For generation 3")]
	Gen3 = 3,
	[Description("For generation 4")]
	Gen4 = 4,
	[Description("For generation 5")]
	Gen5 = 5
}
public enum BonusStusEnum
{
	[Description("For a user that is not qualified ")]
	NotApplicable = 1,
	[Description("For a bonus that has not been paid by admin")]
	Pending = 2,
	[Description("For a bonus that has been paid off by admin")]
	Paid = 3,
	[Description("For a bonus that was rejected by the admin")]
	Rejected = 4,

}

