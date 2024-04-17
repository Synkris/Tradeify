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

namespace Logic.Helpers
{
    public class PaymentHelper : IPaymentHelper
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

	}
}
