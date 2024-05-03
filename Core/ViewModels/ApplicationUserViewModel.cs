using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using X.PagedList;
using Core.Config;

namespace Core.ViewModels
{
    public class ApplicationUserSearchResultViewModel
    {
		private readonly IGeneralConfiguration _generalConfiguration;

		public ApplicationUserSearchResultViewModel(IGeneralConfiguration generalConfiguration)
		{
			_generalConfiguration = generalConfiguration;
		}
		public IPagedList<ApplicationUserViewModel> UserRecords { get; set; }

            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public string Name { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
            public string UserName { get; set; }
            public int PageCount { get; set; }
            public string PasswordHash { get; set; }

            public ApplicationUserSearchResultViewModel()
            {
			    PageNumber = _generalConfiguration.PageNumber;
			    PageSize = _generalConfiguration.PageSize;
		    }

    }
    public class ApplicationUserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
		public string Email { get; set; }
        public string UserName { get; set; }
        public string Phonenumber { get; set; }
        public string Name => FirstName + " " + LastName;
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string GenderName { get; set; }
        public int GenderId { get; set; }
        public virtual CommonDropdowns Gender { get; set; }
        public Guid? RegFeePaymentId { get; set; }
        public virtual PaymentForm RegFeePayment { get; set; }
        public bool? RegFeePaid { get; set; }
        public bool RememberPassword { get; set; }
        public string RefferrerId { get; set; }
        public virtual ApplicationUser Refferrer { get; set; }
        public string ParentId { get; set; }
        public virtual ApplicationUser Parent { get; set; }
        public string RefferrerUserName { get; set; }
        public bool TermsAndConditions { get; set; }
        public DateTime DateRegistered { get; set; }
        public DateTime CurrentLastLoginTime { get; set; }
        public DateTime LastLogoutTime { get; set; }
        public bool Deactivated { get; set; }
        public int LastGenPaid { get; set; }
        public int LastPendingGen { get; set; }
        public bool VTUActivationFeePaid { get; set; }
        public string? CordinatorId { get; set; }
        public string? CordinatorUsername { get; set; }
        public string? PackageName { get; set; }
        public int PackageId { get; set; }
        public bool IsRegFeePaid { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal GrantAmount { get; set; }
        public virtual PvWallet userPv { get; set; }
        public decimal Pv { get; set; }
		public decimal ConvertedGrant { get; set; }
		public decimal ConvertedGrantToGGC { get; set; }
		public decimal ConvertedBalanceToGGC { get; set; }
		public decimal ConvertedBalance { get; set; }
		public decimal ConvertedToken { get; set; }
		public bool MapGenButton { get; set; }
		public decimal? WalletBalance { get; set; } = 0;
		public decimal? PVBalance { get; set; } = 0;
        public decimal? AGCWalletBalance { get; set; } = 0;
		public List<ApplicationUserViewModel> UserbalanceList { get; set; }
		public bool isImpersonated { get; set; }
	}

    public class NewApplicationUser
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public string UserName { get; set; }
        public int GenderId { get; set; }
        public string Gender { get; set; }
        public Guid? RegFeePaymentId { get; set; }
        public virtual PaymentForm RegFeePayment { get; set; }
        public bool? RegFeePaid { get; set; }
        public bool RememberPassword { get; set; }
        public string RefferrerId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ParentId { get; set; }
        public string RefferrerUserName { get; set; }
        public bool TermsAndConditions { get; set; }
        public DateTime DateRegistered { get; set; }
        public DateTime CurrentLastLoginTime { get; set; }
        public DateTime LastLogoutTime { get; set; }
        public bool Deactivated { get; set; }
        public int LastGenPaid { get; set; }
        public int LastPendingGen { get; set; }
        public bool VTUActivationFeePaid { get; set; }
        public string DistributorId { get; set; }
        public int AccessFailedCount { get; set; }
        public decimal WalletBallance { get; set; }
        public decimal PvWallettBallance { get; set; }
        public decimal AGCWallettBallance { get; set; }
        public virtual PvWallet Balance { get; set; }
    }
}
