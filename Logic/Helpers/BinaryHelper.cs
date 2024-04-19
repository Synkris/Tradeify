using Core.Config;
using Core.DB;
using Core.Models;
using Core.ViewModels;
using Hangfire;
using Logic.IHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;

namespace Logic.Helpers
{
	public class BinaryHelper:IBinaryHelper
	{
		private UserManager<ApplicationUser> _userManager;
		private readonly AppDbContext _context;
        private IGeneralConfiguration _generalConfiguration;
        public BinaryHelper(UserManager<ApplicationUser> userManager, AppDbContext context, IGeneralConfiguration generalConfiguration)
        {
            _userManager = userManager;
            _context = context;
            _generalConfiguration = generalConfiguration;
        }

        public void ProcessUserParents(string userId)
		{
			var user = _userManager.Users.Where(s => s.Id == userId).Include(r => r.Refferrer)?.FirstOrDefault();
			if (user.RefferrerId != null && user.RefferrerId != user.Id && user.RegFeePaid == true)
			{
				var userGen1Parent = user.Refferrer;
				if (userGen1Parent.RefferrerId != null && userGen1Parent.RefferrerId != userGen1Parent.Id)
				{
					if (userGen1Parent.RegFeePaid != false)
					{
                        // logging for first gen
                        LogToUserGeneration(userGen1Parent.Id, userId, GenerationEnun.Gen1, BonusStusEnum.Pending); 
                    }

                    //getting the next parent
                    var gen2BonusStatus = BonusStusEnum.Pending;
                    var userGen2Parent = _userManager.Users.Where(s => s.Id == userGen1Parent.RefferrerId).Include(p => p.UserPackages).Include(r => r.Refferrer)?.FirstOrDefault();

					if (userGen2Parent.RefferrerId != null && userGen2Parent.RefferrerId != userGen2Parent.Id) //check for self
					{

						var userMaxGen = userGen2Parent?.UserPackages?.Max(p => p?.MaxGeneration);
						//check if this parent should get bonus for 2nd gen
						if (userGen2Parent.RegFeePaid == true && userMaxGen > 1)
						{
                            // Logging for gen 2
                            LogToUserGeneration(userGen2Parent.Id, userId, GenerationEnun.Gen2, gen2BonusStatus);
                        }

                        //getting the next parent
                        var userGen3Parent = _userManager.Users.Where(s => s.Id == userGen2Parent.RefferrerId).Include(p => p.UserPackages).Include(r => r.Refferrer)?.FirstOrDefault();
						if (userGen3Parent.RefferrerId != null && userGen3Parent.RefferrerId != userGen3Parent.Id)
						{
							 userMaxGen = userGen3Parent?.UserPackages?.Max(p => p?.MaxGeneration);

							//check if this parent should get bonus for 3rd gen
							if (userGen3Parent.RegFeePaid == true && userMaxGen > (int)GenerationEnun.Gen2)
							{
                                //log for 3rd gen
                                LogToUserGeneration(userGen3Parent.Id, userId, GenerationEnun.Gen3, gen2BonusStatus); // this
                            }

                            //getting the next parent
                            var userGen4Parent = _userManager.Users.Where(s => s.Id == userGen3Parent.RefferrerId).Include(p => p.UserPackages).Include(r => r.Refferrer)?.FirstOrDefault();
							gen2BonusStatus = BonusStusEnum.Pending;

							if (userGen4Parent.RefferrerId != null && userGen4Parent.RefferrerId != userGen4Parent.Id) //check for self
							{
								userMaxGen = userGen3Parent?.UserPackages?.Max(p => p?.MaxGeneration);
								//check if this parent should get bonus for 4th gen
								if (userGen4Parent.RegFeePaid == true && userMaxGen > (int)GenerationEnun.Gen3)
								{
                                    //log for 4th gen
                                    LogToUserGeneration(userGen4Parent.Id, userId, GenerationEnun.Gen4, gen2BonusStatus); // this
                                }

                                //getting the next parent
                                var userGen5Parent = _userManager.Users.Where(s => s.Id == userGen4Parent.RefferrerId).Include(p => p.UserPackages).Include(r => r.Refferrer)?.FirstOrDefault();
								if (userGen5Parent.RefferrerId != null && userGen5Parent.RefferrerId != userGen5Parent.Id)
								{
									userMaxGen = userGen3Parent?.UserPackages?.Max(p => p?.MaxGeneration);
									//check if this parent should get bonus for 5th gen
									if (userGen5Parent.RegFeePaid == true && userMaxGen > (int)GenerationEnun.Gen4)
									{
                                        //log for 5th gen
                                        LogToUserGeneration(userGen5Parent.Id, userId, GenerationEnun.Gen5, gen2BonusStatus);
                                    }
                                }
								
							}
						}
                    }
                    _context.SaveChanges();
                }

			}
		}

		private void LogToUserGeneration(string userId, string childId, GenerationEnun gen, BonusStusEnum stusEnum)
		{
			var checkUserLog = _context.UserGenerationLogs.Where(x => x.UserId == userId && x.ChildId == childId && x.Generation == gen).FirstOrDefault();
			if (checkUserLog == null)
			{
                var genLog = new UserGenerationLog
                {
                    Status = stusEnum,
                    Generation = gen,
                    ChildId = childId,
                    UserId = userId,
                    DateCreated = DateTime.Now,
                    BonusAmount = GetGenerationBonus(gen),
                };
                _context.Add(genLog);
            }
		}

		public async Task<BinaryTreeViewModel> GetUserTree(string userName, int? gen)
		{
			try
			{
				var response = new BinaryTreeViewModel();
				gen ??= 1;
				if (userName == null)
				{
					response.message = "User cannot be null ";
					return response;
				}
				
				var currentUser = await _userManager.FindByNameAsync(userName).ConfigureAwait(false);

				if (currentUser == null)
				{
					response.message = "Could not validate the User at this point";
					return response;
				}
				response.RegFeePaid = currentUser.RegFeePaid != null ? (bool)currentUser.RegFeePaid:false;
				response.UserId = currentUser.Id;
				response.UserName = currentUser.UserName;
				response.UserGen = GetGenUsers(currentUser.Id, gen).ToList();

				return response;
			}
			catch (Exception ex)
			{

				throw ex;
			}
		}

		public IQueryable<UserGenerationLog> GetGenUsers(string userId, int? gen)
		{
			var generation = (GenerationEnun)gen;
			return _context.UserGenerationLogs.Where(x => x.UserId == userId && x.Generation == generation).Include(c => c.Child); 
		}

        //This method is supposed to check how may generations a user have.
        public int ProcessUserGen(string userId)
		{
			return 0;
		}

        private decimal GetGenerationBonus(GenerationEnun gen)
		{
			switch (gen) {
				case GenerationEnun.Gen1:
						return _generalConfiguration.Gen1Bonus;
				case GenerationEnun.Gen2:
						return _generalConfiguration.Gen2Bonus;
				case GenerationEnun.Gen3:
					return _generalConfiguration.Gen3Bonus;
				case GenerationEnun.Gen4:
						return _generalConfiguration.Gen4Bonus;
				case GenerationEnun.Gen5:
					return _generalConfiguration.Gen5Bonus;
			}
			return _generalConfiguration.Gen1Bonus;
		}

        public void SendUserGenerationBonuses(string userId)
        {
            BackgroundJob.Enqueue(() => ProcessUserParents(userId));
        }
  //      public void GetAllOldUsers()
		//{
		//	//to get old users:
		//	var getOldUsers = _context.ApplicationUser.Where(a => a.Id != null && a.RefferrerId != null && a.DateRegistered.Year < 2024).ToList();
  //          if (getOldUsers != null)
  //          {
  //              //send each user to userGenLog with user's generation:
  //              foreach (var oldUser in getOldUsers)
  //              {
  //                  ProcessOldUserParents(oldUser.Id);
  //              }
  //          }
  //      }

        public void ProcessOldUserParents(string userId)
        {
            var bonusStatus = BonusStusEnum.Paid;
            var oldMember = _userManager.Users.Where(s => s.Id == userId).Include(r => r.Parent)?.FirstOrDefault();
            if (oldMember?.ParentId != null && oldMember?.ParentId != oldMember?.Id && oldMember?.RegFeePaid == true)
            {
                var userGen1Parent = oldMember?.Parent;
                if (userGen1Parent?.ParentId != null && userGen1Parent?.ParentId != userGen1Parent?.Id)
                {
                    if (userGen1Parent?.RegFeePaid != false)
                    {
                        // gen1
                        AddToUserGenLog(userGen1Parent?.Id, userId, bonusStatus, GenerationEnun.Gen1);
                    }
                    //getting the next parent
                    var userGen2Parent = _userManager.Users.Where(s => s.Id == userGen1Parent.ParentId).Include(r => r.Parent)?.FirstOrDefault();

                    if (userGen2Parent?.ParentId != null && userGen2Parent?.ParentId != userGen2Parent?.Id)
                    {
                        if (userGen2Parent?.RegFeePaid == true)
                        {
                            // Logging for gen 2
                            AddToUserGenLog(userGen2Parent?.Id, userId, bonusStatus, GenerationEnun.Gen2);
                        }
                        //getting the next parent
                        var userGen3Parent = _userManager.Users.Where(s => s.Id == userGen2Parent.ParentId).Include(r => r.Parent)?.FirstOrDefault();
                        if (userGen3Parent?.ParentId != null && userGen3Parent?.ParentId != userGen3Parent?.Id)
                        {
                            if (userGen3Parent?.RegFeePaid == true)
                            {
                                //log for 3rd gen
                                AddToUserGenLog(userGen3Parent?.Id, userId, bonusStatus, GenerationEnun.Gen3);
                            }

                            //getting the next parent
                            var userGen4Parent = _userManager.Users.Where(s => s.Id == userGen3Parent.ParentId).Include(r => r.Parent)?.FirstOrDefault();
                            if (userGen4Parent?.ParentId != null && userGen4Parent?.ParentId != userGen4Parent?.Id)
                            {
                                if (userGen4Parent?.RegFeePaid == true)
                                {
                                    //log for 4th gen
                                    AddToUserGenLog(userGen4Parent?.Id, userId, bonusStatus, GenerationEnun.Gen4);
                                }

                                //getting the next parent
                                var userGen5Parent = _userManager.Users.Where(s => s.Id == userGen4Parent.ParentId).Include(r => r.Parent)?.FirstOrDefault();
                                if (userGen5Parent?.ParentId != null && userGen5Parent?.ParentId != userGen5Parent?.Id)
                                {
                                    if (userGen5Parent?.RegFeePaid == true)
                                    {
                                        //log for 5th gen
                                        AddToUserGenLog(userGen5Parent?.Id, userId, bonusStatus, GenerationEnun.Gen5);
                                    }
                                }

                            }
                        }
                    }
                    _context.SaveChanges();
                }

            }
        }

        public bool AddToUserGenLog(string userId, string chidId, BonusStusEnum bonusStatus, GenerationEnun gen)
        {
            var checkUserLog = _context.UserGenerationLogs.Where(x => x.UserId == userId && x.ChildId == chidId && x.Generation == gen).FirstOrDefault();
            if (checkUserLog == null)
            {
                var genLog = new UserGenerationLog
                {
                    Status = bonusStatus,
                    Generation = gen,
                    ChildId = chidId,
                    UserId = userId,
                    DateCreated = DateTime.Now,
                    BonusAmount = GetGenerationBonus(gen),
                };
                _context.Add(genLog);
            }
            return true;
        }


    }
}

