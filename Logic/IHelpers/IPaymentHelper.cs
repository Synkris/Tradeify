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
		public bool CreateRegFee(PaymentFormViewModel paymentDetails);
		bool CreateCryptoRegFeeAsync(PaymentFormViewModel paymentDetails);
		bool CheckIfUserhasPendingRegPayment(string userId);
		Task<Wallet> GetUserWallet(string userId);
		Task<List<WalletHistory>> GetUserWalletHistory(string userId);
		PvWallet GetUserPvWalletNonAsync(string userId);
		Task<GrantWallet> GetUserGrantWallet(string userId);
		PvWallet CreatePvWalletByUserIdNonAsync(string userId, decimal amount = 0);
		Task<Wallet> CreateWalletByUserId(string userId, decimal amount = 0);
		Task<GrantWallet> CreateGrantWalletByUserId(string userId, decimal amount = 0);
		bool CheckIfApproved(Guid paymentId);
		bool CheckUserRegPayment(string userId);
		bool ApproveRegFee(Guid paymentId, string user);
		bool RejectPayment(Guid paymentId, string loggedInUser);
        GrantWallet GetUserGrantWalletNonAsync(string userId);
        AGCWallet GetUserAGCWalletNonAsync(string userId);
        //Task<List<WalletHistory>> UserWalletHistoryRange(string userId, SortTransactionViewModel transactionVM);
        //Task<List<GrantWalletHistory>> ForUserGrantWalletHistoryByDate(SortTransactionViewModel granTransactionVM, string userId);
        //Task<List<PvWalletHistory>> SortPvWalletHistoriesByDateAndUserName(DateTime dateRangeFrom, DateTime dateRangeTo, string userName);
        //Task<List<GrantWalletHistory>> SortGrantWalletByDateAndUserName(DateTime dateRangeFrom, DateTime dateRangeTo, string userName);
        //Task<List<AGCWalletHistory>> SortAGCWalletHistories(DateTime dateRangeFrom, DateTime dateRangeTo, string userName);
        //List<WithdrawalViewModel> GetPendingWithdrawals();
        //List<PaymentFormViewModel> PendingRegFeeDetails();

        Task<AGCWallet> GetUserAGCWallet(string userId);
		Task<AGCWallet> CreateAGCWalletByUserId(string userId, decimal amount = 0);
		AGCWallet CreateAGCWalletByUserIdNonAsync(string userId, decimal amount = 0);
		GrantWallet CreateGrantWalletByUserIdNonAsync(string userId, decimal amount = 0);
        bool CheckIfUserHasPaidRegPayment(string userId);
		IPagedList<WalletHistoryViewModel> SortUsersWalletHistory(WalletHistorySearchResultViewModel model, int pageNumber, int pageSize);
		Wallet GetUserWalletNonAsync(string userId);
		Wallet? CreateWalletByUserIdNonAsync(string userId, decimal amount = 0);
		Task<bool> CreditWallet(string userId, decimal amount, Guid? paymentId, string details);
		Task<bool> LogWalletHistory(Wallet wallet, decimal amount, TransactionType transactionType, Guid? paymentId, string details);
		Task<bool> CreateWithdrawalRequest(WithdrawalViewModel withdrawFunds, string UserId);
		Task<bool> CreateCryptoWithdrawalRequest(WithdrawalViewModel withdrawFunds, string UserId);
		bool ApproveWithdrawalRequest(WithdrawFunds withdrawFunds, string currentUserId, Guid withdrawalRequestId);
		WithdrawFunds RejectWithdrawalRequest(Guid withdrawalId);
        Task<bool> DebitWallet(string userId, decimal amount, Guid? paymentId, string details);
        UserPackages GetUserPackage(string userId);
		IPagedList<GrantHistoryViewModel> SortUserGrantWalletHistory(GrantHistorySearchResultViewModel grantHistoryViewModel, int pageNumber, int pageSize);
		IPagedList<PvWalletHistoryViewModel> SortPvWalletHistories(PvSearchResultViewModel pvSearchViewModel, int pageNumber, int pageSize);
		IPagedList<AGCWalletHistoryViewModel> SortAGCWalletHistories(AGCSearchResultViewModel agcHistoryViewModel, int pageNumber, int pageSize);

		IPagedList<WalletHistoryViewModel> UserWalletHistoryRange(WalletHistorySearchResultViewModel transactionViewModel, string userId, int pageNumber, int pageSize);
		IPagedList<GrantHistoryViewModel> ForUserGrantWalletHistory(GrantHistorySearchResultViewModel granTransactionViewModel, string userId, int pageNumber, int pageSize);
		IPagedList<AGCWalletHistoryViewModel> UserAGCWalletHistory(AGCSearchResultViewModel agcTransactionViewModel, string userId, int pageNumber, int pageSize);
		IPagedList<WithdrawalViewModel> GetPendingWithdrawals(PendingWithdrawalsSearchResultViewModel searchResultViewModel, int pageNumber, int pageSize);
		IPagedList<PaymentFormViewModel> PendingRegFeeDetails(PendingPaymentsSearchResultViewModel pendingPaymentsSearch, int pageNumber, int pageSize);
		bool CreateCoinPayment(PaymentFormViewModel paymentDetails);
        bool CreateCryptoTokenPayment(PaymentFormViewModel paymentDetails);
		bool ApproveTokenFee(Guid paymentId, string loggedInUser);
        bool CheckIfDeclined(Guid paymentId);
        bool RejectTokenPayment(Guid paymentId, string loggedInUser);
        bool CheckForNoOfTokens(string token);
		bool ApproveReActivationFee(Guid paymentId, string loggedInUser);
		bool RejectReActivationPayment(Guid paymentId, string loggedInUser);
		Task<bool> CreditGGCToken(string userId, decimal token);
        bool CheckifDeactivated(string userId);
		bool ApprovePackageFee(Guid paymentId, string loggedInUser);
		bool RejectPackagePaymentFee(Guid paymentId, string loggedInUser);
        bool CheckActivationAmount(decimal amount);
        decimal GetReactivationAmount();
        bool CheckIfUserhasPendingActPayment(string userId);
    }
}
		
	
