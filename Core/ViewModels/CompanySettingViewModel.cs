using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels
{
    public class CompanySettingViewModel
    {
        public string MinimumToken { get; set; }
        public string MaximumToken { get; set; }
        public decimal Tokenamount { get; set; }
        public decimal MiningQuantity { get; set; } = 0.0m;   // Default value for decimal property
        public int MiningDuration { get; set; } = 0;        // Default value for int property
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal ActivationAmount { get; set; }
    }
}
