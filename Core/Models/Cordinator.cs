using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
	public class Cordinator
	{
		[Key]
		public int? Id { get; set; }
		public string CordinatorUserName { get; set; }
		public DateTime DateAdded { get; set; }
		public string AddedBy { get; set; }
		public bool RemovedAsCordinator { get; set; }

		public string CordinatorId { get; set; }
		[Display(Name = "Cordinator")]
		[ForeignKey("CordinatorId")]
		public virtual ApplicationUser Coordinator { get; set; }
	}
}
