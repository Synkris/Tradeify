using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels
{
	public class MatchingViewModel
	{
		public string Username { get; set; }

		public int generation { get; set; }

        public List<string>? ListOfUserId { get; set; }
        public string? UserIds { get; set; }
    }
}
