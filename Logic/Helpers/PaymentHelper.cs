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

        public PaymentHelper(IEmailService emailService, AppDbContext context, IUserHelper userHelper, IGeneralConfiguration generalConfiguration)
        {
            _emailService = emailService;
            _context = context;
            _userHelper = userHelper;
            _generalConfiguration = generalConfiguration;
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


    }
}
