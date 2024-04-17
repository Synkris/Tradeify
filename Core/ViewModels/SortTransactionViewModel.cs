using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels
{
    public class SortTransactionViewModel
    {
        public DateTime SortTypeFrom { get; set; }
        public DateTime SortTypeTo { get; set; }
        public string UserName { get; set; }
    }
}
