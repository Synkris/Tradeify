using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels
{
	public class BinaryTreeViewModel
	{
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool RegFeePaid { get; set; }
        public string RefLink { get; set; }

        public ICollection<UserGenerationLog> UserGen { get; set; }

        public string message { get; set; }
    }
}
