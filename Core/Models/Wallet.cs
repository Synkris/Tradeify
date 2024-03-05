using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
	public class Wallet
	{
		public Guid Id { get; set; }
		public string UserId { get; set; }
		[Display(Name = "User")]
		[ForeignKey("UserId")]
		public virtual ApplicationUser User { get; set; }
		[Column(TypeName = "decimal(18,4)")]
		public decimal Balance { get; set; }
		public DateTime LastUpdated { get; set; }
	}
}
