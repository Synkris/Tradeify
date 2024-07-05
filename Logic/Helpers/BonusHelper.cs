using Core.Config;
using Core.DB;
using Core.Models;
using Core.ViewModels;
using Hangfire;
using Logic.IHelpers;
using Logic.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace Logic.Helpers
{
    public class BonusHelper : BaseHelper, IBonusHelper
    {
        private readonly AppDbContext _context;
        private readonly IGeneralConfiguration _generalConfiguration;
        private readonly IEmailService _emailService;
        private readonly IUserHelper _userHelper;
        private readonly UserManager<ApplicationUser> _userManager;
        public BonusHelper(AppDbContext context, IGeneralConfiguration generalConfiguration, IEmailService emailService, IUserHelper userHelper, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _generalConfiguration = generalConfiguration;
            _emailService = emailService;
            _userHelper = userHelper;
            _userManager = userManager;
        }


        public async Task AssignRefBonus(string referralId, PaymentForm payment)
        {
            string toEmail = _generalConfiguration.DeveloperEmail;
            string subject = "AssignRefBonus Exception Message on GGC";
            try
            {
                var status = false;
                var wallet = await GetUserWallet(referralId);
                var alreadyReceivedBobus = _context.WalletHistories.Where(s => s.WalletId == wallet.Id && s.PaymentId == payment.Id);

                if (!alreadyReceivedBobus.Any())
                {
                    if (payment.Amount == _generalConfiguration.CryptoRegFeeAmount)
                    {
                        var Cryptodetails = " Referral Bonus Credited and Received from GGC and Cryptoclass registeration beneficary. ";
                        status = await CreditWalletNewAsync(referralId, _generalConfiguration.ReferralBonusForAdifandCryptoClass, payment.Id, Cryptodetails);
                    }
                    else
                    {
                        var bankDetails = "Referral Bonus Credited and Received  from GGC registeration beneficiary only.";
                        status = await CreditWalletNewAsync(referralId, _generalConfiguration.ReferralBonus, payment.Id, bankDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = "Exception " + ex.Message + " and inner exception:" + ex.InnerException.Message + "  Occured at " + DateTime.Now;
                _emailService.SendEmail(toEmail, subject, message);
            }
        }

        public async Task AssignWelcomeBonus(string userId, Guid paymentId)
        {
            string toEmailBug = _generalConfiguration.DeveloperEmail;
            string subjectEmailBug = "AssignWelcomeBonus Exception Message on GGC";
            try
            {
                var status = false;
                var payment = _context.PaymentForms.Where(s => s.Id == paymentId).FirstOrDefault();
                var ConvertedWelcomeBonus = _generalConfiguration.WelcomeBonus / _generalConfiguration.DollarRate;
                var details = " Welcome Bonus of $" + ConvertedWelcomeBonus.ToString("F2") + " received for a Succesful and Verifed Registeration ";
                var wallet = await GetUserWallet(payment.UserId);
                var alreadyReceivedBonus = _context.WalletHistories.Where(s => s.WalletId == wallet.Id && s.PaymentId == paymentId);
                if (!alreadyReceivedBonus.Any(x => x.Details.ToLower().Contains("welcome bonus")))
                {
                    status = await CreditWallet(payment.UserId, _generalConfiguration.WelcomeBonus, paymentId, details);

                    var convertedBalance = wallet.Balance / _generalConfiguration.DollarRate;
                    string toEmail = payment.User.Email;
                    string subject = "Hooray!!!, Welcome Bonus Received ";
                    string message = "Hello " + "<b>" + payment.User.UserName + "<b/>" + ", <br> You have received a welcome bonus of " + "<b>"
                   + "$" + convertedBalance.ToString("F2") + "<b/> For " + " a successful registeration with us. " +
                      " Keep investing with GGC for more bonuses <br> Thanks!!! ";

                    _emailService.SendEmail(toEmail, subject, message);
                }

            }
            catch (Exception ex)
            {
                string message = "Exception " + ex.Message + " and inner exception:" + ex.InnerException.Message + "  Occured at " + DateTime.Now;
                _emailService.SendEmail(toEmailBug, subjectEmailBug, message);
                //throw exp;
            }


        }
        public async Task<GrantWallet> GetUserGrantWallet(string userId)
        {
            try
            {

                if (userId != null)
                {
                    var wallet = await _context.GrantWallets.Where(x => x.UserId == userId)?.Include(s => s.User)?.FirstOrDefaultAsync();
                    if (wallet != null && wallet.UserId != null)
                    {

                        return wallet;
                    }
                    else
                    {
                        return await CreateGrantWalletByUserId(userId);
                    }

                }

                return null;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<GrantWallet> CreateGrantWalletByUserId(string userId, decimal amount = 0)
        {

            try
            {
                if (userId != null)
                {
                    var user = await _userHelper.FindByIdAsync(userId);
                    if (user != null)
                    {

                        var newWallet = new GrantWallet()
                        {
                            UserId = user.Id,
                            Balance = amount,
                            LastUpdated = DateTime.Now,

                        };

                        var result = await _context.AddAsync(newWallet);
                        await _context.SaveChangesAsync();

                        if (result.Entity.Id != null)
                        {
                            return result.Entity;
                        }

                    }


                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> CreditGrantWallet(string userId, decimal amount, Guid? paymentId)
        {

            try
            {
                if (userId != null && amount > 0)
                {

                    var wallet = GetUserGrantWalletNonAsync(userId);
                    if (wallet == null)
                    {
                        wallet = CreateGrantWalletByUserIdNonAsync(userId, amount);
                        if (wallet.Balance > 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        wallet.Balance += amount;
                        _context.Update(wallet);
                        _context.SaveChanges();
                        await LogGrantWalletHistory(wallet, amount, TransactionType.Credit, paymentId);
						return true;

                    }


                }
                return false;
            }
            catch (Exception ex)
            {
				LogError($"Failed to credit grant wallet with userId {userId}");
				throw ex;
            }
        }

        public async Task AssignGrantLimitBonus(string userId, Guid paymentId)
        {
            string toEmailBug = _generalConfiguration.DeveloperEmail;
            string subjectEmailBug = "AssignGrantLimitBonus Exception Message on GGC";
            try
            {
                var status = false;
                var payment = _context.PaymentForms.Where(s => s.Id == paymentId).FirstOrDefault();
                var wallet = await GetUserGrantWallet(payment.UserId);
                var ConvertedGrant = _generalConfiguration.GrantLimit / _generalConfiguration.DollarRate;
                var alreadyReceivedBonus = _context.GrantWalletHistories.Where(s => s.WalletId == wallet.Id && s.PaymentId == paymentId);
                if (!alreadyReceivedBonus.Any())
                {
                    status = await CreditGrantWallet(payment.UserId, _generalConfiguration.GrantLimit, paymentId);

                    string toEmail = payment.User.Email;
                    string subject = "Congratulations!!!, Grant Bonus Received ";
                    string message = "Hello " + "<b>" + payment.User.UserName + "<b/>" + ", <br> You have received a grant of " + "<b>"
                    + " $" + ConvertedGrant.ToString("F2") + "<b/> For " + " a successful registeration with us. " +
                         " Keep investing with GGC for more bonuses <br> Thanks!!! ";

                    _emailService.SendEmail(toEmail, subject, message);
                }

            }
            catch (Exception ex)
            {
                string message = "Exception " + ex.Message + " and inner exception:" + ex.InnerException.Message + "  Occured at " + DateTime.Now;
                _emailService.SendEmail(toEmailBug, subjectEmailBug, message);
                //throw ex;
            }

        }
        public async Task<bool> CreditAGCWallet(string userId, decimal tokens, Guid? paymentId)
        {

            try
            {
                if (userId != null && tokens > 0)
                {

                    var wallet = GetUserAGCWalletNonAsync(userId);
                    if (wallet == null)
                    {
                        wallet = CreateAGCWalletByUserIdNonAsync(userId, tokens);
                        if (wallet?.Balance > 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        //var alreadyPaid = _context.AGCWalletHistories.Where(x => x.PaymentId == paymentId).FirstOrDefault();
                        //if (alreadyPaid != null)
                        //{
                        //    return false;
                        //}
                        wallet.Balance += tokens;
                        _context.Update(wallet);
                        _context.SaveChanges();
                        await LogAGCWalletHistory(wallet, tokens, TransactionType.Credit, paymentId);
						LogInformation("AGC wallet credited");
						return true;

                    }


                }
                return false;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} Failed to credit AGC wallet with userId {userId} and paymentId {paymentId}");
				throw ex;
            }
        }

        public async Task<bool> CreditAdifgeedGiftCardWallet(string userId, Guid paymentId)
        {
            try
            {
                var status = false;
                var payment = _context.PaymentForms.Where(s => s.Id == paymentId).FirstOrDefault();
                var wallet = await GetUserWallet(payment.UserId);

                if (wallet != null && userId != null)
                {
                    var doubleDAmount = payment.Amount * 2;
                    status = await CreditAGCWallet(payment.UserId, doubleDAmount, paymentId);
                    if (status)
                    {
                        string toEmail = payment.User.Email;
                        string subject = " Gift Card Credited ";
                        string message = "Hello " + "<b>" + payment.User.UserName + "<b/>" + ", <br> You have received a  bonus of " + "<b>"
                       + doubleDAmount.ToString("F0") + "<b/>GGC Coins Into your Giftcard wallet for " + " a successful " + payment.PaymentTypeId + "." + "<br/>" + "<br/>" +
                          " Keep investing with GGC for more bonuses <br> Thanks!!! ";

                        _emailService.SendEmail(toEmail, subject, message);
                    }
                }

                return (status);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public async Task<bool> CreditPvWallet(string userId, decimal amount, Guid? paymentId)
        {

            try
            {
                if (userId != null && amount > 0)
                {

                    var wallet = GetUserPvWalletNonAsync(userId);
                    if (wallet == null)
                    {
                        wallet = CreatePvWalletByUserIdNonAsync(userId, amount);
                        if (wallet?.Balance > 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        wallet.Balance += amount;
                        _context.Update(wallet);
                        _context.SaveChanges();
                        await LogPvWalletHistory(wallet, amount, TransactionType.Credit, paymentId);
						LogInformation("PV wallet credited");
						return true;

                    }


                }
                return false;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} Failed to credit PV wallet with userId {userId} and paymentId {paymentId}");
				throw ex;
            }
        }
        public bool IsUserLegFullAndBothLegsRegFeeApproved(string userId)
        {
            return _userManager.Users.Where(s => s.ParentId == userId && s.RegFeePaid == true).Count() > 1;
        }
		public async Task<bool> AssignMatchingBonus(int genLogId)
		{
			try
			{
				var request = await _context.UserGenerationLogs
					.Where(s => s.Id == genLogId && s.Status == BonusStusEnum.Pending)
					.Include(s => s.User)
					.Include(w => w.Child)
					.FirstOrDefaultAsync();

				if (request == null)
				{
				    LogError($"An attempt to get genlog with genlogId{genLogId} failed");
					return false;
				}
				var ConvertedMatchingBonus = request.BonusAmount / _generalConfiguration.DollarRate;
				var details = " Matching Bonus of $" + ConvertedMatchingBonus.ToString("F2") + " received from " + request.Generation.ToString() + " User" + ", ("  
					+ request.Child?.Name + " ) successfully";
				
                var payment = await _context.PaymentForms
					.Where(f => f.UserId == request.ChildId && f.Status == Status.Approved && f.PaymentTypeId == PaymentType.RegistrationFee)
					.FirstOrDefaultAsync();
                if (payment == null)
                {
					LogError($"An attempt to get paymentId for matchingbonus with userId {request.Child} failed");
				}
				var wallet = await GetUserWallet(request.UserId).ConfigureAwait(false);
				if (wallet != null && payment!= null)
				{
					var creditWalletStatus = await CreditWallet(request.UserId, request.BonusAmount, payment?.Id, details).ConfigureAwait(false);
					//var creditPvWalletStatus =  CreditPvWallet(request.UserId, _generalConfiguration.MatchingPV, payment?.Id).Result;
					if (creditWalletStatus)
					{
						request.DatePaid = DateTime.Now;
						request.Status = BonusStusEnum.Paid;
						_context.Update(request);
						_context.SaveChanges();
						return true;
					}
                    else
                    {
					    LogError($"An attempt to credit wallet userId{request.UserId} failed");
					}
				}
				return false;
			}
			catch (Exception ex)
			{
                LogCritical(ex.Message + " , This exception message occurred while trying to assign matching bonus");
				throw ex;
			}
		}
		public bool RejectMatchingBonus(List<int> genLogIds)
		{
            try
            {
                foreach (var genLogId in genLogIds)
                {
                    var userGen =  _context.UserGenerationLogs.Where(s => s.Id == genLogId && s.Status == BonusStusEnum.Pending).Include(s => s.User).FirstOrDefault();
					if (userGen != null)
                    {
                        userGen.Status = BonusStusEnum.Rejected;
                        _context.Update(userGen);
                    }
                    else
                    {
						LogError($"Failed to reject matching bonus with genlogId {genLogId}");
					}
				}
				_context.SaveChanges();
				return true;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} This exception occured while trying to reject multiple bonuses ");
				throw ex;
            }
            
		}
        public bool RejectOneUserGenLog(int genLogId)
        {
            try
            {
                var Request = _context.UserGenerationLogs.Where(s => s.Id == genLogId && s.Status == BonusStusEnum.Pending).Include(s => s.User).FirstOrDefault();
				if (Request != null)
                {
                    Request.Status = BonusStusEnum.Rejected;
                    _context.Update(Request);
                    _context.SaveChanges();
                    return true;
                }
                else
                {
					LogError($"Failed to reject matching bonus with genlogId {genLogId}");
				}
				return false;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} This exception occured while trying to reject one matching bonus with genlogId {genLogId} ");
				throw ex;
            }

        }

        public async Task CreateGrantsRecord(string userId, Guid paymentId)
        {
            string toEmailBug = _generalConfiguration.DeveloperEmail;
            string subjectEmailBug = "CreateGrantsRecord Exception Message on GGC";
            try
            {

                var allGGCUsers = _userManager.Users.Where(x => x.Deactivated != true).Count();
                if (allGGCUsers > 0)
                {
                    var grantAmountPerUser = _generalConfiguration.GrantAmount / allGGCUsers;
                    var paymentFromRegFee = _context.PaymentForms.Where(x => x.Id == paymentId && x.PaymentTypeId == PaymentType.RegistrationFee).FirstOrDefault();

                    if (paymentFromRegFee != null && paymentFromRegFee.Id != null && paymentFromRegFee.Status == Status.Approved && grantAmountPerUser > 0)
                    {
                        var newRegFeeGrant = new RegFeeGrants
                        {
                            PaymentId = paymentFromRegFee.Id,
                            GrantAmountPerUser = grantAmountPerUser,
                            GrantDate = DateTime.Now
                        };

                        var freshRegFeePayment = await _context.AddAsync(newRegFeeGrant);
                        await _context.SaveChangesAsync();
                        if (freshRegFeePayment.Entity.Id != Guid.Empty)
                        {
                            await CreateUnearnedGrants(userId);

                        }
                    }

                }


            }
            catch (Exception ex)
            {
                string message = "Exception " + ex.Message + " and inner exception:" + ex.InnerException.Message + "  Occured at " + DateTime.Now;
                _emailService.SendEmailNonHangFire(toEmailBug, subjectEmailBug, message);
                //throw exp;
            }


        }

        public async Task<RegFeeGrants> CreateUnearnedGrants(string userId)
        {

            try
            {
                var details = "Grant  Share  Received ";

                var lastEarnedDates = DateTime.MinValue;
                if (_context.UserGrantHistories.Any() && _context.UserGrantHistories.Where(x => x.UserId == userId).Any())
                {
                    lastEarnedDates = _context.UserGrantHistories.Where(x => x.UserId == userId).Include(x => x.RegFeeGrants).Max(s => s.DateEarned);
                }
                if (lastEarnedDates == DateTime.MinValue)
                {
                    lastEarnedDates = _context.RegFeeGrants.Include(s => s.Payment).Where(s => s.Payment.UserId == userId).FirstOrDefault().GrantDate;
                }
                if (lastEarnedDates != null)
                {
                    var getGrantDates = _context.RegFeeGrants.Where(x => x.GrantDate >= lastEarnedDates).ToList();

                    foreach (var grant in getGrantDates)
                    {
                        await CreditWallet(userId, grant.GrantAmountPerUser, grant.PaymentId, details);
                        DebitGrantWalletNonAsync(userId, grant.GrantAmountPerUser, grant.PaymentId);
                        LogUserGrantRecord(grant, userId);
                    }

                }
                return null;

            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        public bool LogUserGrantRecord(RegFeeGrants grantHistory, string userId)
        {
            try
            {


                if (grantHistory != null && grantHistory.PaymentId != null && grantHistory.GrantAmountPerUser > 0)
                {
                    UserGrantHistory newHistory = new UserGrantHistory()
                    {

                        AmountEarned = grantHistory.GrantAmountPerUser,
                        RegFeeGrantId = grantHistory.Id,
                        UserId = userId,
                        DateEarned = DateTime.Now,

                    };
                    var result = _context.Add(newHistory);
                    _context.SaveChanges();
                    if (result.Entity.Id != null)
                    {
                        return true;
                    }
                }


                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool DebitGrantWalletNonAsync(string userId, decimal amount, Guid? paymentId)
        {

            try
            {


                if (userId != null && amount > 0)
                {

                    var wallet = GetUserGrantWalletNonAsync(userId);
                    if (wallet != null)
                    {
                        if (wallet.Balance >= amount)
                        {
                            wallet.Balance -= amount;
                            _context.Update(wallet);
                            _context.SaveChanges();
                            LogGrantWalletHistoryNonAsync(wallet, amount, TransactionType.Debit, paymentId);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> CreditWalletNewAsync(string userId, decimal amount, Guid? paymentId, string details)
        {
            try
            {
                if (userId != null && amount > 0)
                {

                    var wallet = await GetUserWallet(userId);
                    if (wallet == null)
                    {
                        wallet = CreateWalletByUserId(userId, amount);
                        if (wallet.Balance > 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        wallet.Balance += amount;
                        _context.Update(wallet);
                        _context.SaveChanges();
                        var result = await LogWalletHistory(wallet, amount, TransactionType.Credit, paymentId, details);
                        if (result)
                            return true;
                    }


                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public GrantWallet CreateGrantWalletByUserIdNonAsync(string userId, decimal amount = 0)
        {

            try
            {
                if (userId != null)
                {
                    var user = _userHelper.FindById(userId);
                    if (user != null)
                    {

                        var newWallet = new GrantWallet()
                        {
                            UserId = user.Id,
                            Balance = amount,
                            LastUpdated = DateTime.Now,

                        };

                        var result = _context.Add(newWallet);
                        _context.SaveChanges();

                        if (result.Entity.Id != null)
                        {
                            return result.Entity;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public AGCWallet CreateAGCWalletByUserIdNonAsync(string userId, decimal amount = 0)
        {
            try
            {
                if (userId != null)
                {
                    var user = _userHelper.FindById(userId);
					
                    if (user != null)
                    {

                        var newWallet = new AGCWallet()
                        {
                            UserId = user.Id,
                            Balance = amount,
                            LastUpdated = DateTime.Now,

                        };

                        var result = _context.Add(newWallet);
                        _context.SaveChanges();

                        if (result.Entity.Id != null)
                        {
                            return result.Entity;
                        }
                    }
                    else
                    {
						LogError($"Failed to find user with userId {userId}");
					}
				}
				return null;
            }
            catch (Exception ex)
            {
				LogCritical(ex.Message + " , This exception message occurred while trying to create AGC wallet");
				throw ex;
            }
        }
        public Wallet CreateWalletByUserIdNonAsync(string userId, decimal amount = 0)
        {
            try
            {
                if (userId != null)
                {
                    var user = _userHelper.FindById(userId);
					if (user != null)
                    {

                        var newWallet = new Wallet()
                        {
                            UserId = user.Id,
                            Balance = amount,
                            LastUpdated = DateTime.Now,

                        };

                        var result = _context.Add(newWallet);
                        _context.SaveChanges();

                        if (result.Entity.Id != null)
                        {
                            return result.Entity;
                        }
                    }
                    else
                    {
						LogError($"An attempt to get user with userId {userId} failed");
					}
				}
                return null;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} Failed to create Wallet with userId {userId}");
				throw ex;
            }
        }

        public async Task<bool> CreditWallet(string userId, decimal amount, Guid? paymentId, string details)
        {
            try
            {
                if (userId != null && amount > 0)
                {
                    var wallet = GetUserWallet(userId).Result;
                    if (wallet == null)
                    {
                        wallet = CreateWalletByUserIdNonAsync(userId, amount);
                        if (wallet?.Balance > 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        wallet.Balance += amount;
                        _context.Update(wallet);
                        _context.SaveChanges();

                        await CreditPvWallet(userId, amount, paymentId);
						var result = await LogWalletHistory(wallet, amount, TransactionType.Credit, paymentId, details);
                        if (result)
							LogInformation("Wallet Credited");
						    return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} Failed to credit Wallet with userId {userId} and paymentId {paymentId}");
				throw ex;
            }
        }

        public async Task<bool> CreditWalletForOldUsers(string userId, decimal amount, Guid? paymentId, string details)
        {
            try
            {
                if (userId != null && amount > 0)
                {
                    var wallet = GetUserWallet(userId).Result;
                    if (wallet == null)
                    {
                        wallet = CreateWalletByUserIdNonAsync(userId, amount);
                        if (wallet?.Balance > 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        wallet.Balance += amount;
                        _context.Update(wallet);
                        _context.SaveChanges();
                        var result = await LogWalletHistory(wallet, amount, TransactionType.Credit, paymentId, details);
                        if (result)
                            return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public PvWallet GetUserPvWalletNonAsync(string userId)
        {
            try
            {
                if (userId != null)
                {
                    var wallet = _context.PvWallets.Where(x => x.UserId == userId)?.Include(s => s.User)?.FirstOrDefault();
					if (wallet != null && wallet.UserId != null)
                    {
                        return wallet;
                    }
                    else
                    {
                        return CreatePvWalletByUserIdNonAsync(userId);
                    }
                }

                return null;
            }
            catch (Exception ex)
			{
				LogCritical($" {ex.Message} This exception message occurred while trying to get userPV with userId {userId}");
				throw ex;
            }
        }

        public async Task<PvWallet> GetUserPvWalletAsync(string userId)
        {
            try
            {
                if (userId != null)
                {
                    var wallet = await _context.PvWallets.Where(x => x.UserId == userId)?.Include(s => s.User)?.FirstOrDefaultAsync();
                    if (wallet != null && wallet.UserId != null)
                    {
                        return wallet;
                    }
                    else
                    {
                        return CreatePvWalletByUserIdNonAsync(userId);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public PvWallet CreatePvWalletByUserIdNonAsync(string userId, decimal amount = 0)
        {
            try
            {
                if (userId != null)
                {
                    var user = _userHelper.FindById(userId);
					if (user != null)
                    {
                        var newWallet = new PvWallet()
                        {
                            UserId = user.Id,
                            Balance = amount,
                            LastUpdated = DateTime.Now,

                        };
                        var result = _context.Add(newWallet);
                        _context.SaveChanges();

                        if (result.Entity.Id != null)
                        {
                            return result.Entity;
                        }
                    }
                    else
                    {
						LogError($"Failed to get user for creating PV wallet with userId {userId}");
					}
				}
                return null;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} This exception message occurred while trying to create userPV wallet with userId {userId}");
				throw ex;
            }
        }
        public GrantWallet GetUserGrantWalletNonAsync(string userId)
        {
            try
            {
                if (userId != null)
                {
                    var wallet = _context.GrantWallets.Where(x => x.UserId == userId)?.Include(s => s.User)?.FirstOrDefault();
                    if (wallet != null && wallet.UserId != null)
                    {
                        return wallet;
                    }
                    else
                    {
                        return CreateGrantWalletByUserIdNonAsync(userId);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public AGCWallet GetUserAGCWalletNonAsync(string userId)
        {
            try
            {
                if (userId != null)
                {
                    var wallet = _context.AGCWallets.Where(x => x.UserId == userId)?.Include(s => s.User)?.FirstOrDefault();
					if (wallet != null && wallet.UserId != null)
                    {
                        return wallet;
                    }
                    else
                    {
                        return CreateAGCWalletByUserIdNonAsync(userId);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
				LogCritical(ex.Message + " , This exception message occurred while trying to get user AGC wallet");
				throw ex;
            }
        }

        private string GenerateNumber()
        {
            return DateTime.Now.ToString().ToLower().Replace("am", "").Replace("pm", " ").Replace(":", "").Replace("/", "").Replace(" ", "");
        }
        public async Task AssignCordinatorBonus(string cordinatortorId, PaymentForm payment)
        {
            string toEmailBug = _generalConfiguration.DeveloperEmail;
            string subjectEmailBug = "AssignDistributorBonus for reg fee Exception Message on GAP";
            try
            {
                if (cordinatortorId != null)
                {
                    var status = false;
                    var wallet = await GetUserWallet(cordinatortorId);
                    var convertedCordinatortorBonus = _generalConfiguration.CoordinatorBonus / _generalConfiguration.DollarRate;
                    var alreadyReceivedBonus = _context.WalletHistories.Where(s => s.WalletId == wallet.Id && s.PaymentId == payment.Id);

                    if (!alreadyReceivedBonus.Any())
                    {
                        var amountInDollars = convertedCordinatortorBonus.ToString("F2");
                        var details = "Cordinatortor Bonus of $" + amountInDollars + " Credited and Received  from GAP registration beneficiary.";
                        status = await CreditWallet(cordinatortorId, _generalConfiguration.CoordinatorBonus, payment.Id, details);
                        if (!status)
                        {
							LogError($"An attempt to credit wallet for cordinator {cordinatortorId} failed");
						}

						string toEmail = wallet.User.Email;
                        string subject = "Hooray!!!, Cordinator Bonus Received ";
                        string message = "Hello you have been chosen as a Cordinator by " + payment.User.UserName + " and a cordinator bonus of $" + amountInDollars +
                            " has been credited to your wallet.<br> Keep Using GAP for more investment bonus.<br> Kind Regards,<br> GAP Team.";
                        _emailService.SendEmail(toEmail, subject, message);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = "Exception " + ex.Message + " and inner exception:" + ex.InnerException.Message + "  Occured at " + DateTime.Now;
				LogCritical($" {ex.Message} This exception occured while trying to assign cordinator bonus with id {cordinatortorId}");
				_emailService.SendEmail(toEmailBug, subjectEmailBug, message);
                //throw exp;
            }
        }
        public async Task<bool> CreateWallet(string userEmail, decimal amount = 0)
        {
            try
            {
                if (userEmail != null)
                {
                    var user = await _userHelper.FindByEmailAsync(userEmail);
                    if (user != null)
                    {
                        var wallet = GetUserWallet(user.Id);
                        if (wallet == null)
                        {
                            var newWallet = new Wallet()
                            {
                                UserId = user.Id,
                                Balance = amount,
                                LastUpdated = DateTime.Now,
                            };
                            var result = await _context.AddAsync(newWallet);
                            await _context.SaveChangesAsync();

                            if (result.Entity.Id != null)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Wallet> GetUserWallet(string userId)
        {
            try
            {
                if (userId != null)
                {
                    var wallet = await _context.Wallets.Where(x => x.UserId == userId)?.Include(s => s.User)?.FirstOrDefaultAsync();

					if (wallet != null && wallet.UserId != null)
                    {
                        return wallet;
                    }
                    else
                    {
                        return CreateWalletByUserId(userId);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} This exception occured while trying to get user wallet");
				throw ex;
            }
        }
        public  Wallet CreateWalletByUserId(string userId, decimal amount = 0)
        {
            try
            {
                if (userId != null)
                {
                    var user = _userHelper.FindByIdAsync(userId).Result;
					if (user != null)
                    {
                        var newWallet = new Wallet()
                        {
                            UserId = user.Id,
                            Balance = amount,
                            LastUpdated = DateTime.Now,
                        };

                        _context.Wallets.Add(newWallet);
                         _context.SaveChanges();

                        return newWallet;
                    }
                    else
                    {
					    LogError($"An attempt to get user for creating wallet with userId{userId} failed");
					}
				}
                return null;
            }
            catch (Exception ex)
            {
				// Log or handle the exception appropriately
				LogCritical($" {ex.Message} This exception occured while trying to create user wallet with userId {userId}");
				throw ex;
            }
        }

        public async Task<bool> LogWalletHistory(Wallet wallet, decimal amount, TransactionType transactionType, Guid? paymentId, string details)
        {
            try
            {
                if (wallet != null && wallet.UserId != null && amount > 0)
                {
                    WalletHistory newWalletHistory = new WalletHistory()
                    {
                        TransactionType = transactionType,
                        Amount = amount,
                        PaymentId = paymentId != Guid.Empty ? paymentId : null,
                        WalletId = wallet.Id,
                        Details = details,
                        NewBalance = wallet.Balance,
                        DateOfTransaction = DateTime.Now,

                    };
                    var result = _context.Add(newWalletHistory);
                    _context.SaveChanges();
                    if (result.Entity.Id != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} This exception occured while trying to log wallet history with userId {wallet.UserId} and {wallet.Id}");
				throw ex;
            }
        }


        public bool LogWalletHistoryNonAsync(Wallet wallet, decimal amount, TransactionType transactionType, Guid? paymentId, string details)
        {
            try
            {
                if (wallet != null && wallet.UserId != null && amount > 0)
                {
                    WalletHistory newWalletHistory = new WalletHistory()
                    {
                        TransactionType = transactionType,
                        Amount = amount,
                        PaymentId = paymentId != Guid.Empty ? paymentId : null,
                        WalletId = wallet.Id,
                        Details = details,
                        NewBalance = wallet.Balance,
                        DateOfTransaction = DateTime.Now,

                    };
                    var result = _context.Add(newWalletHistory);
                    _context.SaveChanges();
                    if (result.Entity.Id != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<bool> LogPvWalletHistory(PvWallet wallet, decimal amount, TransactionType transactionType, Guid? paymentId)
        {
            try
            {
                if (wallet != null && wallet.UserId != null && amount > 0)
                {
                    var newPvWalletHistory = new PvWalletHistory()
                    {
                        TransactionType = transactionType,
                        Amount = amount,
                        PaymentId = paymentId != Guid.Empty ? paymentId : null,
                        WalletId = wallet.Id,
                        NewBalance = wallet.Balance,
                        DateOfTransaction = DateTime.Now,
                        Details = "PV",
                    };
                    var result = _context.Add(newPvWalletHistory);
                    _context.SaveChanges();
                    if (result.Entity.Id != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} This exception occured while trying to log pv wallet history with userId {wallet.UserId} and {wallet.Id}");
				throw ex;
            }
        }

        public async Task<bool> LogGrantWalletHistory(GrantWallet wallet, decimal amount, TransactionType transactionType, Guid? paymentId)
        {
            try
            {
                if (wallet != null && wallet.UserId != null && amount > 0)
                {
                    var newGrantWalletHistory = new GrantWalletHistory()
                    {
                        TransactionType = transactionType,
                        Amount = amount,
                        PaymentId = paymentId != Guid.Empty ? paymentId : null,
                        WalletId = wallet.Id,
                        NewBalance = wallet.Balance,
                        DateOfTransaction = DateTime.Now,
                        Details = "Grants",

                    };
                    var result = _context.Add(newGrantWalletHistory);
                    _context.SaveChanges();
                    if (result.Entity.Id != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} This exception occured while trying to grant wallet history with userId {wallet.UserId} and {wallet.Id}");
				throw ex;
            }
        }

        public async Task<bool> LogAGCWalletHistory(AGCWallet wallet, decimal amount, TransactionType transactionType, Guid? paymentId)
        {
            try
            {
                if (wallet != null && wallet.UserId != null && amount > 0)
                {
                    var newAGCWalletHistory = new AGCWalletHistory ()
                    {
                        TransactionType = transactionType,
                        Amount = amount,
                        PaymentId = paymentId != Guid.Empty ? paymentId : null,
                        WalletId = wallet.Id,
                        NewBalance = wallet.Balance,
                        Details = "GAP GIFT CARD Transaction",
                        DateOfTransaction = DateTime.Now,
                    };
                    var result = _context.Add(newAGCWalletHistory);
                    _context.SaveChanges();
                    if (result.Entity.Id != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} This exception occured while trying to AGC wallet history with userId {wallet.UserId} and {wallet.Id}");
				throw ex;
            }
        }

        public bool LogGrantWalletHistoryNonAsync(GrantWallet wallet, decimal amount, TransactionType transactionType, Guid? paymentId)
        {
            try
            {
                if (wallet != null && wallet.UserId != null && amount > 0)
                {
                    var newGrantWalletHistory = new GrantWalletHistory()
                    {
                        TransactionType = transactionType,
                        Amount = amount,
                        PaymentId = paymentId != Guid.Empty ? paymentId : null,
                        WalletId = wallet.Id,
                        NewBalance = wallet.Balance,
                        DateOfTransaction = DateTime.Now,
                        Details = "Grants",

                    };
                    var result = _context.Add(newGrantWalletHistory);
                    _context.SaveChanges();
                    if (result.Entity.Id != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool LogAGCWalletHistoryNonAsync(AGCWallet wallet, decimal amount, TransactionType transactionType, Guid? paymentId)
        {
            try
            {
                if (wallet != null && wallet.UserId != null && amount > 0)
                {
                    var newAGCWalletHistory = new AGCWalletHistory()
                    {
                        TransactionType = transactionType,
                        Amount = amount,
                        PaymentId = paymentId != Guid.Empty ? paymentId : null,
                        WalletId = wallet.Id,
                        NewBalance = wallet.Balance,
                        DateOfTransaction = DateTime.Now,
                        Details = "GGC GIFT CARD",
                    };
                    var result = _context.Add(newAGCWalletHistory);
                    _context.SaveChanges();
                    if (result.Entity.Id != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task CordinatorAmount(PaymentForm payment)
        {
            string toEmail = _generalConfiguration.DeveloperEmail;
            string subjectEmailBug = " DistributeLeaderBoardAmount Exception Message";
            try
            {
                var status = false;
                //var allLeaderBoardMember = _context.Cordinators.Where(x => x.Id != 0).ToList();
                //if (allLeaderBoardMember != null && allLeaderBoardMember.Count() > 0)
                //{
                //    foreach (var member in allLeaderBoardMember)
                //    {
                //        var wallet = GetUserWalletNonAsync(member.LeaderBoardId);
                //        var alreadyReceivedBonus = _context.WalletHistories.Where(s => s.WalletId == wallet.Id && s.PaymentId == payment.Id);
                //        if (!alreadyReceivedBonus.Any())
                //        {
                //            var convertedLeaderBoardAmount = member.SpecificBoardAmount / _generalConfiguration.DollarRate;
                //            var amountInDollars = convertedLeaderBoardAmount.ToString("F2");
                //            var details = "LeaderBoard Fixed Amount of $" + amountInDollars + "" + "has been Credited after" + "" + payment?.User?.UserName + "'s registration";
                //            status = await CreditWallet(member.LeaderBoardId, member.SpecificBoardAmount, payment.Id, details);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                string message = "Exception " + ex.Message + " and inner exception:" + ex.InnerException.Message + "  Occured at " + DateTime.Now;
                _emailService.SendEmail(toEmail, subjectEmailBug, message);
                //throw ex;               
            }
        }

        public async Task EnqueueApproveRegFeePaymentProcess(PaymentForm payment)
        {
			BackgroundJob.Enqueue(() => AssignCordinatorBonus(payment.User.CordinatorId, payment));
		}

        public IPagedList<UserGenerationLogViewModel> listOfUserGenLog(UserGenerationLogSearchResultViewModel userGenerationLogSearch, int pageNumber, int pageSize)
        {
            try
            {
                var listOfUserGenLogQuery = _context.UserGenerationLogs
                .Where(x => x.Id != 0 && x.Status == BonusStusEnum.Pending && x.UserId != null)
				.OrderByDescending(s => s.DateCreated).AsQueryable();
				if (!string.IsNullOrEmpty(userGenerationLogSearch.UserName))
				{
					listOfUserGenLogQuery = listOfUserGenLogQuery.Where(v =>
						v.User.UserName.ToLower().Contains(userGenerationLogSearch.UserName.ToLower())
					);
				}
				if (userGenerationLogSearch.SortTypeFrom != DateTime.MinValue)
				{
					listOfUserGenLogQuery = listOfUserGenLogQuery.Where(v => v.DatePaid >= userGenerationLogSearch.SortTypeFrom);
				}
				if (userGenerationLogSearch.SortTypeTo != DateTime.MinValue)
				{
					listOfUserGenLogQuery = listOfUserGenLogQuery.Where(v => v.DatePaid <= userGenerationLogSearch.SortTypeTo);
				}


				var totalItemCount = listOfUserGenLogQuery.Count();
				var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

              var listOfUserGenLogs = listOfUserGenLogQuery.Select(a => new UserGenerationLogViewModel
              {
                    Id = a.Id,
                    Generation = a.Generation,
                    ChildId = a.ChildId,
                    UserId = a.UserId,
                    BonusFrom = _userManager.FindByIdAsync(a.ChildId).Result.UserName,
                    UserName = _userManager.FindByIdAsync(a.UserId).Result.UserName,
                    BonusAmount = a.BonusAmount / _generalConfiguration.DollarRate,
                    Status = a.Status,
                    DateCreated = a.DateCreated,
              }).OrderByDescending(x => x.DateCreated).ToPagedList(pageNumber, pageSize, totalItemCount);
                userGenerationLogSearch.PageCount = totalPages;
                userGenerationLogSearch.UserGenerationLogRecords = listOfUserGenLogs;
                if (listOfUserGenLogs != null && listOfUserGenLogs.Count() > 0)
                {
                    return listOfUserGenLogs;
                }
                return null;
            }
            catch (Exception ex)
            {
				LogCritical($" {ex.Message} This exception occured while trying to get listOfUserGenLog");
				throw ex;
            }
        }
        public bool CheckUserRegBonuses(List<int> genLogIds)
        {
            try
            {
                foreach (var genLogId in genLogIds)
                {
                    if (genLogId != null)
                    {
						AssignMatchingBonus(genLogId);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> GetOldMembers()
        {
            try
            {
                string filePath = _generalConfiguration.OldUsers;
                string jsonText = File.ReadAllText(filePath);
                var jsonData = JsonConvert.DeserializeObject<List<NewApplicationUser>>(jsonText);
                if (jsonData != null )
                {
                    if (!_context.ApplicationUser.Where(x => x.Id == jsonData.FirstOrDefault().Id && !x.UserName.ToLower().Contains("system")).Any())
                    {
                        var fullData = jsonData.Where(x => x.Id == null || (x.RefferrerId != null && !x.UserName.ToLower().Contains("system")))
                        .Select(s => new ApplicationUser
                        {
                            AccessFailedCount = s.AccessFailedCount,
                            TermsAndConditions = s.TermsAndConditions,
                            VTUActivationFeePaid = s.VTUActivationFeePaid,
                            CurrentLastLoginTime = s.CurrentLastLoginTime,
                            DateRegistered = s.DateRegistered,
                            Deactivated = true,
                            CordinatorId = s.DistributorId,
                            Email = s.Email,
                            FirstName = s.FirstName,
                            GenderId = GetGenderByName(s.Gender).Id,
                            PhoneNumber = s.PhoneNumber,
                            LastGenPaid = s.LastGenPaid,
                            LastLogoutTime = s.LastLogoutTime,
                            LastName = s.LastName,
                            LastPendingGen = s.LastPendingGen,
                            NormalizedEmail = s.Email.ToUpper(),
                            NormalizedUserName = s.UserName.ToUpper(),
                            ParentId = s.ParentId,
                            PasswordHash = s.PasswordHash,
                            RefferrerId = s.RefferrerId,
                            RefferrerUserName = s.RefferrerUserName,
                            RegFeePaid = s.RegFeePaid,
                            RememberPassword = s.RememberPassword,
                            UserName = s.UserName,
                            Id = s.Id,
                        }).OrderBy(o => o.DateRegistered).ToList();
                        _context.ApplicationUser.AddRange(fullData);
                    }
                    var entries = _context.SaveChanges();
                    foreach (var item in jsonData)
                    {
                        var paymentId = Guid.Empty;
                        var details = " Wallet Balance From Old Account";
                        await CreditWalletForOldUsers(item.Id, item.WalletBallance, null, details);
                        await CreditAGCWallet(item.Id, item.AGCWallettBallance, null);
                        await CreditPvWallet(item.Id, item.PvWallettBallance, null);
                    }
                    return entries;
                }


                //var client = new HttpClient();
                //client.BaseAddress = new Uri(_generalConfiguration.OldGGCEndPoint);
                //var endpoint = "/ActiveUsers";
                //var response = await client.GetAsync(endpoint);
                //if (response.IsSuccessStatusCode)
                //{
                //    var content = await response.Content.ReadAsStringAsync();
                //    var result = JsonConvert.DeserializeObject<List<NewApplicationUser>>(content);
                //    if (result != null)
                //    {
                //        if (!_context.ApplicationUser.Where(x=> x.Id ==result.FirstOrDefault().Id && !x.UserName.ToLower().Contains("system")).Any())
                //        {
                //            var fullData = result.Where(x => x.Id == null || (x.RefferrerId != null && !x.UserName.ToLower().Contains("system")))
                //            .Select(s => new ApplicationUser
                //            {
                //                AccessFailedCount = s.AccessFailedCount,
                //                TermsAndConditions = s.TermsAndConditions,
                //                VTUActivationFeePaid = s.VTUActivationFeePaid,
                //                CurrentLastLoginTime = s.CurrentLastLoginTime,
                //                DateRegistered = s.DateRegistered,
                //                Deactivated = true,
                //                CordinatorId = s.DistributorId,
                //                Email = s.Email,
                //                FirstName = s.FirstName,
                //                GenderId = GetGenderByName(s.Gender).Id,
                //                PhoneNumber = s.PhoneNumber,
                //                LastGenPaid = s.LastGenPaid,
                //                LastLogoutTime = s.LastLogoutTime,
                //                LastName = s.LastName,
                //                LastPendingGen = s.LastPendingGen,
                //                NormalizedEmail = s.Email.ToUpper(),
                //                NormalizedUserName = s.UserName.ToUpper(),
                //                ParentId = s.ParentId,
                //                PasswordHash = s.PasswordHash,
                //                RefferrerId = s.RefferrerId,
                //                RefferrerUserName = s.RefferrerUserName,
                //                RegFeePaid = s.RegFeePaid,
                //                RememberPassword = s.RememberPassword,
                //                UserName = s.UserName,
                //                Id = s.Id,
                //            }).OrderBy(o => o.DateRegistered).ToList();
                //            _context.ApplicationUser.AddRange(fullData);
                //        }
                        
                //        var entries = _context.SaveChanges();
                //        foreach (var item in result)
                //        {

                //            var paymentId = Guid.Empty;
                //            var details = " Wallet Balance From Old Account";
                //            await CreditWallet(item.Id, item.WalletBallance, null, details);
                //            await CreditAGCWallet(item.Id, item.AGCWallettBallance, null);
                //            await CreditPvWallet(item.Id, item.PvWallettBallance, null);
                //        }
                //        return entries;
                //    }
                //    else
                //    {
                //        throw new Exception("Deserialization failed: result is null.");

                //    }
                //}
                else
                {
                    throw new HttpRequestException($"Failed to retrieve application users. Status code:" /*{response.StatusCode}*/);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public CommonDropdowns GetGenderByName(string name)
        {
            return _context.CommonDropdowns.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
        }
    }
}
