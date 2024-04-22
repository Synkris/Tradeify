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
        Task<GrantWallet> GetUserGrantWallet(string userId);
        IPagedList<WalletHistoryViewModel> UserWalletHistoryRange(WalletHistorySearchResultViewModel transactionViewModel, string userId, int pageNumber, int pageSize);
        IPagedList<GrantHistoryViewModel> ForUserGrantWalletHistory(GrantHistorySearchResultViewModel granTransactionViewModel, string userId, int pageNumber, int pageSize);
        IPagedList<AGCWalletHistoryViewModel> UserAGCWalletHistory(AGCSearchResultViewModel agcTransactionViewModel, string userId, int pageNumber, int pageSize);
        IPagedList<GrantHistoryViewModel> SortUserGrantWalletHistory(GrantHistorySearchResultViewModel grantHistoryViewModel, int pageNumber, int pageSize);
        IPagedList<PvWalletHistoryViewModel> SortPvWalletHistories(PvSearchResultViewModel pvSearchViewModel, int pageNumber, int pageSize);
        IPagedList<AGCWalletHistoryViewModel> SortAGCWalletHistories(AGCSearchResultViewModel agcHistoryViewModel, int pageNumber, int pageSize);
        Task<AGCWallet> GetUserAGCWallet(string userId);
        IPagedList<WalletHistoryViewModel> SortUsersWalletHistory(WalletHistorySearchResultViewModel viewModel, int pageNumber, int pageSize);
        bool CheckifDeactivated(string userId);
        bool CheckIfUserhasPendingActPayment(string userId);
        bool CheckForNoOfTokens(string token);
        bool CreateCoinPayment(PaymentFormViewModel paymentDetails);
        bool CheckActivationAmount(decimal amount);
        bool CreateCryptoTokenPayment(PaymentFormViewModel paymentDetails);
        IPagedList<PaymentFormViewModel> PendingRegFeeDetails(PendingPaymentsSearchResultViewModel pendingPaymentsSearch, int pageNumber, int pageSize);
        bool CheckIfApproved(Guid paymentId);
        bool CheckIfDeclined(Guid paymentId);
        bool CheckUserRegPayment(string userId);
        bool ApproveRegFee(Guid paymentId, string loggedInUser);
        bool RejectPayment(Guid paymentId, string loggedInUser);
        bool ApproveTokenFee(Guid paymentId, string loggedInUser);
        bool RejectTokenPayment(Guid paymentId, string loggedInUser);
        Task<bool> CreditWallet(string userId, decimal amount, Guid? paymentId, string details);
        IPagedList<WithdrawalViewModel> GetPendingWithdrawals(PendingWithdrawalsSearchResultViewModel searchResultViewModel, int pageNumber, int pageSize);
        bool ApproveWithdrawalRequest(WithdrawFunds withdrawFunds, string currentUserId, Guid withdrawalRequestId);
        Task<bool> DebitWallet(string userId, decimal amount, Guid? paymentId, string details);
        WithdrawFunds RejectWithdrawalRequest(Guid withdrawalId);





    }
}
