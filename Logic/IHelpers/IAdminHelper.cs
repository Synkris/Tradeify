using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.IHelpers
{
    public interface IAdminHelper
    {
        ApplicationUserViewModel UserDetailsToEdit(string userId);
        bool EditedDetails(ApplicationUserViewModel userDetails);
    }
}
