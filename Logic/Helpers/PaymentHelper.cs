using Core.Config;
using Core.DB;
using Core.Models;
using Core.ViewModels;
using Logic.IHelpers;
using Logic.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Logic.Helpers
{
    public class PaymentHelper : BaseHelper, IPaymentHelper
    {
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IGeneralConfiguration _generalConfiguration;
        private readonly IBonusHelper _bonusHelper;

        public PaymentHelper(IEmailService emailService, AppDbContext context, IUserHelper userHelper, IGeneralConfiguration generalConfiguration, IBonusHelper bonusHelper)
        {
            _emailService = emailService;
            _context = context;
            _userHelper = userHelper;
            _generalConfiguration = generalConfiguration;
            _bonusHelper = bonusHelper;
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
                        return await CreateWalletByUserId(userId);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Wallet?> CreateWalletByUserId(string userId, decimal amount = 0)
        {
            if (userId is null) return null;
            var user = await _userHelper.FindByIdAsync(userId).ConfigureAwait(false);
            if (user is null) return null;
            var newWallet = new Wallet
            {
                UserId = user.Id,
                Balance = amount,
                LastUpdated = DateTime.Now,
            };
            await _context.AddAsync(newWallet);
            await _context.SaveChangesAsync();
            return newWallet.Id != null ? newWallet : null;
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
                }
                return null;
            }
            catch (Exception ex)
            {
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

        public UserPackages GetUserPackage(string userId)
        {
            if (userId != null)
            {
                var userPackage = _context.UserPackages.Where(x => x.UserId == userId && x.Active && !x.Deleted).Include(x => x.Package).OrderByDescending(x => x.DateCreated).LastOrDefault();
                if (userPackage != null)
                {
                    return userPackage;
                }
            }
            return null;
        }

		public bool CheckIfUserhasPendingRegPayment(string userId)
		{
			if (userId != null)
			{
				return _context.PaymentForms.Where(x => x.UserId == userId && x.Status == Status.Pending).Any();
			}
			return false;
		}

		public bool CreateRegFee(PaymentFormViewModel paymentDetails)
		{
			try
			{
				if (paymentDetails != null)
				{
					//var userPackage = GetUserPackage(paymentDetails.UserId);
					var packageAmount = _userHelper.GetPackageforUser(paymentDetails.PackageId);
					var paymentForm = new PaymentForm()
					{
						AccountNumberPaidFrom = paymentDetails.AccountNumberPaidFrom,
						GCCAccountId = paymentDetails.BankAccountId,
						BankNamePaidFrom = paymentDetails.BankNamePaidFrom,
						Amount = (decimal)packageAmount.Price,
						Details = "Bank Registration Fee Payment",
						PaidFrom = paymentDetails?.PaidFrom,
						PackageId = paymentDetails.PackageId,
						PaymentMethod = paymentDetails.PaymentMethod,
						PaymentTypeId = PaymentType.RegistrationFee,
						UserId = paymentDetails.UserId,
						Status = Status.Pending,
						Date = DateTime.Now,
					};
					_context.Add(paymentForm);

					var updateUserpackages = _context.UserPackages.Where(x => x.UserId == paymentForm.UserId && x.Active && !x.Deleted).FirstOrDefault();
					if (updateUserpackages != null)
					{
						updateUserpackages.DateOfPayment = DateTime.Now;
						updateUserpackages.PaymentId = paymentForm.Id;
						_context.Update(updateUserpackages);
					}
					_context.SaveChanges();

					var convertedPaymentAmount = paymentForm.Amount / _generalConfiguration.DollarRate;
					if (paymentForm.Id != Guid.Empty)
					{
						string toEmail = _generalConfiguration.AdminEmail;
						string subject = "GAP Registration  Payment ";
						string message = "Hello Admin, <br> A payment of $ <b>" + convertedPaymentAmount.ToString("F2") + "</b>" + " <br/> has been made to GAP " + "<b>" + paymentForm?.GGCAccount?.Name
								+ "</b>" + " bank account  by: "
								+ "<b>" + paymentForm?.PaidFrom + "</b>" + " <br/>  with account number: " + "<b>" + paymentForm?.AccountNumberPaidFrom + "</b>" + " and bank name of: " + "<b>" + paymentForm?.BankNamePaidFrom + "</b>" + "<br/>  through: " + "<b>" + paymentForm?.PaymentMethod +
							" </b>" + " for Registration Fee. <br/> <br/> Endeavor to confirm the pending Registration Fee Payment. <br/> Thanks!! ";

						_emailService.SendEmail(toEmail, subject, message);
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

        public bool CreateCryptoRegFeeAsync(PaymentFormViewModel paymentDetails)
        {
            try
            {
                if (paymentDetails != null)
                {
                    var packageAmount = _userHelper.GetPackageforUser(paymentDetails.PackageId);
                    var paymentForm = new PaymentForm()
                    {
                        AccountNumberPaidFrom = paymentDetails.AccountNumberPaidFrom,
                        GCCAccountId = paymentDetails.BankAccountId,
                        BankNamePaidFrom = paymentDetails.BankNamePaidFrom,
                        Amount = (decimal)packageAmount.Price,
                        Details = "Crypto Registration Fee Payment",
                        PaidFrom = paymentDetails.PaidFrom,
                        PackageId = paymentDetails.PackageId,
                        PaymentMethod = paymentDetails.PaymentMethod,
                        PaymentTypeId = PaymentType.RegistrationFee,
                        UserId = paymentDetails.UserId,
                        Id = paymentDetails.Id,
                        Status = Status.Pending,
                        Date = DateTime.Now,
                    };
                    _context.Add(paymentForm);

                    var updateUserpackages = _context.UserPackages.Where(x => x.UserId == paymentForm.UserId && x.Active && !x.Deleted).FirstOrDefault();
                    if (updateUserpackages != null)
                    {
                        updateUserpackages.DateOfPayment = DateTime.Now;
                        updateUserpackages.PaymentId = paymentForm.Id;
                        _context.Update(updateUserpackages);
                    }
                    _context.SaveChanges();

                    var convertedPaymentAmount = paymentForm.Amount / _generalConfiguration.DollarRate;
                    if (paymentForm.Id != Guid.Empty)
                    {
                        string toEmail = _generalConfiguration.AdminEmail;
                        string subject = "Crypto Registration  Payment ";
                        string message = "Hello Admin, <br> A payment of $ <b>" + convertedPaymentAmount.ToString("F2") + "</b>" + " <br/> has been made to GAP " + "<b>" + paymentForm?.GGCAccount?.Name
                                + "</b>" + " GAP Crypto Account  by: "
                                + "<b>" + paymentForm?.User.Name + "</b>" + " <br/>  with walletId of: " + "<b>" + paymentForm?.AccountNumberPaidFrom + "</b>" + " and Wallet name of: " + "<b>" + paymentForm?.BankNamePaidFrom + "</b>" + "<br/>  through: " + "<b>" + paymentForm?.PaymentMethod +
                            " </b>" + " for Registration Fee. <br/> <br/> Endeavor to confirm the pending Registration Fee Payment. <br/> <br/> Best Regards, <br/> GAP Supprot Group. <br/> Thanks!! ";

                        _emailService.SendEmail(toEmail, subject, message);
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
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

		public bool CheckIfUserHasPaidRegPayment(string userId)
		{
			if (userId != null)
			{
				var regPay = _context.ApplicationUser.Where(s => s.Id == userId && s.RegFeePaid == true).FirstOrDefault();
				if (regPay != null)
				{
					return true;
				}
			}
			return false;
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
                LogCritical($" {ex.Message} This exception occured while trying to create grant wallet");
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
                    else
                    {
                        LogError($"An attempt to get user for creating grant wallet with userId{userId} failed");
                    }


                }
                return null;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to create grant wallet");
                throw ex;
            }
        }

        public IPagedList<WalletHistoryViewModel> UserWalletHistoryRange(WalletHistorySearchResultViewModel transactionViewModel, string userId, int pageNumber, int pageSize)
        {
            try
            {
                var walletId = GetUserWallet(userId).Result.Id;
                var userWalletHistoryQuery = _context.WalletHistories.Where(s => s.Wallet.UserId == userId && s.WalletId == walletId).Include(s => s.Payment).Include(s => s.Wallet).Include(s => s.Payment.User)
                .OrderByDescending(s => s.DateOfTransaction).AsQueryable();

                if (!string.IsNullOrEmpty(transactionViewModel.UserName))
                {
                    userWalletHistoryQuery = userWalletHistoryQuery.Where(v =>
                        v.Wallet.User.UserName.ToLower().Contains(transactionViewModel.UserName.ToLower())
                    );
                }
                if (!string.IsNullOrEmpty(transactionViewModel.TransactionType))
                {
                    var transactionTypeString = transactionViewModel.TransactionType.ToLower();
                    userWalletHistoryQuery = userWalletHistoryQuery
                        .ToList()
                        .Where(v => v.TransactionType.ToString().ToLower().Contains(transactionTypeString))
                        .AsQueryable();
                }

                if (transactionViewModel.SortTypeFrom != DateTime.MinValue)
                {
                    userWalletHistoryQuery = userWalletHistoryQuery.Where(v => v.DateOfTransaction >= transactionViewModel.SortTypeFrom);
                }
                if (transactionViewModel.SortTypeTo != DateTime.MinValue)
                {
                    userWalletHistoryQuery = userWalletHistoryQuery.Where(v => v.DateOfTransaction <= transactionViewModel.SortTypeTo);
                }

                var totalItemCount = userWalletHistoryQuery.Count();
                var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

                var walletHistories = userWalletHistoryQuery.Select(c => new WalletHistoryViewModel
                {
                    Id = c.Id,
                    Wallet = c.Wallet,
                    Amount = c.Amount,
                    DateOfTransaction = c.DateOfTransaction,
                    Payment = c.Payment,
                    Details = c.Details,
                    NewBalance = c.NewBalance,
                    TransactionType = c.TransactionType,
                    PaymentId = c.PaymentId,
                    WalletId = c.WalletId,
                }).ToPagedList(pageNumber, pageSize, totalItemCount);
                transactionViewModel.PageCount = totalPages;
                transactionViewModel.WalletHistoryRecords = walletHistories;
                if (walletHistories.Count() != 0)
                {
                    return walletHistories;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to get userWalletHistory");
                throw ex;
            }
        }
        public IPagedList<WalletHistoryViewModel> SortUsersWalletHistory(WalletHistorySearchResultViewModel viewModel, int pageNumber, int pageSize)
        {
            try
            {
                var userWalletHistoryQuery = _context.WalletHistories.Include(s => s.Payment).Include(s => s.Wallet).Include(s => s.Wallet.User).Include(s => s.Payment.User).OrderByDescending(s => s.DateOfTransaction).AsQueryable();

                if (!string.IsNullOrEmpty(viewModel.UserName))
                {
                    userWalletHistoryQuery = userWalletHistoryQuery.Where(v =>
                        v.Wallet.User.UserName.ToLower().Contains(viewModel.UserName.ToLower())
                    );
                }
                if (!string.IsNullOrEmpty(viewModel.TransactionType))
                {
                    var transactionTypeString = viewModel.TransactionType.ToLower();
                    userWalletHistoryQuery = userWalletHistoryQuery
                        .ToList()
                        .Where(v => v.TransactionType.ToString().ToLower().Contains(transactionTypeString))
                        .AsQueryable();
                }

                if (viewModel.SortTypeFrom != DateTime.MinValue)
                {
                    userWalletHistoryQuery = userWalletHistoryQuery.Where(v => v.DateOfTransaction >= viewModel.SortTypeFrom);
                }
                if (viewModel.SortTypeTo != DateTime.MinValue)
                {
                    userWalletHistoryQuery = userWalletHistoryQuery.Where(v => v.DateOfTransaction <= viewModel.SortTypeTo);
                }

                var totalItemCount = userWalletHistoryQuery.Count();
                var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);


                var walletHistories = userWalletHistoryQuery.Select(c => new WalletHistoryViewModel
                {
                    Id = c.Id,
                    Wallet = c.Wallet,
                    Amount = c.Amount,
                    DateOfTransaction = c.DateOfTransaction,
                    Payment = c.Payment,
                    Details = c.Details,
                    NewBalance = c.NewBalance,
                    TransactionType = c.TransactionType,
                    PaymentId = c.PaymentId,
                    WalletId = c.WalletId,
                }).ToPagedList(pageNumber, pageSize, totalItemCount);
                viewModel.PageCount = totalPages;
                viewModel.WalletHistoryRecords = walletHistories;
                if (walletHistories.Count() > 0)
                {
                    return walletHistories;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to get WalletHistory for Admin in payment helper");
                throw ex;
            }
        }
        public IPagedList<PvWalletHistoryViewModel> SortPvWalletHistories(PvSearchResultViewModel pvSearchViewModel, int pageNumber, int pageSize)
        {
            try
            {
                var userPvHistoryQuery = _context.PvWalletHistories.Include(s => s.Payment).Include(s => s.Wallet).Include(s => s.Wallet.User).Include(s => s.Payment.User).OrderByDescending(s => s.DateOfTransaction).AsQueryable();

                if (!string.IsNullOrEmpty(pvSearchViewModel.UserName))
                {
                    userPvHistoryQuery = userPvHistoryQuery.Where(v =>
                        v.Wallet.User.UserName.ToLower().Contains(pvSearchViewModel.UserName.ToLower())
                    );
                }
                if (!string.IsNullOrEmpty(pvSearchViewModel.TransactionType))
                {
                    var transactionTypeString = pvSearchViewModel.TransactionType.ToLower();
                    userPvHistoryQuery = userPvHistoryQuery
                        .ToList()
                        .Where(v => v.TransactionType.ToString().ToLower().Contains(transactionTypeString))
                        .AsQueryable();
                }
                if (pvSearchViewModel.SortTypeFrom != DateTime.MinValue)
                {
                    userPvHistoryQuery = userPvHistoryQuery.Where(v => v.DateOfTransaction >= pvSearchViewModel.SortTypeFrom);
                }
                if (pvSearchViewModel.SortTypeTo != DateTime.MinValue)
                {
                    userPvHistoryQuery = userPvHistoryQuery.Where(v => v.DateOfTransaction <= pvSearchViewModel.SortTypeTo);
                }

                var totalItemCount = userPvHistoryQuery.Count();
                var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

                var pvWalletHistorySorts = userPvHistoryQuery.Select(c => new PvWalletHistoryViewModel
                {
                    Id = c.Id,
                    Wallet = c.Wallet,
                    Amount = c.Amount,
                    DateOfTransaction = c.DateOfTransaction,
                    Details = c.Details,
                    Payment = c.Payment,
                    NewBalance = c.NewBalance,
                    TransactionType = c.TransactionType,
                    PaymentId = c.PaymentId,
                    WalletId = c.WalletId,
                }).ToPagedList(pageNumber, pageSize, totalItemCount);
                pvSearchViewModel.PageCount = totalPages;
                pvSearchViewModel.PvWalletHistoryRecords = pvWalletHistorySorts;

                if (pvWalletHistorySorts.Count() > 0)
                {
                    return pvWalletHistorySorts;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to get PvWalletHistory for admin in payment helper");
                throw ex;
            }
        }
        public IPagedList<GrantHistoryViewModel> SortUserGrantWalletHistory(GrantHistorySearchResultViewModel grantHistoryViewModel, int pageNumber, int pageSize)
        {
            try
            {
                var userGrantHistoryQuery = _context.GrantWalletHistories.Include(s => s.Payment).Include(s => s.Wallet).Include(s => s.Wallet.User).Include(s => s.Payment.User).OrderByDescending(s => s.DateOfTransaction).AsQueryable();

                if (!string.IsNullOrEmpty(grantHistoryViewModel.UserName))
                {
                    userGrantHistoryQuery = userGrantHistoryQuery.Where(v =>
                        v.Wallet.User.UserName.ToLower().Contains(grantHistoryViewModel.UserName.ToLower())
                    );
                }
                if (!string.IsNullOrEmpty(grantHistoryViewModel.TransactionType))
                {
                    var transactionTypeString = grantHistoryViewModel.TransactionType.ToLower();
                    userGrantHistoryQuery = userGrantHistoryQuery
                        .ToList()
                        .Where(v => v.TransactionType.ToString().ToLower().Contains(transactionTypeString))
                        .AsQueryable();
                }
                if (grantHistoryViewModel.SortTypeFrom != DateTime.MinValue)
                {
                    userGrantHistoryQuery = userGrantHistoryQuery.Where(v => v.DateOfTransaction >= grantHistoryViewModel.SortTypeFrom);
                }
                if (grantHistoryViewModel.SortTypeTo != DateTime.MinValue)
                {
                    userGrantHistoryQuery = userGrantHistoryQuery.Where(v => v.DateOfTransaction <= grantHistoryViewModel.SortTypeTo);
                }

                var totalItemCount = userGrantHistoryQuery.Count();
                var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

                var grantWalletHistorySorts = userGrantHistoryQuery.Select(c => new GrantHistoryViewModel
                {
                    Id = c.Id,
                    Wallet = c.Wallet,
                    Amount = c.Amount,
                    DateOfTransaction = c.DateOfTransaction,
                    Details = c.Details,
                    Payment = c.Payment,
                    NewBalance = c.NewBalance,
                    TransactionType = c.TransactionType,
                    PaymentId = c.PaymentId,
                    WalletId = c.WalletId,
                }).ToPagedList(pageNumber, pageSize, totalItemCount);
                grantHistoryViewModel.PageCount = totalPages;
                grantHistoryViewModel.GrantHistoryRecords = grantWalletHistorySorts;
                if (grantWalletHistorySorts.Count() > 0)
                {
                    return grantWalletHistorySorts;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to get GrantHistory for admin in paymnethelper");
                throw ex;
            }
        }
        public IPagedList<AGCWalletHistoryViewModel> SortAGCWalletHistories(AGCSearchResultViewModel agcHistoryViewModel, int pageNumber, int pageSize)
        {
            try
            {

                var userAGCHistoryQuery = _context.AGCWalletHistories.Include(s => s.Payment).Include(s => s.Wallet).Include(s => s.Wallet.User).Include(s => s.Payment.User).OrderByDescending(s => s.DateOfTransaction).AsQueryable();

                if (!string.IsNullOrEmpty(agcHistoryViewModel.UserName))
                {
                    userAGCHistoryQuery = userAGCHistoryQuery.Where(v =>
                        v.Wallet.User.UserName.ToLower().Contains(agcHistoryViewModel.UserName.ToLower())
                    );
                }
                if (!string.IsNullOrEmpty(agcHistoryViewModel.TransactionType))
                {
                    var transactionTypeString = agcHistoryViewModel.TransactionType.ToLower();
                    userAGCHistoryQuery = userAGCHistoryQuery
                        .ToList()
                        .Where(v => v.TransactionType.ToString().ToLower().Contains(transactionTypeString))
                        .AsQueryable();
                }
                if (agcHistoryViewModel.SortTypeFrom != DateTime.MinValue)
                {
                    userAGCHistoryQuery = userAGCHistoryQuery.Where(v => v.DateOfTransaction >= agcHistoryViewModel.SortTypeFrom);
                }
                if (agcHistoryViewModel.SortTypeTo != DateTime.MinValue)
                {
                    userAGCHistoryQuery = userAGCHistoryQuery.Where(v => v.DateOfTransaction <= agcHistoryViewModel.SortTypeTo);
                }

                var totalItemCount = userAGCHistoryQuery.Count();
                var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

                var agcWalletHistorySorts = userAGCHistoryQuery.Select(c => new AGCWalletHistoryViewModel
                {
                    Id = c.Id,
                    Wallet = c.Wallet,
                    Amount = c.Amount,
                    DateOfTransaction = c.DateOfTransaction,
                    Details = c.Details,
                    Payment = c.Payment,
                    NewBalance = c.NewBalance,
                    TransactionType = c.TransactionType,
                    PaymentId = c.PaymentId,
                    WalletId = c.WalletId,
                }).ToPagedList(pageNumber, pageSize, totalItemCount);
                agcHistoryViewModel.PageCount = totalPages;
                agcHistoryViewModel.AGCWalletHistoryRecords = agcWalletHistorySorts;
                if (agcWalletHistorySorts.Count() > 0)
                {
                    return agcWalletHistorySorts;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to get AGCWalletHistory for admin in paymenthelper");
                throw ex;
            }
        }
        public IPagedList<AGCWalletHistoryViewModel> UserAGCWalletHistory(AGCSearchResultViewModel agcTransactionViewModel, string userId, int pageNumber, int pageSize)
        {
            try
            {
                var walletId = GetUserAGCWallet(userId).Result.Id;
                var userAGCWalletHistoryQuery = _context.AGCWalletHistories.Where(s => s.Wallet.UserId == userId && s.WalletId == walletId).Include(s => s.Payment).Include(s => s.Wallet).Include(s => s.Payment.User)
                .OrderByDescending(s => s.DateOfTransaction).AsQueryable();

                if (!string.IsNullOrEmpty(agcTransactionViewModel.UserName))
                {
                    userAGCWalletHistoryQuery = userAGCWalletHistoryQuery.Where(v =>
                        v.Wallet.User.UserName.ToLower().Contains(agcTransactionViewModel.UserName.ToLower())
                    );
                }
                if (!string.IsNullOrEmpty(agcTransactionViewModel.TransactionType))
                {
                    var transactionTypeString = agcTransactionViewModel.TransactionType.ToLower();
                    userAGCWalletHistoryQuery = userAGCWalletHistoryQuery
                        .ToList()
                        .Where(v => v.TransactionType.ToString().ToLower().Contains(transactionTypeString))
                        .AsQueryable();
                }

                if (agcTransactionViewModel.SortTypeFrom != DateTime.MinValue)
                {
                    userAGCWalletHistoryQuery = userAGCWalletHistoryQuery.Where(v => v.DateOfTransaction >= agcTransactionViewModel.SortTypeFrom);
                }
                if (agcTransactionViewModel.SortTypeTo != DateTime.MinValue)
                {
                    userAGCWalletHistoryQuery = userAGCWalletHistoryQuery.Where(v => v.DateOfTransaction <= agcTransactionViewModel.SortTypeTo);
                }

                var totalItemCount = userAGCWalletHistoryQuery.Count();
                var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

                var agcWalletHistories = userAGCWalletHistoryQuery.Select(c => new AGCWalletHistoryViewModel
                {
                    Id = c.Id,
                    Wallet = c.Wallet,
                    Amount = c.Amount,
                    DateOfTransaction = c.DateOfTransaction,
                    Payment = c.Payment,
                    Details = c.Details,
                    NewBalance = c.NewBalance,
                    TransactionType = c.TransactionType,
                    PaymentId = c.PaymentId,
                    WalletId = c.WalletId,
                }).ToPagedList(pageNumber, pageSize, totalItemCount);
                agcTransactionViewModel.PageCount = totalPages;
                agcTransactionViewModel.AGCWalletHistoryRecords = agcWalletHistories;
                if (agcWalletHistories.Count() > 0)
                {
                    return agcWalletHistories;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to get AGCWalletHistory for user in paymenthelper");
                throw ex;
            }
        }
        public IPagedList<GrantHistoryViewModel> ForUserGrantWalletHistory(GrantHistorySearchResultViewModel granTransactionViewModel, string userId, int pageNumber, int pageSize)
        {
            try
            {
                var walletId = GetUserGrantWallet(userId).Result.Id;
                var userGrantWalletHistoryQuery = _context.GrantWalletHistories.Where(s => s.Wallet.UserId == userId && s.WalletId == walletId).Include(s => s.Payment).Include(s => s.Wallet).Include(s => s.Payment.User)
                    .OrderByDescending(s => s.DateOfTransaction).AsQueryable();

                if (!string.IsNullOrEmpty(granTransactionViewModel.UserName))
                {
                    userGrantWalletHistoryQuery = userGrantWalletHistoryQuery.Where(v =>
                        v.Wallet.User.UserName.ToLower().Contains(granTransactionViewModel.UserName.ToLower())
                    );
                }
                if (!string.IsNullOrEmpty(granTransactionViewModel.TransactionType))
                {
                    var transactionTypeString = granTransactionViewModel.TransactionType.ToLower();
                    userGrantWalletHistoryQuery = userGrantWalletHistoryQuery
                        .ToList()
                        .Where(v => v.TransactionType.ToString().ToLower().Contains(transactionTypeString))
                        .AsQueryable();
                }

                if (granTransactionViewModel.SortTypeFrom != DateTime.MinValue)
                {
                    userGrantWalletHistoryQuery = userGrantWalletHistoryQuery.Where(v => v.DateOfTransaction >= granTransactionViewModel.SortTypeFrom);
                }
                if (granTransactionViewModel.SortTypeTo != DateTime.MinValue)
                {
                    userGrantWalletHistoryQuery = userGrantWalletHistoryQuery.Where(v => v.DateOfTransaction <= granTransactionViewModel.SortTypeTo);
                }

                var totalItemCount = userGrantWalletHistoryQuery.Count();
                var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

                var grantWalletHistories = userGrantWalletHistoryQuery.Select(c => new GrantHistoryViewModel
                {
                    Id = c.Id,
                    Wallet = c.Wallet,
                    Amount = c.Amount,
                    DateOfTransaction = c.DateOfTransaction,
                    Payment = c.Payment,
                    Details = c.Details,
                    NewBalance = c.NewBalance,
                    TransactionType = c.TransactionType,
                    PaymentId = c.PaymentId,
                    WalletId = c.WalletId,
                }).ToPagedList(pageNumber, pageSize, totalItemCount);
                granTransactionViewModel.PageCount = totalPages;
                granTransactionViewModel.GrantHistoryRecords = grantWalletHistories;
                if (grantWalletHistories.Count() > 0)
                {
                    return grantWalletHistories;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to get grant wallet history for user in paymenthelper");
                throw ex;
            }
        }

        public async Task<AGCWallet> GetUserAGCWallet(string userId)
        {
            try
            {
                if (userId != null)
                {
                    var wallet = await _context.AGCWallets.Where(x => x.UserId == userId)?.Include(s => s.User)?.FirstOrDefaultAsync();
                    if (wallet != null && wallet.UserId != null)
                    {
                        return wallet;
                    }
                    else
                    {
                        return await CreateAGCWalletByUserId(userId);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to Get User AGCWallet");
                throw ex;
            }
        }

        public async Task<AGCWallet> CreateAGCWalletByUserId(string userId, decimal amount = 0)
        {
            try
            {
                if (userId != null)
                {
                    var user = await _userHelper.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var newWallet = new AGCWallet()
                        {
                            UserId = user.Id,
                            Balance = amount,
                            LastUpdated = DateTime.Now,
                        };
                        var result = await _context.AddAsync(newWallet);
                        await _context.SaveChangesAsync();
                        if (result.Entity.Id != Guid.Empty)
                        {
                            return result.Entity;
                        }
                    }
                    else
                    {
                        LogError($" User not found");
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to create User AGCWallet from Payment Helper");
                throw ex;
            }
        }

        public bool CheckifDeactivated(string userId)
        {
            if (userId != null)
            {
                var check = _context.ApplicationUser.Where(a => a.Id == userId && a.Deactivated).FirstOrDefault();
                if (check != null)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckIfUserhasPendingActPayment(string userId)
        {
            var check = _context.PaymentForms.Where(x => x.UserId == userId && x.PaymentTypeId == PaymentType.ReActivationFee && x.Status == Status.Pending).FirstOrDefault();
            if (check != null)
            {
                return true;
            }
            return false;
        }

        public bool CheckForNoOfTokens(string token)
        {
            if (token != null)
            {
                decimal minToken = 0;
                decimal maxToken = 0;
                decimal noOfTokens = 0;

                var checkTokenNumber = _context.CompanySettings.Where(x => x.Id != 0 && x.Active && !x.Deleted).FirstOrDefault();
                if (checkTokenNumber != null)
                {
                    var leastToken = checkTokenNumber.MinimumToken;
                    var highestToken = checkTokenNumber.MaximumToken;
                    var tokenNumber = token;

                    minToken = Convert.ToDecimal(leastToken);
                    maxToken = Convert.ToDecimal(highestToken);
                    noOfTokens = Convert.ToDecimal(tokenNumber);

                    if (noOfTokens >= minToken && noOfTokens <= maxToken)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CreateCoinPayment(PaymentFormViewModel paymentDetails)
        {
            try
            {
                if (paymentDetails != null)
                {
                    var paymentForm = new PaymentForm()
                    {
                        AccountNumberPaidFrom = paymentDetails.AccountNumberPaidFrom,
                        GCCAccountId = paymentDetails.BankAccountId,
                        BankNamePaidFrom = paymentDetails.BankNamePaidFrom,
                        Amount = paymentDetails.Amount,
                        Details = GetPaymentDetails(paymentDetails.PaymentTypeId),
                        PaidFrom = paymentDetails?.PaidFrom,
                        PaymentMethod = paymentDetails.PaymentMethod,
                        PaymentTypeId = paymentDetails.PaymentTypeId,
                        UserId = paymentDetails.UserId,
                        Status = Status.Pending,
                        Date = DateTime.Now,
                        NoOfTokensBought = paymentDetails?.NoOfTokensBought,
                        PackageId = paymentDetails?.PackageId != 0 ? paymentDetails.PackageId : null,
                    };
                    _context.Add(paymentForm);
                    _context.SaveChanges();

                    var convertedAmount = paymentForm.Amount / _generalConfiguration.DollarRate;
                    if (paymentForm.Id != Guid.Empty)
                    {
                        if (paymentForm.PaymentTypeId == PaymentType.TokenFee)
                        {
                            string toEmail = _generalConfiguration.AdminEmail;
                            string subject = "GAP Token Payment ";
                            string message = "Hello Admin, <br> A payment of $ <b>" + convertedAmount.ToString("F2") + "</b>" + " <br/> has		been made to GAP " + "<b>" + paymentForm?.GGCAccount?.Name
                                    + "</b>" + " bank account  by: "
                                    + "<b>" + paymentForm?.PaidFrom + "</b>" + " <br/>  with account number: " + "<b>" + paymentForm?.AccountNumberPaidFrom + "</b>" + " and bank name of: " + "<b>" + paymentForm?.BankNamePaidFrom + "</b>" + "<br/>  through: " + "<b>" + paymentForm?.PaymentMethod +
                                " </b>" + " for Token Purchase. <br/> <br/> Endeavor to confirm the pending Token Payment. <br/> Thank you!!! ";

                            _emailService.SendEmail(toEmail, subject, message);
                            return true;
                        }
                        else if (paymentForm.PaymentTypeId == PaymentType.ReActivationFee)
                        {
                            string toEmail = _generalConfiguration.AdminEmail;
                            string subject = "GAP Re-Activation Payment ";
                            string message = "Hello Admin, <br> A payment of $ <b>" + convertedAmount.ToString("F2") + "</b>" + " <br/> has	been made to GAP " + "<b>" + paymentForm?.GGCAccount?.Name
                                    + "</b>" + " bank account  by: "
                                    + "<b>" + paymentForm?.PaidFrom + "</b>" + " <br/>  with account number: " + "<b>" + paymentForm?.AccountNumberPaidFrom + "</b>" + " and bank name of: " + "<b>" + paymentForm?.BankNamePaidFrom + "</b>" + "<br/>  through: " + "<b>" + paymentForm?.PaymentMethod +
                                " </b>" + " for Re-Activation Payment. <br/> <br/> Endeavor to confirm the pending Re-Activation Payment. <br/> Thank you!!! ";

                            _emailService.SendEmail(toEmail, subject, message);
                            return true;
                        }
                        else if (paymentForm.PaymentTypeId == PaymentType.PackageFee)
                        {
                            string toEmail = _generalConfiguration.AdminEmail;
                            string subject = "GAP Package Payment ";
                            string message = "Hello Admin, <br> A payment of $ <b>" + paymentForm?.Amount.ToString("F2") + "</b>" + " <br/> has been made to GAP " + "<b>" + paymentForm?.GGCAccount?.Name
                                    + "</b>" + " bank account  by: "
                                    + "<b>" + paymentForm?.PaidFrom + "</b>" + " <br/>  with account number: " + "<b>" + paymentForm?.AccountNumberPaidFrom + "</b>" + " and bank name of: " + "<b>" + paymentForm?.BankNamePaidFrom + "</b>" + "<br/>  through: " + "<b>" + paymentForm?.PaymentMethod +
                                " </b>" + " for Package Payment. <br/> <br/> Endeavor to confirm the pending Package Payment. <br/> Thank you!!! ";

                            _emailService.SendEmail(toEmail, subject, message);
                            return true;
                        }

                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to create coin payment in payment helper");
                throw ex;
            }
        }

        private string GetPaymentDetails(PaymentType paymentTypeId)
        {
            switch (paymentTypeId)
            {
                case PaymentType.PackageFee:
                    return "Payment for Package Fee";

                case PaymentType.VtuActivationFee:
                    return "Payment for VTU Activation Fee";

                case PaymentType.TokenFee:
                    return "Payment for Token Fee";

                case PaymentType.ReActivationFee:
                    return "Payment for Account Re-Activation Fee";

                default:
                    return string.Empty;
            }
        }

        public bool CheckActivationAmount(decimal amount)
        {
            if (amount > 0)
            {
                var checkAmount = _context.CompanySettings.Where(x => x.ActivationAmount == amount && x.Active).FirstOrDefault();
                if (checkAmount != null)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CreateCryptoTokenPayment(PaymentFormViewModel paymentDetails)
        {
            try
            {
                if (paymentDetails != null)
                {
                    var paymentForm = new PaymentForm()
                    {
                        AccountNumberPaidFrom = paymentDetails.AccountNumberPaidFrom,
                        GCCAccountId = paymentDetails.BankAccountId,
                        BankNamePaidFrom = paymentDetails.BankNamePaidFrom,
                        Amount = paymentDetails.Amount,
                        Details = GetPaymentDetails(paymentDetails.PaymentTypeId),
                        PaidFrom = paymentDetails.PaidFrom,
                        PaymentMethod = paymentDetails.PaymentMethod,
                        PaymentTypeId = paymentDetails.PaymentTypeId,
                        UserId = paymentDetails.UserId,
                        Status = Status.Pending,
                        Date = DateTime.Now,
                        NoOfTokensBought = paymentDetails?.NoOfTokensBought,
                        PackageId = paymentDetails?.PackageId != 0 ? paymentDetails.PackageId : null,

                    };
                    _context.Add(paymentForm);
                    _context.SaveChanges();

                    var convertedAmount = paymentForm.Amount / _generalConfiguration.DollarRate;
                    if (paymentForm.Id != Guid.Empty)
                    {
                        if (paymentForm.PaymentTypeId == PaymentType.TokenFee)
                        {
                            string toEmail = _generalConfiguration.AdminEmail;
                            string subject = "Crypto Token Payment";
                            string message = "Hello Admin, <br> A payment of $ <b>" + convertedAmount.ToString("F2") + "</b>" + " <br/> has been made to GAP " + "<b>" + paymentForm?.GGCAccount?.Name
                                    + "</b>" + " GAP Crypto Account  by: "
                                    + "<b>" + paymentForm?.User?.Name + "</b>" + " <br/>  with walletId of: " + "<b>" + paymentForm?.AccountNumberPaidFrom + "</b>" + " and Wallet name of: " + "<b>" + paymentForm?.BankNamePaidFrom + "</b>" + "<br/>  through: " + "<b>" + paymentForm?.PaymentMethod +
                                " </b>" + " for Token Payment. <br/> <br/> Endeavor to confirm the pending Token Payment. <br/> <br/> Best Regards, <br/> GAP Support Group. <br/> Thank you !!! ";

                            _emailService.SendEmail(toEmail, subject, message);
                            return true;
                        }
                        else if (paymentForm.PaymentTypeId == PaymentType.ReActivationFee)
                        {
                            string toEmail = _generalConfiguration.AdminEmail;
                            string subject = "Crypto Re-Activation Payment ";
                            string message = "Hello Admin, <br> A payment of $ <b>" + convertedAmount.ToString("F2") + "</b>" + " <br/> has	been made to GAP " + "<b>" + paymentForm?.GGCAccount?.Name
                                    + "</b>" + " GAP Crypto Account  by: "
                                    + "<b>" + paymentForm?.PaidFrom + "</b>" + " <br/>  with walletId of: " + "<b>" + paymentForm?.AccountNumberPaidFrom + "</b>" + " and Wallet name of: " + "<b>" + paymentForm?.BankNamePaidFrom + "</b>" + "<br/>  through: " + "<b>" + paymentForm?.PaymentMethod +
                                " </b>" + " for Re-Activation Payment. <br/> <br/> Endeavor to confirm the pending Re-Activation Payment. <br/> Thank you!!! ";

                            _emailService.SendEmail(toEmail, subject, message);
                            return true;
                        }
                        else if (paymentForm.PaymentTypeId == PaymentType.PackageFee)
                        {
                            string toEmail = _generalConfiguration.AdminEmail;
                            string subject = "GAP Crypto Package Payment ";
                            string message = "Hello Admin, <br> A payment of $ <b>" + paymentForm.Amount.ToString("F2") + "</b>" + " <br/> has	been made to GAP GGC " + "<b>" + paymentForm?.GGCAccount?.Name
                                    + "</b>" + " bank account  by: "
                                    + "<b>" + paymentForm?.PaidFrom + "</b>" + " <br/>  with account number: " + "<b>" + paymentForm?.AccountNumberPaidFrom + "</b>" + " and bank name of: " + "<b>" + paymentForm?.BankNamePaidFrom + "</b>" + "<br/>  through: " + "<b>" + paymentForm?.PaymentMethod +
                                " </b>" + " for Package Payment. <br/> <br/> Endeavor to confirm the pending Package Payment. <br/> Thank you!!! ";

                            _emailService.SendEmail(toEmail, subject, message);
                            return true;
                        }

                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to create crypto token payment in payment helper");
                throw ex;
            }
        }

        public IPagedList<PaymentFormViewModel> PendingRegFeeDetails(PendingPaymentsSearchResultViewModel pendingPaymentsSearch, int pageNumber, int pageSize)
        {
            try
            {
                var pendingPaymentsQuery = _context.PaymentForms.Where(x => x.Status == Status.Pending && x.PaymentTypeId == PaymentType.RegistrationFee || x.PaymentTypeId == PaymentType.TokenFee || x.PaymentTypeId == PaymentType.ReActivationFee || x.PaymentTypeId == PaymentType.PackageFee).Include(x => x.User).ThenInclude(x => x.UserPackages).Include(x => x.GGCAccount).Include(x => x.Packages).OrderByDescending(s => s.Date).AsQueryable();

                if (!string.IsNullOrEmpty(pendingPaymentsSearch.Name))
                {
                    pendingPaymentsQuery = pendingPaymentsQuery.Where(v =>
                        (v.User.FirstName + " " + v.User.LastName).ToLower().Contains(pendingPaymentsSearch.Name.ToLower())
                    );
                }
                if (!string.IsNullOrEmpty(pendingPaymentsSearch.Refferer))
                {
                    pendingPaymentsQuery = pendingPaymentsQuery.Where(v =>
                        (v.User.Refferrer.FirstName + " " + v.User.Refferrer.LastName)
                            .ToLower().Contains(pendingPaymentsSearch.Refferer.ToLower())
                    );
                }
                if (pendingPaymentsSearch.SortTypeFrom != DateTime.MinValue)
                {
                    pendingPaymentsQuery = pendingPaymentsQuery.Where(v => v.Date >= pendingPaymentsSearch.SortTypeFrom);
                }
                if (pendingPaymentsSearch.SortTypeTo != DateTime.MinValue)
                {
                    pendingPaymentsQuery = pendingPaymentsQuery.Where(v => v.Date <= pendingPaymentsSearch.SortTypeTo);
                }

                var totalItemCount = pendingPaymentsQuery.Count();
                var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);
                var pendingPayments = pendingPaymentsQuery.Select(x => new PaymentFormViewModel
                {
                    Details = x.Details,
                    AccountNumberPaidFrom = x.AccountNumberPaidFrom,
                    Amount = x.Amount,
                    Date = x.Date,
                    Name = x.User.Name,
                    Refferer = x.User.Refferrer.Name,
                    PaymentTypeId = x.PaymentTypeId,
                    Id = x.Id,
                    UserId = x.UserId,
                    CordinatorId = x.User.CordinatorId,
                    NoOfTokensBought = x.NoOfTokensBought,
                    Status = x.Status,
                    NewPackage = x.Packages.Name,
                    OldPackage = x.User.UserPackages.OrderByDescending(p => p.DateOfPayment).LastOrDefault().Name,



                }).ToPagedList(pageNumber, pageSize, totalItemCount);
                pendingPaymentsSearch.PageCount = totalPages;
                pendingPaymentsSearch.PaymentRecords = pendingPayments;
                if (pendingPayments.Count() > 0)
                {
                    return pendingPayments;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to get pending reg fee details");
                throw ex;
            }
        }

        public bool CheckIfApproved(Guid paymentId)
        {
            if (paymentId != Guid.Empty)
            {
                return _context.PaymentForms.Where(x => x.Id == paymentId && x.Status == Status.Approved).Include(x => x.User).Include(x => x.GGCAccount).Any();
            }
            return false;
        }

        public bool CheckIfDeclined(Guid paymentId)
        {
            if (paymentId != Guid.Empty)
            {
                return _context.PaymentForms.Where(x => x.Id == paymentId && x.Status == Status.Rejected).Any();
            }
            return false;
        }
        public bool CheckUserRegPayment(string userId)
        {
            if (userId != null)
            {
                var regPay = _context.PaymentForms.Where(s => s.UserId == userId && s.Status == Status.Pending).FirstOrDefault();
                if (regPay != null)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ApproveRegFee(Guid paymentId, string loggedInUser)
        {
            string toEmailBug = _generalConfiguration.DeveloperEmail;
            string subjectEmailBug = "AssignGrantLimitBonus Exception Message on GAP";
            try
            {
                if (paymentId != Guid.Empty)
                {
                    var regApprove = _context.PaymentForms.Where(x => x.Id == paymentId && x.Status == Status.Pending).Include(x => x.User).Include(x => x.Packages).FirstOrDefault();
                    if (regApprove != null)
                    {
                        regApprove.Status = Status.Approved;
                        regApprove.StatusBy = loggedInUser;
                        regApprove.StatuseChangeDate = DateTime.Now;
                        _context.Update(regApprove);

                        var userDetails = _context.ApplicationUser.Where(a => a.Id == regApprove.UserId && a.RegFeePaid == null || a.RegFeePaid == false).FirstOrDefault();
                        if (userDetails != null)
                        {
                            userDetails.RegFeePaid = true;
                            userDetails.RegFeePaymentId = paymentId;
                            _context.Update(userDetails);

                            var updateUserPackage = _context.UserPackages.Where(x => x.UserId == userDetails.Id && x.PaymentId == paymentId && x.Active && !x.Deleted).FirstOrDefault();
                            if (updateUserPackage != null)
                            {
                                if (updateUserPackage.PackageId != regApprove.PackageId)
                                {
                                    updateUserPackage.Name = regApprove.Packages?.Name;
                                    updateUserPackage.Amount = (decimal)regApprove.Packages.Price;
                                    updateUserPackage.UserId = regApprove.UserId;
                                    updateUserPackage.PackageId = (int)regApprove.PackageId;
                                    updateUserPackage.DateOfApproval = DateTime.Now;
                                    updateUserPackage.MaxGeneration = (int)regApprove.Packages?.MaxGeneration;
                                    updateUserPackage.Active = true;
                                    updateUserPackage.PaymentId = paymentId;
                                    _context.Update(updateUserPackage);
                                }
                                else
                                {
                                    updateUserPackage.DateOfApproval = DateTime.Now;
                                    _context.Update(updateUserPackage);
                                }
                                _context.SaveChanges();
                                //var convertedPayment = regApprove.Amount / _generalConfiguration.DollarRate;
                                if (userDetails?.Email != null)
                                {
                                    string toEmail = userDetails?.Email;
                                    string subject = "Hooray!!!, Registration Approved ";
                                    string message = "Hello " + "<b>" + userDetails?.UserName + ", </b>" + "<br>		GAP	 has approved your payment of $" + " <b> " + regApprove?.Amount.ToString("F2") + ". </b> <br> " +

                                   " Continue to explore the myriad opportunities for growth and prosperity with		GAP	 and get all your exclusive bonuses." +
                                   " <br> Keep Investing, Keep Earning. <br> " +
                                   " Thank you !!! ";
                                    _emailService.SendEmail(toEmail, subject, message);
                                    return true;
                                }
                            }
                            else
                            {
                                LogError($" updateUserPackage not found");
                            }
                        }
                        else
                        {
                            LogError($" userDetails not found");
                        }
                    }
                }
                else
                {
                    LogError($" paymentId not found");
                }
                return false;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to approve reg fee");
                string message = "Exception " + ex.Message + " and inner exception:" + ex.InnerException.Message + "  Occured at " + DateTime.Now;
                _emailService.SendEmail(toEmailBug, subjectEmailBug, message);
                throw;
            }
        }

        public bool RejectPayment(Guid paymentId, string loggedInUser)
        {
            try
            {
                var rejectPayment = _context.PaymentForms.Where(a => a.Id == paymentId && a.Status == Status.Pending).Include(a => a.User).FirstOrDefault();

                if (rejectPayment != null)
                {
                    rejectPayment.Status = Status.Rejected;
                    rejectPayment.StatusBy = loggedInUser;
                    rejectPayment.StatuseChangeDate = DateTime.Now;

                    _context.Update(rejectPayment);
                    _context.SaveChanges();
                    var convertedPaymentAmount = rejectPayment.Amount / _generalConfiguration.DollarRate;
                    if (rejectPayment.User.Email != null)
                    {
                        string toEmail = rejectPayment.User.Email;
                        string subject = " Registration Payment Declined";
                        string message = "Hello " + "<b>" + rejectPayment.User.UserName + "</b>" + ", <br> Your payment of $" + "<b>" + convertedPaymentAmount.ToString("F2") + "</b> For " + rejectPayment.Details + " have been Declined. " +
                        " Pls try and make the complete registration payment to continue. Keep Investing with GAP. <br> <br> Thanks!!! ";
                        _emailService.SendEmail(toEmail, subject, message);
                    }
                    return true;

                }
                return false;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to reject regfee payment");
                throw ex;
            }

        }

        public bool ApproveTokenFee(Guid paymentId, string loggedInUser)
        {
            string toEmailBug = _generalConfiguration.DeveloperEmail;
            string subjectEmailBug = "AssignGrantLimitBonus Exception Message on GAP";
            try
            {
                if (paymentId != Guid.Empty)
                {
                    var tokenApprove = _context.PaymentForms.Where(x => x.Id == paymentId && x.Status == Status.Pending).Include(x => x.User).FirstOrDefault();
                    if (tokenApprove != null)
                    {
                        tokenApprove.Status = Status.Approved;
                        tokenApprove.StatusBy = loggedInUser;
                        tokenApprove.StatuseChangeDate = DateTime.Now;
                        _context.Update(tokenApprove);
                        _context.SaveChanges();
                    }
                    var wallet = GetUserWallet(tokenApprove?.UserId).Result;
                    if (wallet != null)
                    {
                        var alreadyReceivedAGC = _context.AGCWalletHistories.Where(s => s.WalletId == wallet.Id && s.PaymentId == paymentId);
                        if (!alreadyReceivedAGC.Any())
                        {
                            var creditAGCWallet = _bonusHelper.CreditAGCWallet(tokenApprove.UserId, (decimal)tokenApprove.NoOfTokensBought, paymentId).Result;

                            var convertedTokenAmount = tokenApprove.Amount / _generalConfiguration.DollarRate;
                            if (creditAGCWallet)
                            {
                                string toEmail = tokenApprove?.User?.Email;
                                string subject = "Hooray!!!, Token Payment Approved ";
                                string message = "Hello " + "<b>" + tokenApprove?.User?.UserName + ", </b>" + "<br> GAP has approved your payment of $" + " <b> " + convertedTokenAmount.ToString("F2") + " </b> and your corresponding token has been added to your Coin Wallet " + "<b>" + "<br> <br>" +

                                " Continue to explore the myriad opportunities for growth and prosperity with GAP and get all your exclusive bonuses." +
                                " <br> Keep Investing, Keep Earning. <br> " +
                                " Thank you !!! ";
                                _emailService.SendEmail(toEmail, subject, message);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        LogError($" wallet not found");
                    }
                }
                else
                {
                    LogError($" paymentId not found");
                }
                return false;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to approve token payment in payment helper");

                string message = "Exception " + ex.Message + " and inner exception:" + ex.InnerException.Message + "  Occured at " + DateTime.Now;
                _emailService.SendEmail(toEmailBug, subjectEmailBug, message);
                throw;
            }
        }

        public bool RejectTokenPayment(Guid paymentId, string loggedInUser)
        {
            var rejectTokenPayment = _context.PaymentForms.Where(a => a.Id == paymentId && a.Status == Status.Pending).Include(a => a.User).FirstOrDefault();
            if (rejectTokenPayment != null)
            {
                rejectTokenPayment.Status = Status.Rejected;
                rejectTokenPayment.StatusBy = loggedInUser;
                rejectTokenPayment.StatuseChangeDate = DateTime.Now;

                _context.Update(rejectTokenPayment);
                _context.SaveChanges();
                var convertedTokenAmount = rejectTokenPayment.Amount / _generalConfiguration.DollarRate;
                if (rejectTokenPayment.User.Email != null)
                {
                    string toEmail = rejectTokenPayment?.User?.Email;
                    string subject = "Token Payment Declined";
                    string message = "Hello " + "<b>" + rejectTokenPayment?.User?.UserName + "</b>" + ", <br> Your payment of $" + "<b>" + convertedTokenAmount.ToString("F2") + "</b> for " + rejectTokenPayment.Details + " have been declined. " +
                    " Pls try and make the complete payment to continue. Keep Investing with GAP. <br> <br> Thanks!!! ";
                    _emailService.SendEmail(toEmail, subject, message);
                }
                return true;
            }
            else
            {
                LogError($" Could not find token payment to reject with paymentId {paymentId}");
            }
            return false;
        }

        public async Task<bool> CreditWallet(string userId, decimal amount, Guid? paymentId, string details)
        {

            try
            {
                if (userId != null && amount > 0)
                {
                    //GetUserWallet
                    var wallet = GetUserWalletNonAsync(userId);
                    if (wallet == null)
                    {
                        wallet = CreateWalletByUserIdNonAsync(userId, amount);
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
                        await _bonusHelper.CreditPvWallet(userId, amount, paymentId);
                        var result = await LogWalletHistory(wallet, amount, TransactionType.Credit, paymentId, details);
                        if (result)
                            return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to credit user Wallet in payment helper");
                throw ex;
            }
        }

        public Wallet GetUserWalletNonAsync(string userId)
        {
            try
            {
                if (userId != null)
                {
                    var wallet = _context.Wallets.Where(x => x.UserId == userId)?.Include(s => s.User)?.FirstOrDefault();
                    if (wallet != null && wallet.UserId != null)
                    {
                        return wallet;
                    }
                    else
                    {
                        return CreateWalletByUserIdNonAsync(userId);
                    }

                }

                return null;

            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to get user Wallet in payment helper");
                throw ex;
            }

        }

        public Wallet? CreateWalletByUserIdNonAsync(string userId, decimal amount = 0)
        {
            if (userId == null)
            {
                LogError($" UserId{userId} not found");
                return null;
            };
            var user = _userHelper.FindById(userId);

            if (user == null)
            {
                LogError($"An attempt to get user for creating wallet with userId{userId} failed");
                return null;
            }
            var newWallet = new Wallet
            {
                UserId = user.Id,
                Balance = amount,
                LastUpdated = DateTime.Now,
            };
            _context.Add(newWallet);
            _context.SaveChanges();
            return newWallet;
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
                    if (result.Entity.Id != Guid.Empty)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to log wallet history in payment helper");
                throw ex;
            }
        }

        public IPagedList<WithdrawalViewModel> GetPendingWithdrawals(PendingWithdrawalsSearchResultViewModel searchResultViewModel, int pageNumber, int pageSize)
        {

            var rate = _generalConfiguration.GGCWithdrawalConversionToNaira;
            var pendingWithdrawQuery = _context.WithdrawFunds
                .Where(s => s.WithdrawStatus == Status.Pending && s.UserId != null)
                .OrderByDescending(s => s.DateRequested).AsQueryable();

            if (!string.IsNullOrEmpty(searchResultViewModel.AccountNumber))
            {
                pendingWithdrawQuery = pendingWithdrawQuery.Where(v =>
                    v.AccountNumber.ToLower().Contains(searchResultViewModel.AccountNumber.ToLower())
                );
            }
            if (!string.IsNullOrEmpty(searchResultViewModel.AccountName))
            {
                pendingWithdrawQuery = pendingWithdrawQuery.Where(v =>
                    v.AccountName.ToLower().Contains(searchResultViewModel.AccountName.ToLower())
                );
            }
            if (searchResultViewModel.SortTypeFrom != DateTime.MinValue)
            {
                pendingWithdrawQuery = pendingWithdrawQuery.Where(v => v.DateRequested >= searchResultViewModel.SortTypeFrom);
            }
            if (searchResultViewModel.SortTypeTo != DateTime.MinValue)
            {
                pendingWithdrawQuery = pendingWithdrawQuery.Where(v => v.DateRequested <= searchResultViewModel.SortTypeTo);
            }

            var totalItemCount = pendingWithdrawQuery.Count();
            var totalPages = (int)Math.Ceiling((double)totalItemCount / pageSize);

            var pendingWithdrawals = pendingWithdrawQuery.Select(s => new WithdrawalViewModel
            {
                Id = s.Id,
                DateRequested = s.DateRequested,
                RequestedBy = s.RequestedBy,
                AccountName = s.AccountName,
                AccountNumber = s.AccountNumber,
                BankAccountName = s.BankAccountName,
                CreditedBy = s.CreditedBy,
                WithdrawalType = s.WithdrawalType,
                WithdrawStatus = s.WithdrawStatus,
                DateApprovedAndSent = s.DateApprovedAndSent,
                Amount = s.Amount / rate,
            }).ToPagedList(pageNumber, pageSize, totalItemCount);
            searchResultViewModel.PageCount = totalPages;
            searchResultViewModel.WithdrawalRecords = pendingWithdrawals;
            if (pendingWithdrawals.Any())
            {
                return pendingWithdrawals;
            }
            return pendingWithdrawals;
        }

        public bool ApproveWithdrawalRequest(WithdrawFunds withdrawFunds, string currentUserId, Guid withdrawalRequestId)
        {
            try
            {
                string toEmail = withdrawFunds.User.Email;
                string subject = " Withdrawal Approved ";
                string message = "Hello "

                + "<b>" + withdrawFunds.User.UserName + "<b/>" + ", <br> Your Account has been credited with NGN " + "<b>"
                + withdrawFunds.Amount.ToString("G29") + "<b/> For " + " requesting withdrawal. Keep Investing with GAP . " + "<br> Thanks!!! ";

                if (withdrawFunds != null && withdrawFunds.WithdrawStatus == Status.Pending && withdrawFunds.User.RegFeePaid == true)
                {
                    withdrawFunds.WithdrawStatus = Status.Approved;
                    withdrawFunds.CreditedBy = withdrawFunds.User.UserName;
                    withdrawFunds.DateApprovedAndSent = DateTime.Now;
                    withdrawFunds.CreditedBy = currentUserId;

                    _context.Update(withdrawFunds);
                    _context.SaveChanges();
                    _emailService.SendEmail(toEmail, subject, message);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to approve withdrawal request in payment helper");
                throw ex;
            }
        }

        public async Task<bool> DebitWallet(string userId, decimal amount, Guid? paymentId, string details)
        {
            try
            {
                if (userId != null && amount > 0)
                {
                    var wallet = await GetUserWallet(userId);
                    if (wallet != null)
                    {
                        if (wallet.Balance >= amount)
                        {
                            wallet.Balance -= amount;
                            _context.Update(wallet);
                            await _context.SaveChangesAsync();
                            await LogWalletHistory(wallet, amount, TransactionType.Debit, paymentId, details);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to debit wallet in payment helper");
                throw ex;
            }
        }

        public WithdrawFunds RejectWithdrawalRequest(Guid withdrawalId)
        {
            try
            {
                if (withdrawalId != Guid.Empty)
                {
                    var withdrawal = _context.WithdrawFunds.Where(s => s.Id == withdrawalId && s.WithdrawStatus == Status.Pending || s.WithdrawStatus == Status.Approved).Include(s => s.User).FirstOrDefault();
                    if (withdrawal != null)
                    {
                        if (withdrawal.WithdrawStatus == Status.Pending)
                        {
                            withdrawal.WithdrawStatus = Status.Rejected;
                            _context.Update(withdrawal);
                            _context.SaveChanges();

                            string toEmail = withdrawal.User?.Email;
                            string subject = " Withdrawal Approved ";
                            string message = "Hello "
                            + "<b>" + withdrawal?.User?.UserName + "<b/>" + ", <br> Your withdrawal request has been rejected. Try earning more funds before applying for withdrawal. " + " <br> <br>. Keep Investing with GAP . " + "<br> Thanks!!! ";
                            return withdrawal;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogCritical($" {ex.Message} This exception occured while trying to reject withdrawal request in payment helper");

                throw ex;
            }
        }


    }
}
