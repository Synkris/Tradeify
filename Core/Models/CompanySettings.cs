using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class CompanySettings : BaseModel
    {
        public string MinimumToken {  get; set; }
        public string MaximumToken {  get; set; }
        public decimal Tokenamount {  get; set; }
        public decimal MiningQuantity {  get; set; } = 0.0m;   // Default value for decimal property
        public int MiningDuration {  get; set; } = 0;        // Default value for int property
        public decimal ActivationAmount { get; set; }
    }
}
