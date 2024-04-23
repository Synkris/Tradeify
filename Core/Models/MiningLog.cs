using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Core.Models
{
	public class MiningLog : BaseModel
	{
		public decimal MiningQuantity { get; set; } = 0.0m;  
		public int MiningDuration { get; set; } = 0;
		public string UserId { get; set; }
		[Display(Name = "User")]
		[ForeignKey("UserId")]
		public virtual ApplicationUser User { get; set; }
	}
}
