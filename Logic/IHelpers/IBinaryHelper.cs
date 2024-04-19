using Core.Models;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.IHelpers
{
	public interface IBinaryHelper
	{
		Task<BinaryTreeViewModel> GetUserTree(string userName, int? gen);
        void ProcessUserParents(string userId);
        void SendUserGenerationBonuses(string userId);
		IQueryable<UserGenerationLog> GetGenUsers(string userId, int? gen);
        //void MapOldUserGenerations();
        //void GetAllOldUsers();
    }
}
