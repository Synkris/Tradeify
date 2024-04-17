using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Core.ViewModels
{
    public class CordinatorViewModel
    {
        public int? Id { get; set; }
        public string CordinatorUserName { get; set; }
        public string Name { get; set; }
        public DateTime DateAdded { get; set; }
        public string AddedBy { get; set; }
        public bool RemovedAsCordinator { get; set; }

        public string CordinatorId { get; set; }
        public virtual ApplicationUser Coordinator { get; set; }
    }
}
