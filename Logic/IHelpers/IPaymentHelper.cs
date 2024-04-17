using Core.Models;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.IHelpers
{
    public interface IPaymentHelper
    {
        Task<Wallet> GetUserWallet(string userId);
        Task<Wallet?> CreateWalletByUserId(string userId, decimal amount = 0);
        PvWallet GetUserPvWalletNonAsync(string userId);
        GrantWallet GetUserGrantWalletNonAsync(string userId);
        UserPackages GetUserPackage(string userId);
        bool CheckIfUserhasPendingRegPayment(string userId);
        bool CreateRegFee(PaymentFormViewModel paymentDetails);
        bool CreateCryptoRegFeeAsync(PaymentFormViewModel paymentDetails);
        AGCWallet GetUserAGCWalletNonAsync(string userId);
        bool CheckIfUserHasPaidRegPayment(string userId);




	}
}
