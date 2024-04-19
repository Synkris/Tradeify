using Core.Models;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Logic.IHelpers
{
    public interface IBonusHelper
    {
        Task AssignCordinatorBonus(string cordinatortorId, PaymentForm payment);
        bool CheckUserRegBonuses(List<int> genLogIds);
        Task<GrantWallet> CreateGrantWalletByUserId(string userId, decimal amount = 0);
        PvWallet CreatePvWalletByUserIdNonAsync(string userId, decimal amount = 0);
        Wallet CreateWalletByUserId(string userId, decimal amount = 0);
        Task<bool> CreditAdifgeedGiftCardWallet(string userId, Guid paymentId);
        Task EnqueueApproveRegFeePaymentProcess(PaymentForm payment);
        ///List<UserGenerationLogViewModel> listOfUserGenLog();
		bool RejectMatchingBonus(List<int> genLogIds);
        Task<bool> AssignMatchingBonus(int genLogId);
        //bool RejectOneUserGenLog(UserGenerationLog matchingBonuses);
        bool RejectOneUserGenLog(int genLogId);
        IPagedList<UserGenerationLogViewModel> listOfUserGenLog(UserGenerationLogSearchResultViewModel userGenerationLogSearch, int pageNumber, int pageSize);
		//bool BatchBonusPayment(string userId, Guid paymentId);
		Task<bool> CreditAGCWallet(string userId, decimal amount, Guid? paymentId);
        Task<bool> LogAGCWalletHistory(AGCWallet wallet, decimal amount, TransactionType transactionType, Guid? paymentId);
        Task<bool> CreditWallet(string userId, decimal amount, Guid? paymentId, string details);
        Task<bool> CreditPvWallet(string userId, decimal amount, Guid? paymentId);
        Task<int> GetOldMembers();
    }
}
