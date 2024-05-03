using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Core.ViewModels
{
    public class ImpersonationViewModel
    {
        public virtual ApplicationUser AdminBeingImpersonated { get; set; }
        public virtual Impersonation ImpersonationRecord { get; set; }
        public virtual ApplicationUser Impersonator { get; set; }
        public bool IsImpersonatorAdmin { get; set; }
        public string ShowEndSession { get; set; }
        public virtual ApplicationUser Impersonatee { get; set; }
        public bool EndDBSession { get; set; }
    }
}
